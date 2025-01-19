using DATA.Models.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnumsController : ControllerBase
    {
        [HttpGet("vehicle-status")]
        public IActionResult GetVehicleStatus()
        {
            var values = Enum.GetNames(typeof(VehicleStatus));
            return Ok(values);
        }
        [HttpGet("vehicle-physical-status")]
        public IActionResult GetVehiclePhysicalStatus()
        {
            var values = Enum.GetNames(typeof(VehiclePhysicalStatus));
            return Ok(values);
        }

    }
}
