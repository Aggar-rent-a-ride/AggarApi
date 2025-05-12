using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATA.Models
{
    public class VehiclePopularity
    {
        [Key]
        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; }
        public int PopularityPoints { get; set; } = 0;
    }
}
