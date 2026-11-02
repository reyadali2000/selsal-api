using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WadiAlAin.Application.Common.Models;
using WadiAlAin.Application.Interfaces;
using WadiAlAin.Domain.Entities;

namespace WadiAlAin.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AuditController : ControllerBase
    {
        private readonly IAuditService _auditService;

        public AuditController(IAuditService auditService)
        {
            _auditService = auditService;
        }

        [HttpGet("logs")]
        public async Task<ActionResult<Result<IEnumerable<AuditLog>>>> GetLogs([FromQuery] string? entityType, [FromQuery] int? entityId)
        {
            var logs = await _auditService.GetAuditLogsAsync(entityType, entityId);
            return Ok(Result<IEnumerable<AuditLog>>.Ok(logs));
        }

        [HttpGet("archived-reports")]
        public async Task<ActionResult<Result<IEnumerable<dynamic>>>> GetArchivedReports([FromQuery] int? logId, [FromQuery] string? logType)
        {
            var reports = await _auditService.GetArchivedReportsAsync(logId, logType);
            return Ok(Result<IEnumerable<dynamic>>.Ok(reports));
        }

        [HttpPost("archived-reports")]
        public async Task<ActionResult<Result<int>>> ArchiveReport([FromBody] System.Text.Json.JsonElement archiveData)
        {
            var res = await _auditService.ArchiveReportAsync(archiveData);
            return Ok(res);
        }
    }
}
