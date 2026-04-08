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
            var entityBuilder = new DataRoomEntityBuilder(migrationBuilder, ActiveDatabase);

            // Default true  — existing rooms keep downloads enabled
            entityBuilder.AddBooleanColumn("EnableDownload", nullable: false, defaultValue: true);

            // Default false — existing rooms are not view-only
            entityBuilder.AddBooleanColumn("EnableViewOnly", nullable: false, defaultValue: false);

            // Default false — existing rooms have no watermark
            entityBuilder.AddBooleanColumn("EnableWatermark", nullable: false, defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var entityBuilder = new DataRoomEntityBuilder(migrationBuilder, ActiveDatabase);
            entityBuilder.DropColumn("EnableDownload");
            entityBuilder.DropColumn("EnableViewOnly");
            entityBuilder.DropColumn("EnableWatermark");
        }
    }
}


//public bool EnableDownload { get; set; } // New property to control download permissions
//public bool EnableViewOnly { get; set; } // New property to control view-only permissions
//public bool EnableWatermark { get; set; } // New property to control watermarking of viewed/downloaded files