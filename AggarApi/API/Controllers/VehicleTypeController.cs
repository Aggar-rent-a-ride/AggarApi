using CORE.Constants;
using CORE.DTOs.VehicleType;
using CORE.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class VehicleTypeController : ControllerBase
    {
        private readonly IVehicleTypeService _vehicleTypeService;

        public VehicleTypeController(IVehicleTypeService vehicleTypeService)
        {
            _vehicleTypeService = vehicleTypeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var response = await _vehicleTypeService.GetAllAsync();
            return StatusCode(response.StatusCode, response);
        }
        [HttpPost]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> CreateAsync([FromForm] CreateVehicleTypeDto dto)
        {
            var response = await _vehicleTypeService.CreateAsync(dto);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> UpdateAsync([FromForm] UpdateVehicleTypeDto dto)
        {
            var response = await _vehicleTypeService.UpdateAsync(dto);
            return StatusCode(response.StatusCode, response);
        }
        [HttpDelete]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> DeleteAsync(int Id)
        {
            var response = await _vehicleTypeService.DeleteAsync(Id);
            return StatusCode(response.StatusCode, response);
        }
    }
}
