using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WadiAlAin.Application.Common.Models;
using WadiAlAin.Application.Interfaces;

namespace WadiAlAin.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DynamicReportsController : ControllerBase
    {
        private readonly IDynamicReportService _dynamicService;

        public DynamicReportsController(IDynamicReportService dynamicService)
        {
            _dynamicService = dynamicService;
        }

        [HttpGet("categories")]
        public async Task<ActionResult<Result<IEnumerable<dynamic>>>> GetCategories()
        {
            var categories = await _dynamicService.GetCategoriesAsync();
            return Ok(Result<IEnumerable<dynamic>>.Ok(categories));
        }

        [HttpGet("templates")]
        public async Task<ActionResult<Result<IEnumerable<dynamic>>>> GetTemplates([FromQuery] int? categoryId)
        {
            var templates = await _dynamicService.GetTemplatesAsync(categoryId);
            return Ok(Result<IEnumerable<dynamic>>.Ok(templates));
        }

        [HttpGet("templates/{id}")]
        public async Task<ActionResult<Result<dynamic>>> GetTemplateDetails(int id)
        {
            var res = await _dynamicService.GetTemplateDetailsAsync(id);
            if (!res.Success) return NotFound(res);
            return Ok(res);
        }

        [HttpGet("records")]
        public async Task<ActionResult<Result<IEnumerable<dynamic>>>> GetRecords([FromQuery] int? reportId, [FromQuery] int? branchId)
        {
            int? effectiveBranchId = GetEffectiveBranchId(branchId);
            var records = await _dynamicService.GetRecordsAsync(reportId, effectiveBranchId);
            return Ok(Result<IEnumerable<dynamic>>.Ok(records));
        }

        [HttpGet("stats")]
        public async Task<ActionResult<Result<object>>> GetStats([FromQuery] string? timeframe, [FromQuery] int? branchId)
        {
            int? effectiveBranchId = GetEffectiveBranchId(branchId);
            var stats = await _dynamicService.GetDynamicReportStatsAsync(timeframe, effectiveBranchId);
            return Ok(Result<object>.Ok(stats));
        }

        [HttpGet("records/pending")]
        public async Task<ActionResult<Result<IEnumerable<dynamic>>>> GetPendingRecords([FromQuery] string roleStatus, [FromQuery] int? branchId)
        {
            int? effectiveBranchId = GetEffectiveBranchId(branchId);
            var records = await _dynamicService.GetPendingRecordsForRoleAsync(roleStatus, effectiveBranchId);
            return Ok(Result<IEnumerable<dynamic>>.Ok(records));
        }

        [HttpGet("records/corrective")]
        public async Task<ActionResult<Result<IEnumerable<dynamic>>>> GetCorrectiveRecords([FromQuery] int? branchId)
        {
            int? effectiveBranchId = GetEffectiveBranchId(branchId);
            var records = await _dynamicService.GetCorrectiveActionRecordsAsync(effectiveBranchId);
            return Ok(Result<IEnumerable<dynamic>>.Ok(records));
        }

        [HttpGet("records/{id}")]
        public async Task<ActionResult<Result<dynamic>>> GetRecordDetails(int id)
        {
            int? effectiveBranchId = GetEffectiveBranchId(null);
            var res = await _dynamicService.GetRecordDetailsAsync(id, effectiveBranchId);
            if (!res.Success)
            {
                if (res.Message.Contains("غير مصرح")) return StatusCode(403, res);
                return NotFound(res);
            }

            return Ok(res);
        }

        [HttpGet("workflows")]
        public async Task<ActionResult<Result<IEnumerable<dynamic>>>> GetWorkflows()
        {
            var workflows = await _dynamicService.GetWorkflowsAsync();
            return Ok(Result<IEnumerable<dynamic>>.Ok(workflows));
        }

        [HttpGet("records/{id}/approvals")]
        public async Task<ActionResult<Result<IEnumerable<dynamic>>>> GetRecordApprovals(int id)
        {
            var approvals = await _dynamicService.GetRecordApprovalsAsync(id);
            return Ok(Result<IEnumerable<dynamic>>.Ok(approvals));
        }

        [HttpPost("records")]
        public async Task<ActionResult<Result<int>>> SaveRecord([FromBody] System.Text.Json.JsonElement body)
        {
            var (userId, username, role) = GetAuthenticatedUser();
            var res = await _dynamicService.SaveRecordAsync(userId, username, role, body);
            if (!res.Success) return StatusCode(500, res);
            return Ok(res);
        }

        [HttpPost("records/{id}/approvals")]
        public async Task<ActionResult<Result<bool>>> AddApproval(int id, [FromBody] System.Text.Json.JsonElement body)
        {
            var (userId, username, role) = GetAuthenticatedUser();
            var res = await _dynamicService.AddApprovalDecisionAsync(id, userId, username, role, body);
            if (!res.Success) return StatusCode(500, res);
            return Ok(res);
        }

        [HttpDelete("templates/{id}")]
        public async Task<ActionResult<Result<bool>>> DeleteTemplate(int id)
        {
            var res = await _dynamicService.DeleteTemplateAsync(id);
            if (!res.Success) return BadRequest(res);
            return Ok(res);
        }

        [HttpDelete("records/{id}")]
        public async Task<ActionResult<Result<bool>>> DeleteRecord(int id)
        {
            var res = await _dynamicService.DeleteRecordAsync(id);
            if (!res.Success) return BadRequest(res);
            return Ok(res);
        }

        [HttpPut("templates/{id}")]
        public async Task<ActionResult<Result<bool>>> UpdateTemplate(int id, [FromBody] System.Text.Json.JsonElement body)
        {
            var res = await _dynamicService.UpdateTemplateAsync(id, body);
            if (!res.Success) return BadRequest(res);
            return Ok(res);
        }

        [HttpPost("templates")]
        public async Task<ActionResult<Result<int>>> SaveTemplate([FromBody] System.Text.Json.JsonElement body)
        {
            var res = await _dynamicService.SaveTemplateAsync(body);
            if (!res.Success) return StatusCode(500, res);
            return Ok(res);
        }

        private (int userId, string username, string role) GetAuthenticatedUser()
        {
            int userId = int.TryParse(User.FindFirst("id")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out int uid) ? uid : 0;
            string username = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "Unknown";
            string role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "Unknown";
            return (userId, username, role);
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
