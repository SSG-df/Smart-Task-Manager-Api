using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTaskManager.Services.Interfaces;
using SmartTaskManager.DTOs;

namespace SmartTaskManager.Controllers
{
    [ApiController]
    [Route("api/reports")]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("completion")]
        public async Task<ActionResult<List<TaskCompletionReportDto>>> GetTaskCompletionReport()
        {
            var report = await _reportService.GetTaskCompletionReportAsync();
            return Ok(report);
        }

        [HttpGet("average-completion-time")]
        public async Task<ActionResult<List<AverageCompletionTimeDto>>> GetAverageCompletionTimeReport()
        {
            var report = await _reportService.GetAverageCompletionTimeReportAsync();
            return Ok(report);
        }
    }
}