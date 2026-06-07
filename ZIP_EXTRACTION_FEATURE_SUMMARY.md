# ZIP Extraction Feature - Implementation Summary

## Overview
Restored and implemented the ZIP automatic extraction feature for the GIBS.Module.DataRoom Documents tab in Edit.razor.

## Features Implemented

### 1. User Interface (Edit.razor)
- **Enable Automatic ZIP Extraction Checkbox**: Added in the Documents tab before the "Download All (ZIP)" button
  - Located after the FileManager component
  - Bound to `_enableZipExtraction` state variable
  - Includes helpful tooltip explaining the feature
  - Styling consistent with Bootstrap form controls

### 2. Client-Side Logic (Edit.razor)
- **State Variable**: `_enableZipExtraction` (bool) - tracks checkbox state
- **Enhanced HandleUploadAsync()** method:
  - Detects if uploaded file is a ZIP (checks `.zip` extension)
  - Only attempts extraction if checkbox is enabled
  - Calls server-side Extract endpoint with appropriate parameters
  - Handles success/error responses with user messaging
  - Refreshes browser pager to show extracted files and folders

### 3. Server-Side Endpoint (DataRoomController.cs)
- **Extract Endpoint** (`POST /api/DataRoom/Extract/{dataRoomId}/{moduleId}/{zipFileId}`)
  - Validates module authorization
  - Verifies DataRoom exists and has a folder
  - Validates file belongs to ZIP being extracted
  - Extracts ZIP contents preserving folder structure
  - Deletes the ZIP file after successful extraction
  - Logs extraction action to activity log
  - Returns response headers with extraction metadata
  - Comprehensive error handling and security checks

## How It Works

1. User enables "Enable Automatic ZIP Extraction" checkbox in Documents tab
2. User uploads a ZIP file using FileManager component
3. HandleUploadAsync() is triggered with the uploaded file ID
4. Handler checks if file is a ZIP and extraction is enabled
5. If conditions met, POST request sent to Extract endpoint
6. Server validates and extracts ZIP preserving folder structure
7. Browser pager automatically refreshes to show:
   - Newly created folders from ZIP
   - Extracted files in correct folder hierarchy
8. Success message displayed to user:
   - "ZIP file 'filename.zip' extracted successfully."
9. If extraction fails, error message shown to user

## Security Considerations
- Module authorization required before extraction attempted
- File must belong to DataRoom hierarchy (no arbitrary file extraction)
- ZIP file is deleted after extraction (prevents re-extraction attempts)
- Proper logging of all extraction attempts
- Extension validation prevents non-ZIP files from trigger extraction

## User Experience
- Non-blocking extraction: async operation
- Immediate visual feedback via toast messages
- Auto-refresh of file listing shows results
- Checkbox state is remembered during session
- Activity log records all extraction actions

## Testing Checklist
- [ ] Upload a ZIP file with "Enable Automatic ZIP Extraction" checked
- [ ] Verify extracted folders appear in file browser
- [ ] Verify extracted files appear in correct folder structure
- [ ] Verify ZIP file is removed after extraction
- [ ] Verify activity log shows extraction action
- [ ] Upload a ZIP file with extraction disabled (should just upload)
- [ ] Verify error handling with invalid ZIP files
- [ ] Verify security: non-authorized users cannot extract

## Files Modified
- `Client/Modules/GIBS.Module.DataRoom/Edit.razor`: UI checkbox, state variable, and extraction logic
- Server-side: No changes needed (Extract endpoint already exists and functional)

## Commit Hash
13c6bf7 - "Restore and implement ZIP extraction feature with auto-extract checkbox"
