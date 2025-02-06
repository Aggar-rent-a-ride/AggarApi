using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.DTOs.Discount
{
    public class UpdateVehicleDiscountsDto
    {
        public int VehicleId { get; set; }
        public ICollection<CreateDiscountDto>? Discounts { get; set; }
    }
}
