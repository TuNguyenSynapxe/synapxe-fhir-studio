# Workflow v1.2 (Updated for Firely & Project‑Scoped FHIR Version)

This document describes the **end‑to‑end user and system workflow** for Phase 1 of Synapxe FHIR Studio, reflecting that FHIR validation is now performed using an **in‑process Firely .NET Validator** and that **FHIR version is configured per project**.

---

## 1. Overview of Steps

1. Create / open Project (choose FHIR version – R4 for now).
2. Upload / select Source Schema.
3. Generate & adjust Sample Data.
4. Select FHIR Resource Model(s).
5. Design Mappings (source → FHIR).
6. Preview FHIR Bundle.
7. Run FHIR Validation.
8. Export Package.

Each step can be revisited; the workflow is NOT strictly linear but presented as a guided wizard for usability.

---

## 2. Step Details

### Step 0 – Project Creation

**Actor:** User  
**Systems:** UI, backend project service

- User creates a new Project.
- User selects:
  - Name, description.
  - FHIR version → **R4** (R5 shown as disabled “coming soon”).
- System persists `Project` with `FhirVersionCode.R4`.

This value is reused in Steps 4–7.

---

### Step 1 – Source Schema Ingestion

**Actor:** User  
**Systems:** UI, `schema-parser-service`

- User uploads:
  - CSV definition file, **or**
  - XML/XSD/JSON schema, **or**
  - Direct metadata (e.g. from legacy spec).
- UI sends schema file/metadata to `schema-parser-service`.
- `schema-parser-service`:
  - Detects source type.
  - Parses fields, types, constraints.
  - Generates normalized `SchemaDefinition` + `SchemaField` records.
- User can:
  - Inspect parsed fields.
  - Mark certain fields as ignored.
  - Manually adjust descriptions, data types (where safe).

---

### Step 2 – Sample Data Generation

**Actor:** User, `sample-generator-service`  

- User triggers “Generate Sample Data”.
- `sample-generator-service`:
  - Reads `SchemaDefinition`.
  - Generates default synthetic data (per-field rules, examples).
- UI displays sample records.
- User may:
  - Edit values.
  - Add additional edge‑case rows.
  - Save as `SampleSet`.

---

### Step 3 – FHIR Resource Model Selection

**Actor:** User, `template-service`  

- UI shows available FHIR resource models and templates **filtered** by project FHIR version (R4).
- User selects:
  - Main resource (e.g. `Encounter`).
  - Supporting resources (e.g. `Patient`, `Observation`).
- System:
  - Locks resource model selection to project.
  - Optionally stores profile URIs to be used in validation.

---

### Step 4 – Mapping Design

**Actor:** User, `mapping-service`, `variable-service`, (optional AI helper)

- UI layout:
  - Left: Source schema tree.
  - Right: FHIR resource tree(s).
  - Side panel: variables, constants, AI suggestions.
- User creates mapping rules:
  - Field → FHIR path.
  - Expressions (e.g. concatenation, conditional logic).
  - Use variables/constants.
- `mapping-service` stores mapping definitions.
- Optional:
  - AI suggests mappings based on names/descriptions (no PHI).
  - User manually approves/edits AI suggestions.

---

### Step 5 – FHIR Bundle Preview

**Actor:** User, `mapping-engine-service`  

- User clicks “Generate Preview”.
- UI sends:
  - ProjectId, MappingId, SampleSetId.
- `mapping-engine-service`:
  - Loads schema, sample data, mappings, templates.
  - Executes mapping.
  - Produces:
    - FHIR Bundle (JSON).
    - Optional per‑record diagnostics.
- UI:
  - Renders Bundle as:
    - Raw JSON.
    - Pretty JSON.
    - FHIR tree view (optional).

User can loop Steps 4–5 until satisfied.

---

### Step 6 – FHIR Validation (Firely)

**Actor:** User, `validation-service`  

- From the preview step, user clicks “Validate”.
- UI submits:
  - `projectId`
  - Bundle JSON
- `validation-service`:
  1. Loads Project → reads `FhirVersionCode` (R4).
  2. Uses Firely `IFhirValidationContextFactory` to select the appropriate validator.
  3. Validates the Bundle against:
     - Core R4 package.
     - Synapxe IG(s).
     - Project profiles (if configured).
  4. Returns a standardized validation result with:
     - list of issues
     - severity (error/warning/info)
     - locations (resource + path)
- UI:
  - Shows validation summary.
  - Allows user to click an issue and jump back to mapping or template definition.

User may revisit Steps 3–5 to resolve issues and re‑validate.

---

### Step 7 – Export Package

**Actor:** User, `export-service`  

- When satisfied, user clicks “Export”.
- `export-service`:
  - Packages:
    - SchemaDefinition.
    - Mapping definitions.
    - FHIR templates/profiles.
    - Sample data.
    - Optional validation report.
  - Produces ZIP artifacts suitable for:
    - Mapping Studio.
    - Runtime mapping engine.

- UI presents download link(s).

---

## 3. Manual Control vs Automation

- All steps are **user‑driven**:
  - No automatic cascade from one step to the next.
  - User explicitly chooses when to:
    - re‑parse schema
    - regenerate sample data
    - rerun mapping
    - rerun validation
- Pipeline is designed to support **partial reruns**, e.g.:
  - If schema changes → rerun Step 1–2, then Step 4–7.
  - If only mapping changes → rerun Step 4–7.
  - If only FHIR templates change → rerun Step 3–7.

---

## 4. Firely & FHIR Version Impact on Workflow

- FHIR version selection happens at **Project creation**.
- All downstream steps (resource model selection, mapping, validation, export) operate under that version.
- UI may show FHIR version indicator on workflow header:
  - e.g. `FHIR R4` badge.
- Future R5 support:
  - Same workflow; only the resolver and templates differ.

---
