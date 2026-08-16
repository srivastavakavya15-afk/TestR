# TestR — User Directory

A small user directory: a React SPA with **Add** and **List** pages over a .NET 8 Web API backed by
SQLite.

- `backend/` — ASP.NET Core 8 minimal API, EF Core + SQLite, layered as Api → Application → Domain
  with Infrastructure implementing the ports.
- `frontend/` — React 19 + TypeScript + Vite, Tailwind CSS, TanStack Query, React Router,
  React Hook Form + Zod.

The two are coupled only through the API's OpenAPI document: the frontend's request/response types
are generated from it into `frontend/src/api/generated/` and never hand-written.

## Prerequisites

| Tool | Version used here |
|---|---|
| .NET SDK | 8.0 |
| Node.js | 20+ |
| Docker + Compose | optional, for the containerized path |

`dotnet-ef` is only needed if you add migrations: `dotnet tool install --global dotnet-ef --version 8.0.*`

## Run it

### Option A — Docker Compose (nothing else to install)

```bash
docker compose up --build
```

- SPA: <http://localhost:5173>
- Swagger UI: <http://localhost:5099/swagger>

nginx serves the built SPA and reverse-proxies `/api` to the API container, so the browser stays on
one origin and no CORS policy is involved. The SQLite file lives on the `api-data` volume, so data
survives rebuilds; `docker compose down -v` discards it.

### Option B — two terminals

```bash
# terminal 1 — API on http://localhost:5099
dotnet watch --project backend/src/TestR.Api

# terminal 2 — SPA on http://localhost:5173
cd frontend && npm install && npm run dev
```

The Vite dev server proxies `/api` and `/swagger` to Kestrel, so again there is no CORS setup in dev.

## API

Swagger UI is served at `/swagger` in the Development environment; the raw document is at
`/swagger/v1/swagger.json`.

| Method | Route | Success | Failure |
|---|---|---|---|
| GET | `/api/users` | 200 + `UserDto[]` (newest first) | — |
| GET | `/api/users/{id}` | 200 + `UserDto` | 404 |
| POST | `/api/users` | 201 + `UserDto` + `Location` | 400 validation, 401 |
| PUT | `/api/users/{id}` | 200 + `UserDto` | 400 validation, 404, 401 |
| DELETE | `/api/users/{id}` | 204 | 404, 401 |

### Validation

Enforced twice on purpose: FluentValidation checks the request shape at the edge, and the `User`
entity enforces the same rules as domain invariants so no code path can persist an invalid record.
The bounds are declared once as constants on `User` and referenced by the validators.

| Field | Rule |
|---|---|
| `name` | required, 2–100 chars, trimmed |
| `age` | required integer, 0–120 |
| `city` | required, ≤100 chars, trimmed |
| `state` | required, ≤100 chars, trimmed |
| `pincode` | required, 4–10 chars, trimmed |

### Error shape

Every failure is RFC 9457 `ProblemDetails`. Validation failures add an `errors` dictionary keyed by
camelCase field name, which the Add form maps straight onto its inputs:

```json
{
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": { "pincode": ["Pincode must be between 4 and 10 characters."] }
}
```

| Cause | Status |
|---|---|
| validation failure | 400 + `errors` |
| malformed JSON body | 400 |
| not found | 404 |
| domain rule violation | 422 |
| unauthenticated | 401 |
| unhandled | 500, generic message, full detail logged |

Response bodies never contain a stack trace or SQL text.

## Database

SQLite via EF Core. The path comes from configuration, so it is not baked into the code:

| Where | Connection string |
|---|---|
| `appsettings.json` (default) | `Data Source=/data/app.db` |
| `appsettings.Development.json` | `Data Source=data/app.db` (project-relative, for `dotnet run`) |
| Docker Compose | `ConnectionStrings__Default=Data Source=/data/app.db` on the `api-data` volume |

Override it anywhere with the `ConnectionStrings__Default` environment variable. Migrations are
applied automatically at startup, and the API creates the parent directory if it is missing.

