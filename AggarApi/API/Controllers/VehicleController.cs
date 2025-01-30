using CORE.DTOs.Vehicle;
using CORE.Services.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
﻿using CORE.DTOs;
using CORE.Helpers;
using DATA.Models.Enums;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class VehicleController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;

        public VehicleController(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }
        [Authorize(Roles = "Renter")]
        [HttpPost]
        public async Task<IActionResult> CreateVehicleAsync([FromForm] CreateVehicleDto createVehicleDto)
        {
            var renterId = UserHelpers.GetUserId(User);
            var response = await _vehicleService.CreateVehicleAsync(createVehicleDto, renterId);
            return StatusCode(response.StatusCode, response);
        }
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVehicleByIdAsync(int id)
        {
            var renterId = UserHelpers.GetUserId(User);
            var response = await _vehicleService.GetVehicleByIdAsync(id);
            return StatusCode(response.StatusCode, response);
        }
        [Authorize(Roles = "Customer")]
        [HttpGet("get-vehicles")]
        public async Task<IActionResult> GetNearestVehiclesAsync([FromQuery] int pageNo, [FromQuery] int pageSize,
            [FromQuery] string? searchKey,
            [FromQuery] int? brandId, [FromQuery] int? typeId, [FromQuery] VehicleTransmission? transmission,
            [FromQuery] double? Rate, [FromQuery] double? minPrice, [FromQuery] double? maxPrice, [FromQuery] int? year)
        {
            int userId = UserHelpers.GetUserId(User);
            
            string baseUrl = HttpContext.Items["BaseUrl"].ToString();

            ResponseDto<PagedResultDto<GetVehicleSummaryDto>> result = await _vehicleService.GetNearestVehiclesAsync(userId, pageNo, pageSize, searchKey, brandId, typeId, transmission, Rate, minPrice, maxPrice, year, baseUrl);
            return StatusCode(result.StatusCode, result);
        }
        [Authorize(Roles = "Renter")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVehicleAsync(int id)
        {
            var renterId = UserHelpers.GetUserId(User);
            var response = await _vehicleService.DeleteVehicleByIdAsync(id, renterId);
            return StatusCode(response.StatusCode, response);
        }
        [Authorize(Roles = "Renter")]
        [HttpPut]
        public async Task<IActionResult> UpdateVehicleAsync([FromForm] UpdateVehicleDto dto)
        {
            var renterId = UserHelpers.GetUserId(User);
            var response = await _vehicleService.UpdateVehicleAsync(dto, renterId);
            return StatusCode(response.StatusCode, response);
        }
        [Authorize(Roles = "Renter")]
        [HttpPut("vehicle-images")]
        public async Task<IActionResult> UpdateVehicleImagesAsync([FromForm] UpdateVehicleImagesDto dto)
        {
            var renterId = UserHelpers.GetUserId(User);
            var response = await _vehicleService.UpdateVehicleImagesAsync(dto, renterId);
            return StatusCode(response.StatusCode, response);
        }
    }
}
