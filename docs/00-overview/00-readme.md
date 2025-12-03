# Synapxe FHIR Studio — Repository Overview  
Wave 4 Expanded Edition (v2.2)

Synapxe FHIR Studio is an end-to-end platform for transforming legacy healthcare data into standardized FHIR representations, generating modernization insights, and synthesizing canonical models for enterprise uplift.

This README serves as the **entry point** for developers, architects, contributors, and integrators.

---

# 1. What is Synapxe FHIR Studio?

A unified ecosystem that provides:

- **Transformation Studio:** Convert legacy schemas → sample data → mapping → FHIR bundles  
- **Mapping Studio:** Visual mapping tool with AI suggestions  
- **Analysis Studio:** Detect duplicates, clusters, deltas, and patterns  
- **Drift & Insight Engine:** Cross-API drift detection + semantic search  
- **Modernization Studio:** Canonical model + modernization roadmap generation  
- **Developer Playground:** Sandbox to run mappings, validations, and API experiments  

The system is built around **20+ stateless microservices**, a **React/TS frontend**, and a hybrid **AI RAG pipeline**.

---

# 2. High-Level Architecture (Mermaid Diagram)

```mermaid
flowchart TD

A[Legacy Schema<br>(CSV/XML/JSON/XSD)] --> B[Transformation Studio]
B --> C[Mapping Studio]
C --> D[Validation Engine]
D --> E[FHIR Output Bundle]

E --> F[Analysis Studio<br>(Clusters, Deltas, Patterns)]
F --> G[Drift & Insight Engine<br>(Semantic Search, Impact Model)]
G --> H[Modernization Studio<br>(Canonical Model & Roadmap)]
```

---

# 3. Repository Structure

```
/synapxe-fhir-studio
│
├── backend/                 # All microservices (.NET 8)
├── frontend/                # React + Zustand app
├── shared/                  # Shared libs for FHIR, mapping, utils
├── infrastructure/          # API gateway, Terraform, CI/CD
├── docs/                    # Full documentation set
└── tools/                   # Developer utilities, CLI tools
```

---

# 4. Quick Start (Developers)

## 4.1 Clone & Install

```
git clone <repo>
cd synapxe-fhir-studio

npm install
dotnet restore
docker compose up -d
```

## 4.2 Run Frontend

```
npm run dev
```

## 4.3 Run Backend Services

```
cd backend/services/<service-name>
dotnet run
```

## 4.4 Run Tests

```
npm test
dotnet test
```

---

# 5. Documentation Index

Documentation lives under `/docs`:

```
00-overview/  
01-core-architecture/  
02-module-specifications/  
03-ui-specs/  
04-backend-apis/  
05-ai-specs/  
06-developer-guides/  
07-appendices/
```

### Highlights:

- **Core Architecture:** microservices, workflows, state machines  
- **Module Specs:** transformation, mapping, analysis, modernization  
- **UI Specs:** navigation, wireframes, component diagrams  
- **AI Specs:** prompts, embeddings, guardrails  
- **Developer Guides:** project structure, testing, onboarding  

---

# 6. How the Studios Fit Together

```
Transformation Studio → Analysis Studio → Drift Engine → Modernization Studio
```

Each phase builds on the previous output:

- Phase 1: Reliable schema → mapping → FHIR  
- Phase 2: Insight discovery  
- Phase 3: Drift detection + semantic intelligence  
- Phase 4: Canonical synthesis + modernization roadmap  

---

# 7. Contribution Workflow

```
git checkout -b feature/<name>
git commit -m "feat: ..."
git push
open PR to main
```

- All PRs require automated checks  
- All mapping/profile changes must include test coverage  
- Architectural changes must align with guardrails  

---

# 8. Versioning

- Platform uses **semantic versioning**: `v2.2.x`  
- Services versioned individually  
- Breaking changes require:
  - Architecture review  
  - Doc updates  
  - Service version bump  

---

# 9. Security Requirements

- No real PHI in development  
- All logs must be masked  
- AI must only process synthetic/truncated data  
- JWT with short-lived tokens  
- Secure-by-default microservices  

---

# 10. Release Process

- Automated CI/CD with blue-green deployment  
- Release notes generated per version  
- Integration tests must pass before publish  

---

# 11. Links to Key Documents

- `/docs/01-core-architecture/core-architecture-v2.2.md`  
- `/docs/01-core-architecture/microservice-architecture-v2.2.md`  
- `/docs/02-module-specifications/transformation-studio-spec-v2.2.md`  
- `/docs/03-ui-specs/component-architecture-v2.2.md`  
- `/docs/05-ai-specs/ai-prompts-and-guardrails-v2.2.md`  

---

# END OF README (Wave 4 Expanded Edition)
