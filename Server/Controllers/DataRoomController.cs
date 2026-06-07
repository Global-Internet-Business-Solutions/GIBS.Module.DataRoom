using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.IO;
using System.IO.Compression;
using Microsoft.AspNetCore.Http;
using Oqtane.Shared;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Controllers;
using Oqtane.Repository;
using GIBS.Module.DataRoom.Models;
using GIBS.Module.DataRoom.Services;
using System.Net;
using System.Linq;
using Oqtane.Extensions;
using Oqtane.Models;

namespace GIBS.Module.DataRoom.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class DataRoomController : ModuleControllerBase
    {
        private readonly IDataRoomService _DataRoomService;
        private readonly IDataRoomActivityLogService _activityLogService;
        private readonly IFileRepository _fileRepository;
        private readonly IFolderRepository _folderRepository;
        private readonly ISettingRepository _settingRepository;

        public DataRoomController(IDataRoomService DataRoomService, IDataRoomActivityLogService activityLogService, IFileRepository fileRepository, IFolderRepository folderRepository, ISettingRepository settingRepository, ILogManager logger, IHttpContextAccessor accessor) : base(logger, accessor)
        {
            _DataRoomService = DataRoomService;
            _activityLogService = activityLogService;
            _fileRepository = fileRepository;
            _folderRepository = folderRepository;
            _settingRepository = settingRepository;
        }

        // GET: api/<controller>?moduleid=x
        [HttpGet]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<IEnumerable<Models.DataRoom>> Get(string moduleid)
        {
            int ModuleId;
            if (int.TryParse(moduleid, out ModuleId) && IsAuthorizedEntityId(EntityNames.Module, ModuleId))
            {
                return await _DataRoomService.GetDataRoomsAsync(ModuleId);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized DataRoom Get Attempt {ModuleId}", moduleid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // GET api/<controller>/5
        [HttpGet("{id}/{moduleid}")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<Models.DataRoom> Get(int id, int moduleid)
        {
            Models.DataRoom DataRoom = await _DataRoomService.GetDataRoomAsync(id, moduleid);
            if (DataRoom != null && IsAuthorizedEntityId(EntityNames.Module, DataRoom.ModuleId))
            {
                return DataRoom;
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized DataRoom Get Attempt {DataRoomId} {ModuleId}", id, moduleid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<Models.DataRoom> Post([FromBody] Models.DataRoom DataRoom)
        {
            if (ModelState.IsValid && IsAuthorizedEntityId(EntityNames.Module, DataRoom.ModuleId))
            {
                DataRoom = await _DataRoomService.AddDataRoomAsync(DataRoom);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized DataRoom Post Attempt {DataRoom}", DataRoom);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                DataRoom = null;
            }
            return DataRoom;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<Models.DataRoom> Put(int id, [FromBody] Models.DataRoom DataRoom)
        {
            if (ModelState.IsValid && DataRoom.DataRoomId == id && IsAuthorizedEntityId(EntityNames.Module, DataRoom.ModuleId))
            {
                DataRoom = await _DataRoomService.UpdateDataRoomAsync(DataRoom);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized DataRoom Put Attempt {DataRoom}", DataRoom);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                DataRoom = null;
            }
            return DataRoom;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}/{moduleid}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task Delete(int id, int moduleid)
        {
            Models.DataRoom DataRoom = await _DataRoomService.GetDataRoomAsync(id, moduleid);
            if (DataRoom != null && IsAuthorizedEntityId(EntityNames.Module, DataRoom.ModuleId))
            {
                await _DataRoomService.DeleteDataRoomAsync(id, DataRoom.ModuleId);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized DataRoom Delete Attempt {DataRoomId} {ModuleId}", id, moduleid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }

        // GET api/DataRoom/download/5/3/2?authmoduleid=2
        // Secure file download: validates the file belongs to the data room folder, logs the download,
        // then streams the file. Append ?authmoduleid={moduleId} to the URL for policy authorization.
        [HttpGet("download/{fileId}/{dataRoomId}/{moduleId}")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<IActionResult> Download(int fileId, int dataRoomId, int moduleId)
        {
            if (!IsAuthorizedEntityId(EntityNames.Module, moduleId))
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized File Download Attempt {FileId}", fileId);
                return StatusCode((int)HttpStatusCode.Forbidden);
            }

            var dataRoom = await _DataRoomService.GetDataRoomAsync(dataRoomId, moduleId);
            if (dataRoom == null)
            {
                return NotFound();
            }

            var file = _fileRepository.GetFile(fileId, false);
            if (file == null || file.FolderId != dataRoom.FolderId)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "File {FileId} Does Not Belong To DataRoom {DataRoomId}", fileId, dataRoomId);
                return StatusCode((int)HttpStatusCode.Forbidden);
            }

            var filePath = _fileRepository.GetFilePath(file);
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            await _activityLogService.AddActivityLogAsync(new DataRoomActivityLog
            {
                DataRoomId = dataRoomId,
                FileId = fileId,
                // UserId = User.Identity?.Name ?? string.Empty,
                UserId = User.UserId().ToString(),
                Action = "Download",
                Timestamp = DateTime.UtcNow,
                IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty
            }, moduleId);

            return PhysicalFile(filePath, GetContentType(file.Extension, file.Name), file.Name);
        }

        // Helper method to recursively collect all files with their relative paths
        private List<(Oqtane.Models.File File, string RelativePath)> CollectFilesRecursively(int rootFolderId, Dictionary<int, Folder> folderLookup, Dictionary<int, List<int>> childrenByParent)
        {
            var result = new List<(Oqtane.Models.File, string)>();
            var folderPaths = new Dictionary<int, string>();

            // Queue for BFS traversal
            var queue = new Queue<(int FolderId, string Path)>();
            queue.Enqueue((rootFolderId, ""));
            folderPaths[rootFolderId] = "";

            while (queue.Count > 0)
            {
                var (currentFolderId, currentPath) = queue.Dequeue();

                if (!folderLookup.TryGetValue(currentFolderId, out var folder))
                {
                    continue;
                }

                // Get all files in the current folder
                var files = _fileRepository.GetFiles(currentFolderId).ToList();
                foreach (var file in files)
                {
                    var fileRelativePath = string.IsNullOrEmpty(currentPath)
                        ? file.Name
                        : $"{currentPath}/{file.Name}";
                    result.Add((file, fileRelativePath));
                }

                // Queue all child folders
                if (childrenByParent.TryGetValue(currentFolderId, out var childIds))
                {
                    foreach (var childId in childIds)
                    {
                        if (folderLookup.TryGetValue(childId, out var childFolder))
                        {
                            var childPath = string.IsNullOrEmpty(currentPath)
                                ? childFolder.Name
                                : $"{currentPath}/{childFolder.Name}";
                            queue.Enqueue((childId, childPath));
                            folderPaths[childId] = childPath;
                        }
                    }
                }
            }

            return result;
        }

        // GET api/DataRoom/zip/3/2?authmoduleid=2
        [HttpGet("zip/{dataRoomId}/{moduleId}")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<IActionResult> Zip(int dataRoomId, int moduleId)
        {
            if (!IsAuthorizedEntityId(EntityNames.Module, moduleId))
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Zip Download Attempt {DataRoomId}", dataRoomId);
                return StatusCode((int)HttpStatusCode.Forbidden);
            }

            var dataRoom = await _DataRoomService.GetDataRoomAsync(dataRoomId, moduleId);
            if (dataRoom == null) return NotFound();

            // Get all folders to build the hierarchy (same approach as client)
            var allFolders = _folderRepository.GetFolders(dataRoom.SiteId);
            if (allFolders == null)
            {
                return NotFound();
            }

            var folderLookup = new Dictionary<int, Folder>();
            var childrenByParent = new Dictionary<int, List<int>>();

            foreach (var folder in allFolders)
            {
                if (!folderLookup.ContainsKey(folder.FolderId))
                {
                    folderLookup[folder.FolderId] = folder;
                }

                if (folder.ParentId.HasValue)
                {
                    if (!childrenByParent.ContainsKey(folder.ParentId.Value))
                    {
                        childrenByParent[folder.ParentId.Value] = new List<int>();
                    }
                    childrenByParent[folder.ParentId.Value].Add(folder.FolderId);
                }
            }

            // Recursively collect all files with their relative paths
            var filesWithPaths = CollectFilesRecursively(dataRoom.FolderId, folderLookup, childrenByParent);

            var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
            {
                // Track created directories to avoid duplicates
                var createdDirs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var (file, relativePath) in filesWithPaths)
                {
                    var filePath = _fileRepository.GetFilePath(file);
                    if (!System.IO.File.Exists(filePath)) continue;

                    // Create all intermediate directories in the ZIP
                    var dirPath = System.IO.Path.GetDirectoryName(relativePath);
                    if (!string.IsNullOrEmpty(dirPath))
                    {
                        var parts = dirPath.Split('/', '\\');
                        var currentPath = "";
                        foreach (var part in parts)
                        {
                            currentPath = string.IsNullOrEmpty(currentPath) ? part : $"{currentPath}/{part}";
                            var dirEntry = $"{currentPath}/";

                            if (!createdDirs.Contains(dirEntry))
                            {
                                archive.CreateEntry(dirEntry);
                                createdDirs.Add(dirEntry);
                            }
                        }
                    }

                    // Create ZIP entry with the relative path (preserves folder structure)
                    var entry = archive.CreateEntry(relativePath, CompressionLevel.Fastest);
                    using var entryStream = entry.Open();
                    using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    await fileStream.CopyToAsync(entryStream);
                }
            }

            memoryStream.Seek(0, SeekOrigin.Begin);
            var zipName = $"{dataRoom.Name.Replace(" ", "_")}_{DateTime.UtcNow:yyyyMMdd}.zip";
            return File(memoryStream, "application/zip", zipName);
        }

        // PUT api/DataRoom/extract/3/2/15?authmoduleid=2
        [HttpPut("extract/{dataRoomId}/{moduleId}/{zipFileId}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<IActionResult> Extract(int dataRoomId, int moduleId, int zipFileId)
        {
            if (!IsAuthorizedEntityId(EntityNames.Module, moduleId))
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Zip Extract Attempt {DataRoomId} {ZipFileId}", dataRoomId, zipFileId);
                return StatusCode((int)HttpStatusCode.Forbidden);
            }

            var dataRoom = await _DataRoomService.GetDataRoomAsync(dataRoomId, moduleId);
            if (dataRoom == null || dataRoom.FolderId <= 0)
            {
                return NotFound();
            }

            var dataRoomRootFolder = _folderRepository.GetFolder(dataRoom.FolderId);
            if (dataRoomRootFolder == null)
            {
                return NotFound();
            }

            var zipFile = _fileRepository.GetFile(zipFileId, false);
            if (zipFile == null)
            {
                return NotFound();
            }

            try
            {
                var extractedFileCount = ExtractArchiveWithRetry(zipFile, dataRoomRootFolder.SiteId);
                if (extractedFileCount <= 0)
                {
                    throw new InvalidDataException("ZIP contained no extractable files.");
                }

                DeleteZipFile(zipFile);

                var zipRecordStillExists = _fileRepository.GetFile(zipFileId, false) != null;
                if (zipRecordStillExists)
                {
                    throw new InvalidOperationException("ZIP record still exists after extraction.");
                }

                await _activityLogService.AddActivityLogAsync(new DataRoomActivityLog
                {
                    DataRoomId = dataRoomId,
                    FileId = zipFileId,
                    UserId = User.UserId().ToString(),
                    Action = "ExtractZip",
                    Timestamp = DateTime.UtcNow,
                    IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty
                }, moduleId);

                Response.Headers["X-DataRoom-Extract"] = "1";
                Response.Headers["X-DataRoom-Extract-Version"] = "3";
                Response.Headers["X-DataRoom-Extract-Files"] = extractedFileCount.ToString();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Create, ex, "Error Extracting Zip File {ZipFileId} For DataRoom {DataRoomId} {Error}", zipFileId, dataRoomId, ex.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError, $"Unable to extract ZIP file: {ex.Message}");
            }
        }

        // GET api/DataRoom/view/5/3/2?authmoduleid=2
        // Serves file inline (Content-Disposition: inline), logs the view, sets no-store cache.
        [HttpGet("view/{fileId}/{dataRoomId}/{moduleId}")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<IActionResult> View(int fileId, int dataRoomId, int moduleId)
        {
            if (!IsAuthorizedEntityId(EntityNames.Module, moduleId))
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized File View Attempt {FileId}", fileId);
                return StatusCode((int)HttpStatusCode.Forbidden);
            }

            var dataRoom = await _DataRoomService.GetDataRoomAsync(dataRoomId, moduleId);
            if (dataRoom == null) return NotFound();

            var file = _fileRepository.GetFile(fileId, false);
            if (file == null)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "File {FileId} Not Found", fileId);
                return StatusCode((int)HttpStatusCode.Forbidden);
            }

            // Check if file belongs to DataRoom or any subfolder
            var allowedFolderIds = GetAllowedFolderIds(dataRoom.FolderId, dataRoom.SiteId);
            if (!allowedFolderIds.Contains(file.FolderId))
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "File {FileId} Does Not Belong To DataRoom {DataRoomId}", fileId, dataRoomId);
                return StatusCode((int)HttpStatusCode.Forbidden);
            }

            var filePath = _fileRepository.GetFilePath(file);
            if (!System.IO.File.Exists(filePath)) return NotFound();

            await _activityLogService.AddActivityLogAsync(new DataRoomActivityLog
            {
                DataRoomId = dataRoomId,
                FileId = fileId,
                UserId = User.UserId().ToString(),
                Action = "View",
                Timestamp = DateTime.UtcNow,
                IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty
            }, moduleId);

            // Inline — browser renders in-place, no Save-As prompt
            Response.Headers["Content-Disposition"] = $"inline; filename=\"{file.Name}\"";
            Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["X-Content-Type-Options"] = "nosniff";

            return PhysicalFile(filePath, GetContentType(file.Extension, file.Name));
        }

        [HttpPost("Rename")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<IActionResult> Rename([FromBody] RenameRequest request)
        {
            if (!IsAuthorizedEntityId(EntityNames.Module, request.ModuleId))
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Rename Attempt FileId:{FileId} FolderId:{FolderId}", request.FileId, request.FolderId);
                return StatusCode((int)HttpStatusCode.Forbidden);
            }

            if (string.IsNullOrWhiteSpace(request.NewName))
            {
                return BadRequest("New name cannot be empty.");
            }

            var dataRoom = await _DataRoomService.GetDataRoomAsync(request.DataRoomId, request.ModuleId);
            if (dataRoom == null) return NotFound();

            // Handle folder rename
            if (request.IsFolder)
            {
                return await RenameFolderAsync(request, dataRoom);
            }
            else
            {
                // Handle file rename
                return RenameFile(request, dataRoom);
            }
        }

        private async Task<IActionResult> RenameFolderAsync(RenameRequest request, Models.DataRoom dataRoom)
        {
            var folder = _folderRepository.GetFolder(request.FolderId);
            if (folder == null)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Folder {FolderId} Not Found", request.FolderId);
                return StatusCode((int)HttpStatusCode.Forbidden);
            }

            // Check if folder belongs to DataRoom or any subfolder
            var allowedFolderIds = GetAllowedFolderIds(dataRoom.FolderId, dataRoom.SiteId);
            if (!allowedFolderIds.Contains(folder.FolderId))
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Folder {FolderId} Does Not Belong To DataRoom {DataRoomId}", request.FolderId, request.DataRoomId);
                return StatusCode((int)HttpStatusCode.Forbidden);
            }

            try
            {
                var oldName = folder.Name;
                folder.Name = request.NewName;
                _folderRepository.UpdateFolder(folder);

                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Folder {FolderId} Renamed From {OldName} To {NewName}", request.FolderId, oldName, request.NewName);

                return Ok(new { success = true, message = "Folder renamed successfully." });
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Update, "Error Renaming Folder {FolderId}: {Error}", request.FolderId, ex.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError, new { success = false, message = "Error renaming folder." });
            }
        }

        private IActionResult RenameFile(RenameRequest request, Models.DataRoom dataRoom)
        {
            var file = _fileRepository.GetFile(request.FileId, false);
            if (file == null)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "File {FileId} Not Found", request.FileId);
                return StatusCode((int)HttpStatusCode.Forbidden);
            }

            // Check if file belongs to DataRoom or any subfolder
            var allowedFolderIds = GetAllowedFolderIds(dataRoom.FolderId, dataRoom.SiteId);
            if (!allowedFolderIds.Contains(file.FolderId))
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "File {FileId} Does Not Belong To DataRoom {DataRoomId}", request.FileId, request.DataRoomId);
                return StatusCode((int)HttpStatusCode.Forbidden);
            }

            try
            {
                // Validate extension is not being removed
                var originalExtension = Path.GetExtension(file.Name);
                var newNameWithExt = request.NewName;
                if (!string.IsNullOrEmpty(originalExtension) && !newNameWithExt.EndsWith(originalExtension, StringComparison.OrdinalIgnoreCase))
                {
                    newNameWithExt = request.NewName + originalExtension;
                }

                // Get old file path
                var oldFilePath = _fileRepository.GetFilePath(file);

                // Update file name in database
                file.Name = newNameWithExt;
                _fileRepository.UpdateFile(file);

                // Rename the physical file
                if (System.IO.File.Exists(oldFilePath))
                {
                    var newFilePath = Path.Combine(Path.GetDirectoryName(oldFilePath), newNameWithExt);
                    System.IO.File.Move(oldFilePath, newFilePath);
                }

                _logger.Log(LogLevel.Information, this, LogFunction.Update, "File {FileId} Renamed From {OldName} To {NewName}", request.FileId, file.Name, newNameWithExt);

                return Ok(new { success = true, message = "File renamed successfully." });
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Update, "Error Renaming File {FileId}: {Error}", request.FileId, ex.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError, new { success = false, message = "Error renaming file." });
            }
        }

        public class RenameRequest
        {
            public int FileId { get; set; }
            public int FolderId { get; set; }
            public int DataRoomId { get; set; }
            public int ModuleId { get; set; }
            public string NewName { get; set; }
            public bool IsFolder { get; set; }
        }

        private HashSet<int> GetAllowedFolderIds(int rootFolderId, int siteId)
        {
            var folders = _folderRepository.GetFolders(siteId).ToList();
            var childrenByParent = folders
                .Where(f => f.ParentId.HasValue)
                .GroupBy(f => f.ParentId.Value)
                .ToDictionary(g => g.Key, g => g.Select(f => f.FolderId).ToList());

            var folderIds = new HashSet<int> { rootFolderId };
            var queue = new Queue<int>();
            queue.Enqueue(rootFolderId);

            while (queue.Count > 0)
            {
                var currentFolderId = queue.Dequeue();
                if (!childrenByParent.TryGetValue(currentFolderId, out var childIds))
                {
                    continue;
                }

                foreach (var childId in childIds)
                {
                    if (folderIds.Add(childId))
                    {
                        queue.Enqueue(childId);
                    }
                }
            }

            return folderIds;
        }

        private int ExtractArchiveWithRetry(Oqtane.Models.File zipFile, int siteId)
        {
            var attempts = 5;
            for (var attempt = 1; attempt <= attempts; attempt++)
            {
                var extracted = ExtractArchive(zipFile, siteId);
                if (extracted > 0)
                {
                    return extracted;
                }

                if (attempt < attempts)
                {
                    System.Threading.Thread.Sleep(400);
                }
            }

            return 0;
        }

        private int ExtractArchive(Oqtane.Models.File zipFile, int siteId)
        {
            var extractedFileCount = 0;
            var rootFolder = _folderRepository.GetFolder(zipFile.FolderId);
            var rootFolderPath = _folderRepository.GetFolderPath(rootFolder);
            var zipPath = Path.Combine(rootFolderPath, zipFile.Name);

            if (!System.IO.File.Exists(zipPath))
            {
                throw new FileNotFoundException("ZIP file not found.", zipPath);
            }

            var folders = _folderRepository.GetFolders(siteId).ToList();

            using (var archive = ZipFile.OpenRead(zipPath))
            {
                foreach (var entry in archive.Entries)
                {
                    var normalizedEntryPath = NormalizeEntryPath(entry.FullName);
                    if (string.IsNullOrEmpty(normalizedEntryPath))
                    {
                        continue;
                    }

                    if (string.IsNullOrEmpty(entry.Name))
                    {
                        EnsureFolder(rootFolder, normalizedEntryPath, folders);
                        continue;
                    }

                    var pathSegments = normalizedEntryPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
                    if (pathSegments.Length == 0)
                    {
                        continue;
                    }

                    var fileName = pathSegments[pathSegments.Length - 1];
                    if (!IsSafeZipName(fileName))
                    {
                        _logger.Log(LogLevel.Warning, this, LogFunction.Security, "Skipping Invalid File Name During Zip Extraction {FileName}", fileName);
                        continue;
                    }

                    var relativeDirectory = (pathSegments.Length > 1)
                        ? string.Join("/", pathSegments.Take(pathSegments.Length - 1))
                        : string.Empty;
                    var targetFolder = EnsureFolder(rootFolder, relativeDirectory, folders);
                    var targetFolderPath = _folderRepository.GetFolderPath(targetFolder);
                    Directory.CreateDirectory(targetFolderPath);

                    var targetFilePath = Path.Combine(targetFolderPath, fileName);
                    entry.ExtractToFile(targetFilePath, true);
                    UpsertFileRecord(targetFolder, fileName, targetFilePath);
                    extractedFileCount++;
                }
            }

            return extractedFileCount;
        }

        private void DeleteZipFile(Oqtane.Models.File zipFile)
        {
            var zipPath = _fileRepository.GetFilePath(zipFile);
            _fileRepository.DeleteFile(zipFile.FileId);
            if (!string.IsNullOrWhiteSpace(zipPath) && System.IO.File.Exists(zipPath))
            {
                System.IO.File.Delete(zipPath);
            }
        }

        private Folder EnsureFolder(Folder rootFolder, string relativePath, List<Folder> knownFolders)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return rootFolder;
            }

            var segments = relativePath
                .Replace("\\", "/")
                .Split('/', StringSplitOptions.RemoveEmptyEntries);

            var current = rootFolder;
            foreach (var segment in segments)
            {
                if (!IsSafeZipName(segment))
                {
                    throw new InvalidDataException($"Invalid folder name in ZIP: {segment}");
                }

                var existing = knownFolders.FirstOrDefault(f =>
                    f.ParentId == current.FolderId &&
                    string.Equals(f.Name, segment, StringComparison.OrdinalIgnoreCase));

                if (existing == null)
                {
                    var path = Utilities.UrlCombine(current.Path, segment);
                    if (!path.EndsWith('/'))
                    {
                        path += "/";
                    }

                    existing = _folderRepository.AddFolder(new Folder
                    {
                        SiteId = current.SiteId,
                        ParentId = current.FolderId,
                        Type = current.Type,
                        Name = segment,
                        Path = path,
                        Order = 1,
                        ImageSizes = current.ImageSizes,
                        Capacity = current.Capacity,
                        IsSystem = false,
                        CacheControl = current.CacheControl,
                        PermissionList = CreateChildFolderPermissions(current)
                    });

                    knownFolders.Add(existing);
                }

                current = existing;
            }

            return current;
        }

        private List<Permission> CreateChildFolderPermissions(Folder parentFolder)
        {
            var inherited = (parentFolder?.PermissionList ?? new List<Permission>())
                .Select(p => p.Clone())
                .ToList();

            // Ensure we have at least the parent folder's permissions
            if (inherited.Any())
            {
                return inherited;
            }

            // Fallback to default permissions if parent has none
            return new List<Permission>
            {
                new Permission(PermissionNames.Browse, RoleNames.Everyone, true),
                new Permission(PermissionNames.View, RoleNames.Everyone, true),
                new Permission(PermissionNames.Edit, RoleNames.Admin, true)
            };
        }

        private void UpsertFileRecord(Folder folder, string fileName, string filePath)
        {
            var existing = _fileRepository.GetFile(folder.FolderId, fileName);
            var fileInfo = new FileInfo(filePath);

            var file = existing ?? new Oqtane.Models.File();
            file.Name = fileName;
            file.FolderId = folder.FolderId;
            file.Extension = fileInfo.Extension.TrimStart('.').ToLowerInvariant();
            file.Size = (int)fileInfo.Length;

            if (existing == null)
            {
                _fileRepository.AddFile(file);
            }
            else
            {
                _fileRepository.UpdateFile(file);
            }
        }

        private bool HasValidFileExtension(string fileName, int siteId)
        {
            var uploadableFiles = _settingRepository.GetSetting(EntityNames.Site, siteId, "UploadableFiles")?.SettingValue;
            uploadableFiles = string.IsNullOrEmpty(uploadableFiles) ? Constants.UploadableFiles : uploadableFiles;
            var extension = Path.GetExtension(fileName).TrimStart('.').ToLowerInvariant();
            var allowed = uploadableFiles
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(e => e.Trim().TrimStart('.').ToLowerInvariant())
                .ToList();
            return allowed.Contains(extension);
        }

        private static bool IsSafeZipName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            var trimmed = name.Trim();
            if (trimmed == "." || trimmed == "..")
            {
                return false;
            }

            return trimmed.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;
        }

        private static string NormalizeEntryPath(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return string.Empty;
            }

            var normalized = fullName.Replace('\\', '/').Trim().TrimStart('/');
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return string.Empty;
            }

            if (Path.IsPathRooted(normalized) || normalized.Contains("..", StringComparison.Ordinal) || normalized.Contains(':'))
            {
                throw new InvalidDataException($"Invalid ZIP entry path: {fullName}");
            }

            return normalized;
        }

        private static string GetContentType(string extension, string fileName = null)
        {
            var ext = (extension ?? string.Empty).Trim().TrimStart('.').ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(ext) && !string.IsNullOrWhiteSpace(fileName))
            {
                ext = Path.GetExtension(fileName).TrimStart('.').ToLowerInvariant();
            }

            return ext switch
            {
                "pdf" => "application/pdf",
                "doc" => "application/msword",
                "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "xls" => "application/vnd.ms-excel",
                "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "ppt" => "application/vnd.ms-powerpoint",
                "pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                "png" => "image/png",
                "jpg" or "jpeg" => "image/jpeg",
                "gif" => "image/gif",
                "txt" => "text/plain",
                "zip" => "application/zip",
                _ => "application/octet-stream"
            };
        }
    }
}
