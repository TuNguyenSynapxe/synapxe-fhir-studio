# Glossary v2.2  
Synapxe FHIR Studio — Wave 4  
(Expanded with developer, architecture, AI, and lifecycle terminology)

This glossary consolidates all core definitions used across the Studios, backend microservices, the frontend, and the AI layer.  
It is meant to serve as the single vocabulary reference for engineers, analysts, architects, and AI components.

---

# 1. Core Domain Terms (FHIR & Transformation)

## **Schema**
A representation of a legacy payload structure (CSV, XML, XSD, JSON).  
Defines:
- field names  
- types  
- constraints  
- hierarchical structure  

## **SchemaDefinition (Normalized)**
The internal schema model produced by `schema-parser-service`.  
Used uniformly across Studios.

## **Sample Data**
AI-generated **synthetic** example input data.  
Used for:
- mapping preview  
- validation  
- debugging  

Synthetic only — never real PHI.

## **Resource Model**
The FHIR resource configuration selected by the user:
- Primary resource (e.g., Encounter)  
- Optional context resources (e.g., Patient)  

Defines the FHIR tree used in mapping.

## **Mapping**
The transformation rules from schema → FHIR paths:
- fieldPath
- transform expression
- FHIR path
- optional variables & conditions

Stored as `mapping.json`.

## **Mapping Workspace**
The interactive UI for applying transformation rules.

Panels:
- Source panel  
- FHIR tree  
- Mapping grid  
- AI suggestion panel  

## **Mapping Engine**
Microservice that executes:
- Mapping rules  
- Expression evaluation  
- Output FHIR Bundle  

## **FHIR Bundle**
The output produced by mapping.  
Contains resources representing the transformed data.

## **Profile**
A customized FHIR StructureDefinition extending or constraining the base resource.

Types:
- local profile  
- canonical profile (Phase 4)

## **Validation**
The process of:
1. Running mapping  
2. Generating a Bundle  
3. Validating via StructureDefinitions  

Outputs structured errors.

## **Do-Not-Map Flag**
User instruction that a source field MUST NOT be mapped.  
AI must not override this.

---

# 2. AI / ML Terms

## **RAG (Retrieval-Augmented Generation)**
Technique where LLM responses are enhanced using relevant retrieved documents/embeddings.

## **Embedding**
Numeric vector representing semantic meaning of:
- fields  
- FHIR elements  
- mapping rules  
- profiles  
- patterns  

Used for search & clustering.

## **Field Embedding**
Vector representation of a legacy field.

Used for:
- mapping suggestions  
- schema similarity  
- canonical merge recommendation  

## **Mapping Embedding**
Vector representation of:
- FHIR path  
- transform expression  
- mapping context  

Used for drift detection.

## **Pattern Embedding**
Vector derived from expression logic (AST).  
Used to detect reusable patterns.

## **Profile Embedding**
Semantic vector for a StructureDefinition.

## **AI Orchestrator**
The microservice that:
- sends requests to OpenAI/Azure OpenAI/local LLMs  
- enforces prompts + guardrails  
- attaches retrieved context for RAG  

## **Confidence Score**
AI-generated probability that a suggestion is appropriate.  
Displayed to the user as 0–1.

## **PHI Masking**
Automatic masking of:
- NRIC  
- phone numbers  
- full names  
- addresses  
Before data enters embeddings or logs.

---

# 3. Analysis & Modernization Terms

## **Cluster**
A group of similar elements:
- schema cluster  
- mapping cluster  
- profile cluster  
- pattern cluster  

Clusters are key to Modernization Studio.

## **Mapping Drift**
When two APIs diverge over time in how they map similar fields.

## **Profile Drift**
Differences in FHIR profiles across APIs where consistency is expected.

## **Delta**
The difference between two profiles or schemas.

## **Canonical Model**
A unified representation synthesized from many legacy schemas.

## **Canonical Profile**
Modernization Studio output representing a standardized StructureDefinition.

## **Consolidation Map**
A diagram showing which APIs can be merged or retired.

## **Impact Heatmap**
Analysis identifying which APIs are most affected by changes.

---

# 4. Microservice Architecture Terms

## **Service Boundary**
The domain that a microservice owns exclusively.

## **Stateless Service**
A service that does not preserve session state.  
All state is externalized (e.g., DB, Blob, vector store).

## **Idempotency**
Repeated identical requests must yield identical results (e.g., for `/generate`).

## **Correlation ID**
UUID that traces a single request across microservices and logs.

## **Event**
System-generated signal that something changed:
- SchemaParsed
- MappingUpdated
- EmbeddingsRebuilt
- ValidationCompleted  
Used in Phase 2–4 pipelines.

## **Async Pipeline**
Event-driven analysis (schema clusters, mapping clusters, canonical synthesis).

## **API Gateway**
Entry point routing client requests to microservices.

## **Shared Library**
Reusable code in `/backend/shared` or `/frontend/components`.

---

# 5. Frontend (UI/UX) Terms

## **Module Shell**
The container component for each Studio.

## **Zustand Slice**
A self-contained state store:
- transformation slice  
- mapping slice  
- analysis slice  
- modernization slice  
- playground slice  

## **Smart Component**
A component that reads/writes store state.

## **Pure Component**
A visual, stateless component with no side effects.

## **Step Flow**
Transformation Studio’s 6-step process:
1. Upload schema  
2. Sample data  
3. Resource model  
4. Mapping  
5. Validation  
6. Export  

## **Side Panel / Drawer**
Used for:
- AI suggestions  
- detail views  
- variable editing  

## **AI Panel**
Collapsible sidebar where AI suggestions appear.

---

# 6. Governance & Lifecycle Terms

## **Architecture Guardrail**
A rule that must not be violated:
- services are independent  
- AI cannot auto-apply mappings  
- PHI cannot be stored  
- UI cannot bypass orchestrator  

## **Release Train**
Scheduled release model (monthly/quarterly).

## **Semantic Versioning**
Platform versioning:
- MAJOR.MINOR.PATCH  
Example: `v2.2.0`

## **CI/CD Pipeline**
Automation for:
- tests  
- builds  
- security scanning  
- deployment  

## **Blue-Green Deployment**
Release strategy with two parallel environments ensuring zero downtime.

## **Modernization Roadmap**
Enterprise-level transformation plan for legacy uplift.

---

# END OF GLOSSARY v2.2
