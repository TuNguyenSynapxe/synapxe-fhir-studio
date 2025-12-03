# Synapxe FHIR Studio – Core Architecture v2.2 (Firely Updated)

> This version supersedes previous drafts that referenced HAPI as the primary validator.  
> Phase 1 now standardises on **Firely .NET FHIR Validator**, with an architecture that is **R4-first, R5‑ready**.

---

## 1. High‑Level Goals

Synapxe FHIR Studio is a multi‑phase platform to help teams:

1. Analyse legacy schemas (CSV / XML / JSON / XSD).
2. Generate sample data and FHIR resource models.
3. Design and maintain mappings to FHIR (using your existing Mapping Studio).
4. Validate resulting FHIR Bundles against profiles.
5. Export reusable artefacts for runtime engines and downstream systems.
6. In later phases, provide analysis, drift detection, canonical models and modernization guidance.

Phase 1 focuses on the **Transformation Studio** (schema → sample → mapping → bundle → validation → export) with:

- .NET 8 microservices.
- Postgres as system-of-record.
- Firely .NET FHIR Validator in‑process.
- Project‑level configurable FHIR version (R4 now, R5 later).

---

## 2. Core Logical Components

### 2.1 Transformation Studio (UI)

- Web UI (React + TS + Zustand).
- Guides user through a 6‑step workflow:
  1. Upload / select source schema.
  2. Generate & refine sample data.
  3. Select FHIR resource model(s).
  4. Create & refine mappings.
  5. Preview FHIR Bundle output.
  6. Run validation & export package.

### 2.2 Backend Services (Phase 1)

- **schema-parser-service** – parses legacy schemas into a normalized `SchemaDefinition`.
- **sample-generator-service** – generates synthetic sample datasets from `SchemaDefinition`.
- **mapping-service** – stores mapping configurations (source fields → FHIR paths, expressions, variables).
- **template-service** – manages reusable FHIR templates and resource models.
- **variable-service** – manages mapping‑time variables and constants.
- **mapping-engine-service** – executes mappings to produce FHIR resources/Bundles.
- **validation-service** – validates FHIR Bundles using **Firely .NET**.
- **export-service** – packages mappings, templates and sample data for external tools (e.g. Mapping Studio, runtime engine).

All services are **stateless** and fronted by an API Gateway / reverse proxy.

### 2.3 Shared Infrastructure

- **PostgreSQL**
  - Stores projects, schemas, mappings, templates, variables, sample sets, exports.
- **Blob Storage (Azure Blob / S3)**
  - Stores exported ZIP packages, large artefacts.
- **Firely FHIR Packages**
  - FHIR R4 core and Synapxe IG packages, mounted as files or stored in blob storage.
- **Authentication**
  - Phase 1: email OTP (reusing implementation from Form Builder).
  - Later phases: OIDC / Entra ID integration.

---

## 3. FHIR Validation – Firely‑Centric Design

### 3.1 Why Firely

- Native .NET library – no Java / external validator.
- Same engine used by Forge/Simplifier.
- Licensing suitable for enterprise use (Apache‑style).
- Strong profile, slicing, invariant and extension support.

### 3.2 Project‑Level FHIR Version

Each **Project** stores its FHIR version:

```csharp
public enum FhirVersionCode
{
    R4 = 1,
    R5 = 2      // reserved for future use
}

public class Project
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public FhirVersionCode FhirVersion { get; set; }  // R4 now, R5 later
    // ...
}
```

- Phase 1 UI: user selects **FHIR R4** at project creation (R5 shown as “coming soon”).
- Phase 4: UI will enable **FHIR R5** without breaking existing projects.

### 3.3 Validation Architecture

- `validation-service` hosts Firely validators **in‑process**.
- `IFhirValidationContextFactory` selects a validator based on `FhirVersionCode` and project context.
- For R4:
  - Load `hl7.fhir.r4.core` package.
  - Load Synapxe R4 IG package(s).
  - Load project‑specific StructureDefinitions, ValueSets, CodeSystems (if any).
- For R5 (future):
  - Analogous R5 packages and profiles.

Validation flow:

1. UI submits `projectId` + FHIR Bundle to `/v1/fhir/validate`.
2. `validation-service` resolves project → `FhirVersionCode`.
3. Factory returns a cached `IValidator` instance for that version.
4. Firely validator produces an `OperationOutcome`.
5. Service maps `OperationOutcome` to a standardized `ValidationResultDto` and API envelope.

---

## 4. End‑to‑End Phase 1 Workflow (Logical)

1. **Schema Ingestion**
   - User uploads schema (CSV/XML/JSON/XSD) or attaches existing source spec.
   - `schema-parser-service` parses and normalizes into `SchemaDefinition` + fields metadata.

2. **Sample Data Generation**
   - `sample-generator-service` generates example records using `SchemaDefinition`.
   - User adjusts sample data (e.g. realistic values, edge cases).
   - Adjusted samples are stored and versioned.

3. **FHIR Resource Model Selection**
   - User chooses FHIR resources (e.g. `Patient`, `Encounter`, `Observation`).
   - `template-service` provides FHIR templates and profiles.
   - Project locks onto a **FHIR version** and set of resource models.

4. **Mapping Design**
   - UI shows **source schema tree** vs **FHIR resource tree**.
   - `mapping-service` stores rules like:
     - Field → FHIR path.
     - Transform expressions.
     - Use of variables, constants, lookups.
   - Optional AI suggestions propose mappings or expressions.

5. **Bundle Preview**
   - `mapping-engine-service` executes mapping using:
     - `SchemaDefinition`
     - Sample data
     - Mapping rules
     - Selected resource templates
   - Produces FHIR Bundle preview for UI.

6. **Validation**
   - UI sends Bundle to `validation-service` with `projectId`.
   - Firely validates against:
     - Core R4 package.
     - Synapxe IG.
     - Project profiles (if any).
   - UI presents errors/warnings grouped by resource, with links back into mapping design.

7. **Export**
   - `export-service` bundles:
     - Schema metadata
     - Mappings
     - FHIR templates / profiles
     - Sample data
   - Export formats:
     - Bundle for Mapping Studio (JSON mapping, sample JSON, FHIR templates).
     - Bundle for runtime engine.

---

## 5. Non‑Functional Requirements (Phase 1)

- **Performance**
  - Interactive operations (parse, preview, validate) should typically complete within a few seconds for typical bundle sizes.
  - Firely validator instances cached to avoid repeated initialization overhead.

- **Scalability**
  - All services stateless; scale horizontally behind API gateway.
  - DB and storage sized for expected number of projects and mappings.

- **Security**
  - No PHI in AI prompts or logs.
  - Bundles may carry pseudo / synthetic data in Phase 1.
  - Production deployments must comply with PDPA / IM8 constraints.

- **Extensibility**
  - Explicit abstraction for FHIR version selection.
  - Validation pipeline decoupled via `IFhirValidationContextFactory`.
  - Mapping engine designed to support future non‑FHIR targets.

---

## 6. Roadmap Hooks in Architecture

The architecture is intentionally designed to support future phases without refactoring:

- **Phase 2 – Analysis Studio**
  - Reads schemas, mappings, validation results from central DB.
  - Adds analysis microservices; re‑uses mapping/validation outputs.

- **Phase 3 – Drift Detection**
  - Listens to future runtime events and detects schema/mapping drift.

- **Phase 4 – Modernization Studio**
  - Builds canonical models and profiles on top of existing mappings and validation library.
  - May add stricter validation (e.g. HL7 Java validator) but Firely remains the in‑process default.

---
