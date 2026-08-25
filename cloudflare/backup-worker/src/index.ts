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

    const installId = (request.headers.get("X-Install-Id") ?? "").trim().toLowerCase();
    if (!GUID.test(installId)) {
      return json(400, { error: "invalid_install_id" });
    }

    const body = await request.arrayBuffer();
    if (body.byteLength === 0) {
      return json(400, { error: "empty_body" });
    }

    const key = `${installId}/${formatUtc(new Date())}.json`;
    await env.BACKUP_BUCKET.put(key, body, {
      httpMetadata: { contentType: "application/json; charset=utf-8" },
    });

    await prune(env.BACKUP_BUCKET, `${installId}/`, KEEP_COUNT);

    return new Response(null, {
      status: 204,
      headers: { "X-Object-Key": key },
    });
  },
};

function bearer(header: string | null): string {
  if (!header || !header.startsWith("Bearer "))
    return "";
  return header.slice("Bearer ".length).trim();
}

function formatUtc(date: Date): string {
  const pad = (n: number) => n.toString().padStart(2, "0");
  return `${date.getUTCFullYear()}${pad(date.getUTCMonth() + 1)}${pad(date.getUTCDate())}T${pad(date.getUTCHours())}${pad(date.getUTCMinutes())}${pad(date.getUTCSeconds())}Z`;
}

async function prune(bucket: R2Bucket, prefix: string, keep: number): Promise<void> {
  const keys: string[] = [];
  let cursor: string | undefined;

  do {
    const page = await bucket.list({ prefix, cursor });
    for (const object of page.objects)
      keys.push(object.key);
    cursor = page.truncated ? page.cursor : undefined;
  } while (cursor);

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
