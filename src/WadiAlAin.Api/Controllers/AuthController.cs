using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WadiAlAin.Application.Common.Models;
using WadiAlAin.Application.DTOs.Auth;
using WadiAlAin.Application.Interfaces;
using WadiAlAin.Domain.Entities;

namespace WadiAlAin.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [AllowAnonymous]
        [HttpGet("ping")]
        public ActionResult<Result<string>> Ping()
        {
            return Ok(Result<string>.Ok("سيرفر الجودة وقاعدة البيانات يعملان بنجاح.", "نجح الاتصال"));
        }

        [HttpPost("login")]
        public async Task<ActionResult<Result<LoginResponse>>> Login([FromBody] LoginRequest request)
        {
            var res = await _authService.LoginAsync(request);
            if (!res.Success)
            {
                return Unauthorized(Result<LoginResponse>.Fail(res.Message));
            }
            return Ok(Result<LoginResponse>.Ok(res));
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<Result<UserDto>>> GetCurrentUser()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized(Result<UserDto>.Fail("مستخدم غير صالح."));

            var res = await _authService.GetCurrentUserAsync(userId);
            if (!res.Success)
                return NotFound(res);
            return Ok(res);
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<ActionResult<Result<bool>>> ChangePassword([FromBody] ChangePasswordRequest req)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized(Result<bool>.Fail("غير مصرح."));

            var res = await _authService.ChangePasswordAsync(userId, req);
            if (!res.Success)
                return BadRequest(res);
            return Ok(res);
        }

        [Authorize]
        [HttpGet("signature")]
        public async Task<ActionResult<Result<UserSignatureDto>>> GetSignature()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized(Result<UserSignatureDto>.Fail("غير مصرح."));

            var res = await _authService.GetSignatureAsync(userId);
            return Ok(res);
        }

        [Authorize]
        [HttpGet("users/{userId}/signature")]
        public async Task<ActionResult<Result<UserSignatureDto>>> GetUserSignature(int userId)
        {
            var res = await _authService.GetSignatureAsync(userId);
            return Ok(res);
        }

        [Authorize]
        [HttpPost("signature")]
        public async Task<ActionResult<Result<bool>>> SaveSignature([FromBody] UserSignatureDto sig)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int userId = 0;
            if (!int.TryParse(userIdStr, out userId) || userId <= 0)
            {
                if (sig.UserId > 0)
                    userId = sig.UserId;
                else
                    return Unauthorized(Result<bool>.Fail("غير مصرح."));
            }

            var targetUserId = (sig.UserId > 0) ? sig.UserId : userId;
            var res = await _authService.SaveSignatureAsync(targetUserId, sig);
            return Ok(res);
        }

        [Authorize]
        [HttpDelete("signature")]
        public async Task<ActionResult<Result<bool>>> DeleteSignature([FromQuery] int? userId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int currentUserId = 0;
            int.TryParse(userIdStr, out currentUserId);

            int targetUserId = (userId.HasValue && userId.Value > 0) ? userId.Value : currentUserId;
            if (targetUserId <= 0)
                return Unauthorized(Result<bool>.Fail("غير مصرح."));

            var res = await _authService.DeleteSignatureAsync(targetUserId);
            return Ok(res);
        }

        [Authorize]
        [HttpGet("users")]
        public async Task<ActionResult<Result<IEnumerable<dynamic>>>> GetAllUsers([FromQuery] int? branchId)
        {
            var users = await _authService.GetAllUsersAsync(branchId);
            return Ok(Result<IEnumerable<dynamic>>.Ok(users));
        }

        [Authorize]
        [HttpPost("users")]
        public async Task<ActionResult<Result<int>>> SaveUser([FromBody] JsonElement body)
        {
            var res = await _authService.SaveUserAsync(body);
            if (!res.Success)
                return BadRequest(res);
            return Ok(res);
        }

        [Authorize]
        [HttpPut("users/{id}/toggle-active")]
        public async Task<ActionResult<Result<bool>>> ToggleUserActive(int id, [FromBody] JsonElement body)
        {
            var res = await _authService.ToggleUserActiveAsync(id, body);
            if (!res.Success)
                return BadRequest(res);
            return Ok(res);
        }

        [AllowAnonymous]
        [HttpGet("roles")]
        public async Task<ActionResult<Result<IEnumerable<Role>>>> GetAllRoles()
        {
            var roles = await _authService.GetAllRolesAsync();
            return Ok(Result<IEnumerable<Role>>.Ok(roles));
        }

        [Authorize]
        [HttpPost("roles")]
        public async Task<ActionResult<Result<bool>>> SaveRole([FromBody] JsonElement body)
        {
            var res = await _authService.SaveRoleAsync(body);
            if (!res.Success)
                return BadRequest(res);
            return Ok(res);
        }

        [Authorize]
        [HttpDelete("roles/{id}")]
        public async Task<ActionResult<Result<bool>>> DeleteRole(int id)
        {
            var res = await _authService.DeleteRoleAsync(id);
            if (!res.Success)
                return BadRequest(res);
            return Ok(res);
        }
    }
}
