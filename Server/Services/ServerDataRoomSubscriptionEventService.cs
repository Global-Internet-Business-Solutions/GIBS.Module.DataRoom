using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Repository;
using Oqtane.Security;
using Oqtane.Shared;
using GIBS.Module.DataRoom.Models;
using GIBS.Module.DataRoom.Repository;

namespace GIBS.Module.DataRoom.Services
{
    public class ServerDataRoomSubscriptionEventService : IDataRoomSubscriptionEventService
    {
        private readonly IDataRoomSubscriptionEventRepository _subscriptionEventRepository;
        private readonly IDataRoomSubscriptionEventProcessor _subscriptionEventProcessor;
        private readonly IDataRoomSubscriptionRepository _subscriptionRepository;
        private readonly IDataRoomRepository _dataRoomRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IUserRepository _userRepository;
        private readonly IFolderRepository _folderRepository;
        private readonly IUserPermissions _userPermissions;
        private readonly ILogManager _logger;
        private readonly IHttpContextAccessor _accessor;
        private readonly Alias _alias;

        public ServerDataRoomSubscriptionEventService(
            IDataRoomSubscriptionEventRepository subscriptionEventRepository,
            IDataRoomSubscriptionEventProcessor subscriptionEventProcessor,
            IDataRoomSubscriptionRepository subscriptionRepository,
            IDataRoomRepository dataRoomRepository,
            INotificationRepository notificationRepository,
            IUserRepository userRepository,
            IFolderRepository folderRepository,
            IUserPermissions userPermissions,
            ITenantManager tenantManager,
            ILogManager logger,
            IHttpContextAccessor accessor)
        {
            _subscriptionEventRepository = subscriptionEventRepository;
            _subscriptionEventProcessor = subscriptionEventProcessor;
            _subscriptionRepository = subscriptionRepository;
            _dataRoomRepository = dataRoomRepository;
            _notificationRepository = notificationRepository;
            _userRepository = userRepository;
            _folderRepository = folderRepository;
            _userPermissions = userPermissions;
            _logger = logger;
            _accessor = accessor;
            _alias = tenantManager.GetAlias();
        }

        public Task<List<SubscriptionEvent>> GetSubscriptionEventsAsync(int dataRoomId, int moduleId)
        {
            if (IsAuthorizedForDataRoom(dataRoomId, moduleId, PermissionNames.Edit))
            {
                return Task.FromResult(_subscriptionEventRepository.GetSubscriptionEvents(dataRoomId).ToList());
            }

            _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Subscription Event Get Attempt {DataRoomId} {ModuleId}", dataRoomId, moduleId);
            return Task.FromResult<List<SubscriptionEvent>>(null);
        }

        public Task<SubscriptionEventProcessResult> ProcessSubscriptionEventsAsync(int dataRoomId, int moduleId)
        {
            if (!IsAuthorizedForDataRoom(dataRoomId, moduleId, PermissionNames.Edit))
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Subscription Event Process Attempt {DataRoomId} {ModuleId}", dataRoomId, moduleId);
                return Task.FromResult<SubscriptionEventProcessResult>(null);
            }

            var result = _subscriptionEventProcessor.ProcessDataRoomSubscriptionEvents(dataRoomId, DateTime.UtcNow);
            return Task.FromResult(result);
        }

        private bool IsAuthorizedForDataRoom(int dataRoomId, int moduleId, string permissionName)
        {
            if (!_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, moduleId, permissionName))
            {
                return false;
            }

            var dataRoom = _dataRoomRepository.GetDataRoom(dataRoomId, false);
            return dataRoom != null && dataRoom.ModuleId == moduleId;
        }

        private string GetSubscriptionEmail(Subscription subscription)
        {
            if (!string.IsNullOrWhiteSpace(subscription.Email))
            {
                return subscription.Email.Trim();
            }

            if (!subscription.UserId.HasValue)
            {
                return string.Empty;
            }

            var user = _userRepository.GetUser(subscription.UserId.Value);
            return user?.Email?.Trim() ?? string.Empty;
        }

        private string GetSubscriptionDisplayName(Subscription subscription, string fallbackEmail)
        {
            if (subscription.UserId.HasValue)
            {
                var user = _userRepository.GetUser(subscription.UserId.Value);
                if (!string.IsNullOrWhiteSpace(user?.DisplayName))
                {
                    return user.DisplayName;
                }
            }

            return fallbackEmail;
        }

        private static bool ShouldNotifySubscription(Subscription subscription, SubscriptionEvent subscriptionEvent, Dictionary<int, Folder> folders)
        {
            if (subscriptionEvent == null)
            {
                return false;
            }

            if (string.Equals(subscriptionEvent.Action, "Upload", StringComparison.OrdinalIgnoreCase) && !subscription.NotifyOnUpload)
            {
                return false;
            }

            if (string.Equals(subscriptionEvent.Action, "Overwrite", StringComparison.OrdinalIgnoreCase) && !subscription.NotifyOnOverwrite)
            {
                return false;
            }

            if (!string.Equals(subscriptionEvent.Action, "Upload", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(subscriptionEvent.Action, "Overwrite", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (subscription.FolderId <= 0)
            {
                return true;
            }

            if (subscriptionEvent.FolderId <= 0)
            {
                return false;
            }

            if (subscriptionEvent.FolderId == subscription.FolderId)
            {
                return true;
            }

            return subscription.IncludeSubfolders && IsDescendantFolder(subscriptionEvent.FolderId, subscription.FolderId, folders);
        }

        private static bool IsDescendantFolder(int folderId, int ancestorFolderId, Dictionary<int, Folder> folders)
        {
            var visited = new HashSet<int>();
            var currentFolderId = folderId;

            while (folders.TryGetValue(currentFolderId, out var folder) && visited.Add(currentFolderId))
            {
                if (!folder.ParentId.HasValue)
                {
                    return false;
                }

                if (folder.ParentId.Value == ancestorFolderId)
                {
                    return true;
                }

                currentFolderId = folder.ParentId.Value;
            }

            return false;
        }


        private class RecipientQueue
        {
            public string Email { get; set; }
            public string DisplayName { get; set; }
            public Dictionary<int, SubscriptionEvent> Events { get; } = new();
        }
    }
}
