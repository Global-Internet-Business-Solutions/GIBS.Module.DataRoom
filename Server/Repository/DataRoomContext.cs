using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Oqtane.Modules;
using Oqtane.Repository;
using Oqtane.Infrastructure;
using Oqtane.Repository.Databases.Interfaces;

namespace GIBS.Module.DataRoom.Repository
{
    public class DataRoomContext : DBContextBase, ITransientService, IMultiDatabase
    {
        public virtual DbSet<Models.DataRoom> DataRoom { get; set; }
        public virtual DbSet<Models.DataRoomActivityLog> DataRoomActivityLog { get; set; }
        public virtual DbSet<Models.Subscription> DataRoomSubscription { get; set; }

        public DataRoomContext(IDBContextDependencies DBContextDependencies) : base(DBContextDependencies)
        {
            // ContextBase handles multi-tenant database connections
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Models.DataRoom>().ToTable(ActiveDatabase.RewriteName("GIBSDataRoom"));
            builder.Entity<Models.DataRoomActivityLog>().ToTable(ActiveDatabase.RewriteName("GIBSDataRoomActivityLog"));
            builder.Entity<Models.Subscription>().ToTable(ActiveDatabase.RewriteName("GIBSDataRoomSubscription"));
        }
    }
}
