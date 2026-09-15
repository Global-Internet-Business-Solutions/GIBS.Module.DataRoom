using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Oqtane.Modules;
using GIBS.Module.DataRoom.Models;

namespace GIBS.Module.DataRoom.Repository
{
    public interface IDataRoomSubscriptionEventRepository
    {
        IEnumerable<SubscriptionEvent> GetSubscriptionEvents(int dataRoomId);
        IEnumerable<SubscriptionEvent> GetPendingSubscriptionEvents(DateTime utcThrough);
        SubscriptionEvent GetSubscriptionEvent(int subscriptionEventId);
        SubscriptionEvent GetSubscriptionEvent(int subscriptionEventId, bool tracking);
        SubscriptionEvent AddSubscriptionEvent(SubscriptionEvent subscriptionEvent);
        SubscriptionEvent UpdateSubscriptionEvent(SubscriptionEvent subscriptionEvent);
    }

    public class DataRoomSubscriptionEventRepository : IDataRoomSubscriptionEventRepository, ITransientService
    {
        private readonly IDbContextFactory<DataRoomContext> _factory;

        public DataRoomSubscriptionEventRepository(IDbContextFactory<DataRoomContext> factory)
        {
            _factory = factory;
        }

        public IEnumerable<SubscriptionEvent> GetSubscriptionEvents(int dataRoomId)
        {
            using var db = _factory.CreateDbContext();
            return db.DataRoomSubscriptionEvent
                .Where(item => item.DataRoomId == dataRoomId)
                .OrderByDescending(item => item.OccurredOn)
                .ToList();
        }

        public IEnumerable<SubscriptionEvent> GetPendingSubscriptionEvents(DateTime utcThrough)
        {
            using var db = _factory.CreateDbContext();
            return db.DataRoomSubscriptionEvent
                .Where(item => !item.IsProcessed && item.OccurredOn <= utcThrough)
                .OrderBy(item => item.OccurredOn)
                .ToList();
        }

        public SubscriptionEvent GetSubscriptionEvent(int subscriptionEventId)
        {
            return GetSubscriptionEvent(subscriptionEventId, true);
        }

        public SubscriptionEvent GetSubscriptionEvent(int subscriptionEventId, bool tracking)
        {
            using var db = _factory.CreateDbContext();
            if (tracking)
            {
                return db.DataRoomSubscriptionEvent.Find(subscriptionEventId);
            }

            return db.DataRoomSubscriptionEvent.AsNoTracking().FirstOrDefault(item => item.SubscriptionEventId == subscriptionEventId);
        }

        public SubscriptionEvent AddSubscriptionEvent(SubscriptionEvent subscriptionEvent)
        {
            using var db = _factory.CreateDbContext();
            db.DataRoomSubscriptionEvent.Add(subscriptionEvent);
            db.SaveChanges();
            return subscriptionEvent;
        }

        public SubscriptionEvent UpdateSubscriptionEvent(SubscriptionEvent subscriptionEvent)
        {
            using var db = _factory.CreateDbContext();
            db.Entry(subscriptionEvent).State = EntityState.Modified;
            db.SaveChanges();
            return subscriptionEvent;
        }
    }
}
