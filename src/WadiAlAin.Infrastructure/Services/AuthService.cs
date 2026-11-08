using System.Security.Cryptography;
using System.Text.Json;
using Dapper;
using WadiAlAin.Application.Common.Interfaces;
using WadiAlAin.Application.Common.Models;
using WadiAlAin.Application.DTOs.Auth;
using WadiAlAin.Application.Interfaces;
using WadiAlAin.Domain.Entities;
using WadiAlAin.Infrastructure.Authentication;

namespace WadiAlAin.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly IDbConnectionFactory _db;
        private readonly JwtTokenService _jwt;

        public AuthService(IDbConnectionFactory db, JwtTokenService jwt)
        {
            _db = db;
            _jwt = jwt;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return new LoginResponse { Success = false, Message = "اسم المستخدم وكلمة المرور مطلوبة." };
            }

            using var conn = _db.CreateConnection();
            var user = await conn.QuerySingleOrDefaultAsync<dynamic>(
                "SELECT * FROM Users WHERE LOWER(username) = LOWER(@Username) AND is_active = 1",
                new { request.Username });

            if (user == null)
            {
                return new LoginResponse { Success = false, Message = "اسم المستخدم أو كلمة المرور غير صحيحة." };
            }

            string storedHash = user.password_hash ?? string.Empty;
            if (!PasswordHasher.VerifyPassword(request.Password, storedHash))
            {
                return new LoginResponse { Success = false, Message = "اسم المستخدم أو كلمة المرور غير صحيحة." };
            }

            int userId = user.id;
            string username = user.username;
            string role = user.role ?? "User";
            int? branchId = user.branch_id;

            List<string> permissions = await ResolveUserPermissionsAsync(conn, role);

            await conn.ExecuteAsync(
                "UPDATE Users SET last_login = @Now WHERE id = @Id",
                new { Now = DateTime.UtcNow.ToString("o"), Id = userId });

            var token = _jwt.GenerateToken(userId, username, role, branchId);

            return new LoginResponse
            {
                Success = true,
                Message = "تم تسجيل الدخول بنجاح.",
                Token = token,
                User = new UserDto
                {
                    Id = userId,
                    BranchId = branchId,
                    FullName = (string)(user.full_name ?? string.Empty),
                    Username = username,
                    Role = role,
                    Email = (string?)user.email,
                    Phone = (string?)user.phone,
                    JobTitle = (string?)user.job_title,
                    IsActive = user.is_active is bool b ? b : (user.is_active == 1),
                    AvatarColor = (long?)user.avatar_color,
                    LastLogin = DateTime.UtcNow.ToString("o"),
                    Permissions = permissions
                },
                Permissions = permissions
            };
        }

        public async Task<Result<UserDto>> GetCurrentUserAsync(int userId)
        {
            using var conn = _db.CreateConnection();
            var user = await conn.QuerySingleOrDefaultAsync<dynamic>(
                "SELECT * FROM Users WHERE id = @Id AND is_active = 1",
                new { Id = userId });

            if (user == null)
                return Result<UserDto>.Fail("لم يتم العثور على المستخدم.");

            string role = (string)(user.role ?? "User");
            List<string> permissions = await ResolveUserPermissionsAsync(conn, role);

            return Result<UserDto>.Ok(new UserDto
            {
                Id = user.id,
                BranchId = user.branch_id,
                FullName = (string)(user.full_name ?? string.Empty),
                Username = user.username,
                Role = role,
                Email = (string?)user.email,
                Phone = (string?)user.phone,
                JobTitle = (string?)user.job_title,
                IsActive = user.is_active is bool b ? b : (user.is_active == 1),
                AvatarColor = (long?)user.avatar_color,
                LastLogin = user.last_login,
                Permissions = permissions
            });
        }

        private async Task<List<string>> ResolveUserPermissionsAsync(System.Data.IDbConnection conn, string role)
        {
            if (string.IsNullOrWhiteSpace(role))
                return new List<string>();

            string normalizedRole = role.Replace("_", "").Replace(" ", "").ToLower();

            var roleRow = await conn.QueryFirstOrDefaultAsync<dynamic>(@"
                SELECT permissions_json, code, title FROM CustomRoles 
                WHERE LOWER(code) = LOWER(@Role)
                   OR LOWER(REPLACE(REPLACE(code, '_', ''), ' ', '')) = @Normalized
                   OR LOWER(title) = LOWER(@Role)
                   OR LOWER(REPLACE(REPLACE(title, '_', ''), ' ', '')) = @Normalized
                   OR (@Role LIKE 'custom_%' AND code = @Role)",
                new { Role = role.Trim(), Normalized = normalizedRole });

            List<string> permissions = new();
            if (roleRow != null && roleRow.permissions_json != null)
            {
                try
                {
                    permissions = JsonSerializer.Deserialize<List<string>>((string)roleRow.permissions_json) ?? new();
                }
                catch { }
            }

            var permSet = new HashSet<string>(permissions, StringComparer.OrdinalIgnoreCase);

            // 1. صلاحيات المسؤول والمدير العام الشاملة
            if (normalizedRole == "admin" || normalizedRole == "generalmanager" || normalizedRole == "gmanager" || role.Contains("مدير عام") || role.Contains("مسؤول النظام"))
            {
                var allStandardPerms = new[]
                {
                    "dept_quality", "dept_food_safety",
                    "enabled_rpt_raw_material", "fill_rpt_raw_material", "view_rpt_raw_material", "print_rpt_raw_material",
                    "enabled_rpt_final_product", "fill_rpt_final_product", "view_rpt_final_product", "print_rpt_final_product",
                    "enabled_rpt_raw_water", "fill_rpt_raw_water", "view_rpt_raw_water", "print_rpt_raw_water",
                    "enabled_rpt_hygiene", "fill_rpt_hygiene", "view_rpt_hygiene", "print_rpt_hygiene",
                    "create_water_reports", "create_raw_material_reports", "create_hygiene_reports",
                    "fill_dynamic_reports", "fill_quality_dynamic_reports", "fill_food_safety_dynamic_reports",
                    "view_reports", "edit_reports", "delete_reports", "design_dynamic_reports",
                    "fill_health_reports", "view_health_reports",
                    "approve_qsupervisor", "approve_qm", "approve_gm", "reject_reports",
                    "view_shifts", "manage_shifts", "manage_users", "manage_roles", "manage_settings", "manage_products",
                    "manage_daily_workers", "view_daily_workers_commitment", "view_factory_compliance_report",
                    "export_pdf", "manage_signature", "change_password"
                };
                foreach (var p in allStandardPerms) permSet.Add(p);
            }
            else
            {
                // 2. ترحيل وتوسيع الصلاحيات التلقائي لمنع اختفاء الأقسام
                if (permSet.Contains("create_water_reports") || permSet.Contains("fill_rpt_final_product") || permSet.Contains("view_rpt_final_product") || permSet.Contains("fill_quality_dynamic_reports"))
                {
                    permSet.Add("dept_quality");
                    permSet.Add("enabled_rpt_final_product");
                    permSet.Add("enabled_rpt_raw_water");
                }
                if (permSet.Contains("create_raw_material_reports") || permSet.Contains("fill_rpt_raw_material"))
                {
                    permSet.Add("dept_quality");
                    permSet.Add("enabled_rpt_raw_material");
                }
                if (permSet.Contains("create_hygiene_reports") || permSet.Contains("fill_health_reports") || permSet.Contains("view_health_reports") || permSet.Contains("fill_food_safety_dynamic_reports") || permSet.Contains("fill_rpt_hygiene"))
                {
                    permSet.Add("dept_food_safety");
                    permSet.Add("enabled_rpt_hygiene");
                    permSet.Add("view_rpt_hygiene");
                    permSet.Add("print_rpt_hygiene");
                }
                if (permSet.Contains("view_reports"))
                {
                    permSet.Add("dept_quality");
                    permSet.Add("view_rpt_final_product");
                    permSet.Add("view_rpt_raw_water");
                    permSet.Add("view_rpt_raw_material");
                    permSet.Add("enabled_rpt_final_product");
                    permSet.Add("enabled_rpt_raw_water");
                    permSet.Add("enabled_rpt_raw_material");
                }
                if (permSet.Contains("approve_qm"))
                {
                    permSet.Add("approve_qsupervisor");
                }
                if (permSet.Count > 0)
                {
                    permSet.Add("manage_signature");
                    permSet.Add("change_password");
                }
            }

            return permSet.ToList();
        }

        public async Task<Result<bool>> ChangePasswordAsync(int userId, ChangePasswordRequest request)
        {
            using var conn = _db.CreateConnection();
            var storedHash = await conn.QuerySingleOrDefaultAsync<string>(
                "SELECT password_hash FROM Users WHERE id = @Id",
                new { Id = userId });

            if (!PasswordHasher.VerifyPassword(request.CurrentPassword, storedHash ?? string.Empty))
            {
                return Result<bool>.Fail("كلمة المرور الحالية غير صحيحة.");
            }

            var newHash = PasswordHasher.HashPassword(request.NewPassword);
            await conn.ExecuteAsync(
                "UPDATE Users SET password_hash = @Hash WHERE id = @Id",
                new { Hash = newHash, Id = userId });

            return Result<bool>.Ok(true, "تم تغيير كلمة المرور بنجاح.");
        }

        public async Task<Result<UserSignatureDto?>> GetSignatureAsync(int userId)
        {
            using var conn = _db.CreateConnection();
            var sig = await conn.QueryFirstOrDefaultAsync<UserSignatureDto>(@"
                SELECT TOP 1 
                    user_id AS UserId, 
                    signature_type AS SignatureType, 
                    signature_data AS SignatureData, 
                    signature_hash AS SignatureHash, 
                    is_active AS IsActive 
                FROM user_signatures 
                WHERE user_id = @UserId AND is_active = 1 
                ORDER BY id DESC",
                new { UserId = userId });

            return Result<UserSignatureDto?>.Ok(sig);
        }

        public async Task<Result<bool>> SaveSignatureAsync(int userId, UserSignatureDto dto)
        {
            var targetUserId = (userId > 0) ? userId : dto.UserId;
            if (targetUserId <= 0)
                return Result<bool>.Fail("معرف المستخدم غير صالح.");

            if (dto == null || string.IsNullOrWhiteSpace(dto.SignatureData))
                return Result<bool>.Fail("بيانات التوقيع فارغة.");

            string cleanBase64 = dto.SignatureData.Contains(",")
                ? dto.SignatureData.Split(',').Last().Trim()
                : dto.SignatureData.Trim();

            if (cleanBase64.Length > 2_800_000)
                return Result<bool>.Fail("حجم صورة التوقيع كبير جداً؛ يرجى اختيار صورة بحجم أصغر.");

            byte[] decodedBytes;
            try
            {
                decodedBytes = Convert.FromBase64String(cleanBase64);
            }
            catch
            {
                return Result<bool>.Fail("صيغة بيانات التوقيع غير صالحة.");
            }

            string computedHash = Convert.ToHexString(SHA256.HashData(decodedBytes)).ToLowerInvariant();
            string effectiveType = string.IsNullOrWhiteSpace(dto.SignatureType) ? "drawn_png" : dto.SignatureType.Trim();

            using var conn = _db.CreateConnection();
            await conn.ExecuteAsync(@"
                UPDATE user_signatures SET is_active = 0, updated_at = @Now WHERE user_id = @UserId;
                IF EXISTS (SELECT 1 FROM user_signatures WHERE user_id = @UserId)
                BEGIN
                    UPDATE TOP (1) user_signatures 
                    SET signature_data = @SignatureData, 
                        signature_type = @SignatureType, 
                        signature_hash = @SignatureHash, 
                        is_active = 1, 
                        updated_at = @Now
                    WHERE id = (SELECT TOP 1 id FROM user_signatures WHERE user_id = @UserId ORDER BY id DESC)
                END
                ELSE
                BEGIN
                    INSERT INTO user_signatures (user_id, signature_type, signature_data, signature_hash, is_active, created_at, updated_at)
                    VALUES (@UserId, @SignatureType, @SignatureData, @SignatureHash, 1, @Now, @Now)
                END",
                new
                {
                    UserId = targetUserId,
                    SignatureType = effectiveType,
                    SignatureData = cleanBase64,
                    SignatureHash = computedHash,
                    Now = DateTime.UtcNow.ToString("o")
                });

            return Result<bool>.Ok(true, "تم حفظ التوقيع الرقمي بنجاح.");
        }

        public async Task<Result<bool>> DeleteSignatureAsync(int userId)
        {
            if (userId <= 0)
                return Result<bool>.Fail("معرف المستخدم غير صالح.");

            using var conn = _db.CreateConnection();
            await conn.ExecuteAsync(@"
                UPDATE user_signatures 
                SET is_active = 0, 
                    updated_at = @Now 
                WHERE user_id = @UserId",
                new
                {
                    UserId = userId,
                    Now = DateTime.UtcNow.ToString("o")
                });

            return Result<bool>.Ok(true, "تم حذف التوقيع بنجاح.");
        }

        public async Task<IEnumerable<dynamic>> GetAllUsersAsync(int? branchId)
        {
            using var conn = _db.CreateConnection();
            string sql = "SELECT * FROM Users";
            if (branchId.HasValue)
                sql += " WHERE branch_id = @BranchId OR branch_id IS NULL";
            sql += " ORDER BY id";

            var users = await conn.QueryAsync<dynamic>(sql, new { BranchId = branchId });
            return users;
        }

        public async Task<Result<int>> SaveUserAsync(JsonElement body)
        {
            using var conn = _db.CreateConnection();
            int id = body.TryGetProperty("id", out var idProp) && idProp.ValueKind == JsonValueKind.Number ? idProp.GetInt32() : 0;
            string fullName = body.TryGetProperty("fullName", out var fn) ? fn.GetString() ?? "" : "";
            string username = body.TryGetProperty("username", out var un) ? un.GetString() ?? "" : "";
            string role = body.TryGetProperty("role", out var r) ? r.GetString() ?? "User" : "User";
            int? branchId = body.TryGetProperty("branchId", out var b) && b.ValueKind == JsonValueKind.Number ? b.GetInt32() : null;
            string? email = body.TryGetProperty("email", out var em) ? em.GetString() : null;
            string? phone = body.TryGetProperty("phone", out var ph) ? ph.GetString() : null;
            string? jobTitle = body.TryGetProperty("jobTitle", out var jt) ? jt.GetString() : null;
            string? password = body.TryGetProperty("password", out var pw) ? pw.GetString() : null;
            bool isActive = !body.TryGetProperty("isActive", out var ia) || ia.GetBoolean();

            if (id > 0)
            {
                string sql = @"
                    UPDATE Users SET
                        full_name = @FullName,
                        username = @Username,
                        role = @Role,
                        branch_id = @BranchId,
                        email = @Email,
                        phone = @Phone,
                        job_title = @JobTitle,
                        is_active = @IsActive";

                if (!string.IsNullOrWhiteSpace(password))
                {
                    string hash = PasswordHasher.HashPassword(password);
                    sql += ", password_hash = @Hash";
                    await conn.ExecuteAsync(sql + " WHERE id = @Id", new { FullName = fullName, Username = username, Role = role, BranchId = branchId, Email = email, Phone = phone, JobTitle = jobTitle, IsActive = isActive ? 1 : 0, Hash = hash, Id = id });
                }
                else
                {
                    await conn.ExecuteAsync(sql + " WHERE id = @Id", new { FullName = fullName, Username = username, Role = role, BranchId = branchId, Email = email, Phone = phone, JobTitle = jobTitle, IsActive = isActive ? 1 : 0, Id = id });
                }

                return Result<int>.Ok(id, "تم تحديث بيانات المستخدم بنجاح.");
            }
            else
            {
                string hash = PasswordHasher.HashPassword(string.IsNullOrWhiteSpace(password) ? "123456" : password);
                int newId = await conn.QuerySingleAsync<int>(@"
                    INSERT INTO Users (full_name, username, password_hash, role, branch_id, email, phone, job_title, is_active, created_at)
                    VALUES (@FullName, @Username, @Hash, @Role, @BranchId, @Email, @Phone, @JobTitle, @IsActive, @Now);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);",
                    new { FullName = fullName, Username = username, Hash = hash, Role = role, BranchId = branchId, Email = email, Phone = phone, JobTitle = jobTitle, IsActive = isActive ? 1 : 0, Now = DateTime.UtcNow.ToString("o") });

                return Result<int>.Ok(newId, "تم إنشاء المستخدم بنجاح.");
            }
        }

        public async Task<Result<bool>> ToggleUserActiveAsync(int id, JsonElement body)
        {
            bool isActive = body.TryGetProperty("isActive", out var ia) && ia.GetBoolean();
            using var conn = _db.CreateConnection();
            await conn.ExecuteAsync("UPDATE Users SET is_active = @IsActive WHERE id = @Id", new { IsActive = isActive ? 1 : 0, Id = id });
            return Result<bool>.Ok(true, "تم تحديث حالة الحساب بنجاح.");
        }

        public async Task<IEnumerable<Role>> GetAllRolesAsync()
        {
            using var conn = _db.CreateConnection();
            return await conn.QueryAsync<Role>("SELECT id, code, title, description, color, icon_name AS IconName, permissions_json AS PermissionsJson, is_system AS IsSystem, created_at AS CreatedAt FROM CustomRoles ORDER BY id");
        }

        public async Task<Result<bool>> SaveRoleAsync(JsonElement body)
        {
            using var conn = _db.CreateConnection();
            int id = body.TryGetProperty("id", out var idProp) && idProp.ValueKind == JsonValueKind.Number ? idProp.GetInt32() : 0;
            string code = body.TryGetProperty("code", out var c) ? c.GetString() ?? "" : "";
            string title = body.TryGetProperty("title", out var t) ? t.GetString() ?? "" : "";
            string? description = body.TryGetProperty("description", out var d) ? d.GetString() : null;
            string? permissionsJson = body.TryGetProperty("permissionsJson", out var pj) ? pj.GetString() : null;

            if (id > 0)
            {
                await conn.ExecuteAsync(@"
                    UPDATE CustomRoles 
                    SET code = @Code, title = @Title, description = @Description, permissions_json = @PermissionsJson
                    WHERE id = @Id",
                    new { Code = code, Title = title, Description = description, PermissionsJson = permissionsJson, Id = id });
            }
            else
            {
                await conn.QuerySingleAsync<int>(@"
                    INSERT INTO CustomRoles (code, title, description, permissions_json, is_system, created_at)
                    VALUES (@Code, @Title, @Description, @PermissionsJson, 0, @Now);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);",
                    new { Code = code, Title = title, Description = description, PermissionsJson = permissionsJson, Now = DateTime.UtcNow.ToString("o") });
            }

            return Result<bool>.Ok(true, "تم حفظ الصلاحيات والدور بنجاح.");
        }

        public async Task<Result<bool>> DeleteRoleAsync(int id)
        {
            if (id <= 0)
                return Result<bool>.Fail("معرف الدور غير صالح.");

            using var conn = _db.CreateConnection();
            var role = await conn.QuerySingleOrDefaultAsync<dynamic>(
                "SELECT id, code, is_system FROM CustomRoles WHERE id = @Id", new { Id = id });

            if (role == null)
                return Result<bool>.Fail("لم يتم العثور على الدور المطلوب.");

            bool isSystem = role.is_system != null && Convert.ToInt32(role.is_system) == 1;
            if (isSystem)
                return Result<bool>.Fail("لا يمكن حذف الأدوار الأساسية للنظام.");

            await conn.ExecuteAsync("DELETE FROM CustomRoles WHERE id = @Id AND COALESCE(is_system, 0) = 0", new { Id = id });
            return Result<bool>.Ok(true, "تم حذف الدور بنجاح.");
        }
    }
}