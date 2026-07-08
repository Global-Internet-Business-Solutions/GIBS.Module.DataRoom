using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Oqtane.Modules;
using GIBS.Module.DataRoom.Models;

namespace GIBS.Module.DataRoom.Repository
{
    public interface IDataRoomSubscriptionRepository
    {
        IEnumerable<Subscription> GetSubscriptions(int dataRoomId);
        IEnumerable<Subscription> GetSubscriptionsAll();
        Subscription GetSubscription(int subscriptionId);
        Subscription GetSubscription(int subscriptionId, bool tracking);
        Subscription AddSubscription(Subscription subscription);
        Subscription UpdateSubscription(Subscription subscription);
        void DeleteSubscription(int subscriptionId);
    }

    public class DataRoomSubscriptionRepository : IDataRoomSubscriptionRepository, ITransientService
    {
        private readonly IDbContextFactory<DataRoomContext> _factory;

        public DataRoomSubscriptionRepository(IDbContextFactory<DataRoomContext> factory)
        {
            _factory = factory;
        }

        public IEnumerable<Subscription> GetSubscriptions(int dataRoomId)
        {
            using var db = _factory.CreateDbContext();
            return db.DataRoomSubscription
                .Where(item => item.DataRoomId == dataRoomId)
                .OrderBy(item => item.Email)
                .ToList();
        }

        public IEnumerable<Subscription> GetSubscriptionsAll()
        {
            using var db = _factory.CreateDbContext();
            return db.DataRoomSubscription
                .OrderBy(item => item.Email)
                .ToList();
        }

        public Subscription GetSubscription(int subscriptionId)
        {
            return GetSubscription(subscriptionId, true);
        }

        public Subscription GetSubscription(int subscriptionId, bool tracking)
        {
            using var db = _factory.CreateDbContext();
            if (tracking)
            {
                return db.DataRoomSubscription.Find(subscriptionId);
            }

            return db.DataRoomSubscription.AsNoTracking().FirstOrDefault(item => item.SubscriptionId == subscriptionId);
        }

        public Subscription AddSubscription(Subscription subscription)
        {
            using var db = _factory.CreateDbContext();
            db.DataRoomSubscription.Add(subscription);
            db.SaveChanges();
            return subscription;
        }

        public Subscription UpdateSubscription(Subscription subscription)
        {
            using var db = _factory.CreateDbContext();
            db.Entry(subscription).State = EntityState.Modified;
            db.SaveChanges();
            return subscription;
        }

        public void DeleteSubscription(int subscriptionId)
        {
            using var db = _factory.CreateDbContext();
            var subscription = db.DataRoomSubscription.Find(subscriptionId);
            if (subscription != null)
            {
                db.DataRoomSubscription.Remove(subscription);
                db.SaveChanges();
            }
        }
    }
}
