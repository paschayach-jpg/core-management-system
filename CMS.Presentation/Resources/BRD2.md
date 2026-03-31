# Business Requirements Document (BRD)
## Core Management System (CMS)

---

## Document Control

| Version | Date | Author | Description |
|---------|------|--------|-------------|
| 1.0 | March 20, 2026 | Business Analyst | Initial BRD Creation |

## 1. Executive Summary

### 1.1 Project Overview

**Project Name:** Core Management System (CMS)  
**Project Code:** 2511AL01  
**Start Date:** March 20, 2026 
**Duration:** 10 Business Days  
**End Date:** March 30, 2026 (estimated)  
**Project Type:** Enterprise Document & Knowledge Management System

### 1.2 Business Context

As an insurance company, data accuracy and service speed are critical factors that build customer trust and confidence. The Core Management (CMS) system is designed to serve as the organization's central repository for documents, knowledge, and business processes.

This system addresses the need for:
- Centralized document storage and management
- Folder structure visualization and management
- Folder Setting and Permission
- Knowledge preservation and sharing
- Compliance and audit trail requirements

### 1.3 Project Scope

**In Scope:**
- Document and folder management system (4กล่อง)
- Folder Setting and Permission Management

**Out of Scope:**
- Core insurance system functionality
- Financial accounting modules
- Customer relationship management (CRM) features
- External customer-facing portal
- Mobile application development (initial release)

---

## 2. Stakeholders

### 2.1 Stakeholder Matrix

| Stakeholder Group | Representative | Role | Interest Level | Influence Level | Communication Frequency |
|-------------------|----------------|------|----------------|-----------------|------------------------|
| Executive Committee | CEO, CFO, COO | Sponsor/Approver | High | High | Monthly |
| Division Managers | Department Heads | Primary Users | High | High | Bi-weekly |
| Department Staff | Team Members | End Users | High | Medium | Weekly |
| IT Department | IT Manager, DBA | System Administrator | Medium | High | Daily (during implementation) |
| Compliance Team | Compliance Officer | Auditor | High | Medium | Monthly |
| HR Department | HR Manager | Employee Data Manager | Medium | Medium | Monthly |
| Project Team | PM, BA, Developers | Implementation | High | High | Daily |

### 2.2 Key Contacts

| Name | Role | Responsibilities | Contact |
|------|------|------------------|---------|
| TBD | Project Sponsor | Final approval, budget allocation, strategic direction | TBD |
| TBD | Business Owner | Requirements definition, UAT sign-off, business decisions | TBD |
| TBD | Infra Manager | Technical oversight, infrastructure support, deployment | TBD |
| TBD | BA Lead | Requirements gathering, documentation, stakeholder liaison | TBD |
| TBD | Project Manager | Timeline, resources, risk management, delivery | TBD |

### 2.3 Communication Plan

| Stakeholder | Communication Method | Frequency | Content |
|-------------|---------------------|-----------|---------|
| Executive Committee | Steering Committee Meeting | Monthly | Project status, key decisions, budget |
| Division Managers | Progress Review | Bi-weekly | Feature demos, timeline, issues |
| End Users | Newsletter, Training | Weekly | Tips, updates, announcements |
| IT Team | Stand-up, Tech Review | Daily/Weekly | Technical progress, issues, solutions |
| Compliance | Audit Review | As needed | Security, compliance, audit trail |

---

## 3. Business Requirements

### 3.1 Folder organization structure

#### BR-001: Folder organization Structure Management
**Priority:** Critical  
**Category:** Core Functionality

**Business Need:**  
The Folder requires a flexible system to maintain and visualize its structure, supporting multiple Folder views based on confidentiality levels.

**Functional Requirements:**

1. **Hierarchy Management**
   - Support 3-tier structure: Sub-Committee → Division → Department
   - Create, edit, delete Folder units at all levels
   - Support Thai language for unit names
   - Maintain parent-child relationships

