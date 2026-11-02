using Dapper;
using WadiAlAin.Application.Common.Interfaces;
using WadiAlAin.Application.Common.Models;
using WadiAlAin.Application.Interfaces;
using WadiAlAin.Domain.Entities;
using WadiAlAin.Application.Common.Helpers;
using System.Text.Json;

namespace WadiAlAin.Infrastructure.Services
{
    public class MasterDataService : IMasterDataService
    {
        private readonly IDbConnectionFactory _db;

        public MasterDataService(IDbConnectionFactory db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Branch>> GetBranchesAsync()
        {
            using var conn = _db.CreateConnection();
            return await conn.QueryAsync<Branch>("SELECT id, name, code, location, is_active AS IsActive FROM Branches ORDER BY id");
        }

        public async Task<IEnumerable<Shift>> GetShiftsAsync(int? branchId)
        {
            using var conn = _db.CreateConnection();
            string sql = "SELECT id, branch_id AS BranchId, name, start_time AS StartTime, end_time AS EndTime, season_name AS SeasonName, is_active AS IsActive FROM Shifts";
            if (branchId.HasValue)
                sql += " WHERE branch_id = @BranchId";
            sql += " ORDER BY id";
            return await conn.QueryAsync<Shift>(sql, new { BranchId = branchId });
        }

        public async Task<Result<Shift>> SaveShiftAsync(JsonElement body)
        {
            int branchId = body.GetInt("branchId", "branch_id", 1);
            string name = body.GetString("name", null, null, "");
            string startTime = body.GetString("startTime", "start_time", null, "");
            string endTime = body.GetString("endTime", "end_time", null, "");
            string seasonName = body.GetString("seasonName", "season_name", null, "التوقيت الصيفي");
            bool isActive = body.GetBool("isActive", "is_active", true);

            if (string.IsNullOrWhiteSpace(name))
                return Result<Shift>.Fail("اسم الوردية مطلوب.");

            using var conn = _db.CreateConnection();

            var shift = new Shift
            {
                BranchId = branchId,
                Name = name.Trim(),
                StartTime = startTime.Trim(),
                EndTime = endTime.Trim(),
                SeasonName = seasonName.Trim(),
                IsActive = isActive
            };

            int id = body.GetInt("id", null, 0);

            if (id > 0)
            {
                shift.Id = id;
                await conn.ExecuteAsync(@"
                    UPDATE Shifts 
                    SET branch_id = @BranchId, 
                        name = @Name, 
                        start_time = @StartTime, 
                        end_time = @EndTime, 
                        season_name = @SeasonName, 
                        is_active = @IsActive
                    WHERE id = @Id",
                    new 
                    { 
                        shift.BranchId, 
                        shift.Name, 
                        shift.StartTime, 
                        shift.EndTime, 
                        shift.SeasonName, 
                        IsActive = shift.IsActive ? 1 : 0, 
                        Id = id 
                    });
            }
            else
            {
                shift.Id = await conn.QuerySingleAsync<int>(@"
                    INSERT INTO Shifts (branch_id, name, start_time, end_time, season_name, is_active)
                    VALUES (@BranchId, @Name, @StartTime, @EndTime, @SeasonName, @IsActive);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);",
                    new 
                    { 
                        shift.BranchId, 
                        shift.Name, 
                        shift.StartTime, 
                        shift.EndTime, 
                        shift.SeasonName, 
                        IsActive = shift.IsActive ? 1 : 0 
                    });
            }

            return Result<Shift>.Ok(shift, "تم حفظ بيانات الوردية بنجاح.");
        }

        public async Task<Result<bool>> ToggleShiftActiveAsync(int id, JsonElement body)
        {
            bool isActive = body.GetBool("isActive", "is_active", false);
            using var conn = _db.CreateConnection();
            await conn.ExecuteAsync("UPDATE Shifts SET is_active = @IsActive WHERE id = @Id", 
                new { IsActive = isActive ? 1 : 0, Id = id });
            return Result<bool>.Ok(true, "تم تحديث حالة الوردية بنجاح.");
        }

        public async Task<Result<bool>> DeleteShiftAsync(int id)
        {
            using var conn = _db.CreateConnection();
            try
            {
                int rows = await conn.ExecuteAsync("DELETE FROM Shifts WHERE id = @Id", new { Id = id });
                if (rows == 0)
                    return Result<bool>.Fail("لم يتم العثور على الوردية.");
                return Result<bool>.Ok(true, "تم حذف الوردية بنجاح.");
            }
            catch (Exception ex)
            {
                return Result<bool>.Fail($"لا يمكن حذف الوردية لوجود تقارير أو سجلات مرتبطة بها. يمكنك تعطيل الوردية بدلاً من حذفها ({ex.Message}).");
            }
        }

