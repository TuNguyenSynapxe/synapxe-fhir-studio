
# API Contracts Master v1.0

## Overview
This document defines the *master reference* for all API contracts across Synapxe FHIR Studio.  
It ensures consistency, versioning discipline, and alignment between microservices, UI clients, 
OpenAPI specifications, and system integration points.

This is the **authoritative source** for:
- API naming conventions  
- Request/response envelopes  
- Shared data model schemas  
- Error format  
- Correlation ID usage  
- FHIR-aware endpoints  
- Service-level contracts for all Phase 1 microservices  

---

## 1. Common API Envelope

All responses **MUST** follow the universal envelope:

```json
{
  "success": true,
  "data": {},
  "error": {
    "code": "string",
    "message": "string",
    "details": "string"
  },
  "correlationId": "string"
}
```

### Rules:
- `success = false` → `error` must exist  
- `success = true` → `data` must exist  
- `correlationId` is always generated server-side  
- No PHI (Patient/identifiable data) may be returned in error messages  

---

## 2. Shared Data Structures

### 2.1 SchemaDefinition
```json
{
  "id": "uuid",
  "name": "string",
  "sourceType": "CSV | XML | JSON",
  "fields": [
    {
      "name": "string",
      "type": "string",
      "description": "string",
      "isRequired": false
    }
  ]
}
```

### 2.2 MappingDefinition
```json
{
  "id": "uuid",
  "projectId": "uuid",
  "mappings": [
    {
      "sourceField": "name",
      "targetPath": "FHIR JSONPath",
      "transformation": "expression or variable reference"
    }
  ]
}
```

### 2.3 FhirTemplate
```json
{
  "id": "uuid",
  "resourceType": "Patient | Observation | ...",
  "fhirVersion": "R4",
  "template": {}
}
```

### 2.4 ValidationRequest
```json
{
  "projectId": "uuid",
  "bundle": {}
}
```

---

## 3. Error Model

### 3.1 Standard Error Codes
| Code | Description |
|------|-------------|
| `VALIDATION_FAILED` | FHIR validation errors |
| `SCHEMA_INVALID` | Schema parsing failed |
| `MAPPING_EXECUTION_ERROR` | Mapping engine runtime error |
| `NOT_FOUND` | Resource does not exist |
| `BAD_REQUEST` | Missing fields or invalid request |
| `INTERNAL_ERROR` | Unexpected server error |

---

## 4. Correlation ID
Every request/response MUST contain:

```
X-Correlation-ID (incoming)
correlationId (outgoing)
```

If user does not supply a correlation ID, backend must generate one.

---

## 5. Service Contract Matrix

| Service | Key Endpoints | Returns | Notes |
|---------|----------------|---------|-------|
| Schema Parser | `/v1/schema/parse` | SchemaDefinition | CSV/XML/JSON |
| Sample Generator | `/v1/sample/generate` | SampleDataSet | Uses AI layer |
| Template Service | `/v1/templates` | Template list | FHIR version aware |
| Mapping Service | `/v1/mappings` | MappingDefinition | CRUD |
| Mapping Engine | `/v1/transform/mapping/preview` | FHIR Bundle | Needs fhirVersion |
| Validation Service | `/v1/fhir/validate` | ValidationResult | Firely validator |
| Variable Service | `/v1/variables` | Variable definitions | Scoped by project |
| Export Service | `/v1/export` | ZIP export | Optional metadata |

---

## 6. API Versioning Rules
- Major version (`v1`) MUST be part of endpoint.
- Breaking changes = bump major version.
- Non-breaking additions = minor version documented in OpenAPI only.
- Deprecations must be recorded in this master document.

---

## 7. OpenAPI Consistency Requirements
Every service MUST:
- Implement the common envelope
- Define correlation ID header
- Include examples for success + error
- Reference shared models using `$ref`
- Keep endpoint names consistent across services

---

## 8. FHIR-Aware Contract Requirements
For any FHIR-related API:
- Must specify `fhirVersion` when relevant
- Must operate on valid FHIR JSON structures
- Must ensure no PHI leaks into logs or error responses
- Must support R4 today, R5 future-ready design

---

# END OF DOCUMENT
