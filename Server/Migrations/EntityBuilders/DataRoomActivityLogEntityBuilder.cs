using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using Oqtane.Migrations.EntityBuilders;

namespace GIBS.Module.DataRoom.Migrations.EntityBuilders
{
    public class DataRoomActivityLogEntityBuilder : AuditableBaseEntityBuilder<DataRoomActivityLogEntityBuilder>
    {
        private const string _entityTableName = "GIBSDataRoomActivityLog";
        private readonly PrimaryKey<DataRoomActivityLogEntityBuilder> _primaryKey = new("PK_GIBSDataRoomActivityLog", x => x.ActivityLogId);
        private readonly ForeignKey<DataRoomActivityLogEntityBuilder> _dataRoomForeignKey = new("FK_GIBSDataRoomActivityLog_DataRoom", x => x.DataRoomId, "GIBSDataRoom", "DataRoomId", ReferentialAction.Cascade);

        public DataRoomActivityLogEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_dataRoomForeignKey);
        }

        protected override DataRoomActivityLogEntityBuilder BuildTable(ColumnsBuilder table)
        {
            ActivityLogId = AddAutoIncrementColumn(table, "ActivityLogId");
            DataRoomId = AddIntegerColumn(table, "DataRoomId");
            FileId = AddIntegerColumn(table, "FileId");
            UserId = AddMaxStringColumn(table, "UserId", true);
            Action = AddMaxStringColumn(table, "Action");
            Timestamp = AddDateTimeColumn(table, "Timestamp");
            IPAddress = AddMaxStringColumn(table, "IPAddress", true);
            AddAuditableColumns(table);
            return this;
        }

        public OperationBuilder<AddColumnOperation> ActivityLogId { get; set; }
        public OperationBuilder<AddColumnOperation> DataRoomId { get; set; }
        public OperationBuilder<AddColumnOperation> FileId { get; set; }
        public OperationBuilder<AddColumnOperation> UserId { get; set; }
        public OperationBuilder<AddColumnOperation> Action { get; set; }
        public OperationBuilder<AddColumnOperation> Timestamp { get; set; }
        public OperationBuilder<AddColumnOperation> IPAddress { get; set; }
    }
}
