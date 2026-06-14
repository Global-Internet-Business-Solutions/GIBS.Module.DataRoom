using Oqtane.Models;
using Oqtane.Modules;

namespace GIBS.Module.DataRoom
{
    public class ModuleInfo : IModule
    {
        public ModuleDefinition ModuleDefinition => new ModuleDefinition
        {
            Name = "DataRoom",
            Description = "Secured Data Room Module for Oqtane",
            Version = "1.2.3",
            ServerManagerType = "GIBS.Module.DataRoom.Manager.DataRoomManager, GIBS.Module.DataRoom.Server.Oqtane",
            ReleaseVersions = "1.0.0,1.2.0,1.2.1,1.2.2,1.2.3",
            Dependencies = "GIBS.Module.DataRoom.Shared.Oqtane",
            PackageName = "GIBS.Module.DataRoom" 
        };
    }
}
