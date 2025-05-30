using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.DTOs.Rental
{
    public class CreatedRentalDto
    {
        public int RentalId { get; set; }
        public string QrCodeBase64 { get; set; }
    }
}
