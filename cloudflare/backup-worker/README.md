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
| `X-Install-Id` | GUID (one per desktop install) |
| `Content-Type` | `application/json` |

Body: UTF-8 JSON (`BackupFile` version 1).

Success: `204` with `X-Object-Key: {installId}/{yyyyMMddTHHmmssZ}.json`.

Each install keeps the newest **14** objects; older keys under that prefix are deleted.

The Worker does not serve downloads. Get files from the R2 dashboard or `wrangler r2 object get`.
