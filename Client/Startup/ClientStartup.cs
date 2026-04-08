using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Oqtane.Services;
using GIBS.Module.DataRoom.Services;

namespace GIBS.Module.DataRoom.Startup
{
    public class ClientStartup : IClientStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            if (!services.Any(s => s.ServiceType == typeof(IDataRoomService)))
            {
                services.AddScoped<IDataRoomService, ClientDataRoomService>();
            }

            if (!services.Any(s => s.ServiceType == typeof(IDataRoomActivityLogService)))
            {
                services.AddScoped<IDataRoomActivityLogService, ClientDataRoomActivityLogService>();
            }
        }
    }
}
