using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WadiAlAin.Application.Common.Models;
using WadiAlAin.Application.DTOs.Common;
using WadiAlAin.Application.Interfaces;

namespace WadiAlAin.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RawMaterialsController : ControllerBase
    {
        private readonly IRawMaterialsService _rawService;

        public RawMaterialsController(IRawMaterialsService rawService)
        {
            _rawService = rawService;
        }

        [HttpGet("reports")]
        public async Task<ActionResult<Result<IEnumerable<dynamic>>>> GetReports([FromQuery] int? branchId)
        {
            var reports = await _rawService.GetReportsAsync(branchId);
            return Ok(Result<IEnumerable<dynamic>>.Ok(reports));
        }

        [HttpGet("reports/{id}")]
        public async Task<ActionResult<Result<dynamic>>> GetReportDetails(int id)
        {
            var res = await _rawService.GetReportDetailsAsync(id);
            if (!res.Success) return NotFound(res);
            return Ok(res);
        }

        [HttpPost("reports")]
        public async Task<ActionResult<Result<int>>> SaveReport([FromBody] System.Text.Json.JsonElement body)
        {
            var res = await _rawService.SaveReportAsync(body);
            if (!res.Success) return StatusCode(500, res);
            return Ok(res);
        }

        [HttpDelete("reports/{id}")]
        public async Task<ActionResult<Result<bool>>> DeleteReport(int id)
        {
            var res = await _rawService.DeleteReportAsync(id);
            return Ok(res);
        }

        [HttpPut("reports/{id}/status")]
        public async Task<ActionResult<Result<bool>>> UpdateReportStatus(int id, [FromBody] UpdateStatusRequest dto)
        {
            var res = await _rawService.UpdateReportStatusAsync(id, dto);
            return Ok(res);
        }
    }
}
