# Synapxe FHIR Modernization Architecture  
## modernization-architecture-v2.2 (Ultra – Conceptual for Phase 4)

_Last updated: 2025-12-02_

---

## 1. Purpose

This document defines the **Modernization Architecture** for Phase 4 of the Synapxe FHIR Studio ecosystem.

While Phase 1 focuses on the **Transformation Studio** (legacy → FHIR mapping + validation), Phase 4 introduces a **Modernization Studio** which uses:

- Collected schemas
- Completed mappings
- FHIR profiles
- Validation history
- Export artefacts

…to help legacy teams:

- Understand their current integration landscape
- Identify patterns and duplication
- Move towards **canonical data models**
- Plan a **tech refresh** (e.g., new APIs, event streams, consolidated FHIR interfaces)

---

## 2. Relationship to Other Phases

- **Phase 1 – Transformation Studio**  
  - Focus: per‑API mapping and validation  
  - Output: mapping JSON, FHIR bundles, profiles, export zips

- **Phase 2 – Analysis Studio**  
  - Focus: cross‑API analysis  
  - Output: coverage reports, catalogue views, cross‑map visuals

- **Phase 3 – AI & Insight Studio**  
  - Focus: AI‑assisted recommendations  
  - Output: mapping smells, reuse recommendations, risk hotspots

- **Phase 4 – Modernization Studio**  
  - Focus: target‑state design and migration planning  
  - Output: canonical models, proposed new APIs, consolidation plans, modernization roadmaps

This document focuses on **Phase 4**, but assumes all earlier phases already exist.

---

## 3. Modernization Studio – High‑Level Goals

The Modernization Studio will:

1. Use **existing artefacts** from Transformation / Analysis:
   - Schemas
   - MappingConfigs
   - Profiles
   - ValidationRuns
   - ExportPackages
2. Build a **canonical integration catalogue**:
   - Logical “business domains”
   - Canonical event types
   - Canonical FHIR bundles
3. Provide **modernization options**:
   - “Consolidate these 5 legacy appointment APIs into 1 canonical FHIR API”
   - “These 3 message schemas could be replaced by a single FHIR Subscription/Bundle”
4. Generate **migration plans**:
   - Which APIs to decommission
   - Which canonical APIs to implement
   - Which mappings can be reused

---

## 4. Core Concepts

### 4.1 Source Artefacts

These are produced by Phase 1–3:

- `Schemas` + `SchemaFields`
- `SampleDataSets`
- `ResourceModels`
- `MappingConfigs` + `MappingRules` + `FieldStatusNotes`
- `Profiles` (FHIR StructureDefinitions)
- `ValidationRuns`
- `ExportPackages`

### 4.2 Canonical Artefacts (New in Phase 4)

New entities introduced by Modernization Studio:

- **CanonicalDomain**
  - e.g. “Patient Management”, “Appointments”, “Billing”
- **CanonicalEventType**
  - e.g. “AppointmentCreated”, “ObservationUpdated”
- **CanonicalBundleDefinition**
  - FHIR bundle design for a canonical event
- **LegacyApiCluster**
  - Group of legacy APIs that serve similar purpose
- **ModernizationProposal**
  - Documented suggestion to move from cluster → canonical design
- **ModernizationDecision**
  - Record of approvals / rejections

---

## 5. High‑Level Architecture

### 5.1 Components

- **Modernization Service**
  - Owns canonical entities
  - Provides APIs to define and manage CanonicalDomains/EventTypes/Bundles
- **Analysis Service (Phase 2/3)**
  - Provides cross‑API statistics, mapping graph
- **AI Modernization Engine**
  - Consumes mapping graph + profiles
  - Suggests canonical candidates and consolidation paths
- **UI – Modernization Studio**
  - Visual explorer for legacy landscape
  - Design canvas for canonical models
  - Proposal & decision workflow

---

## 6. Data Flow – Using Existing Artefacts

### 6.1 Input from Transformation Studio

For each API, Modernization Studio can see:

- `Schemas` – what legacy looks like
- `MappingConfigs` – how legacy fields map to FHIR
- `Profiles` – what FHIR profile is enforced
- `ValidationRuns` – how healthy the mapping is

These are combined into a **Mapping Graph**:

- Nodes: Fields, FHIR paths, Profiles, APIs
- Edges: “mapsTo”, “usedBy”, “belongsTo”, etc.

### 6.2 Enrichment from Analysis / AI Studios

- Analysis computes usage frequencies, redundancy clusters.
- AI suggests:
  - Fields that are semantically identical across APIs.
  - Candidate canonical entities.

---

## 7. Canonical Model Design

### 7.1 CanonicalDomain

Example:

```text
CanonicalDomain:
  Id: "appointments"
  Name: "Appointments"
  Description: "All scheduling and appointment related flows"
```

### 7.2 CanonicalEventType

Example:

