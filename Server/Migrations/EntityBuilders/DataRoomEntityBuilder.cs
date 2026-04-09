using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using Oqtane.Migrations.EntityBuilders;

namespace GIBS.Module.DataRoom.Migrations.EntityBuilders
{
    public class DataRoomEntityBuilder : AuditableBaseEntityBuilder<DataRoomEntityBuilder>
    {
        private const string _entityTableName = "GIBSDataRoom";
        private readonly PrimaryKey<DataRoomEntityBuilder> _primaryKey = new("PK_GIBSDataRoom", x => x.DataRoomId);
        private readonly ForeignKey<DataRoomEntityBuilder> _moduleForeignKey = new("FK_GIBSDataRoom_Module", x => x.ModuleId, "Module", "ModuleId", ReferentialAction.Cascade);

        public DataRoomEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_moduleForeignKey);
        }

        protected override DataRoomEntityBuilder BuildTable(ColumnsBuilder table)
        {
            DataRoomId = AddAutoIncrementColumn(table, "DataRoomId");
            ModuleId = AddIntegerColumn(table, "ModuleId");
            SiteId = AddIntegerColumn(table, "SiteId");
            Name = AddMaxStringColumn(table, "Name");
            Description = AddMaxStringColumn(table, "Description", true);
            FolderId = AddIntegerColumn(table, "FolderId");
            IsActive = AddBooleanColumn(table, "IsActive");
            //NotificationEmails = AddMaxStringColumn(table, "NotificationEmails", true);
            //EnableDownload = AddBooleanColumn(table, "EnableDownload", nullable: false, defaultValue: true);
            //EnableViewOnly = AddBooleanColumn(table, "EnableViewOnly");
            //EnableWatermark = AddBooleanColumn(table, "EnableWatermark");
            AddAuditableColumns(table);
            return this;
        }

        public OperationBuilder<AddColumnOperation> DataRoomId { get; set; }
        public OperationBuilder<AddColumnOperation> ModuleId { get; set; }
        public OperationBuilder<AddColumnOperation> SiteId { get; set; }
        public OperationBuilder<AddColumnOperation> Name { get; set; }
        public OperationBuilder<AddColumnOperation> Description { get; set; }
        public OperationBuilder<AddColumnOperation> FolderId { get; set; }
        public OperationBuilder<AddColumnOperation> IsActive { get; set; }
        //public OperationBuilder<AddColumnOperation> NotificationEmails { get; set; }
        //public OperationBuilder<AddColumnOperation> EnableDownload { get; set; }
        //public OperationBuilder<AddColumnOperation> EnableViewOnly { get; set; }
        //public OperationBuilder<AddColumnOperation> EnableWatermark { get; set; }
    }
}
