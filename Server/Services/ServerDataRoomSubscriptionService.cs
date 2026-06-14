using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Models;
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
        private readonly ILogManager _logger;
        private readonly IHttpContextAccessor _accessor;
        private readonly Alias _alias;

        public ServerDataRoomSubscriptionService(
            IDataRoomSubscriptionRepository subscriptionRepository,
            IDataRoomRepository dataRoomRepository,
            IUserPermissions userPermissions,
            ITenantManager tenantManager,
            ILogManager logger,
            IHttpContextAccessor accessor)
        {
            _subscriptionRepository = subscriptionRepository;
            _dataRoomRepository = dataRoomRepository;
            _userPermissions = userPermissions;
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

                existing = _subscriptionRepository.UpdateSubscription(existing);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Subscription Reactivated {Subscription}", existing);
                return Task.FromResult(existing);
            }

            subscription.Email = normalizedEmail;
            subscription = _subscriptionRepository.AddSubscription(subscription);
            _logger.Log(LogLevel.Information, this, LogFunction.Create, "Subscription Added {Subscription}", subscription);
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
    }
}
