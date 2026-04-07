using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Oqtane.Shared;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using GIBS.Module.DataRoom.Services;
using Oqtane.Controllers;
using System.Net;
using System.Threading.Tasks;

namespace GIBS.Module.DataRoom.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class DataRoomController : ModuleControllerBase
    {
        private readonly IDataRoomService _DataRoomService;

        public DataRoomController(IDataRoomService DataRoomService, ILogManager logger, IHttpContextAccessor accessor) : base(logger, accessor)
        {
            _DataRoomService = DataRoomService;
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
    }
}
