
# Synapxe FHIR Transformation Suite
## Data Model v2.2 (Ultra Detailed)
### PART 1 — Introduction, Principles, and High-Level ERD

---

## 1. Purpose

The Data Model v2.2 defines all Phase 1 entities for the Transformation Studio.
It is designed for:
- Backend developers building microservices
- DB architects designing PostgreSQL schemas
- Analysts and testers validating workflow logic
- Future Phase 2/3/4 systems (Analytics, Insights, Modernization)

This is an ULTRA DETAILED version (40–60 pages equivalent).

---

## 2. Design Principles

### 2.1 Semantic Normalization
Each core concept is represented by a dedicated table:
- SchemaDefinition
- SchemaField
- SampleDataSet
- ResourceModel
- MappingConfig & MappingRule
- ProfileDefinition
- ValidationRun
- ExecutionRun
- ExportPackage

### 2.2 Versioned Entities
The following entities are versioned:
- SchemaDefinition
- MappingConfig
- ProfileDefinition

### 2.3 Explicit “Not Mapped” Tracking
Unmapped fields must be explicitly tracked with reason:
- `FieldStatusNotes.Decision`
- `FieldStatusNotes.Remark`

### 2.4 Future-Proofing
The model supports:
- Analytics (Phase 2)
- AI ML features (Phase 3)
- Canonical Modernization (Phase 4)

WITHOUT altering Phase 1 entities.

---

## 3. High-Level ER Diagram (Text)

```
User 1--* Project 1--* Category 1--* ApiDefinition
                                   |
                                   +--* Schemas 1--* SchemaFields
                                   |
                                   +--* SampleDataSets
                                   |
                                   +--1 ResourceModel 1--* ResourceModelLinks
                                   |
                                   +--* MappingConfigs 1--* MappingRules
                                   |                      |
                                   |                      +--* FieldStatusNotes
                                   |
                                   +--* Profiles
                                   |
                                   +--* ValidationRuns
                                   |
                                   +--* ExecutionRuns
                                   |
                                   +--* ExportPackages
```

---

## 4. Base Tables — Users + Auth

### 4.1 Users Table

| Column | Type | Notes |
|--------|------|--------|
| UserId | uuid | PK |
| Email | varchar(256) | UNIQUE |
| DisplayName | varchar(256) |  |
| Role | varchar(32) | user/admin |
| IsActive | boolean | default true |
| CreatedAt | timestamptz |  |
| LastLoginAt | timestamptz |  |

Users are created automatically during OTP verification.

### 4.2 OTP Tokens (optional table)

If not using Redis, DB schema:

| Column | Type |
|--------|------|
| UserOtpTokenId | uuid |
| Email | varchar |
| OtpHash | varchar |
| ExpiresAt | timestamptz |
| CreatedAt | timestamptz |
| ConsumedAt | timestamptz |

---

## 5. Core Project Hierarchy

### 5.1 Projects
```
Projects(ProjectId, Name, Code, Description, OwnerUserId, Status)
```

### 5.2 Categories
```
Categories(CategoryId, ProjectId, Name, Code, Description, OwnerTeam)
```

### 5.3 ApiDefinition
```
Apis(ApiId, CategoryId, Name, Code, Description, Direction, LegacySystemName)
```

Next chunk will cover:
- Schemas + SchemaFields (full details)
- SampleDataSet
- ResourceModel + Links
- MappingConfig + MappingRules
- FieldStatusNotes
- Profiles
- Validation + Execution
- ExportPackage

---

# PART 2 — Schema & Schema Fields (Ultra Detailed)

## 6. SchemaDefinition Entity (Deep Specification)

A SchemaDefinition represents **one uploaded legacy schema** version for an API.
Supports formats:
- CSV column definitions
- XML or XSD structure
- JSON Schema
- Custom proprietary formats (Phase 2 extension)

### 6.1 Table Structure — Schemas

| Column           | Type         | Notes |
|------------------|--------------|-------|
| SchemaId         | uuid         | PK |
| ApiId            | uuid         | FK → Apis(ApiId) |
| Version          | int          | Starts at 1 per ApiId |
| SourceFormat     | varchar(32)  | CSV/XML/XSD/JSON |
| OriginalFileName | varchar(512) | Stored for audit |
| StorageLocation  | varchar(1024)| Path/URL to object storage |
| ParsedAt         | timestamptz  | Time parsing completed |
| CreatedBy        | uuid         | User reference |
| CreatedAt        | timestamptz  |  |
| IsActive         | boolean      | Only one active per API |

