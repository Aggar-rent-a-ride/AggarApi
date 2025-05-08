using CORE.Constants;
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
        public async Task<IActionResult> CreateReportAsync([FromBody] CreateReportDto createReportDto)
        {
            var userId = UserHelpers.GetUserId(User);
            var result = await _reportService.CreateReportAsync(createReportDto, userId);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> GetReportAsync(int reportId)
        {
            var result = await _reportService.GetReportByIdAsync(reportId);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPut("status")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> UpdateReportsStatusAsync(UpdateReportsStatusDto dto)
        {
            var result = await _reportService.UpdateReportsStatusAsync(dto);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet("filter")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> FilterReportsAsync(ReportFilterDto dto)
        {
            var result = await _reportService.FilterReportsAsync(dto);
            return StatusCode(result.StatusCode, result);
        }
    }
}
