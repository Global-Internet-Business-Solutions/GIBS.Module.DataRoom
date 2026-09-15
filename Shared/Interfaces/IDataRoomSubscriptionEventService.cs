using System.Collections.Generic;
using System.Threading.Tasks;
using GIBS.Module.DataRoom.Models;

namespace GIBS.Module.DataRoom.Services
{
    public interface IDataRoomSubscriptionEventService
    {
        Task<List<SubscriptionEvent>> GetSubscriptionEventsAsync(int dataRoomId, int moduleId);
        Task<SubscriptionEventProcessResult> ProcessSubscriptionEventsAsync(int dataRoomId, int moduleId);
    }
}
