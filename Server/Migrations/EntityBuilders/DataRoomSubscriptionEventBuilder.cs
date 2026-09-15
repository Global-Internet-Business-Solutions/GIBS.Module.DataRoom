using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using Oqtane.Migrations.EntityBuilders;

namespace GIBS.Module.DataRoom.Migrations.EntityBuilders
{
    public class DataRoomSubscriptionEventBuilder : AuditableBaseEntityBuilder<DataRoomSubscriptionEventBuilder>
    {
        private const string _entityTableName = "GIBSDataRoomSubscriptionEvent";
        private readonly PrimaryKey<DataRoomSubscriptionEventBuilder> _primaryKey = new("PK_GIBSDataRoomSubscriptionEvent", x => x.SubscriptionEventId);
        private readonly ForeignKey<DataRoomSubscriptionEventBuilder> _dataRoomForeignKey = new("FK_GIBSDataRoomSubscriptionEvent_DataRoom", x => x.DataRoomId, "GIBSDataRoom", "DataRoomId", ReferentialAction.Cascade);

        public DataRoomSubscriptionEventBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_dataRoomForeignKey);
        }

        protected override DataRoomSubscriptionEventBuilder BuildTable(ColumnsBuilder table)
        {
            SubscriptionEventId = AddAutoIncrementColumn(table, "SubscriptionEventId");
            DataRoomId = AddIntegerColumn(table, "DataRoomId");
            FileId = AddIntegerColumn(table, "FileId");
            FolderId = AddIntegerColumn(table, "FolderId");
            Action = AddMaxStringColumn(table, "Action");
            OccurredOn = AddDateTimeColumn(table, "OccurredOn");
            CreatedByUserId = AddMaxStringColumn(table, "CreatedByUserId", true);
            FileName = AddMaxStringColumn(table, "FileName", true);
            RelativePath = AddMaxStringColumn(table, "RelativePath", true);
            IsProcessed = AddBooleanColumn(table, "IsProcessed", nullable: false, defaultValue: false);
            ProcessedOn = AddDateTimeColumn(table, "ProcessedOn", true);
            AddAuditableColumns(table);
            return this;
        }

        public OperationBuilder<AddColumnOperation> SubscriptionEventId { get; set; }
        public OperationBuilder<AddColumnOperation> DataRoomId { get; set; }
        public OperationBuilder<AddColumnOperation> FileId { get; set; }
        public OperationBuilder<AddColumnOperation> FolderId { get; set; }
        public OperationBuilder<AddColumnOperation> Action { get; set; }
        public OperationBuilder<AddColumnOperation> OccurredOn { get; set; }
        public OperationBuilder<AddColumnOperation> CreatedByUserId { get; set; }
        public OperationBuilder<AddColumnOperation> FileName { get; set; }
        public OperationBuilder<AddColumnOperation> RelativePath { get; set; }
        public OperationBuilder<AddColumnOperation> IsProcessed { get; set; }
        public OperationBuilder<AddColumnOperation> ProcessedOn { get; set; }
    }
}
