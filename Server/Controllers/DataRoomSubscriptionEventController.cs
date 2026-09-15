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
    public class DataRoomSubscriptionEventController : ModuleControllerBase
    {
        private readonly IDataRoomSubscriptionEventService _subscriptionEventService;

        public DataRoomSubscriptionEventController(IDataRoomSubscriptionEventService subscriptionEventService, ILogManager logger, IHttpContextAccessor accessor) : base(logger, accessor)
        {
            _subscriptionEventService = subscriptionEventService;
        }

        [HttpGet]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<IEnumerable<SubscriptionEvent>> Get(string dataroomid, string moduleid)
        {
            if (int.TryParse(dataroomid, out var dataRoomId) && int.TryParse(moduleid, out var moduleId) && IsAuthorizedEntityId(EntityNames.Module, moduleId))
            {
                return await _subscriptionEventService.GetSubscriptionEventsAsync(dataRoomId, moduleId);
            }

            _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Subscription Event Get Attempt {DataRoomId} {ModuleId}", dataroomid, moduleid);
            HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            return null;
        }

        [HttpPost("process")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<ActionResult<SubscriptionEventProcessResult>> Process([FromQuery] string dataroomid, [FromQuery] string moduleid)
        {
            if (int.TryParse(dataroomid, out var dataRoomId) && int.TryParse(moduleid, out var moduleId) && IsAuthorizedEntityId(EntityNames.Module, moduleId))
            {
                var result = await _subscriptionEventService.ProcessSubscriptionEventsAsync(dataRoomId, moduleId);
                if (result != null)
                {
                    return Ok(result);
                }
            }

            _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Subscription Event Process Attempt {DataRoomId} {ModuleId}", dataroomid, moduleid);
            HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            return Forbid();
        }
    }
}
