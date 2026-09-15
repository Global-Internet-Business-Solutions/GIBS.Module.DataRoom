using System;
using System.Threading.Tasks;
using GIBS.Module.DataRoom.Services;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Shared;

namespace GIBS.Module.DataRoom.Infrastructure.Jobs
{
    public class DataRoomSubscriptionEventJob : HostedServiceBase
    {
        public DataRoomSubscriptionEventJob(IServiceScopeFactory serviceScopeFactory) : base(serviceScopeFactory)
        {
            Name = "Data Room Subscription Event Job";
            Frequency = "H";
            Interval = 1;
            IsEnabled = true;
        }

        public override Task<string> ExecuteJobAsync(IServiceProvider provider)
        {
            try
            {
                var processor = provider.GetRequiredService<IDataRoomSubscriptionEventProcessor>();
                var notes = processor.ProcessPendingSubscriptionEvents(DateTime.UtcNow);
                return Task.FromResult(notes);
            }
            catch (Exception ex)
            {
                var logger = provider.GetRequiredService<ILogManager>();
                logger.Log(LogLevel.Error, this, LogFunction.Other, $"Data Room Subscription Event Job failed: {ex}");
                return Task.FromResult($"Data Room Subscription Event Job failed: {ex.Message}");
            }
        }
    }
}
