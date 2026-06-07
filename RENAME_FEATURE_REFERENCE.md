# Rename Functionality Reference

## Overview
You have a complete rename feature scaffolded in `Client/Modules/GIBS.Module.DataRoom/Edit.razor` that allows renaming both files and folders with extension protection for files.

## Current Status
✅ **UI Dialog** - Fully implemented
✅ **Dialog Methods** - Implemented (Open/Close)
✅ **Extension Protection** - Implemented
⚠️ **Server Integration** - Scaffolded, needs implementation

## Usage in Pager

The rename button is already wired up on each pager row (line 131):
```razor
<button class="btn btn-sm btn-outline-secondary ms-1" 
		@onclick="() => OpenRenameFileDialog(item.FileId, item.Name, true)">
	Rename
</button>
```

## Components

### 1. State Variables (Line 336-338)
```csharp
private bool _showRenameFileDialog = false;
private int _renameFileId = 0;
private string _renameItemName = string.Empty;
private string _renameFileExtension = string.Empty;
```

### 2. Rename Dialog UI (Lines 279-305)
- Modal dialog with text input
- Shows file extension (read-only for files)
- Cancel and Rename buttons
- Validates extension is not removable

### 3. OpenRenameFileDialog Method (Lines 604-631)
Opens the rename dialog and:
- Extracts file extension (if it's a file)
- Separates base name from extension
- For folders: handles as single name with no extension
- Sets `_showRenameFileDialog = true`

### 4. CloseRenameFileDialog Method (Lines 633-639)
Closes dialog and clears all state variables

### 5. RenameItemAsync Method (Lines 641-670)
Performs the rename with validations:
- ✅ Prevents empty names
- ✅ Prevents periods in name (extension protection)
- ✅ Reconstructs full name with extension
- ⚠️ **TODO:** Call server API to actually rename the item
- Shows success message

## What's Missing

The `RenameItemAsync` method needs to call a server endpoint. Currently it just closes the dialog and shows a message.

You need to:
1. **Create a server endpoint** in `Server/Controllers/` to handle file/folder rename
2. **Call the service method** in RenameItemAsync
3. **Refresh the browser** after successful rename

Example implementation needed:
```csharp
// In RenameItemAsync, replace the comment with:
var result = await FileService.RenameFileAsync(_renameFileId, newName);
await RefreshBrowserAsync();
```

And create corresponding server-side:
```csharp
// In IFileService
Task<bool> RenameFileAsync(int fileId, string newName);

// In FileFolderService
public Task<bool> RenameFileAsync(int fileId, string newName)
{
	// Call Oqtane's file service to rename
}
```

## Files Involved
- `Client/Modules/GIBS.Module.DataRoom/Edit.razor` - UI and methods
- Server-side: Need to create/update file service for actual rename operation
