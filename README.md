# MultiTenant Workshop

This repo is a tiny, easy-to-follow example of multitenancy in a web API.

## What is multitenancy?
Multitenancy means one application serves multiple customers ("tenants") while keeping
their data isolated from each other. In this demo:
- Each request includes a tenant id in a header (default is `X-Tenant`).
- The API uses that tenant id to filter reads and protect writes.
- Two example tenants are configured: `acme` and `globex`.

## Requirements (one-time setup)
1) Install Git (for cloning the repo)
   - https://git-scm.com/downloads
2) Install the .NET SDK (this project targets net10.0)
   - https://dotnet.microsoft.com/download

Note: If you already have the .NET SDK installed, you can skip step 2.

## Get the code (using VS Code)
1) Install VS Code if you do not have it:
   - https://code.visualstudio.com/download
2) Open VS Code.
3) Open the built-in terminal:
   - Windows/Linux: `Ctrl` + `` ` ``
   - macOS: `Cmd` + `` ` ``
4) In the terminal, run:

```bash
git clone <REPO_URL_HERE>
cd MultiTenant
```

5) Open the repo folder in VS Code:
   - File > Open Folder... > select the `MultiTenant` folder

## Run the API (from VS Code)
In the VS Code terminal, from the repo root, run:

```bash
dotnet run --project Mt.Api
```

When it starts, you should see a message that it is listening on:
- `http://localhost:5276`
- `https://localhost:7166`

Leave this terminal window running.

## Open Swagger (API Explorer)
In your browser, open:

```
http://localhost:5276/swagger
```

You should see the Swagger UI with the API endpoints listed.

## Test with Swagger (step by step)
1) Expand `GET /todos`.
2) Click the **Try it out** button.
3) Find the `X-Tenant` header field and enter:
   - `acme` (or `globex`)
4) Click **Execute**.

You should get a `200` response (or `404` if there are no items yet).

### Create a todo
1) Expand `POST /todos`.
2) Click **Try it out**.
3) In `X-Tenant`, enter `acme`.
4) In the body, use:
   ```json
   { "title": "First task" }
   ```
5) Click **Execute**.

Now run `GET /todos` again with the same tenant to see it.

### Prove tenant isolation
1) Create a todo with `X-Tenant: acme`.
2) Call `GET /todos` with `X-Tenant: globex`.
3) You should NOT see the `acme` data.

## How tenant isolation is enforced (high level)
- A middleware reads `X-Tenant` and sets the `TenantContext`.
- Entity Framework adds a global filter so all reads use the current tenant.
- Writes are blocked if the tenant id does not match.

## Troubleshooting
- **"Unknown Tenant"**: the header value must match a configured tenant (`acme` or `globex`).
- **Swagger loads but calls fail**: make sure you set the `X-Tenant` header.
- **App does not start**: confirm the .NET SDK is installed and on your PATH.

## Stop the API
In the terminal where it is running, press `Ctrl+C`.
