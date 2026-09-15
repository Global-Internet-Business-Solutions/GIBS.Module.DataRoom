using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GIBS.Module.DataRoom.Models;
using GIBS.Module.DataRoom.Repository;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Modules;
using Oqtane.Repository;
using Oqtane.Shared;

namespace GIBS.Module.DataRoom.Services
{
    public interface IDataRoomSubscriptionEventProcessor
    {
        SubscriptionEventProcessResult ProcessDataRoomSubscriptionEvents(int dataRoomId, DateTime utcThrough);
        string ProcessPendingSubscriptionEvents(DateTime utcThrough);
    }

    public class DataRoomSubscriptionEventProcessor : IDataRoomSubscriptionEventProcessor, ITransientService
    {
        private readonly IDataRoomSubscriptionEventRepository _subscriptionEventRepository;
        private readonly IDataRoomSubscriptionRepository _subscriptionRepository;
        private readonly IDataRoomRepository _dataRoomRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IUserRepository _userRepository;
        private readonly IFolderRepository _folderRepository;
        private readonly ILogManager _logger;

        public DataRoomSubscriptionEventProcessor(
            IDataRoomSubscriptionEventRepository subscriptionEventRepository,
            IDataRoomSubscriptionRepository subscriptionRepository,
            IDataRoomRepository dataRoomRepository,
            INotificationRepository notificationRepository,
            IUserRepository userRepository,
            IFolderRepository folderRepository,
            ILogManager logger)
        {
            _subscriptionEventRepository = subscriptionEventRepository;
            _subscriptionRepository = subscriptionRepository;
            _dataRoomRepository = dataRoomRepository;
            _notificationRepository = notificationRepository;
            _userRepository = userRepository;
            _folderRepository = folderRepository;
            _logger = logger;
        }

        public SubscriptionEventProcessResult ProcessDataRoomSubscriptionEvents(int dataRoomId, DateTime utcThrough)
        {
            var pendingEvents = _subscriptionEventRepository
                .GetSubscriptionEvents(dataRoomId)
                .Where(item => !item.IsProcessed && item.OccurredOn <= utcThrough)
                .OrderBy(item => item.OccurredOn)
                .ToList();

            return ProcessDataRoomSubscriptionEvents(dataRoomId, pendingEvents, utcThrough);
        }

        public string ProcessPendingSubscriptionEvents(DateTime utcThrough)
        {
            var pendingEvents = _subscriptionEventRepository.GetPendingSubscriptionEvents(utcThrough).ToList();
            if (!pendingEvents.Any())
            {
                return "No pending Data Room subscription events found.<br />";
            }

            var notes = new StringBuilder();
            foreach (var dataRoomEvents in pendingEvents.GroupBy(item => item.DataRoomId).OrderBy(item => item.Key))
            {
                var result = ProcessDataRoomSubscriptionEvents(dataRoomEvents.Key, dataRoomEvents.OrderBy(item => item.OccurredOn).ToList(), utcThrough);
                notes.Append(result.Message);
                notes.Append("<br />");
            }

            return notes.ToString();
        }

