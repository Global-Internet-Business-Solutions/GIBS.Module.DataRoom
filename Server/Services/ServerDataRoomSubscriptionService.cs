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
    public class ServerDataRoomSubscriptionService : IDataRoomSubscriptionService
    {
        private readonly IDataRoomSubscriptionRepository _subscriptionRepository;
        private readonly IDataRoomRepository _dataRoomRepository;
        private readonly IUserPermissions _userPermissions;
        private readonly INotificationRepository _notificationRepository;
        private readonly ILogManager _logger;
        private readonly IHttpContextAccessor _accessor;
        private readonly Alias _alias;

        public ServerDataRoomSubscriptionService(
            IDataRoomSubscriptionRepository subscriptionRepository,
            IDataRoomRepository dataRoomRepository,
            IUserPermissions userPermissions,
            INotificationRepository notificationRepository,
            ITenantManager tenantManager,
            ILogManager logger,
            IHttpContextAccessor accessor)
        {
            _subscriptionRepository = subscriptionRepository;
            _dataRoomRepository = dataRoomRepository;
            _userPermissions = userPermissions;
            _notificationRepository = notificationRepository;
            _logger = logger;
            _accessor = accessor;
            _alias = tenantManager.GetAlias();
        }

        public Task<List<Subscription>> GetSubscriptionsAsync(int dataRoomId, int moduleId)
        {
            if (IsAuthorizedForDataRoom(dataRoomId, moduleId, PermissionNames.View))
            {
                return Task.FromResult(_subscriptionRepository.GetSubscriptions(dataRoomId).ToList());
            }

            _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Subscription Get Attempt {DataRoomId} {ModuleId}", dataRoomId, moduleId);
            return Task.FromResult<List<Subscription>>(null);
        }

        public Task<Subscription> GetSubscriptionAsync(int subscriptionId, int moduleId)
        {
            var subscription = _subscriptionRepository.GetSubscription(subscriptionId);
            if (subscription == null)
            {
                return Task.FromResult<Subscription>(null);
            }

            if (IsAuthorizedForDataRoom(subscription.DataRoomId, moduleId, PermissionNames.View))
            {
                return Task.FromResult(subscription);
            }

            _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Subscription Get Attempt {SubscriptionId} {ModuleId}", subscriptionId, moduleId);
            return Task.FromResult<Subscription>(null);
        }

        public Task<Subscription> AddSubscriptionAsync(Subscription subscription, int moduleId)
        {
            if (subscription == null || !IsAuthorizedForDataRoom(subscription.DataRoomId, moduleId, PermissionNames.View))
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Subscription Add Attempt {Subscription} {ModuleId}", subscription, moduleId);
                return Task.FromResult<Subscription>(null);
            }

            var normalizedEmail = (subscription.Email ?? string.Empty).Trim();
            if (!subscription.UserId.HasValue && string.IsNullOrWhiteSpace(normalizedEmail))
            {
                return Task.FromResult<Subscription>(null);
            }

            var existing = _subscriptionRepository
                .GetSubscriptions(subscription.DataRoomId)
                .FirstOrDefault(s =>
                    (subscription.UserId.HasValue && s.UserId == subscription.UserId) ||
                    (!subscription.UserId.HasValue && !string.IsNullOrWhiteSpace(normalizedEmail) && string.Equals(s.Email, normalizedEmail, System.StringComparison.OrdinalIgnoreCase)));

            if (existing != null)
            {
                existing.Email = string.IsNullOrWhiteSpace(normalizedEmail) ? existing.Email : normalizedEmail;
                existing.EmailConfirmed = subscription.EmailConfirmed;
                existing.NotifyOnUpload = subscription.NotifyOnUpload;
                existing.NotifyOnOverwrite = subscription.NotifyOnOverwrite;
                existing.IsActive = true;
                existing.UserId = subscription.UserId;
                existing.FolderId = subscription.FolderId;
                existing.IncludeSubfolders = subscription.IncludeSubfolders;

                // If email-based subscription and not confirmed, generate new token
                if (!subscription.UserId.HasValue && !string.IsNullOrWhiteSpace(normalizedEmail) && !existing.EmailConfirmed)
                {
                    existing.ConfirmationToken = GenerateConfirmationToken();
                    existing.ConfirmationExpiresOn = DateTime.UtcNow.AddDays(7);
                }

                existing = _subscriptionRepository.UpdateSubscription(existing);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Subscription Reactivated {Subscription}", existing);

                // Send confirmation email if needed
                if (!subscription.UserId.HasValue && !string.IsNullOrWhiteSpace(normalizedEmail) && !existing.EmailConfirmed)
                {
                    SendConfirmationEmail(existing);
                }

                return Task.FromResult(existing);
            }

            subscription.Email = normalizedEmail;

            // For email-based subscriptions (no UserId), generate confirmation token
            if (!subscription.UserId.HasValue && !string.IsNullOrWhiteSpace(normalizedEmail))
            {
                subscription.EmailConfirmed = false;
                subscription.ConfirmationToken = GenerateConfirmationToken();
                subscription.ConfirmationExpiresOn = DateTime.UtcNow.AddDays(7);
            }
            else if (subscription.UserId.HasValue)
            {
                // Logged-in users don't need email confirmation
                subscription.EmailConfirmed = true;
            }

            subscription = _subscriptionRepository.AddSubscription(subscription);
            _logger.Log(LogLevel.Information, this, LogFunction.Create, "Subscription Added {Subscription}", subscription);

            // Send confirmation email if needed
            if (!subscription.UserId.HasValue && !string.IsNullOrWhiteSpace(normalizedEmail) && !string.IsNullOrWhiteSpace(subscription.ConfirmationToken))
            {
                SendConfirmationEmail(subscription);
            }

            return Task.FromResult(subscription);
        }

        public Task<Subscription> UpdateSubscriptionAsync(Subscription subscription, int moduleId)
        {
            if (subscription != null && IsAuthorizedForDataRoom(subscription.DataRoomId, moduleId, PermissionNames.Edit))
            {
                subscription = _subscriptionRepository.UpdateSubscription(subscription);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Subscription Updated {Subscription}", subscription);
                return Task.FromResult(subscription);
            }

            _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Subscription Update Attempt {Subscription} {ModuleId}", subscription, moduleId);
            return Task.FromResult<Subscription>(null);
        }

        public Task DeleteSubscriptionAsync(int subscriptionId, int moduleId)
        {
            var subscription = _subscriptionRepository.GetSubscription(subscriptionId, false);
            if (subscription != null && IsAuthorizedForDataRoom(subscription.DataRoomId, moduleId, PermissionNames.Edit))
            {
                _subscriptionRepository.DeleteSubscription(subscriptionId);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Subscription Deleted {SubscriptionId}", subscriptionId);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Subscription Delete Attempt {SubscriptionId} {ModuleId}", subscriptionId, moduleId);
            }

            return Task.CompletedTask;
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

        private string GenerateConfirmationToken()
        {
            return Guid.NewGuid().ToString("N");
        }

        private void SendConfirmationEmail(Subscription subscription)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(subscription.Email) || string.IsNullOrWhiteSpace(subscription.ConfirmationToken))
                {
                    return;
                }

                // Build confirmation URL
                var confirmationUrl = $"{_alias.BaseUrl.TrimEnd('/')}/dataroom/confirm-email?token={Uri.EscapeDataString(subscription.ConfirmationToken)}";

                var subject = "Please confirm your Data Room subscription";
                var body = $@"<p>Thank you for subscribing to our Data Room notifications.</p>
