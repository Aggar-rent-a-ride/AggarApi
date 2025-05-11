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
using CORE.Constants;

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
        [Authorize(Roles = $"{Roles.Renter}")]
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
        public async Task<IActionResult> GetVehiclesAsync([FromQuery]VehiclesSearchQuery searchQuery)
        {
            int userId = UserHelpers.GetUserId(User);

            var result = await _vehicleService.GetVehiclesAsync(userId, searchQuery);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Roles = $"{Roles.Renter}, {Roles.Admin}")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVehicleAsync(int id)
        {
            var renterId = UserHelpers.GetUserId(User);
            var roles = UserHelpers.GetUserRoles(User);
            var response = await _vehicleService.DeleteVehicleByIdAsync(id, renterId, roles.ToArray());
            return StatusCode(response.StatusCode, response);
        }
        [Authorize(Roles = $"{Roles.Renter}")]
        [HttpPut]
        public async Task<IActionResult> UpdateVehicleAsync([FromForm] UpdateVehicleDto dto)
        {
            var renterId = UserHelpers.GetUserId(User);
            var response = await _vehicleService.UpdateVehicleAsync(dto, renterId);
            return StatusCode(response.StatusCode, response);
        }
        [Authorize(Roles = $"{Roles.Renter}")]
        [HttpPut("vehicle-images")]
        public async Task<IActionResult> UpdateVehicleImagesAsync([FromForm] UpdateVehicleImagesDto dto)
        {
            var renterId = UserHelpers.GetUserId(User);
            var response = await _vehicleService.UpdateVehicleImagesAsync(dto, renterId);
            return StatusCode(response.StatusCode, response);
        }
        [Authorize(Roles = $"{Roles.Renter}")]
        [HttpPut("vehicle-discounts")]
        public async Task<IActionResult> UpdateVehicleDiscountsAsync(UpdateVehicleDiscountsDto dto)
        {
            var renterId = UserHelpers.GetUserId(User);
            var response = await _vehicleService.UpdateVehicleDiscountsAsync(dto, renterId);
            return StatusCode(response.StatusCode, response);
        }
        [Authorize]
        [HttpGet("vehicles-by-status")]
        public async Task<IActionResult> GetVehiclesByStatusAsync([FromQuery] VehicleStatus status, [FromQuery] int pageNo, [FromQuery] int pageSize)
        {
            var result = await _vehicleService.GetVehiclesByStatusAsync(status, pageNo, pageSize);
            return StatusCode(result.StatusCode, result);
        }
    }
}
