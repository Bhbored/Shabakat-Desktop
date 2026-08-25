# JSON backup (offline restore)

Shabakat Settings → Backup exports and restores one JSON file. Use this when moving data from Visual Studio / `dotnet run` onto an MSI install, or when mapping **online SaaS** data into the desktop app.

**Example file (full dump, every collection + enum):** [`wwwroot/backup.example.json`](../wwwroot/backup.example.json)

Restore **replaces** all business data in SQLite. It does not merge.

## Cloud upload (optional)

Settings → Backup can also **upload** the same JSON to Cloudflare R2 while the app is open. It does **not** restore from the cloud. To recover, download the `.json` from R2 and use Restore as usual.

Cloud upload is skipped unless `CloudBackup:WorkerUrl` and `CloudBackup:Secret` are set (see [`appsettings.Local.json.example`](../appsettings.Local.json.example)). The Worker lives in [`cloudflare/backup-worker/`](../cloudflare/backup-worker/). Local `CloudBackupState` (install id, last upload time) is **not** part of the JSON file.

---

## Root object

| JSON key | Type | Notes |
|---|---|---|
| `version` | number | **JSON backup format** (shape of this file), not the app/MSI version and not an export counter. Currently `1`. Restore rejects any other value. |
| `exportedAt` | DateTime | When this dump was made. Use this (or the file name / R2 object key timestamp) to tell which backup is newest. |
| `appUser` | object or omit | Desktop PIN + license row. **Omit** when importing SaaS data so the PC keeps its own PIN/license. |
| `preferences` | object or omit | Pricing, language (`en` / `ar`), due-date day, ampere flags. |
| `exportColumns` | array | Excel column checkboxes. `appPreferencesId` must match `preferences.id`. |
| `areas` | array | |
| `distributionBoxes` | array | `areaId` must exist in `areas`. |
| `ampereSchedules` | array | |
| `customers` | array | Optional `areaId`, `boxId`, `ampereScheduleId`. |
| `meterReadings` | array | `customerId` must exist. |
| `invoices` | array | `invoiceNumber` is unique. Do not send `amountDue`. |
| `payments` | array | `customerId` + `invoiceId` must exist. `paidAmount` on the invoice must match the sum of payments. |
| `invoiceSkips` | array | Bulk-generate skip reasons for a billing period. |
| `expenses` | array | |
| `auditLogs` | array | Optional. Can be `[]`. |
| `auditLogDetails` | array | `auditLogId` must exist in `auditLogs`. |

Missing arrays are treated as empty. JSON is camelCase; names are case-insensitive on import.

Enums are **PascalCase strings** (not numbers).

---

## `version` vs app / SQLite schema

Three different numbers:

| What | Where | When it changes |
|---|---|---|
| App / MSI version | `Shabakat.csproj`, WiX | Each release. **Not** written into this JSON. |
| EF SQLite schema | `Infrastructure/Persistence/Migrations/` | Any entity/table change. Applied on app start via `Migrate()`. |
| Backup JSON `version` | `BackupFile.CurrentVersion` | Only when the **exported JSON contract** breaks (fields added/removed/renamed in a way old dumps cannot restore). |

`version` does **not** increment on every export. Every dump of the current format is `1` until you bump the constant.

### After a breaking backup-format change

1. Change load/restore in [`Infrastructure/Repository/BackupRepository.cs`](../Infrastructure/Repository/BackupRepository.cs) (and entities as needed).
2. Bump **`BackupFile.CurrentVersion`** in [`Application/Backup/BackupFile.cs`](../Application/Backup/BackupFile.cs) (today `1` → `2`).
3. Export already stamps `Version = BackupFile.CurrentVersion`; restore already fails unless `file.Version` matches ([`BackupService.cs`](../Application/Services/Backup/BackupService.cs)).
4. Update this doc and [`wwwroot/backup.example.json`](../wwwroot/backup.example.json).

A new EF migration alone does **not** require bumping `CurrentVersion` if the JSON keys stay compatible. Bump it when old JSON would be unsafe or incomplete to restore.

