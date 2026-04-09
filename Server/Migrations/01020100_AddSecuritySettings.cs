using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using GIBS.Module.DataRoom.Migrations.EntityBuilders;
using GIBS.Module.DataRoom.Repository;


namespace GIBS.Module.DataRoom.Migrations
{
    [DbContext(typeof(DataRoomContext))]
    [Migration("GIBS.Module.DataRoom.01.02.01.00")]
    public class AddSecuritySettings : MultiDatabaseMigration
    {
        public AddSecuritySettings(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
           
            migrationBuilder.AddColumn<bool>(
                name: "EnableDownload",
                table: "GIBSDataRoom",      // Replace with your actual table name
                type: "bit",                // Optional for SQL Server, as EF infers this from <bool>
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "EnableViewOnly",
                table: "GIBSDataRoom",      // Replace with your actual table name
                type: "bit",                // Standard for SQL Server bools  
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnableWatermark",
                table: "GIBSDataRoom",      // Replace with your actual table name
                type: "bit",                // Standard for SQL Server bools  
                nullable: false,
                defaultValue: false);

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                 name: "EnableDownload",
                 table: "GIBSDataRoom");

            migrationBuilder.DropColumn(
                name: "EnableViewOnly",
                table: "GIBSDataRoom");

            migrationBuilder.DropColumn(
                name: "EnableWatermark",
                table: "GIBSDataRoom");
        }
    }
}

