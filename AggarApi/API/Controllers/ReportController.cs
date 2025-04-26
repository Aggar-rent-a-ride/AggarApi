using CORE.DTOs.Report;
using CORE.Helpers;
using CORE.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateReport([FromBody] CreateReportDto createReportDto)
        {
            var userId = UserHelpers.GetUserId(User);
            var result = await _reportService.CreateReportAsync(createReportDto, userId);
            return StatusCode(result.StatusCode, result);
        }
    }
}
