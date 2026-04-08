using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using GIBS.Module.DataRoom.Migrations.EntityBuilders;
using GIBS.Module.DataRoom.Repository;

namespace GIBS.Module.DataRoom.Migrations
{
    [DbContext(typeof(DataRoomContext))]
    [Migration("GIBS.Module.DataRoom.01.00.00.00")]
    public class InitializeModule : MultiDatabaseMigration
    {
        public InitializeModule(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var dataRoomBuilder = new DataRoomEntityBuilder(migrationBuilder, ActiveDatabase);
            dataRoomBuilder.Create();

            var activityLogBuilder = new DataRoomActivityLogEntityBuilder(migrationBuilder, ActiveDatabase);
            activityLogBuilder.Create();
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop activity log first to satisfy FK constraint
            var activityLogBuilder = new DataRoomActivityLogEntityBuilder(migrationBuilder, ActiveDatabase);
            activityLogBuilder.Drop();

            var dataRoomBuilder = new DataRoomEntityBuilder(migrationBuilder, ActiveDatabase);
            dataRoomBuilder.Drop();
        }
    }
}