        public async Task<IEnumerable<Department>> GetDepartmentsAsync(int? branchId)
        {
            using var conn = _db.CreateConnection();
            string sql = "SELECT id, name, code, description, parent_id AS ParentId, branch_id AS BranchId, is_active AS IsActive, sort_order AS SortOrder, created_at AS CreatedAt, updated_at AS UpdatedAt FROM departments";
            if (branchId.HasValue)
                sql += " WHERE branch_id = @BranchId";
            sql += " ORDER BY sort_order, id";
            return await conn.QueryAsync<Department>(sql, new { BranchId = branchId });
        }

        public async Task<IEnumerable<Position>> GetPositionsAsync(int? departmentId)
        {
            using var conn = _db.CreateConnection();
            string sql = "SELECT id, name, code, department_id AS DepartmentId, level, description, is_active AS IsActive, created_at AS CreatedAt, updated_at AS UpdatedAt FROM positions";
            if (departmentId.HasValue)
                sql += " WHERE department_id = @DepartmentId";
            sql += " ORDER BY level, id";
            return await conn.QueryAsync<Position>(sql, new { DepartmentId = departmentId });
        }

        public async Task<IEnumerable<Product>> GetProductsAsync(int? branchId)
        {
            using var conn = _db.CreateConnection();
            string sql = "SELECT id, branch_id AS BranchId, name, volume_liters AS VolumeLiters, is_active AS IsActive FROM Products";
            if (branchId.HasValue)
                sql += " WHERE branch_id = @BranchId";
            sql += " ORDER BY id";
            return await conn.QueryAsync<Product>(sql, new { BranchId = branchId });
        }

        public async Task<Result<Product>> SaveProductAsync(JsonElement body)
        {
            int branchId = body.GetInt("branchId", "branch_id", 1);
            string name = body.GetString("name", null, null, "");
            decimal volume = body.GetDecimal("volumeLiters", "volume_liters", 0m);

            if (string.IsNullOrWhiteSpace(name))
                return Result<Product>.Fail("اسم العبوة مطلوب.");

            using var conn = _db.CreateConnection();

            var product = new Product
            {
                BranchId = branchId,
                Name = name,
                VolumeLiters = volume,
                IsActive = body.GetBool("isActive", "is_active", true)
            };

            int id = body.GetInt("id", null, 0);

            if (id > 0)
            {
                product.Id = id;
                await conn.ExecuteAsync(@"
                    UPDATE Products 
                    SET branch_id = @BranchId, name = @Name, volume_liters = @Volume, is_active = @IsActive
                    WHERE id = @Id",
                    new { BranchId = branchId, Name = name, Volume = volume, product.IsActive, Id = id });
            }
            else
            {
                product.Id = await conn.QuerySingleAsync<int>(@"
                    INSERT INTO Products (branch_id, name, volume_liters, is_active)
                    VALUES (@BranchId, @Name, @Volume, @IsActive);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);",
                    new { BranchId = branchId, Name = name, Volume = volume, product.IsActive });
            }

            return Result<Product>.Ok(product, "تم حفظ بيانات العبوة بنجاح.");
        }

        public async Task<Result<bool>> ToggleProductActiveAsync(int id, JsonElement body)
        {
            bool isActive = body.GetBool("isActive", "is_active", false);
            using var conn = _db.CreateConnection();
            await conn.ExecuteAsync("UPDATE Products SET is_active = @IsActive WHERE id = @Id", new { IsActive = isActive ? 1 : 0, Id = id });
            return Result<bool>.Ok(true, "تم تحديث حالة العبوة بنجاح.");
        }

        public async Task<Result<bool>> DeleteProductAsync(int id)
        {
            using var conn = _db.CreateConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();
            try
            {
                await conn.ExecuteAsync("DELETE FROM QualityStandards WHERE product_id = @Id", new { Id = id }, trans);
                await conn.ExecuteAsync("DELETE FROM Products WHERE id = @Id", new { Id = id }, trans);
                trans.Commit();
                return Result<bool>.Ok(true, "تم حذف العبوة بنجاح.");
            }
            catch (Exception ex)
            {
                trans.Rollback();
                return Result<bool>.Fail($"حدث خطأ أثناء حذف العبوة: {ex.Message}");
            }
        }

        public async Task<IEnumerable<TestParameter>> GetTestParametersAsync()
        {
            using var conn = _db.CreateConnection();
            return await conn.QueryAsync<TestParameter>("SELECT id, category, name_ar AS NameAr, unit, input_type AS InputType FROM TestParameters ORDER BY id");
        }

