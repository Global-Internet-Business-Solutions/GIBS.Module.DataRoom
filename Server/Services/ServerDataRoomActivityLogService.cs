using System;
using System.Collections.Generic;
using System.Linq;
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
    public class ServerDataRoomActivityLogService : IDataRoomActivityLogService
    {
        private readonly IDataRoomActivityLogRepository _activityLogRepository;
        private readonly IDataRoomRepository _dataRoomRepository;
        private readonly IDataRoomSubscriptionEventRepository _subscriptionEventRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IFileRepository _fileRepository;
        private readonly IFolderRepository _folderRepository;
        private readonly IUserPermissions _userPermissions;
        private readonly ILogManager _logger;
        private readonly IHttpContextAccessor _accessor;
        private readonly Alias _alias;

        public ServerDataRoomActivityLogService(
            IDataRoomActivityLogRepository activityLogRepository,
            IDataRoomRepository dataRoomRepository,
            IDataRoomSubscriptionEventRepository subscriptionEventRepository,
            INotificationRepository notificationRepository,
            IFileRepository fileRepository,
            IFolderRepository folderRepository,
            IUserPermissions userPermissions,
            ITenantManager tenantManager,
            ILogManager logger,
            IHttpContextAccessor accessor)
        {
            _activityLogRepository = activityLogRepository;
            _dataRoomRepository = dataRoomRepository;
            _subscriptionEventRepository = subscriptionEventRepository;
            _notificationRepository = notificationRepository;
            _fileRepository = fileRepository;
            _folderRepository = folderRepository;
            _userPermissions = userPermissions;
            _logger = logger;
            _accessor = accessor;
            _alias = tenantManager.GetAlias();
        }

        public Task<List<DataRoomActivityLog>> GetActivityLogsAsync(int dataRoomId, int moduleId)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, moduleId, PermissionNames.Edit))
            {
                return Task.FromResult(_activityLogRepository.GetActivityLogs(dataRoomId).ToList());
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized ActivityLog Get Attempt {DataRoomId} {ModuleId}", dataRoomId, moduleId);
                return Task.FromResult<List<DataRoomActivityLog>>(null);
            }
        }

        public Task<DataRoomActivityLog> AddActivityLogAsync(DataRoomActivityLog activityLog, int moduleId)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, moduleId, PermissionNames.View))
            {
                activityLog = _activityLogRepository.AddActivityLog(activityLog);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "ActivityLog Added {ActivityLog}", activityLog);

                if (activityLog.Action == "Upload" && activityLog.FileId > 0)
                {
                    SendUploadNotifications(activityLog);
                    QueueSubscriptionEvent(activityLog);
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized ActivityLog Add Attempt {ActivityLog}", activityLog);
                activityLog = null;
            }
            return Task.FromResult(activityLog);
        }

        private void SendUploadNotifications(DataRoomActivityLog activityLog)
        {
            var dataRoom = _dataRoomRepository.GetDataRoom(activityLog.DataRoomId);
            if (dataRoom == null || string.IsNullOrWhiteSpace(dataRoom.NotificationEmails)) return;

            var file = _fileRepository.GetFile(activityLog.FileId);
            var fileName = file?.Name ?? $"File #{activityLog.FileId}";

            var subject = $"New Document Uploaded: {dataRoom.Name}";
            var body = $"A new document <strong>{fileName}</strong> has been uploaded to the Data Room <strong>{dataRoom.Name}</strong>.";

            foreach (var raw in dataRoom.NotificationEmails.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                var email = raw.Trim();
                if (string.IsNullOrEmpty(email)) continue;

                var notification = new Notification(
                    _alias.SiteId,
                    "Data Room",
                    string.Empty,
                    email,
                    email,
                    subject,
                    body);

                _notificationRepository.AddNotification(notification);
                _logger.Log(LogLevel.Information, this, LogFunction.Other,
                    "Upload Notification Queued {DataRoomId} {FileName} {Email}",
                    dataRoom.DataRoomId, fileName, email);
            }
        }

        private void QueueSubscriptionEvent(DataRoomActivityLog activityLog)
        {
            try
            {
                var dataRoom = _dataRoomRepository.GetDataRoom(activityLog.DataRoomId);
                if (dataRoom == null || !dataRoom.EnableSubscription || !dataRoom.NotifySubscriptionOnNewFile)
                {
                    return;
                }

                var file = _fileRepository.GetFile(activityLog.FileId);
                var folderId = file?.FolderId ?? 0;
                var folder = folderId > 0 ? _folderRepository.GetFolder(folderId) : null;
                var fileName = file?.Name ?? $"File #{activityLog.FileId}";
                var relativePath = BuildRelativePath(folder, fileName);

                var subscriptionEvent = new SubscriptionEvent
                {
                    DataRoomId = activityLog.DataRoomId,
                    FileId = activityLog.FileId,
                    FolderId = folderId,
                    Action = activityLog.Action,
                    OccurredOn = activityLog.Timestamp == default ? DateTime.UtcNow : activityLog.Timestamp,
                    CreatedByUserId = activityLog.UserId,
                    FileName = fileName,
                    RelativePath = relativePath,
                    IsProcessed = false,
                    ProcessedOn = null
                };

                subscriptionEvent = _subscriptionEventRepository.AddSubscriptionEvent(subscriptionEvent);
                _logger.Log(LogLevel.Information, this, LogFunction.Create,
                    "Subscription Event Queued {SubscriptionEventId} {DataRoomId} {FileId}",
                    subscriptionEvent.SubscriptionEventId, subscriptionEvent.DataRoomId, subscriptionEvent.FileId);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Other, ex,
                    "Error queueing subscription event {DataRoomId} {FileId}",
                    activityLog.DataRoomId, activityLog.FileId);
            }
        }

        private static string BuildRelativePath(Folder folder, string fileName)
        {
            if (folder == null)
            {
                return fileName;
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                return folder.Path ?? string.Empty;
            }

            return string.IsNullOrWhiteSpace(folder.Path)
                ? fileName
                : Utilities.UrlCombine(folder.Path, fileName);
        }
    }
}
