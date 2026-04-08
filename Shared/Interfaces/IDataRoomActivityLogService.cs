using System.Collections.Generic;
using System.Threading.Tasks;
using GIBS.Module.DataRoom.Models;

namespace GIBS.Module.DataRoom.Services
{
    public interface IDataRoomActivityLogService
    {
        Task<List<DataRoomActivityLog>> GetActivityLogsAsync(int dataRoomId, int moduleId);

        Task<DataRoomActivityLog> AddActivityLogAsync(DataRoomActivityLog activityLog, int moduleId);
    }
}
