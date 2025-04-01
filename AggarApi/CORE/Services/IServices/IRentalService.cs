using CORE.DTOs;
using CORE.DTOs.Rental;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.Services.IServices
{
    public interface IRentalService
    {
        Task<ResponseDto<GetRentalDto?>> GetRentalByIdAsync(int rentalId);
    }
}
