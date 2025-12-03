# UI Wireframes Specification v2.2  
Synapxe FHIR Studio — Wave 3

---

# 1. Purpose  
These wireframes define low-fidelity visual layouts for all major screens in the Synapxe FHIR Studio.  
They act as the baseline for:
- UX discussions  
- Frontend development  
- Component architecture  
- Consistency across modules  

This document uses:
- ASCII/Figma-style mockups  
- Mermaid diagrams for flows & layout relations  
- Structural annotations  

---

# 2. Wireframe Standards  

### 2.1 Visual Style  
- Monochrome (grey blocks)  
- High-level grouping only  
- No pixel-level precision  
- Emphasis on layout & interaction  

### 2.2 Notation  
```
[ Panel ]         → major UI block  
--- section ---   → separated section  
| element |       → list/table elements  
```

### 2.3 Mermaid  
Used for relationships between panels.

---

# 3. Transformation Studio Wireframes

---

## 3.1 Stepper Overview

```
+---------------------------------------------------------+
| TRANSFORMATION STUDIO                                   |
+---------------------------------------------------------+
| Stepper: 1 Upload → 2 Sample → 3 Resource → 4 Mapping → |
|           5 Validation → 6 Export                       |
+---------------------------------------------------------+
```

---

## 3.2 Step 1 — Upload Schema

```
+-----------------------------------------------------------+
| Step 1: Upload Schema                                     |
+-----------------------------------------------------------+
| [ Upload CSV ]  [ Upload XML ] [ Upload JSON ] [ Upload XSD ] |
|                                                       |
| Uploaded Schema Preview                               |
| ----------------------------------------------------- |
| fieldName       | type       | description            |
| ...             | ...        | ...                    |
+-----------------------------------------------------------+
```

---

## 3.3 Step 2 — AI Sample Data

```
+-----------------------------------------------------------+
| Step 2: AI Sample Data                                     |
+-----------------------------------------------------------+
| [ Generate Sample Data ] [ Regenerate ] [ Edit JSON ]     |
|                                                           |
| Sample JSON Editor                                        |
| --------------------------------------------------------- |
| {                                                         |
|   "patientName": "John Tan",                              |
|   "dob": "1985-03-10"                                     |
| }                                                         |
|                                                           |
| Right Panel: AI Suggestions                               |
+-----------------------------------------------------------+
```

---

## 3.4 Step 3 — Resource Model Selection

```
+-----------------------------------------------------------+
| Step 3: Select Resource Model                             |
+-----------------------------------------------------------+
| [ Encounter ] [ Patient ] [ Observation ] [ Questionnaire ] |
|                                                           |
| Selected Resource: Encounter                              |
| --------------------------------------------------------- |
| Element Tree                                              |
| Encounter                                                 |
| ├── status                                                |
| ├── subject (Reference)                                   |
| └── period.start                                          |
+-----------------------------------------------------------+
```

---

## 3.5 Step 4 — Mapping Workspace

```
+-----------------------------------------------------------+
| TRANSFORMATION STUDIO — STEP 4: MAPPING WORKSPACE         |
+-----------------------------------------------------------+
| Left: Source JSON                                         |
| --------------------------------------------------------- |
| { "event": { "date": "2025-01-01", "type": "Appt" }}      |
|                                                           |
| Center: Mapping Rules Grid                                |
| --------------------------------------------------------- |
| fieldPath           | transform           | status        |
| event.date          | toFHIRDateTime()    | mapped        |
| event.type          | mapEventType()      | mapped        |
| event.code          |                     | unmapped      |
|                                                           |
| Right: FHIR Tree                                          |
| --------------------------------------------------------- |
| Encounter                                                |
|  ├─ status                                               |
|  ├─ class                                                |
|  └─ period.start                                         |
+-----------------------------------------------------------+
| Bottom: AI Suggestions Panel                              |
| --------------------------------------------------------- |
| "Recommended mapping: event.code → Encounter.type"        |
+-----------------------------------------------------------+
```

---

## 3.6 Step 5 — Validation Workspace

```
+-----------------------------------------------------------+
| Step 5: Validation                                        |
+-----------------------------------------------------------+
| Left: Bundle Output                                       |
| Center: Validation Messages                               |
| Right: AI Explanation                                     |
+-----------------------------------------------------------+
```

---

## 3.7 Step 6 — Export / Publish

```
+-----------------------------------------------------------+
| Step 6: Export                                            |
+-----------------------------------------------------------+
| [ Download Mapping JSON ]                                 |
| [ Download Sample JSON ]                                   |
| [ Download FHIR Bundle ]                                   |
| [ Publish to Repo ]                                        |
+-----------------------------------------------------------+
```

---

# 4. Mapping Studio Wireframes

---

## 4.1 Mapping Workspace Layout

```
+-----------------------------------------------------------+
| MAPPING STUDIO                                             |
+-----------------------------------------------------------+
| Left: Source Panel  | Middle: Mapping Rules | Right: FHIR |
+-----------------------------------------------------------+
```

---

## 4.2 AI Assist Panel

```
+-----------------------------------------------------------+
| AI Assist                                                 |
| --------------------------------------------------------- |
| "Suggested FHIR Path: Encounter.class"                    |
| "Suggested Transform: normalizeString()"                  |
+-----------------------------------------------------------+
```

---

# 5. Analysis Studio Wireframes

---

## 5.1 Schema Cluster View

```
+-----------------------------------------------------------+
| Schema Cluster Dashboard                                   |
+-----------------------------------------------------------+
| Cluster A: 14 APIs                                         |
| Cluster B: 3 APIs                                          |
| Cluster C: 9 APIs                                          |
+-----------------------------------------------------------+
```

---

## 5.2 Profile Delta Explorer

```
+-----------------------------------------------------------+
| Profile Delta Explorer                                     |
+-----------------------------------------------------------+
| Encounter Profile                                           |
|  - status min: 0 → 1                                        |
|  - class coding change                                      |
+-----------------------------------------------------------+
```

---

# 6. Modernization Studio Wireframes

---

## 6.1 Canonical Model Explorer

```
+-----------------------------------------------------------+
| Canonical Schema                                           |
+-----------------------------------------------------------+
| Field        | Type          | Source Fields               |
| --------------------------------------------------------- |
| patientName  | HumanName     | name, full_name, pt_name    |
+-----------------------------------------------------------+
```

---

## 6.2 Impact Dashboard

```
+-----------------------------------------------------------+
| Modernization Impact Dashboard                             |
+-----------------------------------------------------------+
| - High-risk APIs (7)                                       |
| - Medium-risk APIs (12)                                    |
| - Low-risk APIs (20)                                       |
+-----------------------------------------------------------+
```

---

# 7. Developer Playground Wireframes

---

## 7.1 Mapping Test Workspace

```
+-----------------------------------------------------------+
| Sample JSON | Mapping Rules | FHIR Output | Validation    |
+-----------------------------------------------------------+
```

---

## 7.4 AI Assistant Chat Panel

```
+-----------------------------------------------------------+
| AI Assistant                                               |
| --------------------------------------------------------- |
| User: "Why is period.start invalid?"                      |
| AI: "The value must be a dateTime with timezone."         |
+-----------------------------------------------------------+
```

---

# 8. Cross-Module Wireframes

---

## Send to Mapping Studio

```
Upload Completed → [ Send to Mapping Studio ] → Preloaded Mapping Workspace
```

---

# END OF UI WIREFRAMES SPEC v2.2
