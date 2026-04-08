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

namespace GIBS.Module.DataRoom.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class DataRoomController : ModuleControllerBase
    {
        private readonly IDataRoomService _DataRoomService;
        private readonly IDataRoomActivityLogService _activityLogService;
        private readonly IFileRepository _fileRepository;

        public DataRoomController(IDataRoomService DataRoomService, IDataRoomActivityLogService activityLogService, IFileRepository fileRepository, ILogManager logger, IHttpContextAccessor accessor) : base(logger, accessor)
        {
            _DataRoomService = DataRoomService;
            _activityLogService = activityLogService;
            _fileRepository = fileRepository;
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
                UserId = User.Identity?.Name ?? string.Empty,
                Action = "Download",
                Timestamp = DateTime.UtcNow,
                IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty
            }, moduleId);

            return PhysicalFile(filePath, GetContentType(file.Extension, file.Name), file.Name);
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

            var files = _fileRepository.GetFiles(dataRoom.FolderId).ToList();

            var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
            {
                foreach (var file in files)
                {
                    var filePath = _fileRepository.GetFilePath(file);
                    if (!System.IO.File.Exists(filePath)) continue;

                    var entry = archive.CreateEntry(file.Name, CompressionLevel.Fastest);
                    using var entryStream = entry.Open();
                    using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    await fileStream.CopyToAsync(entryStream);
                }
            }

            memoryStream.Seek(0, SeekOrigin.Begin);
            var zipName = $"{dataRoom.Name.Replace(" ", "_")}_{DateTime.UtcNow:yyyyMMdd}.zip";
            return File(memoryStream, "application/zip", zipName);
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
            if (file == null || file.FolderId != dataRoom.FolderId)
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
                UserId = User.Identity?.Name ?? string.Empty,
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
