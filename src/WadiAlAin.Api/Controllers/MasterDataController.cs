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
    public class MasterDataController : ControllerBase
    {
        private readonly IMasterDataService _masterService;

        public MasterDataController(IMasterDataService masterService)
        {
            _masterService = masterService;
        }

        [AllowAnonymous]
        [HttpGet("branches")]
        public async Task<ActionResult<Result<IEnumerable<Branch>>>> GetBranches()
        {
            var branches = await _masterService.GetBranchesAsync();
            return Ok(Result<IEnumerable<Branch>>.Ok(branches));
        }

        [HttpGet("shifts")]
        public async Task<ActionResult<Result<IEnumerable<Shift>>>> GetShifts([FromQuery] int? branchId)
        {
            var shifts = await _masterService.GetShiftsAsync(branchId);
            return Ok(Result<IEnumerable<Shift>>.Ok(shifts));
        }

        [HttpPost("shifts")]
        public async Task<ActionResult<Result<Shift>>> SaveShift([FromBody] System.Text.Json.JsonElement body)
        {
            var res = await _masterService.SaveShiftAsync(body);
            if (!res.Success)
                return BadRequest(res);
            return Ok(res);
        }

        [HttpPut("shifts/{id}/toggle-active")]
        public async Task<ActionResult<Result<bool>>> ToggleShiftActive(int id, [FromBody] System.Text.Json.JsonElement body)
        {
            var res = await _masterService.ToggleShiftActiveAsync(id, body);
            return Ok(res);
        }

        [HttpDelete("shifts/{id}")]
        public async Task<ActionResult<Result<bool>>> DeleteShift(int id)
        {
            var res = await _masterService.DeleteShiftAsync(id);
            if (!res.Success)
                return StatusCode(500, res);
            return Ok(res);
        }

        [HttpGet("departments")]
        public async Task<ActionResult<Result<IEnumerable<Department>>>> GetDepartments([FromQuery] int? branchId)
        {
            var departments = await _masterService.GetDepartmentsAsync(branchId);
            return Ok(Result<IEnumerable<Department>>.Ok(departments));
        }

        [HttpGet("positions")]
        public async Task<ActionResult<Result<IEnumerable<Position>>>> GetPositions([FromQuery] int? departmentId)
        {
            var positions = await _masterService.GetPositionsAsync(departmentId);
            return Ok(Result<IEnumerable<Position>>.Ok(positions));
        }

        [HttpGet("products")]
        public async Task<ActionResult<Result<IEnumerable<Product>>>> GetProducts([FromQuery] int? branchId)
        {
            var products = await _masterService.GetProductsAsync(branchId);
            return Ok(Result<IEnumerable<Product>>.Ok(products));
        }

        [HttpPost("products")]
        public async Task<ActionResult<Result<Product>>> SaveProduct([FromBody] System.Text.Json.JsonElement body)
        {
            var res = await _masterService.SaveProductAsync(body);
            if (!res.Success)
                return BadRequest(res);
            return Ok(res);
        }

        [HttpPut("products/{id}/toggle-active")]
        public async Task<ActionResult<Result<bool>>> ToggleProductActive(int id, [FromBody] System.Text.Json.JsonElement body)
        {
            var res = await _masterService.ToggleProductActiveAsync(id, body);
            return Ok(res);
        }

        [HttpDelete("products/{id}")]
        public async Task<ActionResult<Result<bool>>> DeleteProduct(int id)
        {
            var res = await _masterService.DeleteProductAsync(id);
            if (!res.Success)
                return StatusCode(500, res);
            return Ok(res);
        }

        [HttpGet("test-parameters")]
        public async Task<ActionResult<Result<IEnumerable<TestParameter>>>> GetTestParameters()
        {
            var parameters = await _masterService.GetTestParametersAsync();
            return Ok(Result<IEnumerable<TestParameter>>.Ok(parameters));
        }

        [HttpGet("quality-standards")]
        public async Task<ActionResult<Result<IEnumerable<QualityStandard>>>> GetQualityStandards(
            [FromQuery] int? productId, 
            [FromQuery] string? waterType)
        {
            var standards = await _masterService.GetQualityStandardsAsync(productId, waterType);
            return Ok(Result<IEnumerable<QualityStandard>>.Ok(standards));
        }

        [HttpGet("daily-workers")]
        public async Task<ActionResult<Result<IEnumerable<DailyWorker>>>> GetDailyWorkers([FromQuery] int? branchId)
        {
            var workers = await _masterService.GetDailyWorkersAsync(branchId);
            return Ok(Result<IEnumerable<DailyWorker>>.Ok(workers));
        }

        [HttpPost("daily-workers")]
        public async Task<ActionResult<Result<DailyWorker>>> SaveDailyWorker([FromBody] DailyWorker worker)
        {
            var res = await _masterService.SaveDailyWorkerAsync(worker);
            if (!res.Success)
                return BadRequest(res);
            return Ok(res);
        }

        [HttpPut("daily-workers/{id}/toggle-active")]
        public async Task<ActionResult<Result<bool>>> ToggleDailyWorkerActive(int id, [FromBody] System.Text.Json.JsonElement body)
        {
            var res = await _masterService.ToggleDailyWorkerActiveAsync(id, body);
            return Ok(res);
        }

        [HttpDelete("daily-workers/{id}")]
        public async Task<ActionResult<Result<bool>>> DeleteDailyWorker(int id)
        {
            var res = await _masterService.DeleteDailyWorkerAsync(id);
            return Ok(res);
        }

        [HttpGet("employees")]
        public async Task<ActionResult<Result<IEnumerable<Employee>>>> GetEmployees([FromQuery] int? branchId)
        {
            var employees = await _masterService.GetEmployeesAsync(branchId);
            return Ok(Result<IEnumerable<Employee>>.Ok(employees));
        }
    }
}