2. **Multi-View Support**
   - View Folder by confidentiality level:
     - 1HC (High Confidential) → Sub-Committee → Division → Department
     - 2SC (Semi Confidential) → Sub-Committee → Division → Department
     - 3NC (Normal Confidential) → Sub-Committee → Division → Department
     - 4PC (Public Confidential) → Sub-Committee → Division → Department
   - View Folder by organization:
     - Sub-Committee → 1HC (High Confidential) → 2SC (Semi Confidential) → 3NC (Normal Confidential) → 4PC (Public Confidential)
		- Division → 1HC (High Confidential) → 2SC (Semi Confidential) → 3NC (Normal Confidential) → 4PC (Public Confidential)
			- Department → 1HC (High Confidential) → 2SC (Semi Confidential) → 3NC (Normal Confidential) → 4PC (Public Confidential)

   - Switch between confidentiality views and organization view

3. **Visualization**
   - Display in hierarchical TreeView format
   - Expand/collapse nodes
   - Search within tree structure

**Acceptance Criteria:**
- ✓ User can create/edit/delete Folder units
- ✓ Tree view displays complete hierarchy with expand/collapse
- ✓ User can switch between confidentiality views and organization view
- ✓ Changes are logged with user ID and timestamp

**Business Rules:**
- Each Department must belong to at only one Division
- Each Division must belong to at least one Sub-Committee
- Folder unit names must be unique within parent
- Deleted units are soft-deleted and retained for audit
- Only users with "Admin" role can modify structure

**Example Structure:**
```
 ├── Sub-Committee/ (คณะกรรมการความเสี่ยง)
 ├── Sub-Committee/ (คณะกรรมการสินไหม)
 │	 └── Division (ฝ่ายสินไหมทดแทน-)
 │		 ├── Department (แผนกรับแจ้ง)
 │		 ├── Department (แผนกสำรวจภัย)
 │		 └── Department (แผนกสินไหมซ่อม)
 └── Sub-Committee/ (คณะกรรมการรับประกัน)
     ├── Division (ฝ่ายรับประกันภัย)
     └── Division (ฝ่ายการตลาด)
		 ├── Department (แผนกธุรกิจ 1)
		 └── Department (แผนกธุรกิจ 2)
```

---
### 3.2 Document Management System

#### BR-002: Folder Structure Management
**Priority:** Critical  
**Category:** Core Functionality

**Business Need:**  
Organize documents in a hierarchical folder structure that mirrors the folder organizational structure and supports templates for consistency.

**Functional Requirements:**

1. **Folder Creation**
   - Create folders from predefined templates
   - Create custom folders manually
   - Support multi-level sub-folders (up to 10 levels)
   - Bulk folder creation from template
   - Copy folder structure (without files)

2. **Folder Hierarchy Display**
   - TreeView display showing:
     - Root Folders (Sub-Committee/Division/department)
     - Sub-Folders
   - Color-coded by priority level (1HC=Red, 2SC=Orange, 3NC=Blue, 4PC=Green)
   - Show folder statistics (file count, size, last updated)
   - Quick access to favorite folders

3. **Folder Operations**
   - Create, Rename, Move, Delete folders
   - Archive folders (retain but hide)
   - Restore from recycle bin (30-day retention)
   - Set folder properties and metadata
   - Folder templates management

4. **Folder Templates**
   - Define standard folder structures
   - Template for different such as departments/projects, and etc.
   - Apply template to existing structure

**Acceptance Criteria:**
- ✓ Users can create folders from templates in ≤3 clicks
- ✓ Folder tree displays complete hierarchy with color coding
- ✓ Folder operations complete within 2 seconds
- ✓ Deleted folders recoverable within 30 days
- ✓ Folder templates are reusable and editable

**Business Rules:**
- Folder names must be unique within same parent
- Maximum folder depth: 10 levels
- Folder names cannot contain: \\ / : * ? " < > |
- Deleted folders move to recycle bin for 30 days
- Empty folders can be deleted by owner or admin
- Folders with files require admin approval to delete

---

#### BR-003: File Upload and Storage
**Priority:** Critical  
**Category:** Core Functionality

**Business Need:**  
Enable users to upload and store files securely with proper access control, virus scanning, and audit trail.

**Functional Requirements:**

1. **File Upload Methods**
   - Browse and select files (multi-select up to 10 files)
   - Drag and drop interface
   - Upload progress indicator
   - Resume interrupted uploads
   - Cancel upload in progress

