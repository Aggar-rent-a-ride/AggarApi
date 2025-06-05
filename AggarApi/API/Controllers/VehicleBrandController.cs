using CORE.Constants;
using CORE.DTOs.VehicleBrand;
using CORE.DTOs.VehicleType;
using CORE.Services;
using CORE.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class VehicleBrandController : ControllerBase
    {
        private readonly IVehicleBrandService _vehicleBrandService;

        public VehicleBrandController(IVehicleBrandService vehicleBrandService)
        {
            _vehicleBrandService = vehicleBrandService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var response = await _vehicleBrandService.GetAllAsync();
            return StatusCode(response.StatusCode, response);
        }
        [HttpPost]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> CreateAsync([FromForm] CreateVehicleBrandDto dto)
        {
            var response = await _vehicleBrandService.CreateAsync(dto);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> UpdateAsync([FromForm] UpdateVehicleBrandDto dto)
        {
            var response = await _vehicleBrandService.UpdateAsync(dto);
            return StatusCode(response.StatusCode, response);
        }
        [HttpDelete]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> DeleteAsync(int Id)
        {
            var response = await _vehicleBrandService.DeleteAsync(Id);
            return StatusCode(response.StatusCode, response);
        }
    }
}
