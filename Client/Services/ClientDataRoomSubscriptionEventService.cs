using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Services;
using Oqtane.Shared;
using GIBS.Module.DataRoom.Models;

namespace GIBS.Module.DataRoom.Services
{
    public class ClientDataRoomSubscriptionEventService : ServiceBase, IDataRoomSubscriptionEventService
    {
        public ClientDataRoomSubscriptionEventService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string ApiUrl => CreateApiUrl("DataRoomSubscriptionEvent");

        public async Task<List<SubscriptionEvent>> GetSubscriptionEventsAsync(int dataRoomId, int moduleId)
        {
            return await GetJsonAsync<List<SubscriptionEvent>>(
                CreateAuthorizationPolicyUrl($"{ApiUrl}?dataroomid={dataRoomId}&moduleid={moduleId}", EntityNames.Module, moduleId));
        }

        public async Task<SubscriptionEventProcessResult> ProcessSubscriptionEventsAsync(int dataRoomId, int moduleId)
        {
            return await PostJsonAsync<int, SubscriptionEventProcessResult>(
                CreateAuthorizationPolicyUrl($"{ApiUrl}/process?dataroomid={dataRoomId}&moduleid={moduleId}", EntityNames.Module, moduleId), dataRoomId);
        }
    }
}