<p>To confirm your subscription and start receiving notifications, please click the link below:</p>
<p><a href=""{confirmationUrl}"">Confirm Your Subscription</a></p>
<p>If you did not subscribe to this Data Room, you can safely ignore this email.</p>
<p>This confirmation link will expire in 7 days.</p>";

                var notification = new Notification(
                    _alias.SiteId,
                    "Data Room",           // fromDisplayName
                    string.Empty,          // fromEmail (system notification)
                    subscription.Email,    // toDisplayName
                    subscription.Email,    // toEmail
                    subject,
                    body);

                _notificationRepository.AddNotification(notification);
                _logger.Log(LogLevel.Information, this, LogFunction.Other,
                    "Confirmation Email Queued {SubscriptionId} {Email}",
                    subscription.SubscriptionId, subscription.Email);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Other, ex,
                    "Error sending confirmation email for subscription {SubscriptionId}",
                    subscription.SubscriptionId);
            }
        }

        public Task<bool> ConfirmSubscriptionEmailAsync(string token, int moduleId)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return Task.FromResult(false);
            }

            var subscriptions = _subscriptionRepository.GetSubscriptionsAll();
            var subscription = subscriptions.FirstOrDefault(s => s.ConfirmationToken == token);

            if (subscription == null)
            {
                _logger.Log(LogLevel.Warning, this, LogFunction.Other, "Invalid confirmation token");
                return Task.FromResult(false);
            }

            // Check if token is expired (7 days)
            if (subscription.ConfirmationExpiresOn.HasValue && subscription.ConfirmationExpiresOn < DateTime.UtcNow)
            {
                _logger.Log(LogLevel.Warning, this, LogFunction.Other, "Confirmation token expired for subscription {SubscriptionId}", subscription.SubscriptionId);
                return Task.FromResult(false);
            }

            subscription.EmailConfirmed = true;
            subscription.ConfirmationToken = null;
            subscription.ConfirmationExpiresOn = null;

            try
            {
                _subscriptionRepository.UpdateSubscription(subscription);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Subscription email confirmed {SubscriptionId}", subscription.SubscriptionId);
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Other, ex, "Error confirming subscription email {SubscriptionId}", subscription.SubscriptionId);
                return Task.FromResult(false);
            }
        }
    }
}
