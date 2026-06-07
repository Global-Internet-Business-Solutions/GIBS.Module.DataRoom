using System.Collections.Generic;
using System.Threading.Tasks;

namespace GIBS.Module.DataRoom.Services
{
    public interface IDataRoomService 
    {
        Task<List<Models.DataRoom>> GetDataRoomsAsync(int ModuleId);

        Task<Models.DataRoom> GetDataRoomAsync(int DataRoomId, int ModuleId);

        Task<Models.DataRoom> AddDataRoomAsync(Models.DataRoom DataRoom);

        Task<Models.DataRoom> UpdateDataRoomAsync(Models.DataRoom DataRoom);

        Task DeleteDataRoomAsync(int DataRoomId, int ModuleId);

        Task<int> ExtractZipAsync(int DataRoomId, int ModuleId, int ZipFileId);
    }
}
