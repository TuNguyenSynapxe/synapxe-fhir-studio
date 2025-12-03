# OpenAPI Summary v2.2 (Firely‑Aware Validation Service)

This document summarizes the backend API surface, with emphasis on Phase 1 transformation services and the validation endpoint updated to use Firely .NET FHIR Validator.

---

## 1. Service List & Purpose

- **schema-parser-service**
  - `/v1/transform/schema/parse`
- **sample-generator-service**
  - `/v1/transform/sample/generate`
- **mapping-service**
  - `/v1/mappings` (CRUD)
- **template-service**
  - `/v1/templates` (CRUD/listing)
- **variable-service**
  - `/v1/variables` (CRUD)
- **mapping-engine-service**
  - `/v1/transform/mapping/preview`
- **validation-service**
  - `/v1/fhir/validate`
- **export-service**
  - `/v1/transform/export/package`

All endpoints:

- Follow standard response envelope.
- Require `X-Correlation-Id` header.
- Return JSON.

---

## 2. Validation-Service Endpoint (Updated)

### 2.1 `POST /v1/fhir/validate`

**Purpose:**  
Validate a FHIR Bundle against the project’s configured FHIR version and profiles, using **Firely .NET Validator in‑process**.

**Request (JSON):**

```json
{
  "projectId": "guid",
  "bundle": {
    "resourceType": "Bundle",
    "type": "collection",
    "entry": [ ... ]
  }
}
```

- `projectId`:
  - Used to resolve FHIR version and project‑specific profiles.
- `bundle`:
  - FHIR JSON payload to validate.

**Response (200 OK):**

```json
{
  "success": true,
  "data": {
    "isValid": false,
    "issues": [
      {
        "severity": "error",
        "code": "invalid",
        "details": "The required element 'status' is missing.",
        "location": "Bundle.entry[0].resource.status"
      }
    ]
  },
  "warnings": [],
  "correlationId": "..."
}
```

**Error Responses:**

- `400 Bad Request` – invalid JSON or missing fields.
- `500 Internal Server Error` – unexpected server-side errors (no PHI in message).

### 2.2 Notes

- Implementation detail:
  - Firely `OperationOutcome` is not exposed directly.
  - Instead, it is mapped to a stable DTO for the UI.
- FHIR version:
  - Derived from project (`R4` for Phase 1).
  - Future: R5 handled by selecting the appropriate validator.

---

## 3. Common Patterns

### 3.1 Correlation ID

- All endpoints must require `X-Correlation-Id`.
- Responses must echo the `correlationId` in the envelope.

### 3.2 Error Envelope

Shared error format:

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

### 3.3 OpenAPI Files

Each microservice has its own OpenAPI YAML in `/openapi/`:

- `schema-parser-service.openapi.yaml`
- `sample-generator-service.openapi.yaml`
- `mapping-engine-service.openapi.yaml`
- `validation-service.openapi.yaml`
- etc.

The UI and any external clients should be generated from these OpenAPI contracts wherever possible.

---
