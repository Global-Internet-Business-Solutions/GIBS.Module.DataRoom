# GIBS.Module.DataRoom

Secure Data Room module for [Oqtane](https://github.com/oqtane/oqtane.framework), built with .NET 10.

## Overview

`GIBS.Module.DataRoom` provides a controlled document management area inside Oqtane where authorized users can:

- Browse files organized in hierarchical folder structures
- Navigate between folders with "Up One Level" controls
- View files inline (PDF with optional watermark, images, text)
- Download individual files
- Upload single or multiple files with optional automatic ZIP extraction
- Download all files in a room as a structured ZIP preserving folder hierarchy
- Sort documents by Name, Type, Size, or Modified Date
- Track all file interactions with activity logging (view/download/upload)

## Features

### For End Users
- **Hierarchical Folder Browsing**: Navigate folder structures with "Up One Level" controls and breadcrumb path display
- **File Viewing**: Supports PDF (with optional watermarking), images (PNG, JPG, GIF), and plain text inline viewing
- **Sortable Columns**: Click column headers to sort documents by Name, Type, Size, or Modified Date with visual sort indicators
- **Multi-File Upload**: Select and upload multiple files at once
- **ZIP Download**: Export entire room structure as a single ZIP file preserving folders and subfolders
- **Configurable Page Size**: Adjust items displayed per page via module settings
- **Activity History**: View complete audit trail of file access and uploads

### For Administrators
- **Data Room Management**: Create rooms with customizable name, description, folder assignment, and role-based access control
- **Document Management**: Browse and delete documents within folder hierarchies directly from admin panel
- **ZIP Upload & Extraction**: Upload ZIP files with optional automatic extraction into folder hierarchy with inline checkbox control
- **Permission Inheritance**: Extracted files automatically inherit parent folder permissions recursively
- **Activity Audit Log**: Comprehensive log of upload/view/download actions with user ID, timestamp (UTC), IP address, and file name
- **Module Settings**: Configure items per page for document browser and activity log pagers

### Core Security
- Module-level authorization using Oqtane policies (View/Edit roles)
- Recursive folder hierarchy access validation
- File ownership validation per Data Room and folder
- Folder-level permission checks for upload/delete operations
- Recursive permission inheritance on ZIP extraction
- Secure file streaming from server storage
- Anti-cache headers (no-store, no-cache, no-sniff) for sensitive content
- Optional watermarking for sensitive PDFs
- Complete audit logging with user, timestamp, and IP tracking

## Tech Stack

- .NET 10
- ASP.NET Core
- Oqtane Framework
- Blazor (Client module UI)
- SQL-backed services/repositories through Oqtane patterns

## Solution Structure

- `Client/` – Blazor client module with folder browser UI, document management, and sorting
- `Server/` – API controllers and server-side services for file operations, ZIP handling, and logging
- `Shared/` – shared models, contracts, and interfaces (DataRoom, DataRoomActivityLog, etc.)
- `Package/` – packaging artifacts (`.nuspec`, package project)

## Prerequisites

- Visual Studio 2026 (or compatible .NET 10 SDK tooling)
- Oqtane Framework source/build available locally (expected by project references)
- SQL database configured for Oqtane host instance

## Local Development

1. Clone this repository.
2. Ensure Oqtane source exists at a relative path expected by project references:
   - `..\oqtane.framework\`
3. Build Oqtane and this module in `Debug` first.
4. Build this solution.
5. Run Oqtane host and install/load the module.

## Build and Package

From Visual Studio:

- Build `Client`, `Server`, `Shared`
- Build `Package` project in `Release` to generate module package output

> The package metadata includes:
> `<packageType name="Oqtane.Framework" version="10.1.2" />`  
> Set this to the lowest Oqtane version that is verified in testing.

## API Endpoints (Server)

Base route follows Oqtane module controller conventions.

Key endpoints in `DataRoomController` include:

**File Operations:**
- `GET api/DataRoom/view/{fileId}/{dataRoomId}/{moduleId}` – Stream file inline (supports PDF, images, text)
- `GET api/DataRoom/download/{fileId}/{dataRoomId}/{moduleId}` – Download file as attachment
- `GET api/DataRoom/zip/{dataRoomId}/{moduleId}` – Download all files/folders as structured ZIP with folder hierarchy
- `POST api/DataRoom/extract/{dataRoomId}/{moduleId}` – Extract uploaded ZIP into folder hierarchy

**Data Room CRUD:**
- `GET api/DataRoom` – List all Data Rooms for current site
- `GET api/DataRoom/{id}` – Get Data Room details
- `POST api/DataRoom` – Create Data Room (Edit permission required)
- `PUT api/DataRoom/{id}` – Update Data Room (Edit permission required)
- `DELETE api/DataRoom/{id}` – Delete Data Room (Edit permission required)

Authorization policy: `ViewModule` (or `EditModule` for mutations).

## Configuration

### Module Settings

Add settings via Oqtane Admin > Modules > Settings:

- **NumberPerPage**: Items to display per page in document browser and activity log pagers (default: 10, recommended: 10-25)

### Data Room Settings

Each Data Room can be configured with:

- **Name**: Display name of the Data Room (required, max 200 characters)
- **Description**: Optional long-form description shown to viewers (max 2000 characters)
- **Folder**: Select the Oqtane folder containing documents (required)
- **View Role**: Role required to access the Data Room (optional; defaults to all authenticated users if not specified)
- **IsActive**: Enable/disable user access to this room
- **EnableDownload**: Allow individual file downloads
- **EnableViewOnly**: Restrict users to view-only mode (no download option in UI)
- **EnableWatermark**: Add watermark text to PDF inline viewing

## Security Notes

- Access is validated against module authorization (ViewModule or EditModule policy).
- File access is restricted to files belonging to the target Data Room folder and all subfolders.
- Users must have explicit **Edit** permissions on the Oqtane folder to upload or delete files (folder-level permissions in File Management).
- ZIP extraction creates folders with inherited parent folder permissions for consistency.
- Unauthorized access attempts are logged with user ID, IP address, and action.
- View/download responses include anti-cache (no-store), no-sniff, and no-cache headers.
- Activity logs include complete audit trail:
  - Action (View / Download / Upload)
  - User ID and display name
  - File name
  - Timestamp (UTC)
  - Client IP address
  - Data Room and File ID

## Use Cases

- **Board/Investor Rooms**: Organized document hierarchies with controlled role-based access
- **Client Onboarding**: Bulk ZIP uploads of compliance materials and legal documents
- **Internal Policy Libraries**: Departmental documentation with folder organization and version tracking
- **Project Handoff**: Time-limited document distribution with full audit trail
- **Regulatory Compliance**: Document management with complete access history and watermark support
- **Legal Discovery**: Secure document review with no-cache headers and comprehensive logging

## License

MIT (see repository license if included).

## Maintainer

GIBS (Global Internet Business Solutions)

## Version History

- **1.2.2**: Hierarchical folder browsing, sortable columns, ZIP upload/extraction, configurable pagination
- **1.2.2**: ZIP export with folder structure preservation  
- **1.2.2**: Allow renaming of both files and folders with extension protection for files
- **1.2.0**: Activity logging for all file actions
- **1.0.0**: Initial release with basic file view/download and access logging

Global Internet Business Solutions (GIBS)