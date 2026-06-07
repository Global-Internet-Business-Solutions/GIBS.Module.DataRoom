using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Security;
using Oqtane.Shared;
using GIBS.Module.DataRoom.Repository;

namespace GIBS.Module.DataRoom.Services
{
    public class ServerDataRoomService : IDataRoomService
    {
        private readonly IDataRoomRepository _DataRoomRepository;
        private readonly IUserPermissions _userPermissions;
        private readonly ILogManager _logger;
        private readonly IHttpContextAccessor _accessor;
        private readonly Alias _alias;

        public ServerDataRoomService(IDataRoomRepository DataRoomRepository, IUserPermissions userPermissions, ITenantManager tenantManager, ILogManager logger, IHttpContextAccessor accessor)
        {
            _DataRoomRepository = DataRoomRepository;
            _userPermissions = userPermissions;
            _logger = logger;
            _accessor = accessor;
            _alias = tenantManager.GetAlias();
        }

        public Task<List<Models.DataRoom>> GetDataRoomsAsync(int ModuleId)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, ModuleId, PermissionNames.View))
            {
                return Task.FromResult(_DataRoomRepository.GetDataRooms(ModuleId).ToList());
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized DataRoom Get Attempt {ModuleId}", ModuleId);
                return null;
            }
        }

        public Task<Models.DataRoom> GetDataRoomAsync(int DataRoomId, int ModuleId)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, ModuleId, PermissionNames.View))
            {
                var dataRoom = _DataRoomRepository.GetDataRoom(DataRoomId);
                // Ensure the DataRoom belongs to this module
                if (dataRoom != null && dataRoom.ModuleId == ModuleId)
                {
                    return Task.FromResult(dataRoom);
                }
                else if (dataRoom != null)
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "DataRoom {DataRoomId} does not belong to Module {ModuleId}", DataRoomId, ModuleId);
                    return Task.FromResult<Models.DataRoom>(null);
                }
                else
                {
                    return Task.FromResult<Models.DataRoom>(null);
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized DataRoom Get Attempt {DataRoomId} {ModuleId}", DataRoomId, ModuleId);
                return Task.FromResult<Models.DataRoom>(null);
            }
        }

        public Task<Models.DataRoom> AddDataRoomAsync(Models.DataRoom DataRoom)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, DataRoom.ModuleId, PermissionNames.Edit))
            {
                DataRoom = _DataRoomRepository.AddDataRoom(DataRoom);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "DataRoom Added {DataRoom}", DataRoom);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized DataRoom Add Attempt {DataRoom}", DataRoom);
                DataRoom = null;
            }
            return Task.FromResult(DataRoom);
        }

        public Task<Models.DataRoom> UpdateDataRoomAsync(Models.DataRoom DataRoom)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, DataRoom.ModuleId, PermissionNames.Edit))
            {
                DataRoom = _DataRoomRepository.UpdateDataRoom(DataRoom);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "DataRoom Updated {DataRoom}", DataRoom);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized DataRoom Update Attempt {DataRoom}", DataRoom);
                DataRoom = null;
            }
            return Task.FromResult(DataRoom);
        }

        public Task DeleteDataRoomAsync(int DataRoomId, int ModuleId)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, ModuleId, PermissionNames.Edit))
            {
                _DataRoomRepository.DeleteDataRoom(DataRoomId);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "DataRoom Deleted {DataRoomId}", DataRoomId);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized DataRoom Delete Attempt {DataRoomId} {ModuleId}", DataRoomId, ModuleId);
            }
            return Task.CompletedTask;
        }

        public Task<int> ExtractZipAsync(int DataRoomId, int ModuleId, int ZipFileId)
        {
            var authorized = _userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, ModuleId, PermissionNames.Edit);
            if (!authorized)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized DataRoom Zip Extract Attempt {DataRoomId} {ModuleId} {ZipFileId}", DataRoomId, ModuleId, ZipFileId);
            }

            return Task.FromResult(0);
        }
    }
}
