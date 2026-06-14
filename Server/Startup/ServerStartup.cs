using Microsoft.AspNetCore.Builder; 
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Infrastructure;
using GIBS.Module.DataRoom.Repository;
using GIBS.Module.DataRoom.Services;

namespace GIBS.Module.DataRoom.Startup
{
    public class ServerStartup : IServerStartup
    {
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // not implemented
        }

        public void ConfigureMvc(IMvcBuilder mvcBuilder)
        {
            // not implemented
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IDataRoomService, ServerDataRoomService>();
            services.AddTransient<IDataRoomActivityLogService, ServerDataRoomActivityLogService>();
            services.AddTransient<IDataRoomSubscriptionService, ServerDataRoomSubscriptionService>();
            services.AddDbContextFactory<DataRoomContext>(opt => { }, ServiceLifetime.Transient);
        }
    }
}
