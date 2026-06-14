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
            services.AddScoped<IDataRoomSubscriptionService, ClientDataRoomSubscriptionService>();

            // Register localization with resource path for Italian and other cultures
            services.AddLocalization(options => options.ResourcesPath = "Resources");
        }
    }
}
