using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using GIBS.Module.DataRoom.Migrations.EntityBuilders;
using GIBS.Module.DataRoom.Repository;


namespace GIBS.Module.DataRoom.Migrations
{
    [DbContext(typeof(DataRoomContext))]
    [Migration("GIBS.Module.DataRoom.01.02.03.00")]
    public class AddSubscriptions : MultiDatabaseMigration
    {
        public AddSubscriptions(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AddColumn<bool>(
                name: "EnableSubscription",
                table: "GIBSDataRoom",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "NotifySubscriptionOnNewFile",
                table: "GIBSDataRoom",
                type: "bit",
                nullable: false,
                defaultValue: false);

            var dataRoomBuilder = new DataRoomSubscriptionBuilder(migrationBuilder, ActiveDatabase);
            dataRoomBuilder.Create();

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                 name: "EnableDownload",
                 table: "GIBSDataRoom");

            migrationBuilder.DropColumn(
                name: "EnableViewOnly",
                table: "GIBSDataRoom");

           
            var dataRoomBuilder = new DataRoomSubscriptionBuilder(migrationBuilder, ActiveDatabase);
            dataRoomBuilder.Drop();
        }
    }
}