To add a migration (note the `--project` / `--startup-project` split — the migration belongs to
Infrastructure but the DbContext is configured by Api's DI):

```bash
dotnet ef migrations add <Name> \
  --project backend/src/TestR.Infrastructure \
  --startup-project backend/src/TestR.Api \
  --output-dir Persistence/Migrations
```

There is no seed data — the List page's empty state is part of the acceptance criteria.

## Authentication (bonus)

OAuth2/OIDC bearer-token validation, **provider-agnostic**: the API validates JWTs against whatever
OIDC issuer you configure, so Microsoft Entra ID and Auth0 both work without a code change.

**It ships disabled** (`Auth:Enabled=false`) so the app runs with zero identity-provider setup.

### The access decision

| Route | Access | Why |
|---|---|---|
| `GET /api/users`, `GET /api/users/{id}` | **public** | The directory is readable by anyone; keeps the List page working for unauthenticated visitors, which is also what makes the "List is public, Add is protected" split visible. |
| `POST`, `PUT`, `DELETE` | **bearer token required** | Anything that mutates the directory is guarded by the `WriteAccess` policy. |

The SPA mirrors this: `/` renders for everyone, `/add` sits behind a guard that prompts for sign-in.
With auth disabled the guard renders straight through, so there is one code path either way.

This is pinned down by tests in `backend/tests/TestR.Api.Tests/AuthEnabledEndpointsTests.cs` —
reads still return 200/404 without a token, writes return 401, and a garbage token is rejected.

### Turning it on

Backend — any of appsettings, environment variables, or user-secrets:

```jsonc
"Auth": {
  "Enabled": true,
  "Authority": "https://login.microsoftonline.com/<tenant-id>/v2.0",  // or https://<tenant>.auth0.com/
  "Audience": "api://<api-client-id>"                                 // or your Auth0 API identifier
}
```

A missing `Authority` while `Enabled` is true fails at boot rather than at the first request.

Frontend — `frontend/.env.local`:

```bash
VITE_AUTH_ENABLED=true
VITE_OIDC_AUTHORITY=https://login.microsoftonline.com/<tenant-id>/v2.0
VITE_OIDC_CLIENT_ID=<spa-client-id>
VITE_OIDC_SCOPE=openid profile email api://<api-client-id>/access_as_user
# Auth0 only — without it Auth0 issues an opaque token instead of a JWT:
# VITE_OIDC_AUDIENCE=https://testr-api
```

For Compose, copy `.env.example` to `.env` and set `AUTH_ENABLED=true` plus the `OIDC_*` values.
The frontend values are compiled into the bundle, so re-run `docker compose up --build` after a change.

Register `http://localhost:5173/` as a SPA redirect URI (authorization code + PKCE) with your IdP.

## Tests

```bash
# backend — 59 tests
dotnet test TestR.sln

# frontend component tests — 8 tests
cd frontend && npm test

# browser end-to-end — 5 tests, starts its own API and dev server
cd frontend && npm run test:e2e
```

Three layers, each covering what the one below it cannot:

- `TestR.Application.Tests` — domain invariants, the UUIDv7 helper, each handler against a
  hand-written fake repository, and the validators.
- `TestR.Api.Tests` — `WebApplicationFactory` integration tests over all five routes against
  in-memory SQLite, asserting status codes *and* body shape, plus the auth matrix above.
- `frontend` (Vitest + React Testing Library + MSW) — the List page's loading, empty, populated and
  error states, and the Add page's client validation, success path (toast + redirect), server
  field-error mapping, and transport failure. Fast, but the transport is mocked and jsdom is not a
  browser.
- `frontend/e2e` (Playwright + Chromium) — the real chain: browser → Vite → Kestrel → SQLite. It
  proves what a mocked transport cannot: that the bundle boots without console errors, lazy route
  chunks resolve, a deep link to `/add` survives a cold load, and a created user is still there
  after a full page reload.

### Running the e2e suite

`npm run test:e2e` starts everything it needs — no servers to start first. It uses **port 5199 for
the API and 5174 for the dev server**, and its own SQLite file at `frontend/.playwright/e2e.db`
which is deleted at the start of every run. So it neither collides with a dev server on 5099/5173
nor touches your development database. `npm run test:e2e:ui` opens the interactive runner.

First-time setup needs the browser binary:

```bash
npx playwright install --with-deps chromium
```

The suite runs serially against one API instance, so the tests share database state by design —
the empty-state assertion depends on running first.

## Checks

```bash
dotnet build TestR.sln && dotnet test TestR.sln
cd frontend && npm run lint && npm run typecheck && npm test && npm run build

# needs the Chromium binary; starts its own servers
cd frontend && npm run test:e2e
```

## Regenerating the API client

Any change to a DTO or endpoint must be followed by regenerating the TypeScript client in the same
change, or the frontend build breaks for whoever pulls next.

```bash
dotnet run --project backend/src/TestR.Api   # in another terminal
cd frontend && npm run gen:api && npm run typecheck
```

The generated output is committed so CI and fresh checkouts type-check without booting the API.

## Notes and trade-offs

- **Swashbuckle rather than the built-in `AddOpenApi`** — `Microsoft.AspNetCore.OpenApi`'s
  `MapOpenApi` arrived in .NET 9; this project targets .NET 8, where Swashbuckle is the standard
  choice. Two options are set explicitly (`SupportNonNullableReferenceTypes` plus a schema filter
  marking non-nullable properties `required`) because without them every field is emitted as
  optional and nullable, and the generated TypeScript would force null checks on fields the API
  always sends.
- **`CreatedAtUtc` is `DateTime`, not `DateTimeOffset`** — SQLite cannot `ORDER BY` a
  `DateTimeOffset`, and an always-UTC instant has no offset worth storing. A value converter forces
  `DateTimeKind.Utc` on read, so the JSON carries its `Z` and browsers don't reinterpret it as local
  time.
- **UUIDv7 primary keys** — sequential GUIDs index far better than random ones. `Guid.CreateVersion7()`
  is .NET 9+, so `SequentialGuid` implements it; delete that type if the target framework is raised.
- **The API client resolves `globalThis.fetch` per call** rather than letting `openapi-fetch` capture
  it at construction. Binding it once breaks anything that patches `fetch` later — MSW in tests, and
  tracing or polyfill layers in the browser.
- **The API container runs as root** so it can write to the mounted `/data` volume without a
  `chown` step. Fine for local development; a deployed image should run as a non-root user with the
  volume's ownership set to match.
- **Update and delete have no UI.** They are specified as API endpoints only, and are covered by
  integration tests; the SPA's required surface is List and Add.
