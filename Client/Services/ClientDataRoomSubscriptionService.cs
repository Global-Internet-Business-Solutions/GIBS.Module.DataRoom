using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Services;
using Oqtane.Shared;
using GIBS.Module.DataRoom.Models;

namespace GIBS.Module.DataRoom.Services
{
    public class ClientDataRoomSubscriptionService : ServiceBase, IDataRoomSubscriptionService
    {
        public ClientDataRoomSubscriptionService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string ApiUrl => CreateApiUrl("DataRoomSubscription");

        public async Task<List<Subscription>> GetSubscriptionsAsync(int dataRoomId, int moduleId)
        {
            return await GetJsonAsync<List<Subscription>>(
                CreateAuthorizationPolicyUrl($"{ApiUrl}?dataroomid={dataRoomId}&moduleid={moduleId}", EntityNames.Module, moduleId));
        }

        public async Task<Subscription> GetSubscriptionAsync(int subscriptionId, int moduleId)
        {
            return await GetJsonAsync<Subscription>(
                CreateAuthorizationPolicyUrl($"{ApiUrl}/{subscriptionId}/{moduleId}", EntityNames.Module, moduleId));
        }

        public async Task<Subscription> AddSubscriptionAsync(Subscription subscription, int moduleId)
        {
            return await PostJsonAsync<Subscription>(
                CreateAuthorizationPolicyUrl($"{ApiUrl}?moduleid={moduleId}", EntityNames.Module, moduleId), subscription);
        }

        public async Task<Subscription> UpdateSubscriptionAsync(Subscription subscription, int moduleId)
        {
            return await PutJsonAsync<Subscription>(
                CreateAuthorizationPolicyUrl($"{ApiUrl}/{subscription.SubscriptionId}?moduleid={moduleId}", EntityNames.Module, moduleId), subscription);
        }

        public async Task DeleteSubscriptionAsync(int subscriptionId, int moduleId)
        {
            await DeleteAsync(CreateAuthorizationPolicyUrl($"{ApiUrl}/{subscriptionId}/{moduleId}", EntityNames.Module, moduleId));
        }
    }
}
