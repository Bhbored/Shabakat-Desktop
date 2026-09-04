# Billing-run invoices (bulk create + month PDF export)

Desktop Shabakat implements two related invoice workflows that a SaaS backend must match if operators expect the same monthly “billing run” behavior:

1. **Bulk create** — generate Ampere + Kilowatt invoices for eligible active customers (Fixed Kilowatt is never bulk-created).
2. **Billing-run PDF export** — one multi-page PDF for a picked calendar month, using the **same HTML print templates** as single-invoice print, language from **Settings / preferences**.

This doc maps **business rules**, **desktop file pointers**, and **what to build on the SaaS API**. Lebanon demo seed under `Infrastructure/Persistence/Seed/` is unrelated; ignore it for product logic.

---

## Mental model: what “September billing run” means

When an operator runs bulk create on **1 September** (day 1–2):

| Plan | Consumption / `IssueDate` | Notes |
|---|---|---|
| **Ampere** | Start = today (1 Sep) → end = 30 Sep. Stored `IssueDate` = consumption start (**1 Sep**). | Mid-month Ampere catch-up still uses “today” as start. |
| **Kilowatt** | On day **1 or 2** only, billing month is **previous** calendar month: **1–31 Aug**. `IssueDate` = **1 Aug**. | Needs a meter reading in that period (plus a prior/initial reading). |
| **FixedKilowatt** | Not part of bulk create or billing-run PDF export. | Manual / separate flow only. |

When they later **export September**:

| Include | Rule |
|---|---|
| Ampere | `IssueDate` in **September** |
| Kilowatt | `IssueDate` in **August** (previous month) |
| FixedKilowatt | **Never** |

So August export ≠ September export (they no longer share “any invoice in either month”).

```text
Export(year, month=9)
  Ampere  where IssueDate in [2026-09-01 .. 2026-09-30]
  Kilowatt where IssueDate in [2026-08-01 .. 2026-08-31]
```

---

## Domain facts SaaS must keep

| Concept | Desktop meaning | SaaS tip |
|---|---|---|
| `Invoice.IssueDate` | Consumption period **start** (also used as “consumption start” on the print model). | Do not invent a separate “billing month” column unless you also keep `IssueDate` semantics identical. |
| `Invoice.DueDate` | On create via bulk/standard path: set to consumption **end** (see `PersistStandardInvoiceAsync`). Print “due” label uses preference due-day via `ResolvePaymentDueDate`. | Keep create vs print due-date rules in sync with desktop if prints must match. |
| `Customer.Plan` | `Ampere` \| `Kilowatt` \| `FixedKilowatt` | Enum string PascalCase in backups; same in APIs is fine. |
| Preferences `Language` | `en` / `ar` — chooses `invoice.html` vs `invoice.ar.html`. | Tenant or user setting; pass into template render. |
| Preferences pricing | Unit price, fixed charge, TVA, type rates, ampere schedule / prorate flags. | Same inputs to calculation helpers. |
| Meter readings | Kilowatt bulk/create fails without a reading in the resolved period; consumption = current − previous. | Same query windows as desktop. |

---

## 1) Bulk create

### API shape (desktop)

```csharp
Task<BulkCreateInvoiceResponse> BulkCreateAsync(PlanType? planType = null);
// planType null = Ampere + Kilowatt; FixedKilowatt → DomainException Error.BulkPlanNotSupported
// Response: Created, Skipped, Message
```

### Algorithm (port this)

1. Load preferences; `today = DateOnly.FromDateTime(DateTime.Now)` (SaaS: use tenant timezone or explicit “as-of” date).
2. Bounds:
   - Ampere: `GetBillingMonthBounds(Ampere, today)`
   - Kilowatt: `GetBillingMonthBounds(Kilowatt, today)` → on day 1–2 this is **previous** month.
3. Load active customers who still need an invoice for those windows (`GetActiveWithoutInvoiceAsync`).
4. Optional filter by `planType`.
5. For each customer: prepare + persist invoice (or skip + record skip reason).
6. **Extra Ampere pass** if `today.Day >= 25` and filter is not Kilowatt-only: create Ampere invoices for **next** calendar month when eligible (`GetActiveAmpereReadyForNextMonthAsync`).
7. Save; audit `InvoiceBulkCreated`.

### Period helpers (copy or reimplement identically)

| Method | File |
|---|---|
| Day 1–2 → Kilowatt previous month | [`Application/Helper/BillingPeriodHelper.cs`](../Application/Helper/BillingPeriodHelper.cs) |
| Ampere start = `today`, end = month end; Kilowatt start = month start | `ResolveConsumptionPeriod` in same file |
| Meter reading window end | `ResolveKilowattMeterReadingPeriodEnd` |
| Preference due-day for **print** | `ResolvePaymentDueDate` |

### Totals

