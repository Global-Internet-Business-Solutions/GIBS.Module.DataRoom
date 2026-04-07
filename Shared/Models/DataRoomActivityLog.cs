using Oqtane.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GIBS.Module.DataRoom.Models
{
    [Table("GIBSDataRoomActivityLog")]
    public class DataRoomActivityLog : ModelBase
    {
        [Key]
        public int ActivityLogId { get; set; }
        public int DataRoomId { get; set; }
        public int FileId { get; set; }
        public string UserId { get; set; }
        public string Action { get; set; } // "View", "Download", "Upload"
        public DateTime Timestamp { get; set; }
        public string IPAddress { get; set; }
    }
}
