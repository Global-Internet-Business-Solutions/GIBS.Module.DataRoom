using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using GIBS.Module.DataRoom.Migrations.EntityBuilders;
using GIBS.Module.DataRoom.Repository;

namespace GIBS.Module.DataRoom.Migrations
{
    [DbContext(typeof(DataRoomContext))]
    [Migration("GIBS.Module.DataRoom.01.02.05.00")]
    public class AddSubscriptionEvents : MultiDatabaseMigration
    {
        public AddSubscriptionEvents(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var subscriptionEventBuilder = new DataRoomSubscriptionEventBuilder(migrationBuilder, ActiveDatabase);
            subscriptionEventBuilder.Create();
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var subscriptionEventBuilder = new DataRoomSubscriptionEventBuilder(migrationBuilder, ActiveDatabase);
            subscriptionEventBuilder.Drop();
        }
    }
}
