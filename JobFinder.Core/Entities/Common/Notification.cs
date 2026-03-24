using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobFinder.Core.Entities.Common
{
    public class Notification
    {
        public int Id { get; set; }

        public string UserId { get; set; } = null!;
        public NotificationTarget Target { get; set; }
        public NotificationType Type { get; set; }

        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string? Link { get; set; }

        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
    }
}
