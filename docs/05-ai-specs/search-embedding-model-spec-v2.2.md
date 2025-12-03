# Search & Embedding Model Specification v2.2  
Synapxe FHIR Studio — Wave 3

---

# 1. Purpose

This specification defines the semantic search + embedding architecture that powers:

- Field similarity detection  
- Mapping recommendations  
- Profile comparison  
- Canonical consolidation  
- Global cross-API search  
- AI grounding (RAG) across all Synapxe FHIR Studio modules  

It is the **semantic backbone** across all Studios: Transformation, Mapping, Analysis, Modernization, and Playground.

---

# 2. Vector Store Architecture

We use a multi-collection vector store with typed metadata.

## 2.1 Collections

```
fields
mappings
profiles
patterns
canonical
fhir-spec
```

## 2.2 Storage Backends

Preferred:
- **pgvector** (Phase 1–3)
- Milvus (Phase 4, heavy workloads)

Similarity metric:
- cosine

Hybrid search (BM25 + embeddings) supported.

---

# 3. Embedding Models

### Default model (cloud)
- **OpenAI text-embedding-3-large** (3072 dims)  
High accuracy for schema/mapping semantics.

### Secondary model
- **text-embedding-3-small** for cost-optimized batch jobs.

### On-prem fallback (Phase 4)
- **BGE-small-en-v1.5**
- **Instructor-xl**

These support transformation & modernization clusters.

---

# 4. Embedding Types & Metadata

## 4.1 Field Embedding

```json
{
  "type": "field",
  "vector": [...],
  "metadata": {
    "schemaId": "uuid",
    "fieldPath": "event.date",
    "fieldName": "date",
    "fieldType": "string",
    "description": "Event date",
    "sampleValues": ["2024-01-01"]
  }
}
```

Targets:
- mapping suggestions  
- schema clustering  
- canonical consolidation  

---

## 4.2 Mapping Embedding

```json
{
  "type": "mapping",
  "vector": [...],
  "metadata": {
    "mappingId": "uuid",
    "sourceField": "event.date",
    "fhirPath": "Encounter.period.start",
    "transform": "toFHIRDateTime()"
  }
}
```

Used for:
- drift detection  
- reusable mapping discovery  
- cross-API mapping conflicts  

---

## 4.3 Profile Embedding

```json
{
  "type": "profile",
  "vector": [...],
  "metadata": {
    "profileId": "uuid",
    "resource": "Encounter",
    "elements": ["status", "period.start"]
  }
}
```

Used for:
- profile diff  
- profile clustering  
- modernization recommendations  

---

## 4.4 Pattern Embedding

Captured from:
- AST of transform expressions  
- Normalized expression patterns  

Used for:
- pattern reuse  
- smell detection  
- modernization candidate detection  

---

# 5. Search APIs (search-service)

## 5.1 Search Fields

```
POST /search/fields
{
  "query": "appointment date",
  "filters": { "schemaId": "uuid" }
}
```

Response:
```json
[
  {
    "fieldPath": "event.date",
    "score": 0.91
  }
]
```

---

## 5.2 Search FHIR Paths

```
POST /search/fhir-paths
{
  "query": "visit date"
}
```

Returns ranked FHIR paths:
- Encounter.period.start  
- Encounter.meta.lastUpdated  
- Observation.effectiveDateTime  

---

## 5.3 Search Mappings

Find mappings used in similar contexts across APIs.

---

# 6. Indexing & Refresh Rules

Triggered by events.

### Schema updated →  
- rebuild field embeddings  
- rebuild schema clusters  

### Mapping updated →  
- rebuild mapping embeddings  
- update mapping patterns  

### Profile updated →  
- rebuild profile embeddings  

### Canonical updated →  
- rebuild canonical embeddings  

Events:
```
FieldEmbeddingsRebuilt
MappingEmbeddingsRebuilt
ProfileEmbeddingsRebuilt
CanonicalEmbeddingsRebuilt
```

---

# 7. Query Scenarios

### 7.1 Suggest FHIR element (Transformation Studio)
- search fields  
- search FHIR paths  
- combine results with resource model constraints  

### 7.2 Detect inconsistent mappings (Analysis Studio)
- retrieve mappings based on semantic similarity  
- detect conflicting FHIR paths  

### 7.3 Canonical merge candidates (Modernization Studio)
Similarity > 0.8 = candidate for canonical consolidation.

### 7.4 Recommend canonical field name  
Based on centroid of cluster + HL7 spec reference.

### 7.5 Pattern reuse (Transformation / Playground)
Find similar transformation rules for reuse.

---

# 8. RAG Integration

```
retrieved = vectorStore.search(query)
prompt = {
  "context": retrieved,
  "task": "mapping-suggest",
  "userPrompt": originalQuery
}
```

RAG used by:
- AI mapping suggestions  
- Error explanation  
- Canonical suggestions  
- Modernization synthesis  

---

# 9. PHI Guardrails for Search

Rules:
- No raw sample data stored  
- Mask sample values before embedding  
- NRIC/contact/address masked  
- Logs must NOT contain embedded text  
- Only summaries stored  

---

# 10. On-Prem Model Readiness

Supports full offline mode:

- Local embedding model  
- Local vector store  
- No cloud dependency  
- Hot-swappable via AI Orchestrator  

---

# END OF SEARCH & EMBEDDING MODEL SPEC v2.2
