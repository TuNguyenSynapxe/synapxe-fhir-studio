---

## Batch 2 — Detailed Integration Requirements (Hybrid: Current + Future)

This batch defines the **interop contract** between:

- **Transformation Studio** (TS) — this new project
- **FHIR Mapping Studio** (MS) — your existing + future-enhanced studio

Design goal:  
> TS and MS must be able to **round-trip mappings** reliably, even as Mapping Studio becomes more powerful over time.

---

# 7. Unified Interop Model

## 7.1 Shared Mapping Engine Contract

Both studios integrate with the same **.NET Mapping Engine**, which executes a **MappingConfig JSON** in a common format.

Core concepts:

- `sourceSchema` – flattened source fields
- `targetFhirPaths` – FHIR element paths (e.g. `Patient.identifier[0].value`)
- `rules` – mapping rules (field → FHIR path + transform)
- `variables` – reusable expressions
- `constants` – static values
- `options` – engine execution settings

The engine contract is **versioned** via:

```json
{
  "engineVersion": "1.0",
  "mappingVersion": 3,
  "templateVersion": 1
}
```

Transformation Studio must output mappings that are **valid for the engine**, even if they do not use all advanced features.

---

# 8. Export Format: TS → Mapping Studio

TS exports a **single ZIP** containing:

- `mapping.json`
- `fhir-template.json`
- `samples.json`
- `profiles/StructureDefinition-*.json`
- `bundle-preview.json` (optional)
- `metadata.json`

## 8.1 mapping.json (Core Schema)

Baseline structure (simplified):

```json
{
  "engineVersion": "1.0",
  "mappingId": "api-123",
  "name": "LegacyAppointmentToFHIR",
  "schemaVersion": 2,
  "resourceModel": {
    "primaryResource": "Encounter",
    "contextResources": ["Patient", "Practitioner"]
  },
  "rules": [
    {
      "id": "rule-1",
      "sourceFieldPath": "appointment.date",
      "targetFhirPath": "Encounter.period.start",
      "transform": "identity",
      "condition": null,
      "notes": [],
      "status": "Mapped"
    }
  ],
  "variables": [],
  "constants": [],
  "options": {
    "strictMode": true
  }
}
```

### 8.1.1 Requirements

- `rules` must contain only constructs that the **current engine** understands.
- `transform` may initially be `"identity"` / `"toString"` / simple casts.
- More complex expressions are allowed later (Phase 2/3), but TS must **still be able to parse and display** them, even if it cannot fully edit them.

---

## 8.2 fhir-template.json

Represents the FHIR resource tree used by Mapping Studio.

Example:

```json
{
  "templateVersion": 1,
  "primaryResource": "Encounter",
  "resources": [
    {
      "resourceType": "Encounter",
      "path": "Encounter",
      "elements": [
        { "path": "Encounter.id", "type": "id" },
        { "path": "Encounter.status", "type": "code" },
        { "path": "Encounter.subject", "type": "Reference(Patient)" }
      ]
    },
    {
      "resourceType": "Patient",
      "path": "Patient",
      "elements": [
        { "path": "Patient.id", "type": "id" },
        { "path": "Patient.name[0].family", "type": "string" }
      ]
    }
  ]
}
```

### 8.2.1 Requirements

- Must match the ResourceModel from TS.
- Mapping Studio can extend this template with **more elements**, but TS should be able to re-import at least the subset it knows.

---

## 8.3 samples.json

Contains sample input payloads.

```json
{
  "samples": [
    {
      "id": "s1",
      "label": "Happy path appointment",
      "format": "json",
      "content": "{ ... }"
    }
  ]
}
```

Transformation Studio:

- Produces this file from its SampleDataSets.
- Must handle both TS-generated and MS-augmented samples.

---

## 8.4 profiles/StructureDefinition-*.json

- FHIR StructureDefinitions generated in TS.
- Mapping Studio does **not** have to consume these directly (Phase 1), but:
  - They may be exposed later for **profile-aware mapping**.
  - They ensure mapping, validation, and profiling all align.

---

## 8.5 bundle-preview.json (Optional)

If TS has run validation, it may include:

```json
{
  "bundle": {
    "resourceType": "Bundle",
    "type": "transaction",
    "entry": [ ... ]
  },
  "status": "Success",
  "validationMessages": [ ... ]
}
```

Mapping Studio can use this as:

- A reference for expected FHIR output.
- A sanity check when re-running mappings.

---

## 8.6 metadata.json

Metadata file:

```json
{
  "apiId": "api-123",
  "name": "Legacy Appointment API",
  "createdBy": "user@domain.com",
  "createdAt": "2025-12-02T10:00:00Z",
  "transformationStudioVersion": "2.2",
  "mappingEngineVersion": "1.0",
  "interopVersion": "1.0"
}
```

This allows Mapping Studio to:

- Validate compatibility.
- Warn if engine versions differ.

---

# 9. Import Format: Mapping Studio → Transformation Studio

When Mapping Studio is the authoring system, it can export a superset of the above:

- `mapping.json` (may contain complex transforms, variables, lookups)
- `fhir-template.json`
- `samples.json`
- `metadata.json`

Transformation Studio must **import these gracefully**, while respecting differences in capabilities.

---

## 9.1 mapping.json (Extended)

Example (future-capable):