### 6.2 Versioning Rules

- First uploaded schema → Version = 1
- Each re-upload increments version
- Exactly one version marked `IsActive = true`
- Older versions preserved for diff & audit

### 6.3 Supported Formats (Phase 1)

#### 6.3.1 CSV Schema Support
- User uploads CSV definition file (NOT data)
- Required columns may include:
  - “FieldName”
  - “Description”
  - “Type”
  - “Length”
- System interprets each CSV row as SchemaField.

#### 6.3.2 XML/XSD Support
- XSD preferred, XML auto-inferred when possible
- Extracts:
  - Element name
  - Cardinality (minOccurs/maxOccurs)
  - Type
  - Description (annotation/documentation)

#### 6.3.3 JSON Schema Support
- Parse draft-07 compatible JSON schemas
- Extract:
  - jsonPath
  - type
  - description
  - required arrays

---

## 7. SchemaField Entity (Deep Specification)

SchemaField represents **one field** in the parsed schema.

### 7.1 Table Structure — SchemaFields

| Column         | Type         | Notes |
|----------------|--------------|-------|
| SchemaFieldId  | uuid         | PK |
| SchemaId       | uuid         | FK |
| ParentFieldId  | uuid         | For nested XML/JSON |
| Name           | varchar(256) | Raw field name |
| Path           | varchar(1024)| Canonical path |
| DataType       | varchar(64)  | Normalized type |
| RawType        | varchar(128) | Original type |
| Description    | text         | Extracted or AI-enhanced |
| IsRequired     | boolean      | Derived or null |
| Cardinality    | varchar(16)  | e.g., 0..1 or 0..* |
| SourceGroup    | varchar(128) | Header/body grouping |
| OrderIndex     | int          | UI ordering |
| CreatedAt      | timestamptz  |  |

### 7.2 Path Rules

- CSV uses `"Root.FieldName"`
- XML uses XPath-like `"Event/Header/FieldName"`
- JSON uses `"root.data.fieldName"`

The Path is **always stable** and does not change after creation.

### 7.3 Hierarchy Rules

Parent-child hierarchy determined by format:
- CSV → flat
- XML/XSD → nested by elements
- JSON Schema → nested via `properties`

### 7.4 AI Enhancements (Optional Phase 1)

AI can:
- Improve descriptions
- Identify semantic categories (ID fields, names, dates)
- Suggest grouping

This metadata is stored externally (Phase 2+).

---

## 8. Schema Diffing

When a new schema is uploaded:
- Compare by Path:
  - New fields → highlight “added”
  - Removed fields → highlight “removed”
  - Changed types → highlight “modified”
- UI displays a color-coded diff tree
- Mapping screen warns of impacted fields

---

## 9. Sample Data (Full Specification)

SampleDataSet represents example payloads used in mapping and validation.

### 9.1 Table — SampleDataSets

| Column       | Type         | Notes |
|--------------|--------------|-------|
| SampleId     | uuid         | PK |
| ApiId        | uuid         | FK |
| Label        | varchar(256) | User-friendly name |
| ContentType  | varchar(64)  | application/json or application/xml |
| Payload      | text         | Raw payload |
| Source       | varchar(32)  | UserProvided / AIGenerated |
| CreatedBy    | uuid         | FK |
| CreatedAt    | timestamptz  |  |

### 9.2 Multi-Sample Strategy

Users can provide:
- Typical example
- Edge case
- Null/missing fields scenario
- Bulk scenario (Phase 2+)

Validation will be run on each sample individually.

### 9.3 AI-Generated Sample Metadata

For AI-generated samples:
- Store classification hints:
  - “Realistic typical”
  - “Edge-case: missing optional fields”
  - “Edge-case: extreme values”

### 9.4 Storage Strategy

Samples stored directly in DB as text.
Large examples (>1 MB) may be offloaded (Phase 2 extension).

---

## Next Chunk (Part 3) Will Include:

- ResourceModel + Links (deep)
- MappingConfig model (deep)
- MappingRule (transformations, conditions, functions)
- FieldStatusNotes (NotMapped with remark)
- ProfileDefinition (FHIR StructureDefinition)
- ValidationRun & ExecutionRun
- ExportPackage
