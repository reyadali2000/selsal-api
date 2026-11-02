using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WadiAlAin.Application.Common.Models;
using WadiAlAin.Application.Interfaces;

namespace WadiAlAin.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationsService _notificationsService;

        public NotificationsController(INotificationsService notificationsService)
        {
            _notificationsService = notificationsService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<Result<IEnumerable<dynamic>>>> GetNotifications(
            [FromQuery] string? targetRole, 
            [FromQuery] string? role,
            [FromQuery] int? branchId, 
            [FromQuery] bool? isRead)
        {
            var effectiveRole = !string.IsNullOrWhiteSpace(targetRole) ? targetRole : role;
            var notifs = await _notificationsService.GetNotificationsAsync(effectiveRole, branchId, isRead);
            return Ok(Result<IEnumerable<dynamic>>.Ok(notifs));
        }

        [AllowAnonymous]
        [HttpGet("unread-count")]
        public async Task<ActionResult<Result<int>>> GetUnreadCount(
            [FromQuery] string? role, 
            [FromQuery] string? targetRole,
            [FromQuery] int? branchId)
        {
            var effectiveRole = !string.IsNullOrWhiteSpace(targetRole) ? targetRole : role;
            var res = await _notificationsService.GetUnreadCountAsync(effectiveRole, branchId);
            return Ok(res);
        }

        [HttpPost]
        public async Task<ActionResult<Result<int>>> CreateNotification([FromBody] JsonElement body)
        {
            var res = await _notificationsService.CreateNotificationAsync(body);
            if (!res.Success)
                return BadRequest(res);
            return Ok(res);
        }

        [HttpPut("{id}/read")]
        public async Task<ActionResult<Result<bool>>> MarkAsRead(int id)
        {
            var res = await _notificationsService.MarkAsReadAsync(id);
            if (!res.Success)
                return NotFound(res);
            return Ok(res);
        }

        [HttpPut("read-all")]
        public async Task<ActionResult<Result<bool>>> MarkAllAsRead(
            [FromQuery] string? role, 
            [FromQuery] string? targetRole,
            [FromQuery] int? branchId)
        {
            var effectiveRole = !string.IsNullOrWhiteSpace(targetRole) ? targetRole : role;
            var res = await _notificationsService.MarkAllAsReadAsync(effectiveRole, branchId);
            return Ok(res);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Result<bool>>> DeleteNotification(int id)
        {
            var res = await _notificationsService.DeleteNotificationAsync(id);
            if (!res.Success)
                return NotFound(res);
            return Ok(res);
        }
    }
}
