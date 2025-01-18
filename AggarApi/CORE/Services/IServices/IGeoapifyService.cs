using CORE.DTOs.Geoapify;
using DATA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.Services.IServices
{
    public interface IGeoapifyService
    {
        Task<GeoapifyAddressResponse> GetAddressByLocationAsync(Location location);
    }
}
