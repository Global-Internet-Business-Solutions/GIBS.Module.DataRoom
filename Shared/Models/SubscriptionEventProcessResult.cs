using System;

namespace GIBS.Module.DataRoom.Models
{
    public class SubscriptionEventProcessResult
    {
        public int PendingEventCount { get; set; }
        public int ProcessedEventCount { get; set; }
        public int NotificationCount { get; set; }
        public int SubscriberCount { get; set; }
        public DateTime ProcessedOn { get; set; }
        public string Message { get; set; }
    }
}
