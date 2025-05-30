using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.Services.IServices
{
    public interface IQrCodeService
    {
        public string GenerateQrHashToken(string data);
        public string GenerateQrCode(string token);
    }
}
