# Testing Specification v1.2 (Updated for Firely & Multi‑FHIR Support)

This document defines the minimum testing requirements for Phase 1 of Synapxe FHIR Studio, updated to reflect:

- Firely .NET FHIR Validator (in‑process).
- Project‑level FHIR version handling (R4 now, R5 in future).
- Standardised API envelopes and error handling.

---

## 1. Testing Scope

The following layers must be covered:

1. **Unit Tests**
   - Pure business logic, mapping logic, validators, and helpers.
2. **Integration Tests**
   - Service + database + external dependencies (e.g. Firely packages from disk).
3. **Contract Tests**
   - API contracts from the perspective of clients (UI, other systems).
4. **End‑to‑End (E2E) Tests**
   - UI → backend → DB → validator → back to UI.

Target coverage (backend):

- **≥ 80% line coverage** per microservice.
- Critical logic paths should have branch coverage.

---

## 2. Unit Test Guidelines

### 2.1 General

- Use xUnit for .NET services.
- Use descriptive test names: `MethodName_Scenario_ExpectedResult`.
- Avoid real network/filesystem where possible; use mocks/stubs.

### 2.2 Validation-Service Unit Tests

- Validator initialization for R4:
  - Firely `IValidator` can be constructed with expected configuration.
- Validation of:
  - Valid FHIR Bundle → no errors.
  - Bundle with known structural issues → expected set of issues.
- Project FHIR version switching:
  - When `Project.FhirVersion = R4`, validator for R4 is used.
  - Future: R5 path is covered when implemented.
- OperationOutcome mapping:
  - Firely issues → `ValidationResultDto`.
  - Severity mapping (error/warning/info).
  - Location/path mapping.

### 2.3 Mapping Engine Unit Tests

- Field mapping scenarios (simple copy, transformation).
- Variable and constant resolution.
- Error scenarios for invalid expressions.

### 2.4 Schema Parser Unit Tests

- CSV/XSD/JSON/XML parsing for small fixtures.
- Type inference rules.
- Edge cases: missing headers, unexpected formats.

### 2.5 Sample Generator Unit Tests

- Generation of default values by type.
- Nullability handling.
- Deterministic behaviour when required (configurable).

---

## 3. Integration Test Guidelines

Integration tests are executed against:

- Real Postgres (local container).
- Real Firely packages from disk.
- Real HTTP endpoints of microservices (via test host / WebApplicationFactory).

### 3.1 Validation-Service Integration

- Load core R4 packages and a small Synapxe IG fixture.
- Validate:
  - Basic resource instance.
  - Bundle with profile conformance.
  - Bundle with value set binding violation (if terminology mocked).
- Negative tests:
  - Malformed JSON.
  - Unsupported FHIR version (if misconfigured project).

### 3.2 Mapping Engine Integration

- End‑to‑end transformation from sample data → FHIR Bundle.
- Use real mapping configurations and templates.
- Ensure no unexpected nulls or missing required fields (pre‑validation).

### 3.3 Export-Service Integration

- Create realistic data in DB (schemas, mappings, templates).
- Invoke export endpoint.
- Assert ZIP contents:
  - Contains mapping definition JSON.
  - Contains FHIR templates.
  - Contains sample data.

---

## 4. Contract Tests

Contract tests verify that **public APIs** adhere to their contracts defined in OpenAPI specs and API contracts master.

### 4.1 Approach

- Generate client stubs from OpenAPI.
- For each endpoint:
  - Send typical and edge‑case requests.
  - Assert response envelope adheres to schema:
    - `success` flag.
    - `data` shape where applicable.
    - `error.code`, `error.message`, `error.details` when failure.
    - `correlationId` present.

### 4.2 Validation-Service Contract Tests

- Confirm:
  - `/v1/fhir/validate` returns `200` with a well‑formed `ValidationResultDto`.
  - On invalid input, returns `400` with structured error.
  - On internal errors, returns `500` with error + correlationId (no stack traces).

---

## 5. End‑to‑End Tests

Use Playwright or similar to exercise the UI with a running backend stack.

**Minimum E2E flows:**

1. **Happy Path**
   - Create project (R4).
   - Upload schema.
   - Generate sample data.
   - Select resource model.
   - Define basic mappings.
   - Generate preview.
   - Run validation (expect no errors).
   - Export package.

2. **Validation Error Flow**
   - Same as above, but mapping misconfigured.
   - Validate → view errors.
   - Update mapping to fix errors.
   - Re‑validate → errors resolved.

3. **Partial Rerun Flow**
   - Modify schema and re‑generate sample data.
   - Confirm mapping step indicates required re‑alignment.

---

## 6. Test Data & Fixtures

- Use **non‑PHI synthetic data** only.
- Test FHIR Bundles:
  - Simple sample patients/encounters/observations.
  - Example conformance to basic profiles.
- Keep fixtures small but representative for performance.

---

## 7. CI Requirements

- All unit and integration tests must run in CI.
- Code coverage report generated and stored as artefact.
- PR checks:
  - Build passes.
  - Tests pass.
  - Coverage threshold ≥ 80% not regressed.

---
