using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATA.Models
{
    public class UserConnection
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string ConnectionId { get; set; } = null!;
        public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DisconnectedAt { get; set; }
        public bool IsConnected => DisconnectedAt == null;
        public AppUser User { get; set; }
    }
}
