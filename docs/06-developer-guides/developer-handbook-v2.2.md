# Developer Handbook v2.2 (Firely & Multi‑FHIR Ready)

This handbook provides practical guidance for developers working on Synapxe FHIR Studio, updated to account for:

- Firely .NET FHIR Validator as the primary validation engine.
- Project‑level FHIR version handling (R4 now, R5 later).
- Microservice‑oriented backend and modular frontend.

---

## 1. Tech Stack Overview

### Backend

- .NET 8, ASP.NET Core.
- RESTful microservices.
- PostgreSQL for persistence.
- Firely .NET FHIR packages for validation.
- Optional Redis for caching.

### Frontend

- React + TypeScript.
- Vite build tooling.
- Zustand for state.
- TailwindCSS (and optionally Ant Design) for UI.
- React Query (or similar) for data fetching.

---

## 2. Local Development Setup

### 2.1 Prerequisites

- .NET SDK 8.x
- Node.js 20+
- Docker Desktop
- Postgres (via Docker)
- Git

### 2.2 Recommended Docker Compose (Conceptual)

```yaml
services:
  postgres:
    image: postgres:16
    environment:
      POSTGRES_USER: fhir
      POSTGRES_PASSWORD: localdev
      POSTGRES_DB: fhir_studio
    ports:
      - "5432:5432"
  # add redis / blob emulator later if needed
```

Configure connection strings via `appsettings.Development.json` or `.env`.

---

## 3. Firely FHIR Validator Usage

### 3.1 Packages

Add these NuGet packages to `validation-service`:

- `Hl7.Fhir.R4`
- `Hl7.Fhir.Specification`
- `Hl7.Fhir.Validation`
- `Hl7.FhirPath`

R5 packages will be added later for multi‑version support.

### 3.2 Loading Packages

Typical pattern:

```csharp
var coreSource = ZipSource.CreateValidationSource();               // Core R4
var igSource   = new DirectorySource("fhir-packages/ig-r4");       // Synapxe IG
var projectSrc = new DirectorySource($"fhir-packages/projects/{projectId}");

// Compose resolvers
var resolver = new MultiResolver(coreSource, igSource, projectSrc);
var settings = ValidationSettings.CreateDefault();
var validator = new Validator(resolver, settings);
```

In production, you should **cache** validators per version to avoid repeated initialization:

- `Dictionary<FhirVersionCode, IValidator>` managed by `IFhirValidationContextFactory`.

---

## 4. Project Model & FHIR Version

All backend services that operate on a Project must honour:

```csharp
public enum FhirVersionCode { R4 = 1, R5 = 2 }

public class Project
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public FhirVersionCode FhirVersion { get; set; }
    // ...
}
```

- For now, only **R4** is active.
- When R5 is added:
  - Keep existing R4 projects untouched.
  - Only new / explicitly migrated projects use R5.

Frontend should always show the project’s FHIR version in the workspace header.

---

## 5. Service Guidelines

### 5.1 General Backend Patterns

- Follow Clean Architecture principles where reasonable:
  - API layer (controllers / endpoints).
  - Application layer (handlers/services).
  - Domain layer (core models).
  - Infrastructure (DB, external).

- Use dependency injection for:
  - Repositories.
  - Validators.
  - Logging.

- Use DTOs for external API contracts; do not expose EF entities.

### 5.2 Error Handling

All services should return:

```json
{
  "success": false,
  "error": {
    "code": "VALIDATION_FAILED",
    "message": "One or more validation errors occurred.",
    "details": ["..."],
    "target": "ValidationService",
    "traceId": "..."
  },
  "correlationId": "..."
}
```

`traceId` is internal; `correlationId` is user‑visible.

### 5.3 Logging

- Use structured logging (e.g. Serilog).
- Always log with:
  - `correlationId`
  - project id
  - user id (if applicable)
- Avoid logging PHI / sensitive data.

---

## 6. Frontend Development

### 6.1 Module Layout

- `transformation-studio` – main 6‑step wizard.
- `shared` – components (tables, trees, panels).
- `auth` – OTP login flow.
- `api` – typed API clients (generated from OpenAPI).

### 6.2 Guidelines

- Keep components small and composable.
- Use Zustand for application state (project selection, step state, etc.).
- Use React Query for server calls (caching, retries).
- Ensure UX supports:
  - Manual step control.
  - Clear state of “what needs rerun” after schema/mapping changes.

---

## 7. Testing Expectations

See `testing-specification-v1.2.md` for details; summarized:

- Unit tests for business logic and validation mapping.
- Integration tests for microservices.
- Contract tests for API compatibility.
- E2E tests for main user journeys.

---

## 8. Adding FHIR R5 in Future

When extending to R5:

1. Add Firely R5 packages.
2. Extend `FhirVersionCode` and configuration binding.
3. Update `IFhirValidationContextFactory`:
   - Add `BuildValidatorForR5()`.
4. Populate R5 templates and IG packages.
5. UI:
   - Enable R5 in project creation.
   - Indicate FHIR version across screens.
6. Add tests:
   - R5 validator initialization.
   - R5 resource validation cases.

The current architecture is intentionally designed so this is an **additive** change, not a disruptive refactor.

---
