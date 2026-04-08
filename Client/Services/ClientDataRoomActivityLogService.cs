using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Services;
using Oqtane.Shared;
using GIBS.Module.DataRoom.Models;

namespace GIBS.Module.DataRoom.Services
{
    public class ClientDataRoomActivityLogService : ServiceBase, IDataRoomActivityLogService
    {
        public ClientDataRoomActivityLogService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string Apiurl => CreateApiUrl("DataRoomActivityLog");

        public async Task<List<DataRoomActivityLog>> GetActivityLogsAsync(int dataRoomId, int moduleId)
        {
            return await GetJsonAsync<List<DataRoomActivityLog>>(
                CreateAuthorizationPolicyUrl($"{Apiurl}?dataroomid={dataRoomId}&moduleid={moduleId}", EntityNames.Module, moduleId));
        }

        public async Task<DataRoomActivityLog> AddActivityLogAsync(DataRoomActivityLog activityLog, int moduleId)
        {
            return await PostJsonAsync<DataRoomActivityLog>(
                CreateAuthorizationPolicyUrl($"{Apiurl}?moduleid={moduleId}", EntityNames.Module, moduleId), activityLog);
        }
    }
}
