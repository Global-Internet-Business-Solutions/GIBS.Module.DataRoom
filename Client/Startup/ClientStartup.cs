using Microsoft.Extensions.DependencyInjection;
using Oqtane.Services;
using GIBS.Module.DataRoom.Services;

namespace GIBS.Module.DataRoom.Startup
{
    public class ClientStartup : IClientStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<ClientDataRoomService>();
            services.AddScoped<IDataRoomService, ClientDataRoomService>();
            services.AddScoped<IDataRoomActivityLogService, ClientDataRoomActivityLogService>();
        }
    }
}
