# Coding Standards
Synapxe FHIR Studio — Wave 4

## 1. Backend (.NET 8) Standards
### 1.1 Architecture
- Follow Clean Architecture
- No circular dependencies
- All services stateless

### 1.2 Naming Conventions
- Classes: PascalCase
- Methods: PascalCase
- Variables: camelCase
- DTOs end with `Dto`
- Async methods end with `Async`

### 1.3 Error Handling
- Use ProblemDetails
- No raw exceptions returned
- Always include correlation ID

### 1.4 Logging
- Structured logs only
- No PHI logged
- Use Serilog enrichers

---

## 2. Frontend (React + TypeScript) Standards
### 2.1 Component Rules
- Use functional components only
- Use hooks for stateful logic
- Use Zustand slices for global state
- Keep components pure

### 2.2 Naming Conventions
- Components: PascalCase
- Hooks: useCamelCase
- Zustand slices: <feature>Slice.ts

### 2.3 UI Rules
- Reusable components must reside under /components
- No inline styles — use Tailwind
- Avoid deeply nested state

---

## 3. AI Prompt Standards
### 3.1 Prompt Template Rules
- Fixed format
- Clear instruction hierarchy
- Include guardrails
- Avoid ambiguity

### 3.2 Token Efficiency
- Prefer compressed context
- Use embeddings for large lookups

