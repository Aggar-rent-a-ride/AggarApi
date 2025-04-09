using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CORE.DTOs.Rental.GetRentalsByUserIdDto.GetRentalsByUserIdDtoBooking;

namespace CORE.DTOs.Rental
{
    public class GetRentalsByUserIdDto
    {
        public int Id { get; set; }
        public int CustomerReviewId { get; set; }
        public int RenterReviewId { get; set; }
        public int BookingId { get; set; }
        public GetRentalsByUserIdDtoBooking Booking { get; set; }
        public class GetRentalsByUserIdDtoBooking
        {
            public int Id { get; set; }
            public int CustomerId { get; set; }
            public GetRentalsByUserIdDtoVehicle Vehicle { get; set; }
            public class GetRentalsByUserIdDtoVehicle
            {
                public int Id { get; set; }
                public int RenterId { get; set; }
            }
        }
    }
}
