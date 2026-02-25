# Prior Authorization Management System

An enterprise healthcare application for managing prior authorization requests between providers and payers. Built to the FHIR R4 specification with real-time status updates via SignalR, Clean Architecture principles, and PostgreSQL persistence. The system tracks authorization requests through a full lifecycle—from draft submission through clinical review to final determination—giving providers, reviewers, and administrators role-appropriate views into the process.

## Architecture

The solution follows four-layer Clean Architecture: **Domain** (entities, value objects, domain rules), **Application** (use cases, CQRS handlers, DTOs), **Infrastructure** (EF Core persistence, repository implementations, external services), and **API** (controllers, middleware, SignalR hubs). FHIR R4 endpoints expose healthcare-interoperable resources. SignalR pushes real-time status updates to connected clients. A demo authentication middleware provides role-based access without requiring a full identity provider.

## Local Setup

**Prerequisites:** .NET 10 SDK, Node 18+, PostgreSQL 15+

1. **Database:** Create a PostgreSQL database named `priorauth` on `localhost:5432` (user: `postgres`, password: `postgres`). Run migrations from the Infrastructure project:
   ```
   dotnet ef database update --project src/PriorAuthSystem.Infrastructure --startup-project src/PriorAuthSystem.API
   ```

2. **API:** Start the backend from the API project:
   ```
   dotnet run --project src/PriorAuthSystem.API
   ```
   The API will be available at http://localhost:5278.

3. **Frontend:** Start the React/Vite dev server:
   ```
   cd frontend
   cp .env.example .env
   npm install
   npm run dev
   ```
   The frontend will be available at http://localhost:5173.

## Demo Credentials

Authentication uses a demo middleware that reads the `X-Demo-Role` HTTP header to assign a role. No login credentials are required. Set the header to one of three values: `Admin` (full access), `Reviewer` (review and action pending requests), or `Provider` (submit and track own requests). The frontend login page provides one-click role selection that sets this header automatically.

## Seed Data

The database initializer seeds 6 patients, 4 providers, 2 payers, and 8 prior authorization requests covering all 7 statuses in the lifecycle (Draft, Submitted, UnderReview, PendingInfo, Approved, Denied, Cancelled).

## FHIR Endpoints

FHIR R4-compliant resources are available at the `/fhir/r4/` prefix, returning standard Patient, Practitioner, Coverage, and Claim resources.

## Key Config Quirk

The application sets `AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true)` at startup. This is required for correct timestamp handling between .NET 10 and the Npgsql PostgreSQL driver.
