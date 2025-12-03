# Microservice Architecture v2.2 (Firely & Multi‑FHIR Update)

This document describes the Phase 1 microservice layout and updates all validation‑related flows to use **Firely .NET FHIR Validator** in‑process.

---

## 1. Service Catalogue (Phase 1)

**Transformation Pipeline Services**

1. **schema-parser-service**
   - Input: legacy schema (CSV/XML/JSON/XSD or metadata CSV).
   - Output: normalized `SchemaDefinition` + `SchemaField` records.
2. **sample-generator-service**
   - Input: `SchemaDefinition`.
   - Output: synthetic sample data records.
3. **template-service**
   - Manages FHIR resource templates and profiles (per version).
4. **mapping-service**
   - CRUD for mapping configurations.
5. **variable-service**
   - CRUD for mapping variables/constants.
6. **mapping-engine-service**
   - Executes mappings and produces FHIR Bundles.
7. **validation-service**
   - Validates Bundles with in‑process Firely validator.
8. **export-service**
   - Packages mappings, templates, sample data and profiles.

**Shared / Cross‑Cutting**

- **auth-service** (OTP for Phase 1).
- **configuration-service** (project + environment config; may be embedded in backend for Phase 1).
- **audit/logging subsystem** (shared logging strategy).

Each service exposes a **REST API**; all are stateless and share:

- Standard API envelope (`success`, `data`, `error`, `correlationId`).
- Standard error codes (API contracts master).
- Correlation IDs propagated from the UI / gateway.

---

## 2. Validation-Service Micro-Architecture

### 2.1 Key Responsibilities

- Validate FHIR resources/Bundles against:
  - FHIR core packages (R4 for now).
  - Synapxe IG packages.
  - Project-specific StructureDefinitions / profiles.
- Map Firely `OperationOutcome` into standard API responses.
- Support multiple FHIR versions in future (R4, R5).

### 2.2 Core Components

- `IFhirValidationContextFactory`
  - Accepts `FhirVersionCode` + `projectId`.
  - Returns a ready‑to‑use `IValidator` instance.
- `FirelyValidatorProvider`
  - Handles:
    - Loading of core packages (`hl7.fhir.r4.core` initial).
    - Loading of Synapxe IG packages.
    - Loading of project profiles from storage.
  - Builds `IResourceResolver` chain: core → IG → project.
- `ValidationController`
  - Exposes `/v1/fhir/validate`.
  - Handles request parsing, project lookup, error mapping.

### 2.3 Multi‑Version Strategy

- Phase 1: only **R4** is configured.
- Architectural abstraction is already in place to add **R5**:
  - New packages and resolver registration.
  - Update `Project.FhirVersion` allowed values.
  - UI toggle to enable R5.

---

## 3. Service‑to‑Service Interactions (Phase 1)

### 3.1 Happy‑Path: Schema → Export

1. UI → `schema-parser-service`
2. UI → `sample-generator-service`
3. UI → `template-service` (load templates for chosen version & resources)
4. UI → `mapping-service` (create/update mapping)
5. UI → `mapping-engine-service` (Bundle preview)
6. UI → `validation-service` (validate Bundle)
7. UI → `export-service` (produce export bundle)

Services communicate through REST only in Phase 1.  
Phase 2+ may introduce async events (e.g. for analysis & drift detection).

---

## 4. Data Ownership

- `schema-parser-service`
  - Owns `schemas`, `schema_fields`.
- `sample-generator-service`
  - Owns `sample_sets`, `sample_records`.
- `mapping-service`
  - Owns `mappings`, `mapping_rules`.
- `template-service`
  - Owns `resource_models`, `fhir_templates`.
- `variable-service`
  - Owns `variables`, `variable_groups`.
- `validation-service`
  - Owns `validation_runs`, `validation_issues`.
- `export-service`
  - Owns `export_packages`.

All tables are hosted in the shared Postgres instance; logical ownership is per service to keep boundaries clear.

---

## 5. Cross‑Cutting Concerns

- **Authentication**
  - Phase 1: OTP email (single‑tenant).
- **Authorization**
  - Simple project‑level scoping; no fine-grained RBAC initially.
- **Observability**
  - Each service emits structured logs with `correlationId`.
  - Later: OpenTelemetry instrumentation for tracing.
- **Resilience**
  - Services stateless; restart trivially.
  - UI handles transient failures with retries on idempotent calls.

---

## 6. Extension Points

Microservice architecture is prepared for:

- Add-on analysis services (Phase 2).
- Drift detection and live runtime connectors (Phase 3).
- Modernization / canonical modelling services (Phase 4).
- Additional target formats beyond FHIR (non‑FHIR mappings).

The Firely‑centric design ensures that all future FHIR versions and canonical models share one consistent validation stack.