| Helper | File |
|---|---|
| Ampere / Kilowatt / FixedKilowatt math | [`Application/Helper/InvoiceCalculationHelper.cs`](../Application/Helper/InvoiceCalculationHelper.cs) |
| Resolve unit price + fixed + TVA (overrides, customer type, schedule) | [`Application/Services/Pricing/PricingService.cs`](../Application/Services/Pricing/PricingService.cs) |

### Desktop pointers — bulk create

| Layer | Path |
|---|---|
| Contract | [`Application/Contracts/Services/IInvoiceService.cs`](../Application/Contracts/Services/IInvoiceService.cs) → `BulkCreateAsync` |
| Implementation | [`Application/Services/Invoice/InvoiceService.cs`](../Application/Services/Invoice/InvoiceService.cs) → `BulkCreateAsync`, `TryCreateBulkInvoiceAsync`, prepare/persist helpers |
| Skip messages / language | [`Application/Helper/InvoiceSkipMessages.cs`](../Application/Helper/InvoiceSkipMessages.cs) |
| Customer eligibility queries | Customer repository (methods used from `BulkCreateAsync`: `GetActiveWithoutInvoiceAsync`, `GetActiveAmpereReadyForNextMonthAsync`) |
| DTO | [`Application/DTOs/Invoices/InvoiceDtos.cs`](../Application/DTOs/Invoices/InvoiceDtos.cs) → `BulkCreateInvoiceResponse` |
| UI | [`Components/Features/Invoices/Components/BulkCreateInvoiceSheet.razor`](../Components/Features/Invoices/Components/BulkCreateInvoiceSheet.razor), toolbar in `InvoicesToolbar.razor`, wired in `Invoices.razor` |
| Audit | [`Application/Helper/AuditLogEntries.cs`](../Application/Helper/AuditLogEntries.cs) → `InvoiceBulkCreated` |

### SaaS endpoint sketch

```http
POST /api/invoices/bulk-create
Content-Type: application/json

{ "planType": null }   // or "Ampere" | "Kilowatt"
```

```json
{ "created": 42, "skipped": 3, "message": "..." }
```

Idempotency: desktop skips customers who already have an invoice whose `IssueDate` falls in that plan’s billing window (`ExistsForCustomerInPeriodAsync`). Replicate that, or return conflict counts the same way.

---

## 2) Billing-run month PDF export

### API shape (desktop)

```csharp
IAsyncEnumerable<double> ExportBillingRunPdfAsync(
    int year, int month, string destinationPath,
    CancellationToken cancellationToken = default);
// yields 0..1 progress; writes .pdf to destinationPath
```

SaaS should typically return a **file download** (or signed URL) instead of a local path, and may stream progress over SignalR / SSE if the UI needs a progress bar.

### Algorithm (port this)

1. Validate `month` ∈ 1..12.
2. `selectedStart/End` = first/last day of `(year, month)`.
3. `previousStart/End` = previous calendar month.
4. Query invoices (**plan-aware** — critical):

```csharp
// Infrastructure/Repository/InvoiceRepository.cs → GetForIssueDateRangesAsync
(Ampere  && IssueDate in selected month)
|| (Kilowatt && IssueDate in previous month)
// FixedKilowatt excluded
// Order: IssueDate, InvoiceNumber
```

5. If empty → error `Error.InvoiceExportEmpty`.
6. For each invoice id:
   - Build print model (`BuildPrintModelAsync` — shared with single print).
   - Render HTML: `_templateRenderer.Render(model, preferences.Language)`.
7. Combine HTML documents into portrait A4 pages, sizing each `.invoice` block to one-third of the page and inserting a page break after every third invoice.
8. Convert combined HTML → one PDF (desktop uses Edge headless `--print-to-pdf`; SaaS should use Chromium/Playwright/Puppeteer or an HTML→PDF service — **same templates**, not a second layout).

### Print model + templates (must stay shared with single print)

| Piece | Path |
|---|---|
| Print DTO | [`Application/DTOs/Invoices/InvoicePrintModel.cs`](../Application/DTOs/Invoices/InvoicePrintModel.cs) |
| Build model (logo, readings, TVA rows, dates) | `InvoiceService.BuildPrintModelAsync` in [`InvoiceService.cs`](../Application/Services/Invoice/InvoiceService.cs) |
| Single-invoice HTML API | `RenderPrintHtmlAsync` (same renderer) |
| Renderer contract | [`Application/Contracts/Abstractions/IInvoiceTemplateRenderer.cs`](../Application/Contracts/Abstractions/IInvoiceTemplateRenderer.cs) |
| Renderer | [`Application/Services/Invoice/InvoiceTemplateRenderer.cs`](../Application/Services/Invoice/InvoiceTemplateRenderer.cs) |
| EN template | [`Infrastructure/Templates/invoice.html`](../Infrastructure/Templates/invoice.html) |
| AR template | [`Infrastructure/Templates/invoice.ar.html`](../Infrastructure/Templates/invoice.ar.html) |
| Arabic fonts (embedded at render) | `Templates/Fonts/NotoSansArabic-*.ttf` (loaded by renderer) |
| Combine HTML + PDF write | [`Application/Services/Invoice/InvoicePdfBuilder.cs`](../Application/Services/Invoice/InvoicePdfBuilder.cs) |

