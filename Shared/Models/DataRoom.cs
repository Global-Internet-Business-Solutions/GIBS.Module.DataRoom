using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Oqtane.Models;

namespace GIBS.Module.DataRoom.Models
{
    [Table("GIBSDataRoom")]
    public class DataRoom : ModelBase
    {
        [Key]
        public int DataRoomId { get; set; }
        public int ModuleId { get; set; }
        public int SiteId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int FolderId { get; set; }        // Link to an Oqtane Folder
        public bool IsActive { get; set; }
        public string NotificationEmails { get; set; }  // nullable - comma-separated, notified on upload
        public bool EnableSubscription { get; set; }
        public bool NotifySubscriptionOnNewFile { get; set; }
        public bool EnableDownload { get; set; } // New property to control download permissions
        public bool EnableViewOnly { get; set; } // New property to control view-only permissions
        public bool EnableWatermark { get; set; } // New property to control watermarking of viewed/downloaded files
    }
}
