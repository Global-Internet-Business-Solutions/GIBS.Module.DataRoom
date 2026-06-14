using System.Collections.Generic;
using System.Threading.Tasks;
using GIBS.Module.DataRoom.Models;

namespace GIBS.Module.DataRoom.Services
{
    public interface IDataRoomSubscriptionService
    {
        Task<List<Subscription>> GetSubscriptionsAsync(int dataRoomId, int moduleId);

        Task<Subscription> GetSubscriptionAsync(int subscriptionId, int moduleId);

        Task<Subscription> AddSubscriptionAsync(Subscription subscription, int moduleId);

        Task<Subscription> UpdateSubscriptionAsync(Subscription subscription, int moduleId);

        Task DeleteSubscriptionAsync(int subscriptionId, int moduleId);
    }
}