2. **File Storage**
   - **Physical Storage:**
     - Network Path: `\\\\bui-filecenter\\Core Management` setup to config file
     - Credentials: pmc1/Bui@12345 (RWM), pmc2/Bui@12345 (R)
     - Organized by Folder organization Structure combine with folders Structure by folder id
   
   - **Metadata Storage (Database):**
     - File ID (unique identifier)
     - Original filename
     - File size and type
     - Upload date and time
     - Uploaded by (user ID)
     - Status (active, archived, deleted)
     - Checksum for integrity

3. **File Display**
   - ListView with columns:
     - Icon (by file type)
     - Filename (with extension)
     - Size (human-readable)
     - Upload date
     - Uploaded by
     - Status indicator
     - Actions (View/Download/Delete/Share)
   - Sort by any column
   - Filter by type, date, user
   - Preview for supported types

4. **File Validation**
   - Automatic virus scan before storage
   - File type validation
   - File size limit check (100 MB max)
   - Duplicate detection
   - Corrupt file detection

**Acceptance Criteria:**
- ✓ Users can upload multiple files simultaneously
- ✓ Files stored on network drive with correct permissions
- ✓ Metadata accurately stored in database
- ✓ Upload progress displayed in real-time
- ✓ Virus-infected files rejected with notification

**Business Rules:**
- Maximum file size: 100 MB per file
- Supported file types: PDF, DOC, DOCX, XLS, XLSX, PPT, PPTX, JPG, PNG, GIF, ZIP
- Files automatically scanned for viruses
- File names sanitized (remove special characters)
- Duplicate filenames auto-renamed with timestamp
- Upload quota: 40 GB per department(Can Settings)
- Files older than 7 years moved to archive storage (Can Settings)

---

#### BR-004: Folder Settings and Security
**Priority:** Critical  
**Category:** Security & Access Control

**Business Need:**  
Control access to sensitive documents through configurable folder-level security settings based on organizational hierarchy and user roles.

**Functional Requirements:**

1. **Priority Level Configuration by default**
   - **1HC (High Confidential):**
     - Color: Red
     - Access: Executive level only
     - All actions logged
   
   - **2SC (Semi Confidential):**
     - Color: Orange
     - Access: Department head and above
     - Approval required for external sharing
   
   - **3NC (Normal Confidential):**
     - Color: Blue
     - Access: Authorized staff within department
     - Standard access controls
   
   - **4PC (Public Confidential):**
     - Color: Green
     - Access: All company staff
     - Read-only for most users

2. **Permission Management**
   - **Permission Types:**
     - Read: View files and folder structure
     - Write: Upload/modify files
     - Delete: Remove files
     - Modify: Change folder settings
     - Full Control: All permissions + grant access
   
   - **Permission Assignment:**
     - By User (individual)
     - By Role (job function)
     - By Department/ Division/ Sub-Committee (organizational unit)
     - By Group (custom user groups)
   
   - **Permission Inheritance:**
     - Child folders inherit parent permissions
     - Override inheritance if needed
     - Explicit deny overrides allow

3. **Approval Workflows**
   - Define approval rules for:
     - File upload (requires review)
     - File deletion (requires approval)
     - Folder sharing (requires authorization)
     - Permission changes (requires admin approval)
   - Multi-level approval chains
   - Approval delegation support
   - Timeout and escalation rules

4. **Visual Indicators**
   - Folder color based on priority
   - Lock icon for restricted access
   - Shared icon for folders shared externally
   - Badge showing pending approvals

**Acceptance Criteria:**
- ✓ Folder colors match priority levels
- ✓ Unauthorized users cannot access restricted folders
- ✓ Permission changes take effect immediately
- ✓ Approval workflows enforced correctly
- ✓ All permission changes logged in audit trail

**Business Rules:**
- High Confidential folders require executive approval to create
- Permission changes require approval from folder owner or admin
- Inheritance can be broken only by admins
- Audit log must capture all permission changes
- Minimum permission level: Read-only
- External sharing requires department head approval
- Permission expiration can be set (auto-revoke)

---
