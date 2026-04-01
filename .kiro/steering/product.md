# Product Overview

This is an enterprise Content Management System (CMS) for document and file management, built as a Windows desktop application.

## Core Capabilities

- Hierarchical folder management mapped to organizational units (OrgUnits)
- File upload, versioning, and storage on a network drive
- Role-based access control (RBAC) with permission inheritance across folder hierarchies
- Data classification by confidentiality levels (HC / SC / NC / PC) per CRAF requirements
- Approval workflows for sensitive operations
- Immutable audit logging and compliance tracking
- Virus scanning via ClamAV before file storage
- Recycle bin with configurable retention policies
- Quick access bookmarks for frequently used folders
- PDPA (Personal Data Protection Act) consent tracking
- Employee/org-unit sync via an external Employee API

## Target Users

Internal enterprise users organized into org units. Permissions are granted per user, per group, or per org unit, and can be inherited or explicitly overridden at any folder level.
