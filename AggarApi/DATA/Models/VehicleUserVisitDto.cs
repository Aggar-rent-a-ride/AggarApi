using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATA.Models
{
    public class VehicleUserVisitDto
    {
        public int VehicleId { get; set; }
        public int AppUserId { get; set; }
        public DateTime LastTimeVisited { get; set; } = DateTime.UtcNow;
    }
}
