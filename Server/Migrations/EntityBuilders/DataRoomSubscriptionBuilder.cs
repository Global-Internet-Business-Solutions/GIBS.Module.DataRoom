using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using Oqtane.Migrations.EntityBuilders;

namespace GIBS.Module.DataRoom.Migrations.EntityBuilders
{
    public class DataRoomSubscriptionBuilder : AuditableBaseEntityBuilder<DataRoomSubscriptionBuilder>
    {
        private const string _entityTableName = "GIBSDataRoomSubscription";
        private readonly PrimaryKey<DataRoomSubscriptionBuilder> _primaryKey = new("PK_GIBSDataRoomSubscription", x => x.SubscriptionId);
        private readonly ForeignKey<DataRoomSubscriptionBuilder> _dataRoomForeignKey = new("FK_GIBSDataRoomSubscription_DataRoom", x => x.DataRoomId, "GIBSDataRoom", "DataRoomId", ReferentialAction.Cascade);

        public DataRoomSubscriptionBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_dataRoomForeignKey);
        }

        protected override DataRoomSubscriptionBuilder BuildTable(ColumnsBuilder table)
        {
            SubscriptionId = AddAutoIncrementColumn(table, "SubscriptionId");
            DataRoomId = AddIntegerColumn(table, "DataRoomId");
            UserId = AddIntegerColumn(table, "UserId", true);
            Email = AddStringColumn(table, "Email", 250, true);
            EmailConfirmed = AddBooleanColumn(table, "EmailConfirmed");
            ConfirmationToken = AddStringColumn(table, "ConfirmationToken", 400, true);
            ConfirmationExpiresOn = AddDateTimeColumn(table, "ConfirmationExpiresOn", true);
            NotifyOnUpload = AddBooleanColumn(table, "NotifyOnUpload", nullable: false, defaultValue: true);
            NotifyOnOverwrite = AddBooleanColumn(table, "NotifyOnOverwrite", nullable: false, defaultValue: true);
            IsActive = AddBooleanColumn(table, "IsActive", nullable: false, defaultValue: true);
            LastNotifiedOn = AddDateTimeColumn(table, "LastNotifiedOn", true);
            AddAuditableColumns(table);
            return this;
        }

        public OperationBuilder<AddColumnOperation> SubscriptionId { get; set; }
        public OperationBuilder<AddColumnOperation> DataRoomId { get; set; }
        public OperationBuilder<AddColumnOperation> UserId { get; set; }
        public OperationBuilder<AddColumnOperation> Email { get; set; }
        public OperationBuilder<AddColumnOperation> EmailConfirmed { get; set; }
        public OperationBuilder<AddColumnOperation> ConfirmationToken { get; set; }
        public OperationBuilder<AddColumnOperation> ConfirmationExpiresOn { get; set; }
        public OperationBuilder<AddColumnOperation> NotifyOnUpload { get; set; }
        public OperationBuilder<AddColumnOperation> NotifyOnOverwrite { get; set; }
        public OperationBuilder<AddColumnOperation> IsActive { get; set; }
        public OperationBuilder<AddColumnOperation> LastNotifiedOn { get; set; }
    }
}
