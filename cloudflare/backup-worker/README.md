# Shabakat backup Worker

Accepts `BackupFile` JSON from the desktop app and stores it in R2.

## Setup

1. Create an R2 bucket named `shabakat-backups` (or change `bucket_name` in `wrangler.toml`).
2. From this folder (always pass `-c ./wrangler.toml`):

```bash
npx wrangler deploy -c ./wrangler.toml
npx wrangler secret put BACKUP_APP_SECRET -c ./wrangler.toml
```

The Worker URL will look like:

`https://shabakat-backup-worker.<your-subdomain>.workers.dev`

Do **not** use `https://shabakat.<subdomain>.workers.dev` — that is a different Worker from a root static-assets Wrangler config (if present). Always deploy with `-c ./wrangler.toml` from this folder.

3. Copy that Worker URL (must include `https://`) and the same secret into `appsettings.Local.json` at the repo root (see `appsettings.Local.json.example`).
   Example: `"WorkerUrl": "https://shabakat-backup-worker.YOUR_SUBDOMAIN.workers.dev"`

## API

`POST /v1/backups`

| Header | Value |
|---|---|
| `Authorization` | `Bearer` + `BACKUP_APP_SECRET` |
| `X-User-Id` | Restorable `AppUser.Id` GUID used as the stable backup owner |
| `X-Business-Name` | Optional percent-encoded business name |
| `X-Install-Id` | Legacy installation GUID, used only to migrate old folders |
| `Content-Type` | `application/json` |

Body: UTF-8 JSON (`BackupFile` version 1).

Success: `204` with `X-Backup-Folder` and `X-Object-Key` response headers.

Objects are stored as `{userId}-{businessName}/backup-{yyyyMMddTHHmmssfffZ}.json`.
When no business name is set, the folder is simply `{userId}/`. Unsafe folder-name
characters are replaced with hyphens. When the business name changes, existing objects
are moved to the new prefix. Legacy installation-ID folders are migrated on the first upload.
Older app versions without `X-User-Id` continue writing to their installation folder until
the app is updated; the first user-based upload then migrates that folder.

Each user keeps the newest **14** objects; older keys under that prefix are deleted.

The Worker does not serve downloads. Get files from the R2 dashboard or `wrangler r2 object get`.
