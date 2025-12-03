# State Machine v1.2 (Firely & FHIR Version Aware)

This document describes the **internal state machine** for a Project’s transformation workflow, updated so that the validation states reflect **in‑process Firely validation** and **project‑level FHIR version selection**.

---

## 1. High‑Level States

For each **Project**, the system transitions through these macro‑states:

1. `PROJECT_CREATED`
2. `SCHEMA_DEFINED`
3. `SAMPLE_DATA_READY`
4. `RESOURCE_MODEL_SELECTED`
5. `MAPPINGS_DEFINED`
6. `BUNDLE_PREVIEW_READY`
7. `VALIDATION_ENGINE_SELECTED`
8. `VALIDATION_COMPLETED`
9. `EXPORT_READY`

The user can loop around some of these states multiple times; state transitions must be **idempotent** and **monotonic**, never “losing” earlier user work.

---

## 2. Detailed State Definitions

### 2.1 PROJECT_CREATED

- Trigger: user creates a project.
- Data:
  - `Project.Id`, `Name`, `Description`.
  - `Project.FhirVersion` (R4 now, R5 later).
- Next possible transitions:
  - → `SCHEMA_DEFINED` (after Step 1).

---

### 2.2 SCHEMA_DEFINED

- Precondition:
  - At least one `SchemaDefinition` exists for project.
- Entered when:
  - `schema-parser-service` successfully parses uploaded schema.
- Data:
  - Schema metadata + fields.
- Re‑entry:
  - If schema is re‑uploaded, state remains `SCHEMA_DEFINED` but schema version increments.
- Next transitions:
  - → `SAMPLE_DATA_READY`.

---

### 2.3 SAMPLE_DATA_READY

- Precondition:
  - At least one `SampleSet` stored.
- Entered when:
  - `sample-generator-service` generates initial sample data, OR
  - User imports/provides manual sample data.
- Re‑entry:
  - When sample data is regenerated/edited.
- Next transitions:
  - → `RESOURCE_MODEL_SELECTED`.

---

### 2.4 RESOURCE_MODEL_SELECTED

- Precondition:
  - Project knows its `FhirVersion`.
  - At least one resource model selected (e.g. `Encounter`).
- Entered when:
  - User selects FHIR resource templates for the project.
- Next transitions:
  - → `MAPPINGS_DEFINED`.

---

### 2.5 MAPPINGS_DEFINED

- Precondition:
  - At least one mapping rule exists for project.
- Entered when:
  - User saves mapping rules (source → FHIR path, expressions, variables).
- Re‑entry:
  - Any mapping change keeps the state but updates version.
- Next transitions:
  - → `BUNDLE_PREVIEW_READY`.

---

### 2.6 BUNDLE_PREVIEW_READY

- Precondition:
  - `MAPPINGS_DEFINED`.
  - At least one sample record.
- Entered when:
  - `mapping-engine-service` successfully executes mapping for sample data.
- Data:
  - FHIR Bundle preview (last successful run).
- Next transitions:
  - → `VALIDATION_ENGINE_SELECTED` (when user initiates validation).

---

### 2.7 VALIDATION_ENGINE_SELECTED

- New state introduced for Firely + multi‑FHIR version design.
- Precondition:
  - Project has `FhirVersion` assigned.
- Entered when:
  - `validation-service` resolves which validation engine to use:
    - In Phase 1 this is always Firely with R4 packages.
    - In future, may branch between R4/R5 or additional validators.
- Internal actions:
  - Resolve / create `IValidator` instance.
  - Warm up package resolvers if needed.
- Next transitions:
  - → `VALIDATION_COMPLETED`.

---

### 2.8 VALIDATION_COMPLETED

- Precondition:
  - A validation run has completed for the current preview.
- Data:
  - Latest `ValidationRun` record.
  - List of issues (errors/warnings/info).
- Outcomes:
  - If **no errors**:
    - User may proceed → `EXPORT_READY`.
  - If **errors exist**:
    - UI will encourage user to go back to:
      - Mapping design
      - Resource model selection
      - Schema/sample adjustments

State machine does **not force** error‑free status to proceed, but UI should warn strongly for production use.

---

### 2.9 EXPORT_READY

- Precondition:
  - At least one successful preview exists.
  - Validation run exists (optional but recommended).
- Entered when:
  - User clicks “Prepare Export” and `export-service` successfully builds package.
- Data:
  - Export package metadata.
- Next transitions:
  - `EXPORT_READY` is effectively a terminal state for Phase 1.
  - User can still loop back to re‑edit and generate new exports.

---

## 3. Loops and Partial Re‑Runs

The design intentionally supports:

- Re‑upload schema → re‑enter `SCHEMA_DEFINED` and re‑run following steps.
- Regenerate sample data → re‑enter `SAMPLE_DATA_READY`.
- Modify mappings → re‑enter `MAPPINGS_DEFINED`.
- Re‑generate preview → `BUNDLE_PREVIEW_READY`.
- Re‑validate → `VALIDATION_ENGINE_SELECTED` → `VALIDATION_COMPLETED`.
- Re‑export → new `EXPORT_READY`.

This makes the system tolerant to iterative refinement.

---

## 4. Failure States

Operational failures do **not** create new persistent states but should be logged and surfaced as transient errors:

- Parse failure.
- Sample generation failure.
- Mapping execution failure.
- Validation engine failure (e.g. configuration error).
- Export failure.

All such failures keep the Project in its previous state; user can retry after corrective actions.

---
