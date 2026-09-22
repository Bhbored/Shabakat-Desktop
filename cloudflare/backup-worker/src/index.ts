export interface Env {
  BACKUP_BUCKET: R2Bucket;
  BACKUP_APP_SECRET: string;
}

const KEEP_COUNT = 14;
const GUID =
  /^[0-9a-f]{8}-[0-9a-f]{4}-[1-8][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;

export default {
  async fetch(request: Request, env: Env): Promise<Response> {
    const url = new URL(request.url);
    if (request.method !== "POST" || url.pathname !== "/v1/backups") {
      return json(404, { error: "not_found" });
    }

    const token = bearer(request.headers.get("Authorization"));
    if (!env.BACKUP_APP_SECRET || token !== env.BACKUP_APP_SECRET) {
      return json(401, { error: "unauthorized" });
    }

    const userId = (request.headers.get("X-User-Id") ?? "").trim().toLowerCase();
    if (userId && !GUID.test(userId)) {
      return json(400, { error: "invalid_user_id" });
    }

    const installId = (request.headers.get("X-Install-Id") ?? "").trim().toLowerCase();
    if (installId && !GUID.test(installId)) {
      return json(400, { error: "invalid_install_id" });
    }
    if (!userId && !installId) {
      return json(400, { error: "missing_backup_owner" });
    }

    let businessName = "";
    const encodedBusinessName = request.headers.get("X-Business-Name");
    if (encodedBusinessName) {
      try {
        businessName = decodeURIComponent(encodedBusinessName);
      } catch {
        return json(400, { error: "invalid_business_name" });
      }
    }

    const body = await request.arrayBuffer();
    if (body.byteLength === 0) {
      return json(400, { error: "empty_body" });
    }

    const folder = userId ? backupFolder(userId, businessName) : installId;
    const prefix = `${folder}/`;

    if (userId)
      await migratePreviousFolders(env.BACKUP_BUCKET, userId, installId, prefix);

    const key = `${prefix}backup-${formatUtc(new Date())}.json`;
    await env.BACKUP_BUCKET.put(key, body, {
      httpMetadata: { contentType: "application/json; charset=utf-8" },
    });

    await prune(env.BACKUP_BUCKET, prefix, KEEP_COUNT);

    return new Response(null, {
      status: 204,
      headers: {
        "X-Backup-Folder": folder,
        "X-Object-Key": key,
      },
    });
  },
};

function bearer(header: string | null): string {
  if (!header || !header.startsWith("Bearer "))
    return "";
  return header.slice("Bearer ".length).trim();
}

function formatUtc(date: Date): string {
  const pad = (n: number, width = 2) => n.toString().padStart(width, "0");
  return `${date.getUTCFullYear()}${pad(date.getUTCMonth() + 1)}${pad(date.getUTCDate())}T${pad(date.getUTCHours())}${pad(date.getUTCMinutes())}${pad(date.getUTCSeconds())}${pad(date.getUTCMilliseconds(), 3)}Z`;
}

function backupFolder(userId: string, businessName: string): string {
  const normalized = businessName
    .normalize("NFKC")
    .trim()
    .replace(/[^\p{L}\p{N}._ -]+/gu, "-")
    .replace(/\s+/g, "-")
    .replace(/-+/g, "-")
    .replace(/^[._-]+|[._-]+$/g, "");
  const segment = Array.from(normalized).slice(0, 80).join("");
  return segment ? `${userId}-${segment}` : userId;
}

async function migratePreviousFolders(
  bucket: R2Bucket,
  userId: string,
  installId: string,
  targetPrefix: string,
): Promise<void> {
  const sourcePrefixes = new Set<string>();
  const userKeys = await listKeys(bucket, userId);

  for (const key of userKeys) {
    const slash = key.indexOf("/");
    if (slash < 0)
      continue;

    const prefix = key.slice(0, slash + 1);
    const belongsToUser = prefix === `${userId}/` || prefix.startsWith(`${userId}-`);
    if (belongsToUser && prefix !== targetPrefix)
      sourcePrefixes.add(prefix);
  }

  if (installId && installId !== userId)
    sourcePrefixes.add(`${installId}/`);

  for (const sourcePrefix of sourcePrefixes)
    await movePrefix(bucket, sourcePrefix, targetPrefix);
}

async function movePrefix(
  bucket: R2Bucket,
  sourcePrefix: string,
  targetPrefix: string,
): Promise<void> {
  if (sourcePrefix === targetPrefix)
    return;

  const sourceKeys = await listKeys(bucket, sourcePrefix);
  for (const sourceKey of sourceKeys) {
    const object = await bucket.get(sourceKey);
    if (object === null)
      continue;

    const baseDestinationKey = targetPrefix + sourceKey.slice(sourcePrefix.length);
    const destination = await resolveDestinationKey(bucket, baseDestinationKey, sourceKey);

    if (!destination.alreadyCopied) {
      await bucket.put(destination.key, object.body, {
        httpMetadata: object.httpMetadata,
        customMetadata: {
          ...(object.customMetadata ?? {}),
          migrationSource: sourceKey,
        },
      });
    }

    await bucket.delete(sourceKey);
  }
}

async function resolveDestinationKey(
  bucket: R2Bucket,
  baseKey: string,
  sourceKey: string,
): Promise<{ key: string; alreadyCopied: boolean }> {
  for (let attempt = 0; attempt <= 100; attempt++) {
    const key = attempt === 0 ? baseKey : withMigrationSuffix(baseKey, attempt);
    const existing = await bucket.head(key);
    if (existing === null)
      return { key, alreadyCopied: false };
    if (existing.customMetadata?.migrationSource === sourceKey)
      return { key, alreadyCopied: true };
  }

  throw new Error(`Unable to move backup object ${sourceKey}`);
}

function withMigrationSuffix(key: string, attempt: number): string {
  const extensionIndex = key.lastIndexOf(".");
  if (extensionIndex < 0)
    return `${key}-migrated-${attempt}`;
  return `${key.slice(0, extensionIndex)}-migrated-${attempt}${key.slice(extensionIndex)}`;
}

async function listKeys(bucket: R2Bucket, prefix: string): Promise<string[]> {
  const keys: string[] = [];
  let cursor: string | undefined;

  do {
    const page = await bucket.list({ prefix, cursor });
    for (const object of page.objects)
      keys.push(object.key);
    cursor = page.truncated ? page.cursor : undefined;
  } while (cursor);

  return keys;
}

async function prune(bucket: R2Bucket, prefix: string, keep: number): Promise<void> {
  const keys = await listKeys(bucket, prefix);

  keys.sort((a, b) => (a < b ? 1 : a > b ? -1 : 0));
  const stale = keys.slice(keep);
  await Promise.all(stale.map((key) => bucket.delete(key)));
}

function json(status: number, body: unknown): Response {
  return new Response(JSON.stringify(body), {
    status,
    headers: { "Content-Type": "application/json" },
  });
}
