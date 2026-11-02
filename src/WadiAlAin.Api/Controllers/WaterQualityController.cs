using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WadiAlAin.Application.Common.Models;
using WadiAlAin.Application.DTOs.Common;
using WadiAlAin.Application.Interfaces;
using WadiAlAin.Domain.Entities;

namespace WadiAlAin.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WaterQualityController : ControllerBase
    {
        private readonly IWaterQualityService _waterService;

        public WaterQualityController(IWaterQualityService waterService)
        {
            _waterService = waterService;
        }

        [HttpGet("logs")]
        public async Task<ActionResult<Result<IEnumerable<WaterQualityReport>>>> GetLogs(
            [FromQuery] int? branchId, 
            [FromQuery] string? category, 
            [FromQuery] string? fromDate, 
            [FromQuery] string? toDate)
        {
            int? effectiveBranchId = GetEffectiveBranchId(branchId);
            var logs = await _waterService.GetLogsAsync(effectiveBranchId, category, fromDate, toDate);
            return Ok(Result<IEnumerable<WaterQualityReport>>.Ok(logs));
        }

        [HttpGet("logs/{id}")]
        public async Task<ActionResult<Result<dynamic>>> GetLogById(int id)
        {
            var res = await _waterService.GetLogByIdAsync(id);
            if (!res.Success)
                return NotFound(res);

            int? effectiveBranchId = GetEffectiveBranchId(null);
            if (effectiveBranchId.HasValue)
            {
                try
                {
                    var json = System.Text.Json.JsonSerializer.SerializeToElement(res.Data);
                    System.Text.Json.JsonElement logElem;
                    if (json.TryGetProperty("Log", out logElem) || json.TryGetProperty("log", out logElem))
                    {
                        int? logBranchId = null;
                        if (logElem.TryGetProperty("branch_id", out var bProp) && bProp.TryGetInt32(out var bVal))
                        {
                            logBranchId = bVal;
                        }
                        int effectiveLogBranch = (logBranchId == null || logBranchId == 0) ? 1 : logBranchId.Value;
                        if (effectiveLogBranch != effectiveBranchId.Value)
                        {
                            return StatusCode(403, Result<dynamic>.Fail("غير مصرح لك بالاطلاع على سجلات تابعة لمصنع آخر."));
                        }
                    }
                }
                catch { }
            }

            return Ok(res);
        }

        [HttpPost("logs")]
        public async Task<ActionResult<Result<int>>> SaveLog([FromBody] System.Text.Json.JsonElement body)
        {
            var res = await _waterService.SaveLogAsync(body);
            if (!res.Success)
                return StatusCode(500, res);
            return Ok(res);
        }

        [HttpPut("logs/{id}/status")]
        [HttpPut("logs/{id}/approval")]
        public async Task<ActionResult<Result<bool>>> UpdateLogStatus(int id, [FromBody] System.Text.Json.JsonElement body)
        {
            var res = await _waterService.UpdateLogStatusAsync(id, body);
            if (!res.Success)
                return StatusCode(500, res);
            return Ok(res);
        }

        [HttpPost("logs/{id}/micro")]
        public async Task<ActionResult<Result<bool>>> SaveMicroResults(int id, [FromBody] System.Text.Json.JsonElement body)
        {
            var res = await _waterService.SaveMicroResultsAsync(id, body);
            if (!res.Success)
                return StatusCode(500, res);
            return Ok(res);
        }

        [HttpDelete("logs/{id}")]
        public async Task<ActionResult<Result<bool>>> DeleteLog(int id)
        {
            var res = await _waterService.DeleteLogAsync(id);
            return Ok(res);
        }

        [HttpGet("daily-reports")]
        public async Task<ActionResult<Result<IEnumerable<dynamic>>>> GetDailyReports()
        {
            var reports = await _waterService.GetDailyWaterReportsAsync();
            return Ok(Result<IEnumerable<dynamic>>.Ok(reports));
        }

        [HttpPost("daily-reports")]
        public async Task<ActionResult<Result<int>>> SaveDailyReport([FromBody] System.Text.Json.JsonElement body)
        {
            var res = await _waterService.SaveDailyWaterReportAsync(body);
            if (!res.Success)
                return StatusCode(500, res);
            return Ok(res);
        }

        private int? GetEffectiveBranchId(int? requestedBranchId)
        {
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "";
            bool isSuperUser = role.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                               role.Equals("GeneralManager", StringComparison.OrdinalIgnoreCase);

            var branchClaim = User.FindFirst("branch_id")?.Value;
            if (!isSuperUser && int.TryParse(branchClaim, out int tokenBranchId) && tokenBranchId > 0)
            {
                return tokenBranchId;
            }

            return requestedBranchId;
        }
    }
}
