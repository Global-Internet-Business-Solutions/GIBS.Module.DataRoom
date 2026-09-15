using Oqtane.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GIBS.Module.DataRoom.Models
{
    [Table("GIBSDataRoomSubscriptionEvent")]
    public class SubscriptionEvent : ModelBase
    {
        [Key]
        public int SubscriptionEventId { get; set; }
        public int DataRoomId { get; set; }
        public int FileId { get; set; }
        public int FolderId { get; set; }
        public string Action { get; set; }
        public DateTime OccurredOn { get; set; }
        public string CreatedByUserId { get; set; }
        public string FileName { get; set; }
        public string RelativePath { get; set; }
        public bool IsProcessed { get; set; }
        public DateTime? ProcessedOn { get; set; }
    }
}
