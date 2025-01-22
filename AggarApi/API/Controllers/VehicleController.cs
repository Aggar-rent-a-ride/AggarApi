using CORE.DTOs;
using CORE.DTOs.Vehicle;
using CORE.Helpers;
using CORE.Services.IServices;
using DATA.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Customer")]
    public class VehicleController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;
        public VehicleController(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        [HttpGet("get-vehicles")]
        public async Task<IActionResult> GetNearestVehiclesAsync([FromQuery] int pageNo, [FromQuery] int pageSize,
            [FromQuery] string? searchKey,
            [FromQuery] int? brandId, [FromQuery] int? typeId, [FromQuery] VehicleTransmission? transmission,
            [FromQuery] double? Rate, [FromQuery] double? minPrice, [FromQuery] double? maxPrice, [FromQuery] int? year)
        {
            int userId = UserHelper.GetUserId(User);
            ResponseDto<PagedResultDto<GetVehicleSummaryDto>> result = await _vehicleService.GetNearestVehiclesAsync(userId, pageNo, pageSize, searchKey, brandId, typeId, transmission, Rate, minPrice, maxPrice, year);
            return StatusCode(result.StatusCode, result);
        }
    }
}
