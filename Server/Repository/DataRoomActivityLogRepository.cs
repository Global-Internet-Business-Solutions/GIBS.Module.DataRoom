using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Oqtane.Modules;
using GIBS.Module.DataRoom.Models;

namespace GIBS.Module.DataRoom.Repository
{
    public interface IDataRoomActivityLogRepository
    {
        IEnumerable<DataRoomActivityLog> GetActivityLogs(int dataRoomId);
        DataRoomActivityLog AddActivityLog(DataRoomActivityLog activityLog);
    }

    public class DataRoomActivityLogRepository : IDataRoomActivityLogRepository, ITransientService
    {
        private readonly IDbContextFactory<DataRoomContext> _factory;

        public DataRoomActivityLogRepository(IDbContextFactory<DataRoomContext> factory)
        {
            _factory = factory;
        }

        public IEnumerable<DataRoomActivityLog> GetActivityLogs(int dataRoomId)
        {
            using var db = _factory.CreateDbContext();
            return db.DataRoomActivityLog
                .Where(item => item.DataRoomId == dataRoomId)
                .OrderByDescending(item => item.Timestamp)
                .ToList();
        }

        public DataRoomActivityLog AddActivityLog(DataRoomActivityLog activityLog)
        {
            using var db = _factory.CreateDbContext();
            db.DataRoomActivityLog.Add(activityLog);
            db.SaveChanges();
            return activityLog;
        }
    }
}
