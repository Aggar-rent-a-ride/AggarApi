using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.DTOs.Rental
{
    public class GetRentalsByVehicleIdDto
    {
        public int Id { get; set; }
        public int CustomerReviewId { get; set; }
        public int RenterReviewId { get; set; }
        public int BookingId { get; set; }
    }
}
