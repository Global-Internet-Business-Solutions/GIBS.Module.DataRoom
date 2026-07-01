using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using GIBS.Module.DataRoom.Migrations.EntityBuilders;
using GIBS.Module.DataRoom.Repository;


namespace GIBS.Module.DataRoom.Migrations
{
    [DbContext(typeof(DataRoomContext))]
    [Migration("GIBS.Module.DataRoom.01.02.04.00")]
    public class AddSubscriptionFolder : MultiDatabaseMigration
    {
        public AddSubscriptionFolder(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AddColumn<bool>(
                name: "IncludeSubfolders",
                table: "GIBSDataRoomSubscription",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "FolderId",
                table: "GIBSDataRoomSubscription",
                type: "int",
                nullable: false,
                defaultValue: 0);

            var dataRoomBuilder = new DataRoomSubscriptionBuilder(migrationBuilder, ActiveDatabase);
            dataRoomBuilder.Create();

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                 name: "IncludeSubfolders",
                 table: "GIBSDataRoomSubscription");

            migrationBuilder.DropColumn(
                name: "FolderId",
                table: "GIBSDataRoomSubscription");

           
            var dataRoomBuilder = new DataRoomSubscriptionBuilder(migrationBuilder, ActiveDatabase);
            dataRoomBuilder.Drop();
        }
    }
}