        private SubscriptionEventProcessResult ProcessDataRoomSubscriptionEvents(int dataRoomId, List<SubscriptionEvent> pendingEvents, DateTime utcThrough)
        {
            var dataRoom = _dataRoomRepository.GetDataRoom(dataRoomId, false);
            var result = new SubscriptionEventProcessResult
            {
                PendingEventCount = pendingEvents.Count,
                ProcessedOn = utcThrough
            };

            if (dataRoom == null)
            {
                result.Message = $"Data Room {dataRoomId} not found.";
                return result;
            }

            if (!pendingEvents.Any())
            {
                result.Message = $"No pending subscription events found for Data Room '{dataRoom.Name}'.";
                return result;
            }

            var folders = _folderRepository.GetFolders(dataRoom.SiteId).ToDictionary(item => item.FolderId);
            var subscriptions = _subscriptionRepository.GetSubscriptions(dataRoomId)
                .Where(item => item.IsActive && (item.EmailConfirmed || item.UserId.HasValue))
                .ToList();

            var notificationsQueued = 0;
            var notifiedSubscriptionIds = new HashSet<int>();
            var recipientEvents = new Dictionary<string, RecipientQueue>(StringComparer.OrdinalIgnoreCase);

            foreach (var subscription in subscriptions)
            {
                var email = GetSubscriptionEmail(subscription);
                if (string.IsNullOrWhiteSpace(email))
                {
                    continue;
                }

                var matchingEvents = pendingEvents
                    .Where(item => ShouldNotifySubscription(subscription, item, folders))
                    .OrderBy(item => item.OccurredOn)
                    .ToList();

                if (!matchingEvents.Any())
                {
                    continue;
                }

                if (!recipientEvents.TryGetValue(email, out var queue))
                {
                    queue = new RecipientQueue
                    {
                        Email = email,
                        DisplayName = GetSubscriptionDisplayName(subscription, email)
                    };
                    recipientEvents[email] = queue;
                }

                foreach (var subscriptionEvent in matchingEvents)
                {
                    queue.Events[subscriptionEvent.SubscriptionEventId] = subscriptionEvent;
                }

                notifiedSubscriptionIds.Add(subscription.SubscriptionId);
            }

            foreach (var recipient in recipientEvents.Values)
            {
                var notification = new Notification(
                    dataRoom.SiteId,
                    "Data Room",
                    string.Empty,
                    recipient.DisplayName,
                    recipient.Email,
                    $"Data Room Updates: {dataRoom.Name}",
                    BuildNotificationBody(dataRoom, recipient.Events.Values.OrderBy(item => item.OccurredOn).ToList()));

                _notificationRepository.AddNotification(notification);
                notificationsQueued++;
            }

            foreach (var subscriptionEvent in pendingEvents)
            {
                subscriptionEvent.IsProcessed = true;
                subscriptionEvent.ProcessedOn = result.ProcessedOn;
                _subscriptionEventRepository.UpdateSubscriptionEvent(subscriptionEvent);
            }

            foreach (var subscription in subscriptions.Where(item => notifiedSubscriptionIds.Contains(item.SubscriptionId)))
            {
                subscription.LastNotifiedOn = result.ProcessedOn;
                _subscriptionRepository.UpdateSubscription(subscription);
            }

            result.ProcessedEventCount = pendingEvents.Count;
            result.NotificationCount = notificationsQueued;
            result.SubscriberCount = recipientEvents.Count;
            result.Message = $"Processed {result.ProcessedEventCount} event(s) and queued {result.NotificationCount} notification(s) for {result.SubscriberCount} subscriber(s) in Data Room '{dataRoom.Name}'.";

            _logger.Log(LogLevel.Information, this, LogFunction.Other,
                "Subscription Event Queue Processed {DataRoomId} {ProcessedEventCount} {NotificationCount} {SubscriberCount}",
                dataRoomId, result.ProcessedEventCount, result.NotificationCount, result.SubscriberCount);

            return result;
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

        private string BuildNotificationBody(Models.DataRoom dataRoom, List<SubscriptionEvent> events)
        {
            var builder = new StringBuilder();
            builder.Append($"<p>The following file updates were queued for <strong>{System.Net.WebUtility.HtmlEncode(dataRoom.Name)}</strong>:</p>");
            builder.Append("<ul>");

            foreach (var subscriptionEvent in events)
            {
                var fileName = System.Net.WebUtility.HtmlEncode(subscriptionEvent.FileName ?? $"File #{subscriptionEvent.FileId}");
                var relativePath = System.Net.WebUtility.HtmlEncode(subscriptionEvent.RelativePath ?? string.Empty);
                var occurredOn = subscriptionEvent.OccurredOn.ToString("g");
                builder.Append($"<li><strong>{fileName}</strong>");

                if (!string.IsNullOrWhiteSpace(relativePath))
                {
                    builder.Append($" ({relativePath})");
                }

                builder.Append($" - {System.Net.WebUtility.HtmlEncode(subscriptionEvent.Action)} on {occurredOn}</li>");
            }

            builder.Append("</ul>");
            builder.Append($"<p>Visit the Data Room to view new files.</p>");
            return builder.ToString();
        }

        private class RecipientQueue
        {
            public string Email { get; set; }
            public string DisplayName { get; set; }
            public Dictionary<int, SubscriptionEvent> Events { get; } = new();
        }
    }
}