        public async Task<IEnumerable<QualityStandard>> GetQualityStandardsAsync(int? productId, string? waterType)
        {
            using var conn = _db.CreateConnection();
            string sql = "SELECT id, product_id AS ProductId, water_type AS WaterType, parameter_id AS ParameterId, min_value AS MinValue, max_value AS MaxValue, expected_text AS ExpectedText FROM QualityStandards WHERE 1=1";
            if (productId.HasValue)
                sql += " AND (product_id = @ProductId OR product_id IS NULL)";
            if (!string.IsNullOrWhiteSpace(waterType))
                sql += " AND (water_type = @WaterType OR water_type IS NULL)";
            sql += " ORDER BY id";
            return await conn.QueryAsync<QualityStandard>(sql, new { ProductId = productId, WaterType = waterType });
        }

        public async Task<IEnumerable<DailyWorker>> GetDailyWorkersAsync(int? branchId)
        {
            using var conn = _db.CreateConnection();
            string sql = "SELECT id, branch_id AS BranchId, full_name AS FullName, name_key AS NameKey, job_title AS JobTitle, is_active AS IsActive, created_at AS CreatedAt FROM DailyWorkers";
            if (branchId.HasValue)
                sql += " WHERE branch_id = @BranchId";
            sql += " ORDER BY full_name";
            return await conn.QueryAsync<DailyWorker>(sql, new { BranchId = branchId });
        }

        public async Task<Result<DailyWorker>> SaveDailyWorkerAsync(DailyWorker worker)
        {
            if (string.IsNullOrWhiteSpace(worker.FullName))
                return Result<DailyWorker>.Fail("اسم العامل مطلوب.");

            using var conn = _db.CreateConnection();
            var nameKey = worker.FullName.Trim();
            int branchId = worker.BranchId <= 0 ? 1 : worker.BranchId;

            if (worker.Id > 0)
            {
                await conn.ExecuteAsync(@"
                    UPDATE DailyWorkers 
                    SET full_name = @FullName, name_key = @NameKey, job_title = @JobTitle, branch_id = @BranchId, is_active = @IsActive
                    WHERE id = @Id",
                    new { FullName = worker.FullName.Trim(), NameKey = nameKey, JobTitle = string.IsNullOrWhiteSpace(worker.JobTitle) ? "عامل بالأجر اليومي" : worker.JobTitle.Trim(), BranchId = branchId, worker.IsActive, worker.Id });
            }
            else
            {
                worker.Id = await conn.QuerySingleAsync<int>(@"
                    INSERT INTO DailyWorkers (branch_id, full_name, name_key, job_title, is_active, created_at)
                    VALUES (@BranchId, @FullName, @NameKey, @JobTitle, 1, @Now);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);",
                    new { BranchId = branchId, FullName = worker.FullName.Trim(), NameKey = nameKey, JobTitle = string.IsNullOrWhiteSpace(worker.JobTitle) ? "عامل بالأجر اليومي" : worker.JobTitle.Trim(), Now = DateTime.UtcNow.ToString("o") });
            }

            return Result<DailyWorker>.Ok(worker, "تم حفظ بيانات العامل بنجاح.");
        }

        public async Task<Result<bool>> ToggleDailyWorkerActiveAsync(int id, JsonElement body)
        {
            bool isActive = body.GetBool("isActive", "is_active", false);
            using var conn = _db.CreateConnection();
            await conn.ExecuteAsync("UPDATE DailyWorkers SET is_active = @IsActive WHERE id = @Id", new { IsActive = isActive ? 1 : 0, Id = id });
            return Result<bool>.Ok(true, "تم تحديث حالة العامل بنجاح.");
        }

        public async Task<Result<bool>> DeleteDailyWorkerAsync(int id)
        {
            using var conn = _db.CreateConnection();
            await conn.ExecuteAsync("DELETE FROM DailyWorkers WHERE id = @Id", new { Id = id });
            return Result<bool>.Ok(true, "تم حذف العامل بنجاح.");
        }

        public async Task<IEnumerable<Employee>> GetEmployeesAsync(int? branchId)
        {
            using var conn = _db.CreateConnection();
            string sql = "SELECT id, branch_id AS BranchId, full_name AS FullName, job_title AS JobTitle, is_active AS IsActive FROM Employees";
            if (branchId.HasValue)
                sql += " WHERE branch_id = @BranchId";
            sql += " ORDER BY full_name";
            return await conn.QueryAsync<Employee>(sql, new { BranchId = branchId });
        }
    }
}
