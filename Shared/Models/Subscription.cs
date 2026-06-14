using Oqtane.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GIBS.Module.DataRoom.Models
{
    [Table("GIBSDataRoomSubscription")]
    public class Subscription : ModelBase
    {
        [Key]
        public int SubscriptionId { get; set; }

        public int DataRoomId { get; set; }

        // One of UserId or Email should be set
        public int? UserId { get; set; }
        public string Email { get; set; }

        public bool EmailConfirmed { get; set; }
        public string ConfirmationToken { get; set; }
        public DateTime? ConfirmationExpiresOn { get; set; }

        public bool NotifyOnUpload { get; set; } = true;
        public bool NotifyOnOverwrite { get; set; } = true;
        public bool IsActive { get; set; } = true;

        public DateTime? LastNotifiedOn { get; set; }
    }
}