```json
{
  "engineVersion": "1.2",
  "mappingId": "api-123",
  "name": "LegacyAppointmentToFHIR",
  "schemaVersion": 3,
  "resourceModel": {
    "primaryResource": "Encounter",
    "contextResources": ["Patient"]
  },
  "variables": [
    {
      "name": "normalizedIc",
      "expression": "stripNonDigits(source.patientIc)"
    }
  ],
  "constants": [
    {
      "name": "defaultEncounterStatus",
      "value": "finished"
    }
  ],
  "rules": [
    {
      "id": "rule-1",
          "sourceFieldPath": "appointment.date",
      "targetFhirPath": "Encounter.period.start",
      "transform": "toFHIRDateTime(appointment.date)",
      "condition": null,
      "status": "Mapped"
    },
    {
      "id": "rule-2",
      "sourceFieldPath": "patient.ic",
      "targetFhirPath": "Patient.identifier[0].value",
      "transform": "variables.normalizedIc",
      "condition": "notEmpty(patient.ic)",
      "status": "Mapped"
    }
  ]
}
```

### 9.1.1 Transformation Studio Behaviour

When importing complex mappings:

- TS must **parse**:
  - Variables
  - Constants
  - Expressions (transform & condition)
- TS may:

  - Display expressions as **read-only** (Phase 1).
  - Allow editing only of simple `identity`/`toString` mappings.
  - Display warnings for advanced constructs it cannot fully support yet.

No data should be lost on import:

- Even if TS UI cannot edit it, the mapping.json must be preserved **as-is**.

---

# 10. Mapping Rule Compatibility Matrix

We define a compatibility matrix between:

- **TS minimal mapping set**  
- **MS advanced mapping set**

## 10.1 Supported in BOTH (Baseline)

- One-to-one mappings  
- Simple cast transforms (string ↔ int, string ↔ date)  
- Simple conditions (isEmpty / notEmpty / equals)  
- Basic concatenation (future)  

TS **can author and display** these natively.

---

## 10.2 Authored in MS, Viewable in TS (Future-Aware)

- Complex expressions: `if`, `case`, nested functions  
- Advanced string ops: `substring`, `regexReplace`, `trim`, `pad`  
- Advanced date ops: `addDays`, `formatDate`, `parseDateTime`  
- Multi-field aggregation: `concat(a, " ", b)`  
- Lookup tables (e.g. mapping old codes → new codes)  
- Variable scoping rules  

TS behaviour:

- Shows full expression in read-only text area.
- Highlights advanced expressions with an icon (e.g. “advanced rule (edit in Mapping Studio)”).
- Does not attempt to re-interpret or simplify.

---

## 10.3 Unsupported / Out-of-Scope Constructs (Phase 1)

For future-phase but **not required now**:

- Inline JavaScript  
- External callable functions (e.g. HTTP lookups at mapping time)  
- Stateful transforms  

TS must **not reject** mappings that contain these, but:

- Must mark them as “advanced” and preserve content verbatim.

---

# 11. Conflict Detection & Reconciliation

Integration must detect and surface conflicts when:

- TS and MS both edit the same mapping in parallel.
- TS re-imports a mapping that diverges from its database version.

## 11.1 Version Conflicts

Each mapping.json contains:

```json
"mappingVersion": 3
```

On import:

- If TS database has `version = 3` and file is `version = 4` → accept and update.
- If TS database has `version = 4` and file is `version = 3` → treat as **stale import**:
  - Warn user
  - Offer option:
    - Overwrite with older version
    - Keep newer version
    - Merge manually (future)

---

## 11.2 Path-Level Conflicts

If two versions of mapping.json differ on the same `targetFhirPath`:

- TS shows a diff:
  - Old sourceFieldPath vs new sourceFieldPath
  - Old transform vs new transform
- For Phase 1, TS can:
  - Choose “keep external version” (from MS)
  - Or “keep TS version”
- Future phase: interactive merge UI.

---

## 11.3 Variable Name Conflicts

If variable names in mapping.json conflict with TS local variable naming rules:

- TS must preserve external variable exactly.
- TS may not allow editing of conflicting variable in UI, but must **not modify** it.

---

# 12. Versioning Across Studios

## 12.1 Version Fields

- `schemaVersion`  
- `mappingVersion`  
- `templateVersion`  
- `profileVersion` (implicit via profiles folder)  
- `interopVersion` (overall interop schema version)  

### 12.1.1 Rules

- Each time mappings are structurally changed (new rules, changed target paths, different resource model), `mappingVersion++`.
- Each time template structure changes (resource tree), `templateVersion++`.
- `interopVersion` is bumped only for breaking changes to **export format schema**.

TS and MS both:

- Compare engineVersion and interopVersion on import.
- Warn if the incoming file was produced by a **newer interopVersion**.

---

# 13. Change Impact Analysis

Integration must allow downstream modules (Analysis, Modernization) to track:

- Drift between current TS mapping vs last imported MS mapping  
- FHIR profile changes vs mapping assumptions  
- Schema changes vs mapping coverage  

### 13.1 Drift Types

- **Schema Drift** – schema changed but mapping not updated.
- **Mapping Drift (TS vs MS)** – one side changed mapping.
- **Profile Drift** – profile changed but mapping remains same.

TS stores minimal metadata to support drift detection:

```json
"lastImportedFrom": "MappingStudio",
"lastImportedAt": "...",
"lastExportedTo": "MappingStudio",
"lastExportedAt": "..."
```

---

# 14. AI-Assisted Cross-Studio Alignment

Although not required in Phase 1, the integration spec anticipates AI usage to:

- Detect mapping differences between TS and MS.
- Explain how two versions differ.
- Suggest a merged “best version”.

Example prompt (conceptual):

> “Compare these two mapping.json versions and explain differences for each FHIR path.  
> Highlight where TS is more complete vs MS, and vice versa.”

TS/AI behaviour:

- Never auto-merge.
- Only present suggestions & explanation to architects.

---

# END OF BATCH 2
