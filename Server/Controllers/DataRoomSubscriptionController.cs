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
    public class DataRoomSubscriptionController : ModuleControllerBase
    {
        private readonly IDataRoomSubscriptionService _subscriptionService;

        public DataRoomSubscriptionController(IDataRoomSubscriptionService subscriptionService, ILogManager logger, IHttpContextAccessor accessor) : base(logger, accessor)
        {
            _subscriptionService = subscriptionService;
        }

        [HttpGet]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<IEnumerable<Subscription>> Get(string dataroomid, string moduleid)
        {
            if (int.TryParse(dataroomid, out var dataRoomId) && int.TryParse(moduleid, out var moduleId) && IsAuthorizedEntityId(EntityNames.Module, moduleId))
            {
                return await _subscriptionService.GetSubscriptionsAsync(dataRoomId, moduleId);
            }

            _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Subscription Get Attempt {DataRoomId} {ModuleId}", dataroomid, moduleid);
            HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            return null;
        }

        [HttpGet("{id}/{moduleid}")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<Subscription> Get(int id, int moduleid)
        {
            if (IsAuthorizedEntityId(EntityNames.Module, moduleid))
            {
                var subscription = await _subscriptionService.GetSubscriptionAsync(id, moduleid);
                if (subscription != null)
                {
                    return subscription;
                }
            }

            _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Subscription Get Attempt {SubscriptionId} {ModuleId}", id, moduleid);
            HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            return null;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<Subscription> Post([FromBody] Subscription subscription, [FromQuery] string moduleid)
        {
            if (ModelState.IsValid && int.TryParse(moduleid, out var moduleId) && IsAuthorizedEntityId(EntityNames.Module, moduleId))
            {
                return await _subscriptionService.AddSubscriptionAsync(subscription, moduleId);
            }

            _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Subscription Post Attempt {Subscription}", subscription);
            HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            return null;
        }

        [HttpPut("{id}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<Subscription> Put(int id, [FromBody] Subscription subscription, [FromQuery] string moduleid)
        {
            if (ModelState.IsValid && subscription.SubscriptionId == id && int.TryParse(moduleid, out var moduleId) && IsAuthorizedEntityId(EntityNames.Module, moduleId))
            {
                return await _subscriptionService.UpdateSubscriptionAsync(subscription, moduleId);
            }

            _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Subscription Put Attempt {Subscription}", subscription);
            HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            return null;
        }

        [HttpDelete("{id}/{moduleid}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task Delete(int id, int moduleid)
        {
            if (IsAuthorizedEntityId(EntityNames.Module, moduleid))
            {
                await _subscriptionService.DeleteSubscriptionAsync(id, moduleid);
                return;
            }

            _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Subscription Delete Attempt {SubscriptionId} {ModuleId}", id, moduleid);
            HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
        }

        [HttpPost("confirm-email")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string token, [FromQuery] string moduleid)
        {
            if (string.IsNullOrWhiteSpace(token) || !int.TryParse(moduleid, out var moduleId) || !IsAuthorizedEntityId(EntityNames.Module, moduleId))
            {
                _logger.Log(LogLevel.Warning, this, LogFunction.Security, "Unauthorized Email Confirmation Attempt");
                HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return BadRequest("Invalid confirmation request");
            }

            var confirmed = await _subscriptionService.ConfirmSubscriptionEmailAsync(token, moduleId);

            if (confirmed)
            {
                _logger.Log(LogLevel.Information, this, LogFunction.Other, "Subscription email confirmed with token");
                return Ok(new { success = true, message = "Your subscription has been confirmed!" });
            }

            _logger.Log(LogLevel.Warning, this, LogFunction.Other, "Failed to confirm subscription email - invalid or expired token");
            HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return BadRequest("Invalid or expired confirmation token");
        }
    }
}