```text
CanonicalEventType:
  Id: "appointment-booked"
  DomainId: "appointments"
  Name: "Appointment Booked"
  Description: "Represents the act of creating a new appointment"
```

### 7.3 CanonicalBundleDefinition

Example:

```text
CanonicalBundleDefinition:
  Id: "appointment-booked-bundle"
  EventTypeId: "appointment-booked"
  FhirResources:
    - Encounter (primary)
    - Patient (context)
    - Practitioner (context)
    - Location (context)
```

The bundle definition describes:

- Which FHIR resources are included.
- Their roles (primary vs context).
- Key constraints (profile URLs).

---

## 8. Legacy API Clustering

### 8.1 LegacyApiCluster

Groups together legacy APIs that are candidates for consolidation.

```text
LegacyApiCluster:
  Id: "legacy-apt-cluster-1"
  DomainId: "appointments"
  Members:
    - ApiId: "L-APT-001"
    - ApiId: "L-APT-002"
    - ApiId: "L-APT-003"
```

The cluster is formed based on:

- Overlapping FHIR target resources.
- Similar field semantics.
- Overlapping consuming systems.

### 8.2 Cluster Formation

Cluster formation uses:

- Rule‑based heuristics (e.g. same Patient + Encounter mapping).
- AI‑based similarity analysis across mapping rules.

---

## 9. Modernization Proposals

### 9.1 ModernizationProposal Entity

A proposal describes:

- Which legacy APIs are in scope.
- Which canonical event/bundle they should move to.
- Proposed migration approach.

Fields may include:

- Id
- Title
- Description
- LegacyApiClusterId
- TargetCanonicalEventTypeId
- TargetCanonicalBundleDefinitionId
- RiskLevel
- EstimatedComplexity
- SuggestedTimeline
- Status (`Draft`, `UnderReview`, `Approved`, `Rejected`)

### 9.2 Proposal Lifecycle

1. Draft by architect or generated by AI.
2. Reviewed by team.
3. Approved or rejected.
4. Linked to implementation backlog.

---

## 10. Runtime View – From Legacy to Canonical

Once a modernization proposal is implemented, the runtime may look like:

- Legacy systems send events to:
  - New FHIR‑based canonical API, or
  - Event bus (e.g., Kafka) with canonical FHIR Bundles.
- Legacy endpoints can be:
  - Decommissioned
  - Or redirected through adaptation layers.

The Transformation Studio artefacts are reused as:

- Reference for building new adapters.
- Validation rules for accepting canonical FHIR messages.
- Example mappings for interoperability testing.

---

## 11. How Phase 1 Design Enables Phase 4

The Phase 1 data model already supports:

- Fine‑grained mapping rules (`MappingRules`)
- Explicit non‑mapped fields with reasons (`FieldStatusNotes`)
- Structured profiles (`Profiles`)
- Repeatable exports (`ExportPackages`)

These are critical for:

- Identifying unused or legacy‑only fields.
- Discovering common FHIR patterns across APIs.
- Building canonical bundles that match real usage.

The **key design constraints** that enable future modernization:

1. **Semantic granularity**  
   Every mapping is field → FHIR path with optional transform.

2. **Traceability**  
   All transformations are explicitly recorded and versioned.

3. **Profiles as contracts**  
   FHIR StructureDefinitions act as machine‑readable contracts.

4. **Export reusability**  
   Export packages can be re‑ingested as inputs to Modernization Studio.

---

## 12. Modernization Studio UI (Conceptual)

Key views:

1. **Landscape View**  
   - Graph of legacy APIs and their FHIR targets.
   - Colour‑coded by domain, status, and health.

2. **Domain Explorer**  
   - For each CanonicalDomain: list of canonical events, bundles, and linked legacy clusters.

3. **Cluster Detail View**  
   - Shows all legacy APIs in the cluster.
   - Highlights overlapping fields and FHIR targets.

4. **Proposal Workspace**  
   - Side‑by‑side:
     - Legacy cluster
     - Proposed canonical design
   - Change impact summary.

---

## 13. Integration with Roadmap & Governance

Modernization Studio outputs feed into:

- Enterprise integration roadmap.
- Funding and planning cycles.
- Technical backlog (tickets per modernization proposal).

Governance bodies can:

- Review canonical designs.
- Track modernization progress.
- Ensure that new systems consume canonical APIs instead of adding more point‑to‑point endpoints.

---

## 14. Summary

Phase 4 – Modernization Studio:

- Sits **on top of** Transformation, Analysis, and AI Insights.
- Uses all previous artefacts to construct a **canonical integration layer**.
- Guides legacy teams towards **simpler, more coherent FHIR‑based architectures**.
- Is supported from Day 1 by the decisions already made in Phase 1 design.

This `modernization-architecture-v2.2.md` should be treated as the conceptual blueprint for future phases; Phase 1 implementation choices must remain compatible with this direction.
