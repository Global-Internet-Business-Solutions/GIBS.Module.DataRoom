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
            DataRoomId = AddAutoIncrementColumn(table,"DataRoomId");
            ModuleId = AddIntegerColumn(table,"ModuleId");
            Name = AddMaxStringColumn(table,"Name");
            AddAuditableColumns(table);
            return this;
        }

        public OperationBuilder<AddColumnOperation> DataRoomId { get; set; }
        public OperationBuilder<AddColumnOperation> ModuleId { get; set; }
        public OperationBuilder<AddColumnOperation> Name { get; set; }
    }
}
