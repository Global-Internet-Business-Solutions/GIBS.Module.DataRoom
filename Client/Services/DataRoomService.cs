using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Services;
using Oqtane.Shared;

namespace GIBS.Module.DataRoom.Services
{
    public interface IDataRoomService 
    {
        Task<List<Models.DataRoom>> GetDataRoomsAsync(int ModuleId);

        Task<Models.DataRoom> GetDataRoomAsync(int DataRoomId, int ModuleId);

        Task<Models.DataRoom> AddDataRoomAsync(Models.DataRoom DataRoom);

        Task<Models.DataRoom> UpdateDataRoomAsync(Models.DataRoom DataRoom);

        Task DeleteDataRoomAsync(int DataRoomId, int ModuleId);
    }

    public class DataRoomService : ServiceBase, IDataRoomService
    {
        public DataRoomService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string Apiurl => CreateApiUrl("DataRoom");

        public async Task<List<Models.DataRoom>> GetDataRoomsAsync(int ModuleId)
        {
            List<Models.DataRoom> DataRooms = await GetJsonAsync<List<Models.DataRoom>>(CreateAuthorizationPolicyUrl($"{Apiurl}?moduleid={ModuleId}", EntityNames.Module, ModuleId), Enumerable.Empty<Models.DataRoom>().ToList());
            return DataRooms.OrderBy(item => item.Name).ToList();
        }

        public async Task<Models.DataRoom> GetDataRoomAsync(int DataRoomId, int ModuleId)
        {
            return await GetJsonAsync<Models.DataRoom>(CreateAuthorizationPolicyUrl($"{Apiurl}/{DataRoomId}/{ModuleId}", EntityNames.Module, ModuleId));
        }

        public async Task<Models.DataRoom> AddDataRoomAsync(Models.DataRoom DataRoom)
        {
            return await PostJsonAsync<Models.DataRoom>(CreateAuthorizationPolicyUrl($"{Apiurl}", EntityNames.Module, DataRoom.ModuleId), DataRoom);
        }

        public async Task<Models.DataRoom> UpdateDataRoomAsync(Models.DataRoom DataRoom)
        {
            return await PutJsonAsync<Models.DataRoom>(CreateAuthorizationPolicyUrl($"{Apiurl}/{DataRoom.DataRoomId}", EntityNames.Module, DataRoom.ModuleId), DataRoom);
        }

        public async Task DeleteDataRoomAsync(int DataRoomId, int ModuleId)
        {
            await DeleteAsync(CreateAuthorizationPolicyUrl($"{Apiurl}/{DataRoomId}/{ModuleId}", EntityNames.Module, ModuleId));
        }
    }
}
