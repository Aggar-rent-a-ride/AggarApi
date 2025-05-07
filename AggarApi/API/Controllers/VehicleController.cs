using CORE.DTOs.Vehicle;
using CORE.Services.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CORE.DTOs;
using CORE.Helpers;
using DATA.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using CORE.DTOs.Discount;
using DATA.Models;

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
        [HttpGet]
        public async Task<IActionResult> GetVehicleByIdAsync([FromQuery] int id)
        {
            var renterId = UserHelpers.GetUserId(User);
            var response = await _vehicleService.GetVehicleByIdAsync(id);
            return StatusCode(response.StatusCode, response);
        }
        [Authorize(Roles = "Customer")]
        [HttpGet("get-vehicles")]
        public async Task<IActionResult> GetVehiclesAsync([FromQuery] int pageNo, [FromQuery] int pageSize,
            [FromQuery] bool byNearest, [FromQuery] double? latitude, [FromQuery] double? longitude,
            [FromQuery] string? searchKey,
            [FromQuery] int? brandId, [FromQuery] int? typeId, [FromQuery] VehicleTransmission? transmission,
            [FromQuery] double? Rate, [FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice, [FromQuery] int? year)
        {
            int userId = UserHelpers.GetUserId(User);

            ResponseDto<PagedResultDto<GetVehicleSummaryDto>> result = await _vehicleService.GetVehiclesAsync(userId, pageNo, pageSize, byNearest, latitude, longitude, searchKey, brandId, typeId, transmission, Rate, minPrice, maxPrice, year);
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
        [Authorize(Roles = "Renter")]
        [HttpPut("vehicle-discounts")]
        public async Task<IActionResult> UpdateVehicleDiscountsAsync(UpdateVehicleDiscountsDto dto)
        {
            var renterId = UserHelpers.GetUserId(User);
            var response = await _vehicleService.UpdateVehicleDiscountsAsync(dto, renterId);
            return StatusCode(response.StatusCode, response);
        }
    }
}
