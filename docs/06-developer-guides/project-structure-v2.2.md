# Project Structure v2.2  
Synapxe FHIR Studio — Wave 4

This document defines the **official monorepo structure** for the Synapxe FHIR Studio.  
It ensures consistent organization across backend microservices, frontend apps, shared libraries, infrastructure, and documentation.

---

# 1. Monorepo Philosophy

The entire platform is stored in a **single repository** to support:

- Shared code between services  
- Consistent versioning  
- Unified CI/CD  
- Enforced architecture rules  
- Faster cross-module updates  
- Standardized developer onboarding  

Each microservice remains **independent and stateless**, but the repo itself is unified.

---

# 2. Top-Level Directory Layout

```
/synapxe-fhir-studio
│
├── backend/
├── frontend/
├── shared/
├── infrastructure/
├── docs/
└── tools/
```

---

# 3. Backend Directory Structure

Each microservice lives under `/backend/services`.

```
backend/
  services/
    schema-parser-service/
      src/
      tests/
      Dockerfile
      README.md

    sample-generator-service/
    mapping-engine-service/
    validation-service/
    export-service/

    mapping-service/
    template-service/
    variable-service/

    schema-analysis-service/
    mapping-analysis-service/
    profile-analysis-service/
    pattern-analysis-service/
    insight-service/
    report-service/

    canonical-model-service/
    canonical-profile-service/
    modernization-impact-service/
    modernization-planner-service/
    modernization-ai-service/
    modernization-report-service/

    playground-engine-service/
    playground-validator-service/
    playground-ai-service/
    playground-export-service/

  shared/
    fhir/
    utils/
    common-libs/
    domain/
```

### Backend Guidelines

- Each service **must have**:
  - `src/`
  - `tests/`
  - its own `README.md`
  - its own `Dockerfile`
- No service depends on another service’s domain model
- Shared logic goes into `/backend/shared/`

---

# 4. Frontend Directory Structure

```
frontend/
  app-shell/
  modules/
    transformation/
    mapping/
    analysis/
    modernization/
    playground/
  components/
  stores/
  assets/
  utils/
  types/
  api/
```

### Module Rules

Each module is isolated under `/modules/<studio>`:

```
frontend/modules/transformation/
  pages/
  components/
  hooks/
  stores/
  services/
```

- Shared components go into `/components/`
- Shared API clients go into `/api/`
- Shared Zustand slices belong in `/stores/`

---

# 5. Shared Libraries

```
shared/
  fhir/
    - StructureDefinition helpers
    - FHIRPath utilities
    - Bundle utilities
  mapping/
    - expression engine
    - common transforms
  ai/
    - orchestrator client
    - prompt registry
  core/
    - logging
    - configuration
    - common exceptions
```

### Shared Code Principles

- No circular dependencies  
- Shared libs must contain **pure logic only**  
- No environment-specific configs  
- Strict versioned exports  

---

# 6. Infrastructure

```
infrastructure/
  api-gateway/
  docker/
  terraform/
  helm/
  scripts/
  ci-cd/
```

### Responsibilities

- API gateway routes  
- Container base images  
- K8s manifests  
- Terraform/Helm for infra provisioning  
- CI/CD workflows (GitHub Actions)

---

# 7. Documentation

```
docs/
  00-overview/
  01-core-architecture/
  02-module-specifications/
  03-ui-specs/
  04-backend-apis/
  05-ai-specs/
  06-developer-guides/
  07-appendices/
```

### Documentation Rules

- Every microservice README must link to relevant docs  
- Architecture diagrams stored under `/docs/diagrams`  
- Specs versioned by `vX.Y`  

---

# 8. Tools Folder

```
tools/
  dev-scripts/
  db/
  schema-extractors/
  lint/
  build/
```

Use this for non-production utilities:
- Local DB seeding  
- Schema extraction from XML/XSD  
- Mapping analysis CLI tools  
- AI batch processors  

---

# 9. Naming Conventions

### Backend Services
```
<function>-service
e.g. mapping-engine-service, schema-parser-service
```

### Frontend Modules
```
<module-name>
e.g. transformation, mapping, analysis
```

### Shared Libraries
```
<domain>-utils, <domain>-core
```

### Branches
```
feature/<name>
fix/<name>
chore/<name>
```

---

# 10. Versioning Strategy

- Monorepo version uses semantic versioning: `v2.2.x`
- Each microservice has its own internal version in `package.json` or `.csproj`
- Breaking changes require incrementing:
  - documentation version  
  - API version  
  - service major version  

---

# END OF PROJECT STRUCTURE v2.2