---

## Do not send

- Navigation objects (`area`, `customer`, `payments`, `details`, …)
- `amountDue` (SQLite computed: `totalAmount - paidAmount`)
- `hasPricingOverride` (derived from `priceOverride`)

---

## Enums

| Field | Values |
|---|---|
| `customerType` | `Residential`, `Commercial`, `Industrial` |
| `customerRelation` | `Friend`, `Family`, `Owner`, or `null` |
| `customerStatus` | `Active`, `Suspended`, `Terminated` |
| `plan` | `Ampere`, `Kilowatt`, `FixedKilowatt` |
| `invoiceStatus` | `Unpaid`, `PartiallyPaid`, `Paid` |
| `paymentMethod` | `Cash`, `Wish` |
| `expenseType` | `Fuel`, `Maintenance`, `Employees`, `Other` |
| `language` | `en`, `ar` |
| audit `action` | `CustomerCreated`, `CustomerUpdated`, `CustomerDeleted`, `InvoiceCreated`, `InvoiceBulkCreated`, `InvoicePaymentRecorded`, `InvoiceFixedKilowattCharge`, `ExpenseCreated`, `ExpenseUpdated`, `ExpenseDeleted` |
| audit `entityType` | `Customer`, `Invoice`, `Payment`, `Expense`, or `null` |
| audit `status` | `Success`, `Failed` |

---

## Dates and money

- `DateOnly` (`subscriptionDate`, `issueDate`, `dueDate`, `readingDate`, `expenseDate`, skip period): `"yyyy-MM-dd"`
- `DateTime` (`createdAt`, `updatedAt`, `paymentDate`, `exportedAt`): ISO local, e.g. `"2026-03-15T14:00:00"`
- `licensedUntil`: `DateTimeOffset`, e.g. `"2027-01-01T00:00:00+03:00"`
- Decimals as numbers. `paidAmount` must be `>= 0` and `<= totalAmount`.

Every row that uses `Base` needs `id` (GUID), `createdAt`, `updatedAt`. `auditLogs` / `auditLogDetails` have `id` only (no `updatedAt` on the log).

---

## Mapping online SaaS → this file

Keep GUIDs stable so FKs still line up. If the SaaS uses integers, generate new GUIDs and rewrite every FK.

| Online concept | Desktop JSON |
|---|---|
| Company / profile | `preferences` (+ optional `appUser.businessName`). Skip `appUser` unless you intend to overwrite PIN/license. |
| Logo | Desktop stores a **local file path** in `appUser.logoUrl`, not a URL or data URI. Copy the file into `%LOCALAPPDATA%\Shabakat\logo\` after restore, or leave logo empty and set it in Settings. |
| Areas / boxes / ampere schedules | `areas`, `distributionBoxes`, `ampereSchedules` |
| Subscribers | `customers`. `planValue` is amps or kWh depending on `plan`. |
| Custom price | `priceOverride`, `fixedChargeOverride`, `tvaOverride` (all null = no override) |
| Meter history | `meterReadings`. First reading: `isInitial: true`. |
| Invoices | `invoices`. This app has **no** consumption-start/end columns — map those to `issueDate` / `dueDate`. |
| Payments | `payments`. Then set invoice `paidAmount` and `invoiceStatus` (`Unpaid` / `PartiallyPaid` / `Paid`). |
| Expenses | `expenses` |
| Skipped bulk invoices | `invoiceSkips` (`reason` is a stored English or Arabic sentence; see `InvoiceSkipMessages`) |
| Audit trail | `auditLogs` + `auditLogDetails`, or `[]` |

`logoUrl` and `passwordHash` / `licenseStamp` are desktop-only. Do not copy SaaS passwords into `passwordHash`.

---

## Restore

**Settings → Backup → Restore**, or from the license gate on a fresh MSI install. Pick the JSON. Confirm replace-all.

If `appUser` is present, that row is replaced (PIN, licensed-until, stamp, business name, logo path). If it is omitted, the existing desktop user is kept.
