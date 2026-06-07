using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Services;
using Oqtane.Shared;

namespace GIBS.Module.DataRoom.Services
{

    public class ClientDataRoomService : ServiceBase, IDataRoomService
    {
        public ClientDataRoomService(HttpClient http, SiteState siteState) : base(http, siteState) { }

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

        public async Task<int> ExtractZipAsync(int DataRoomId, int ModuleId, int ZipFileId)
        {
            var url = CreateAuthorizationPolicyUrl($"{Apiurl}/extract/{DataRoomId}/{ModuleId}/{ZipFileId}", EntityNames.Module, ModuleId);
            var response = await GetHttpClient().PutAsync(url, null);

            if (!response.IsSuccessStatusCode)
            {
                var details = await response.Content.ReadAsStringAsync();
                throw new System.InvalidOperationException($"ZIP extract failed ({(int)response.StatusCode}): {details}");
            }

            if (url.Contains("/api/", System.StringComparison.OrdinalIgnoreCase) &&
                !(response.RequestMessage?.RequestUri?.AbsolutePath?.Contains("/api/", System.StringComparison.OrdinalIgnoreCase) ?? false))
            {
                throw new System.InvalidOperationException("ZIP extract request was not routed to an API endpoint.");
            }

            var mediaType = response.Content?.Headers?.ContentType?.MediaType;
            if (!string.IsNullOrEmpty(mediaType) && mediaType.Contains("text/html", System.StringComparison.OrdinalIgnoreCase))
            {
                throw new System.InvalidOperationException("ZIP extract returned HTML instead of API response.");
            }

            if (!response.Headers.TryGetValues("X-DataRoom-Extract", out var values) || !values.Contains("1"))
            {
                throw new System.InvalidOperationException("ZIP extract response did not include the expected verification header.");
            }

            if (!response.Headers.TryGetValues("X-DataRoom-Extract-Version", out var versionValues) || !versionValues.Contains("3"))
            {
                throw new System.InvalidOperationException("ZIP extract endpoint is outdated. Restart the server and refresh the browser.");
            }

            if (!response.Headers.TryGetValues("X-DataRoom-Extract-Files", out var fileValues) || !int.TryParse(fileValues.FirstOrDefault(), out var extractedCount))
            {
                throw new System.InvalidOperationException("ZIP extract response did not include the extracted file count.");
            }

            if (extractedCount <= 0)
            {
                throw new System.InvalidOperationException("ZIP extract completed but extracted 0 files.");
            }

            return extractedCount;
        }
    }
}
