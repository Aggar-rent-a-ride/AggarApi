using CORE.DTOs.Vehicle;
using CORE.Services.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
﻿using CORE.DTOs;
using CORE.Helpers;
using DATA.Models.Enums;
using Microsoft.AspNetCore.Authorization;
﻿using CORE.DTOs;
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
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class VehicleController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;
<<<<<<< HEAD

=======
        public VehicleController(IVehicleService vehicleService)
        }
<<<<<<< HEAD
        [HttpPost]
        public async Task<IActionResult> CreateVehicleAsync([FromForm]CreateVehicleDto createVehicleDto)
        {
            var response = await _vehicleService.CreateVehicleAsync(createVehicleDto, 8);
            return StatusCode(response.StatusCode, response);
=======
>>>>>>> a1be340c42144636a2970dbb31acfc2b35d2df61

        [Authorize(Roles = "Customer")]
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
