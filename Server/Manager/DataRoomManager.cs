using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Oqtane.Modules;
using Oqtane.Models;
using Oqtane.Infrastructure;
using Oqtane.Interfaces;
using Oqtane.Enums;
using Oqtane.Repository;
using GIBS.Module.DataRoom.Repository;
using System.Threading.Tasks;

namespace GIBS.Module.DataRoom.Manager
{
    public class DataRoomManager : MigratableModuleBase, IInstallable, IPortable, ISearchable
    {
        private readonly IDataRoomRepository _DataRoomRepository;
        private readonly IDBContextDependencies _DBContextDependencies;

        public DataRoomManager(IDataRoomRepository DataRoomRepository, IDBContextDependencies DBContextDependencies)
        {
            _DataRoomRepository = DataRoomRepository;
            _DBContextDependencies = DBContextDependencies;
        }

        public bool Install(Tenant tenant, string version)
        {
            return Migrate(new DataRoomContext(_DBContextDependencies), tenant, MigrationType.Up);
        }

        public bool Uninstall(Tenant tenant)
        {
            return Migrate(new DataRoomContext(_DBContextDependencies), tenant, MigrationType.Down);
        }

        public string ExportModule(Oqtane.Models.Module module)
        {
            string content = "";
            List<Models.DataRoom> DataRooms = _DataRoomRepository.GetDataRooms(module.ModuleId).ToList();
            if (DataRooms != null)
            {
                content = JsonSerializer.Serialize(DataRooms);
            }
            return content;
        }

        public void ImportModule(Oqtane.Models.Module module, string content, string version)
        {
            List<Models.DataRoom> DataRooms = null;
            if (!string.IsNullOrEmpty(content))
            {
                DataRooms = JsonSerializer.Deserialize<List<Models.DataRoom>>(content);
            }
            if (DataRooms != null)
            {
                foreach(var DataRoom in DataRooms)
                {
                    _DataRoomRepository.AddDataRoom(new Models.DataRoom { ModuleId = module.ModuleId, Name = DataRoom.Name });
                }
            }
        }

        public Task<List<SearchContent>> GetSearchContentsAsync(PageModule pageModule, DateTime lastIndexedOn)
        {
           var searchContentList = new List<SearchContent>();

           foreach (var DataRoom in _DataRoomRepository.GetDataRooms(pageModule.ModuleId))
           {
               if (DataRoom.ModifiedOn >= lastIndexedOn)
               {
                   searchContentList.Add(new SearchContent
                   {
                       EntityName = "GIBSDataRoom",
                       EntityId = DataRoom.DataRoomId.ToString(),
                       Title = DataRoom.Name,
                       Body = DataRoom.Name,
                       ContentModifiedBy = DataRoom.ModifiedBy,
                       ContentModifiedOn = DataRoom.ModifiedOn
                   });
               }
           }

           return Task.FromResult(searchContentList);
        }
    }
}
