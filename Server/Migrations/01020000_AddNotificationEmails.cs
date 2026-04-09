using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using GIBS.Module.DataRoom.Migrations.EntityBuilders;
using GIBS.Module.DataRoom.Repository;

namespace GIBS.Module.DataRoom.Migrations
{
    [DbContext(typeof(DataRoomContext))]
    [Migration("GIBS.Module.DataRoom.01.02.00.00")]
    public class AddNotificationEmails : MultiDatabaseMigration
    {
        public AddNotificationEmails(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //   var entityBuilder = new DataRoomEntityBuilder(migrationBuilder, ActiveDatabase);
            //   entityBuilder.AddMaxStringColumn("NotificationEmails", true);
            migrationBuilder.AddColumn<string>(
                name: "NotificationEmails",
                table: "GIBSDataRoom",
                type: "nvarchar(max)", // Explicitly sets the database type
                nullable: true);
        }
                

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotificationEmails",
                table: "GIBSDataRoom");
        }
    }
}
