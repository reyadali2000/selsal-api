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
    public class HygieneController : ControllerBase
    {
        private readonly IHygieneService _hygieneService;
        private readonly IMasterDataService _masterService;

        public HygieneController(IHygieneService hygieneService, IMasterDataService masterService)
        {
            _hygieneService = hygieneService;
            _masterService = masterService;
        }

        [HttpGet("reports")]
        public async Task<ActionResult<Result<IEnumerable<dynamic>>>> GetReports([FromQuery] int? branchId)
        {
            int? effectiveBranchId = GetEffectiveBranchId(branchId);
            var reports = await _hygieneService.GetReportsAsync(effectiveBranchId);
            return Ok(Result<IEnumerable<dynamic>>.Ok(reports));
        }

        [HttpGet("reports/{id}")]
        public async Task<ActionResult<Result<dynamic>>> GetReportDetails(int id)
        {
            int? effectiveBranchId = GetEffectiveBranchId(null);
            var res = await _hygieneService.GetReportDetailsAsync(id, effectiveBranchId);
            if (!res.Success)
            {
                if (res.Message.Contains("غير مصرح")) return StatusCode(403, res);
                return NotFound(res);
            }

            return Ok(res);
        }

        [HttpDelete("reports/{id}")]
        public async Task<ActionResult<Result<bool>>> DeleteReport(int id)
        {
            var res = await _hygieneService.DeleteReportAsync(id);
            return Ok(res);
        }

        [HttpGet("checklists")]
        public async Task<ActionResult<Result<IEnumerable<dynamic>>>> GetChecklists()
        {
            var checklists = await _hygieneService.GetChecklistsAsync();
            return Ok(Result<IEnumerable<dynamic>>.Ok(checklists));
        }

        [HttpGet("workers")]
        public async Task<ActionResult<Result<IEnumerable<dynamic>>>> GetWorkers([FromQuery] int? branchId)
        {
            int? effectiveBranchId = GetEffectiveBranchId(branchId);
            var workers = await _masterService.GetDailyWorkersAsync(effectiveBranchId);
            return Ok(Result<IEnumerable<dynamic>>.Ok(workers));
        }

        [HttpGet("employees")]
        public async Task<ActionResult<Result<IEnumerable<dynamic>>>> GetEmployees([FromQuery] int? branchId)
        {
            int? effectiveBranchId = GetEffectiveBranchId(branchId);
            var employees = await _masterService.GetEmployeesAsync(effectiveBranchId);
            return Ok(Result<IEnumerable<dynamic>>.Ok(employees));
        }

        [HttpPost("reports")]
        public async Task<ActionResult<Result<int>>> SaveReport([FromBody] System.Text.Json.JsonElement body)
        {
            var res = await _hygieneService.SaveReportAsync(body);
            if (!res.Success) return StatusCode(500, res);
            return Ok(res);
        }

        [HttpGet("logs")]
        public async Task<ActionResult<Result<IEnumerable<dynamic>>>> GetHygieneLogs([FromQuery] int? reportId, [FromQuery] int? branchId)
        {
            int? effectiveBranchId = GetEffectiveBranchId(branchId);
            var logs = await _hygieneService.GetHygieneLogsAsync(reportId, effectiveBranchId);
            return Ok(Result<IEnumerable<dynamic>>.Ok(logs));
        }

        [HttpPut("reports/{id}/status")]
        public async Task<ActionResult<Result<bool>>> UpdateReportStatus(int id, [FromBody] UpdateStatusRequest dto)
        {
            var res = await _hygieneService.UpdateReportStatusAsync(id, dto);
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
