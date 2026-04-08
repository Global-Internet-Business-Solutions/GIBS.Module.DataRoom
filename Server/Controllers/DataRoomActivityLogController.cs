using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Controllers;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Shared;
using GIBS.Module.DataRoom.Models;
using GIBS.Module.DataRoom.Services;

namespace GIBS.Module.DataRoom.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class DataRoomActivityLogController : ModuleControllerBase
    {
        private readonly IDataRoomActivityLogService _activityLogService;

        public DataRoomActivityLogController(IDataRoomActivityLogService activityLogService, ILogManager logger, IHttpContextAccessor accessor) : base(logger, accessor)
        {
            _activityLogService = activityLogService;
        }

        // GET: api/<controller>?dataroomid=x&moduleid=y
        [HttpGet]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<IEnumerable<DataRoomActivityLog>> Get(string dataroomid, string moduleid)
        {
            if (int.TryParse(dataroomid, out int dataRoomId) && int.TryParse(moduleid, out int moduleId) && IsAuthorizedEntityId(EntityNames.Module, moduleId))
            {
                return await _activityLogService.GetActivityLogsAsync(dataRoomId, moduleId);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized ActivityLog Get Attempt {DataRoomId}", dataroomid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // POST: api/<controller>?moduleid=y
        [HttpPost]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<DataRoomActivityLog> Post([FromBody] DataRoomActivityLog activityLog, [FromQuery] string moduleid)
        {
            if (ModelState.IsValid && int.TryParse(moduleid, out int moduleId) && IsAuthorizedEntityId(EntityNames.Module, moduleId))
            {
                activityLog = await _activityLogService.AddActivityLogAsync(activityLog, moduleId);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized ActivityLog Post Attempt {ActivityLog}", activityLog);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                activityLog = null;
            }
            return activityLog;
        }
    }
}
