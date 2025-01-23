using CORE.DTOs.Vehicle;
using CORE.Services.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehicleController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;

        public VehicleController(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }
        [HttpPost]
        public async Task<IActionResult> CreateVehicleAsync([FromForm]CreateVehicleDto createVehicleDto)
        {
            var response = await _vehicleService.CreateVehicleAsync(createVehicleDto, 8);
            return StatusCode(response.StatusCode, response);
        }
    }
}
