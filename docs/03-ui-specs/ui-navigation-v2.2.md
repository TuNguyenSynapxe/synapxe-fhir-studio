# UI Navigation Specification v2.2  
Synapxe FHIR Studio — Wave 3

---

# 1. Purpose

This document defines the **navigation architecture** for Synapxe FHIR Studio.  
It ensures:
- Consistency across all modules  
- Predictable navigation behavior  
- Seamless transitions across Transformation, Mapping, Analysis, Modernization, and Playground  
- Microfrontend readiness  
- Optimal user experience for developers, architects, analysts, and testers  

---

# 2. Navigation Principles

### 2.1 Predictability  
All modules follow consistent navigation patterns: sidebars, steppers, tabs, and breadcrumbs.

### 2.2 Microfrontend Isolation  
Each module can run independently, but the platform supports integrated navigation.

### 2.3 State Persistence  
The app remembers:
- last module  
- last opened workspace  
- last opened schema/mapping/profile  

### 2.4 Low Cognitive Load  
Navigation avoids deep nesting; everything is 1–2 clicks away.

---

# 3. Global Navigation (Top-Level Menu)

```text
┌──────────────────────────────────────────────┐
│ Dashboard | Transformation | Mapping | Analysis │
│ Modernization | Playground | Settings | Profile │
└──────────────────────────────────────────────┘
```

### Modules:
1. **Dashboard**
2. **Transformation Studio**
3. **Mapping Studio**
4. **Analysis Studio**
5. **Modernization Studio**
6. **Developer Playground**
7. **Settings**
8. **Profile**

---

# 4. Module-Level Navigation

Each Studio has its own sidebar navigation.

---

# 4.1 Transformation Studio Navigation

```
Transformation Studio
│
├── Step 1: Upload Schema
├── Step 2: AI Sample Data
├── Step 3: Resource Model Selection
├── Step 4: Mapping Workspace
├── Step 5: Validation
└── Step 6: Export / Publish
```

Navigation Type: **Stepper + Sidebar hybrid**

Rules:
- User may jump backward freely  
- User may only jump forward if prerequisite steps are complete  
- Steps show status: Pending | In-progress | Completed  

---

# 4.2 Mapping Studio Navigation

```
Mapping Studio
│
├── Mapping Workspace
├── Source Preview
├── Variable Manager
├── FHIR Template Viewer
└── AI Assist Panel
```

Rules:
- Users may navigate freely between mapping components  
- FHIR Tree always available  
- Works as standalone or embedded microfrontend  

---

# 4.3 Analysis Studio Navigation

```
Analysis Studio
│
├── Schema Clusters
├── Mapping Clusters
├── Profile Deltas
├── Pattern Analysis
├── Insight Dashboard
└── Canonical Recommendations
```

Rules:
- Tabs within each view for advanced visualizations  
- Modules must support large dataset visualisation  

---

# 4.4 Modernization Studio Navigation

```
Modernization Studio
│
├── Canonical Model Explorer
├── Impact Dashboard
├── Consolidation Map
├── Modernization Planner
└── Roadmap Viewer
```

Rules:
- State machine gating:  
  - Canonical Model must exist before Planner  
  - Impact must be run before Roadmap  

---

# 4.5 Developer Playground Navigation

```
Developer Playground
│
├── Mapping Test
├── Validation Test
├── Schema Explorer
├── AI Assistant
└── Canonical Test
```

Rules:
- Completely free-form, no workflow restrictions  
- Multi-tab support (open multiple samples/mappings at once)  

---

# 5. Cross-Module Navigation

### Supported Quick Actions:
- **Send to Mapping Studio**
- **Send to Analysis Studio**
- **Open in Playground**
- **Export as ZIP**
- **Duplicate to New Workspace**

### Example:
From Transformation → “Send to Mapping Studio” opens Mapping Studio with:
- mapping.json  
- fhir-template.json  
- sample.json  

pre-loaded.

---

# 6. Breadcrumb Pattern

Breadcrumb format:

```
Transformation Studio / Mapping Workspace / Field Detail
```

Rules:
- Max 3 levels  
- No deep nesting  
- Reflects microfrontend route isolation  

---

# 7. Navigation State Model

Navigation components supported:

### 7.1 Sidebar  
Module-level navigation.

### 7.2 Stepper  
Transformation Studio only.

### 7.3 Tabs  
Used in:
- Analysis Studio  
- Playground  

### 7.4 Microfrontend Route Boundaries  
Routes must be isolated at `/ts`, `/ms`, `/as`, `/mod`, `/pg`.

Example:
```
/ts/step-2
/ms/workspace
/as/schema-clusters
/mod/impact
/pg/mapping-test
```

---

# 8. Authentication Redirect Logic

### First-Time Login
Redirect → **Transformation Studio Step 1**

### Returning User
Redirect → **last active module and workspace**

### Session Timeout
Redirect → Login → Return to previous module.

---

# 9. Responsive & Mobile Behavior

Minimal mobile support, but responsive rules:
- Sidebar collapses to drawer  
- Steppers become horizontal scroll  
- Tables become card-based stacking  
- Mapping Workspace collapses secondary panels  

---

# END OF UI NAVIGATION SPEC v2.2
