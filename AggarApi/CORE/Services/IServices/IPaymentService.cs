using CORE.DTOs;
using CORE.DTOs.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.Services.IServices
{
    public interface IPaymentService
    {
        public Task<ResponseDto<StripeAccountDto>> CreateStripeAccountAsync(CreateConnectedAccountDto dto, int renterId);
    }
}
