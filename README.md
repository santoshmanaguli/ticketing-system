# Sparkle ERP Ticketing

Full-stack support ticketing system for a Jewellery ERP support team.

## What Is Inside

```txt
sparkleerp-ticketing/
  backend/SparkleTicketing.Api/   ASP.NET Core Web API
  frontend/                       React + Vite + TypeScript app
  database/InitialCreate.sql       SQL Server script generated from EF migration
  NuGet.config                    NuGet package source for .NET packages
  dotnet-tools.json               Local dotnet-ef migration tool
```

## Beginner Backend Map

If you know frontend, map it like this:

```txt
React component        -> ASP.NET Controller
TypeScript type        -> C# DTO
Frontend form state    -> API request body
Backend model class    -> SQL Server table
DbContext              -> database connection/session
Migration              -> database table creation script
```

Example flow:

```txt
New Ticket form
  POST /api/tickets
    TicketsController.CreateTicket()
      SparkleTicketingDbContext.Tickets.Add(...)
        SQL Server inserts row into Tickets table
```

## Backend Features

- Customers and branches
- Jewellery ERP modules such as POS Billing, Inventory, Gold Rate, Diamond Stock, Repair, Karigar, and Reports
- Tickets with priority, status, assignment, customer, branch, and module
- Ticket comments
- Ticket status history
- Dashboard summary
- Seed data for first run

## SQL Server Setup

The current default connection string uses SQL Server Express because this machine has `SQLEXPRESS` running:

```json
"Server=localhost\\SQLEXPRESS;Database=SparkleTicketingDb;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False;MultipleActiveResultSets=true"
```

If you use LocalDB or full SQL Server, update `backend/SparkleTicketing.Api/appsettings.Development.json`.

Examples:

```json
"Server=(localdb)\\MSSQLLocalDB;Database=SparkleTicketingDb;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False;MultipleActiveResultSets=true"
```

```json
"Server=localhost;Database=SparkleTicketingDb;User Id=sa;Password=YourPassword;TrustServerCertificate=True;Encrypt=False;MultipleActiveResultSets=true"
```

## Run Backend

```powershell
dotnet restore
dotnet build backend\SparkleTicketing.Api\SparkleTicketing.Api.csproj
dotnet run --project backend\SparkleTicketing.Api\SparkleTicketing.Api.csproj --launch-profile http
```

The API runs at:

```txt
http://localhost:5001
```

OpenAPI JSON:

```txt
http://localhost:5001/openapi/v1.json
```

On startup, the API runs migrations and seeds starter data when `Database:RunMigrationsOnStartup` is `true`.

The generated SQL table script is available at `database/InitialCreate.sql` if you want to inspect what EF Core sends to SQL Server.

## Run Frontend

Install Node.js LTS first if `npm` is not available on your machine.

```powershell
cd frontend
npm install
npm run dev
```

The React app runs at:

```txt
http://localhost:5173
```

On this machine, npm was not globally available, so a workspace-local npm copy was bootstrapped under `.tools/`. The frontend can also be run with the bundled Codex Node runtime:

```powershell
cd frontend
& "$env:USERPROFILE\.cache\codex-runtimes\codex-primary-runtime\dependencies\node\bin\node.exe" ..\.tools\npm\package\bin\npm-cli.js install
& "$env:USERPROFILE\.cache\codex-runtimes\codex-primary-runtime\dependencies\node\bin\node.exe" node_modules\vite\bin\vite.js --host 127.0.0.1 --port 5173
```

## Migration Commands

EF Core migration files are already created. When you change models later:

```powershell
dotnet tool restore
dotnet tool run dotnet-ef migrations add YourMigrationName --project backend\SparkleTicketing.Api\SparkleTicketing.Api.csproj --startup-project backend\SparkleTicketing.Api\SparkleTicketing.Api.csproj --output-dir Migrations
```

To manually apply migrations:

```powershell
dotnet tool run dotnet-ef database update --project backend\SparkleTicketing.Api\SparkleTicketing.Api.csproj --startup-project backend\SparkleTicketing.Api\SparkleTicketing.Api.csproj
```

## Auth

Seed users (password for all: `Sparkle@123`):

```txt
aarav@sparkleerp.local   SupportManager
nisha@sparkleerp.local   SupportAgent
rohan@sparkleerp.local   Developer
priya@sparkleerp.local   Admin
```

```txt
POST   /api/auth/register
POST   /api/auth/login
GET    /api/auth/me          (requires Bearer token)
```

New registrations are created as `SupportAgent`. Set `Jwt:Secret` in configuration for production (32+ characters).

## API Endpoints

```txt
GET    /api/dashboard/summary
GET    /api/lookups/customers
GET    /api/lookups/erp-modules
GET    /api/lookups/users
GET    /api/lookups/ticket-statuses
GET    /api/lookups/ticket-priorities
GET    /api/tickets
GET    /api/tickets/{id}
POST   /api/tickets
PATCH  /api/tickets/{id}/status
POST   /api/tickets/{id}/comments
```

## Next Features

- Login with JWT
- Role-based permissions
- File attachments for screenshots
- SLA due dates and breach indicators
- Customer-facing portal
- Email/WhatsApp notifications
- More alignment with the existing Sparkle ERP app