**Do not** reintroduce a separate QuestPDF/layout clone. Desktop already switched export to these HTML templates so Ampere/Kilowatt/Fixed layouts stay identical to Print.

### Query contract

| Piece | Path |
|---|---|
| Repository interface | [`Application/Contracts/Repository/IInvoiceRepository.cs`](../Application/Contracts/Repository/IInvoiceRepository.cs) → `GetForIssueDateRangesAsync` |
| EF implementation | [`Infrastructure/Repository/InvoiceRepository.cs`](../Infrastructure/Repository/InvoiceRepository.cs) |

### Desktop UI pointers

| Piece | Path |
|---|---|
| Month sheet + progress | [`Components/Features/Invoices/Components/ExportBillingRunSheet.razor`](../Components/Features/Invoices/Components/ExportBillingRunSheet.razor) |
| Toolbar button | [`Components/Features/Invoices/Components/InvoicesToolbar.razor`](../Components/Features/Invoices/Components/InvoicesToolbar.razor) |
| Orchestration (picker, `await foreach` progress, toast) | [`Components/Features/Invoices/Invoices.razor`](../Components/Features/Invoices/Invoices.razor) → `ExportBillingRunAsync` |
| Save dialog | [`Application/Helper/InvoicePdfFilePicker.cs`](../Application/Helper/InvoicePdfFilePicker.cs) |
| Month labels | [`Application/Helper/FormatHelper.cs`](../Application/Helper/FormatHelper.cs) → `MonthYear` |

### Localization keys (en/ar)

| Key | Use |
|---|---|
| `Invoices.ExportMonth` | Button |
| `Invoices.ExportMonthTitle` / `Hint` / `Label` / `Rule` | Sheet |
| `Invoices.Exporting` / `ExportMonthDone` | Progress / success |
| `Error.InvoiceExportEmpty` | No matching invoices |
| `Error.InvoiceExportPdfFailed` | PDF engine failure (desktop: Edge missing) |
| `Error.PrintTemplateNotFound` | Template file missing |

Resources: [`Resources/Localization/SharedResource.resx`](../Resources/Localization/SharedResource.resx) and `.ar.resx`.

### SaaS endpoint sketch

```http
POST /api/invoices/billing-run-export
Content-Type: application/json

{ "year": 2026, "month": 9 }
```

Response options:

- `202` + job id, then `GET /api/jobs/{id}` / download URL when ready  
- or synchronous `application/pdf` for small tenants  

Always render with tenant preference language (`en` / `ar`), same token model as desktop.

Suggested download name (desktop): `shabakat-invoices-YYYY-MM.pdf`.

---

## Shared create/print pipeline SaaS should reuse

Bulk create and single create both funnel through prepare → persist. Print/export both funnel through `BuildPrintModelAsync` → template render.

```text
BulkCreate / Create
  → BillingPeriodHelper + PricingService + InvoiceCalculationHelper
  → Invoice row (IssueDate = consumption start)

Print one / Export billing run
  → BuildPrintModelAsync (readings, costs, formatted dates)
  → InvoiceTemplateRenderer (invoice.html | invoice.ar.html)
  → (export only) combine pages → PDF
```

Porting tip: implement **one** “print HTML for invoice id” service on SaaS, then:

- UI “Print” → open/print that HTML  
- “Export month” → N × that HTML → one PDF  

---

## Acceptance checks for SaaS parity

- [ ] Bulk on day 1–2: Ampere `IssueDate` in current month; Kilowatt `IssueDate` in previous month.
- [ ] Bulk rejects FixedKilowatt plan filter.
- [ ] Day ≥ 25: optional next-month Ampere bulk still works when product wants desktop parity.
- [ ] Export month M: only Ampere in M + Kilowatt in M−1; FixedKilowatt never included.
- [ ] Export(August) and Export(September) are **not** the same set when both months have data.
- [ ] PDF visual matches single-invoice print (same HTML templates + language).
- [ ] Empty selection returns a clear, localizable error (not an empty PDF).

---

## Out of scope on desktop (do not assume)

- DOCX export  
- Cloud upload of billing-run PDFs  
- Changing bulk-create rules via the export UI  
- Restoring invoices from PDF  

Cloud JSON backup of invoice rows is separate: see [`docs/backup.md`](backup.md).
