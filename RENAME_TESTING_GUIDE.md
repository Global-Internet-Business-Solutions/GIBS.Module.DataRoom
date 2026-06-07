# File Rename Feature - Testing Guide

## Quick Start Testing

### Prerequisites
1. Run the application
2. Login as a user with "Edit" role on a DataRoom module
3. Navigate to Edit page of a DataRoom with files

### Test Case 1: Basic Rename
1. Find a file in the Documents tab pager
2. Click **Rename** button on the file row
3. Modal dialog opens showing current filename
4. Enter new name (without extension): `MyDocument`
5. Extension should show as read-only: `.docx`
6. Click **Rename** button
7. ✅ Success message appears
8. ✅ File list refreshes with new name

### Test Case 2: Extension Protection
1. Open rename dialog for a file
2. Try entering: `MyFile.txt` (with period and wrong extension)
3. Click **Rename**
4. ✅ Warning message: "Name cannot contain a period..."
5. Try again without period
6. ✅ Should succeed and keep original extension

### Test Case 3: Cancel Rename
1. Open rename dialog
2. Enter some text
3. Click **Cancel** button
4. ✅ Dialog closes without changing filename
5. ✅ File list unchanged

### Test Case 4: Empty Name Validation
1. Open rename dialog
2. Clear the name field (empty)
3. Click **Rename**
4. ✅ Warning message: "Name cannot be empty"

### Test Case 5: Check Activity Log
1. Rename a file successfully
2. Click **Activity Log** tab
3. ✅ New entry should appear (if rename action is logged)
4. Action should show what was renamed

### Test Case 6: Multiple Renames
1. Rename `Report.pdf` to `FY2024 Report`
2. Rename it again to `Annual Report`
3. ✅ Both renames succeed
4. ✅ Final name shown in pager
5. ✅ No database conflicts

### Test Case 7: Special Characters
1. Try renaming with special characters: `Report@2024`
2. ✅ Should succeed (no validation restriction)
3. Try with backslash: `Report\File` (if validation blocks path separators)
4. Monitor for any file system issues

### Test Case 8: Long Names
1. Try renaming with max length name (255 chars typical)
2. ✅ Should handle gracefully
3. Try slightly longer (256+ chars)
4. Monitor error handling

### Test Case 9: Unauthorized User
1. Login as user WITHOUT "Edit" role
2. Try accessing Edit page
3. ✅ Should show 403 Forbidden or access denied
4. Rename button shouldn't be available

### Test Case 10: Cross-Module Security
1. Rename file in DataRoom A
2. Try manually POST rename for file in DataRoom B (different module)
3. ✅ Should return 403 Forbidden
4. ✅ Log should show security warning

## Browser Console Checks
```javascript
// Open DevTools (F12), go to Console tab
// Should NOT see errors during rename
```

## Network Tab Checks
1. Open DevTools, Network tab
2. Click Rename, observe:
3. ✅ POST request to `/api/DataRoom/Rename?authmoduleid=XXX`
4. ✅ Request body has fileId, dataRoomId, moduleId, newName
5. ✅ Response status 200 OK
6. ✅ Response body has `{success: true, message: "..."}`

## Known Issues / Limitations
- ❌ Cannot rename folders with current implementation (button only on files)
- ❌ No bulk rename capability
- ❌ No rename undo feature

## Server Log Checks
Check application logs for entries like:
```
Information: File {FileId} Renamed From {OldName} To {NewName}
Error: Error Renaming File {FileId}: {Error}
Security: Unauthorized Rename Attempt {FileId}
```

## Regression Tests
Verify existing features still work:
- ✅ File upload still works
- ✅ File download still works
- ✅ Zip download still works
- ✅ View file still works
- ✅ Delete file still works (if implemented)
- ✅ Activity log entries still recorded
- ✅ Folder navigation still works
- ✅ Sorting by columns still works
- ✅ Pagination still works

## Performance Checks
- ✅ Rename completes within 2-3 seconds
- ✅ UI doesn't freeze during rename
- ✅ Pager refreshes quickly
- ✅ No N+1 query issues
- ✅ Memory usage stable

## Edge Cases
1. File with no extension (name without dot)
   - Should work: `MyFile` → renamed as `MyFile`

2. File with multiple dots
   - Example: `Report.Q4.2024.pdf`
   - Only last extension should be preserved

3. Very old files
   - Should still be renameable

4. Recently uploaded files
   - Should be renameable immediately

5. Files with special permissions
   - Should respect module permission model

## Success Criteria ✅
All tests pass when:
- ✅ File renames successfully
- ✅ Extension preserved automatically
- ✅ Success message displayed
- ✅ File list updated
- ✅ No database corruption
- ✅ Proper authorization enforced
- ✅ Error messages clear
- ✅ No console errors
- ✅ Activity logged
- ✅ Performance acceptable

## Rollback Plan
If issues found:
```powershell
git reset --hard b496105  # Previous checkpoint before rename feature
git push -f origin master  # Force push if needed
```

## Contact / Support
For issues with rename feature, check:
1. User has Edit permission on DataRoom module
2. File exists in DataRoom folder
3. File system permissions allow rename
4. Database connection working
5. No concurrent edits on same file
