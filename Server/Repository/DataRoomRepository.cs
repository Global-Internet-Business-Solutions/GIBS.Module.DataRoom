using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using Oqtane.Modules;

namespace GIBS.Module.DataRoom.Repository
{
    public interface IDataRoomRepository
    {
        IEnumerable<Models.DataRoom> GetDataRooms(int ModuleId);
        Models.DataRoom GetDataRoom(int DataRoomId);
        Models.DataRoom GetDataRoom(int DataRoomId, bool tracking);
        Models.DataRoom AddDataRoom(Models.DataRoom DataRoom);
        Models.DataRoom UpdateDataRoom(Models.DataRoom DataRoom);
        void DeleteDataRoom(int DataRoomId);
    }

    public class DataRoomRepository : IDataRoomRepository, ITransientService
    {
        private readonly IDbContextFactory<DataRoomContext> _factory;

        public DataRoomRepository(IDbContextFactory<DataRoomContext> factory)
        {
            _factory = factory;
        }

        public IEnumerable<Models.DataRoom> GetDataRooms(int ModuleId)
        {
            using var db = _factory.CreateDbContext();
            return db.DataRoom.Where(item => item.ModuleId == ModuleId).ToList();
        }

        public Models.DataRoom GetDataRoom(int DataRoomId)
        {
            return GetDataRoom(DataRoomId, true);
        }

        public Models.DataRoom GetDataRoom(int DataRoomId, bool tracking)
        {
            using var db = _factory.CreateDbContext();
            if (tracking)
            {
                return db.DataRoom.Find(DataRoomId);
            }
            else
            {
                return db.DataRoom.AsNoTracking().FirstOrDefault(item => item.DataRoomId == DataRoomId);
            }
        }

        public Models.DataRoom AddDataRoom(Models.DataRoom DataRoom)
        {
            using var db = _factory.CreateDbContext();
            db.DataRoom.Add(DataRoom);
            db.SaveChanges();
            return DataRoom;
        }

        public Models.DataRoom UpdateDataRoom(Models.DataRoom DataRoom)
        {
            using var db = _factory.CreateDbContext();
            db.Entry(DataRoom).State = EntityState.Modified;
            db.SaveChanges();
            return DataRoom;
        }

        public void DeleteDataRoom(int DataRoomId)
        {
            using var db = _factory.CreateDbContext();
            Models.DataRoom DataRoom = db.DataRoom.Find(DataRoomId);
            db.DataRoom.Remove(DataRoom);
            db.SaveChanges();
        }
    }
}
