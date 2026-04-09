# GIBS.Module.DataRoom

Secure Data Room module for [Oqtane](https://github.com/oqtane/oqtane.framework), built with .NET 10.

## Overview

`GIBS.Module.DataRoom` provides a controlled document area inside Oqtane where authorized users can:

- View only files inline
- Option to Download individual files
- Download all files in a room as ZIP
- Track file access activity (view/download)

## Features

- Module-level authorization using Oqtane policies
- Folder/file ownership validation per Data Room
- Secure file streaming from server storage
- Activity logging:
  - DataRoomId
  - FileId
  - UserId
  - Action (`View` / `Download`)
  - Timestamp (UTC)
  - IP address
- ZIP export for all files in a Data Room

## Tech Stack

- .NET 10
- ASP.NET Core
- Oqtane Framework
- Blazor (Client module UI)
- SQL-backed services/repositories through Oqtane patterns

## Solution Structure

- `Client/` – Blazor client module
- `Server/` – API controllers and server-side services
- `Shared/` – shared models/contracts
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

- `GET download/{fileId}/{dataRoomId}/{moduleId}`
- `GET zip/{dataRoomId}/{moduleId}`
- `GET view/{fileId}/{dataRoomId}/{moduleId}`

Authorization policy: `ViewModule` (or `EditModule` for mutations).

## Security Notes

- Access is validated against module authorization.
- File access is restricted to files belonging to the target Data Room folder.
- Unauthorized access attempts are logged.
- View responses include anti-cache/no-sniff headers.

## License

MIT (see repository license if included).

## Maintainer

Global Internet Business Solutions (GIBS)