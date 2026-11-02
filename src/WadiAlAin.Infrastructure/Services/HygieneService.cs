using Dapper;
using WadiAlAin.Application.Common.Interfaces;
using WadiAlAin.Application.Common.Models;
using WadiAlAin.Application.Interfaces;
using WadiAlAin.Application.Common.Helpers;
using WadiAlAin.Application.DTOs.Common;
using System.Text.Json;

namespace WadiAlAin.Infrastructure.Services
{
    public class HygieneService : IHygieneService
    {
        private readonly IDbConnectionFactory _db;

        public HygieneService(IDbConnectionFactory db)
        {
            _db = db;
        }

        public async Task<IEnumerable<dynamic>> GetReportsAsync(int? branchId)
        {
            using var conn = _db.CreateConnection();
            string sql = "SELECT * FROM HygieneReports";
            if (branchId.HasValue && branchId.Value > 0)
            {
                if (branchId.Value == 1)
                    sql += " WHERE (branch_id = 1 OR branch_id IS NULL OR branch_id = 0)";
                else
                    sql += " WHERE branch_id = @BranchId";
            }
            sql += " ORDER BY id DESC";
            return await conn.QueryAsync<dynamic>(sql, new { BranchId = branchId });
        }

        public async Task<Result<dynamic>> GetReportDetailsAsync(int id, int? effectiveBranchId = null)
        {
            using var conn = _db.CreateConnection();
            var report = await conn.QuerySingleOrDefaultAsync<dynamic>(
                "SELECT * FROM HygieneReports WHERE id = @Id", new { Id = id });

            if (report == null)
                return Result<dynamic>.Fail("لم يتم العثور على تقرير النظافة.");

            if (effectiveBranchId.HasValue)
            {
                var dict = (IDictionary<string, object>)report;
                int repBranch = dict.TryGetValue("branch_id", out var bVal) && bVal != null && int.TryParse(bVal.ToString(), out int b) && b > 0 ? b : 1;
                if (repBranch != effectiveBranchId.Value)
                {
                    return Result<dynamic>.Fail("غير مصرح لك بالاطلاع على سجلات تابعة لمصنع آخر.");
                }
            }

            var employees = await conn.QueryAsync<dynamic>(@"
                SELECT l.id AS log_id, l.employee_id, COALESCE(l.checklist_id, 1) AS checklist_id,
                       COALESCE(dw.full_name, e.full_name, 'عامل') AS full_name,
                       COALESCE(dw.job_title, e.job_title, 'عامل إنتاج') AS job_title
                FROM HygieneLogs l
                LEFT JOIN DailyWorkers dw ON l.employee_id = dw.id
                LEFT JOIN Employees e ON l.employee_id = e.id
                WHERE l.report_id = @ReportId
                ORDER BY l.id ASC", new { ReportId = id });

            var details = await conn.QueryAsync<dynamic>(@"
                SELECT d.id, d.hygiene_log_id, l.employee_id, d.checklist_item_id, d.is_compliant, d.notes
                FROM HygieneLogDetails d
                INNER JOIN HygieneLogs l ON d.hygiene_log_id = l.id
                WHERE l.report_id = @ReportId", new { ReportId = id });

            var criteria = await conn.QueryAsync<dynamic>(@"
                SELECT id, checklist_id, item_name 
                FROM ChecklistItems 
                WHERE checklist_id = 1
                ORDER BY id ASC");

            return Result<dynamic>.Ok(new { 
                Report = report, 
                Employees = employees, 
                Details = details, 
                Criteria = criteria, 
                Logs = employees 
            });
        }

        public async Task<Result<int>> SaveReportAsync(JsonElement body)
        {
            using var conn = _db.CreateConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();

            try
            {
                int reportId = body.GetInt("id", null, 0);
                string createdAt = body.GetString("createdAt", "created_at", null, DateTime.UtcNow.ToString("o"));
                string supervisorName = body.GetString("supervisorName", "supervisor_name", null, "أخصائي الجودة");
                int employeeCount = body.GetInt("employeeCount", "employee_count", 0);
                string approvalStatus = body.GetString("approvalStatus", "approval_status", null, "Pending_QM");
                int branchId = body.GetInt("branchId", "branch_id", 1);
                string branchName = body.GetString("branchName", "branch_name", null, "مصنع أرتل");
                int shiftId = body.GetInt("shiftId", "shift_id", 1);
                string shiftName = body.GetString("shiftName", "shift_name", null, "الوردية الأولى");

                bool existsInDb = reportId > 0 &&
                    (await conn.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM HygieneReports WHERE id = @reportId", new { reportId }, trans)) > 0;

                if (existsInDb)
                {
                    await conn.ExecuteAsync(@"
                        UPDATE HygieneReports SET 
                            supervisor_name = @SupervisorName, employee_count = @EmployeeCount, approval_status = @ApprovalStatus, 
                            branch_id = @BranchId, branch_name = @BranchName, shift_id = @ShiftId, shift_name = @ShiftName
                        WHERE id = @Id",
                        new { Id = reportId, SupervisorName = supervisorName, EmployeeCount = employeeCount, ApprovalStatus = approvalStatus, BranchId = branchId, BranchName = branchName, ShiftId = shiftId, ShiftName = shiftName }, trans);

                    var oldLogIds = await conn.QueryAsync<int>("SELECT id FROM HygieneLogs WHERE report_id = @Id", new { Id = reportId }, trans);
                    if (oldLogIds.Any())
                    {
                        await conn.ExecuteAsync("DELETE FROM HygieneLogDetails WHERE hygiene_log_id IN @Ids", new { Ids = oldLogIds }, trans);
                        await conn.ExecuteAsync("DELETE FROM HygieneLogs WHERE report_id = @ReportId", new { ReportId = reportId }, trans);
                    }
                }
                else
                {
                    reportId = await conn.QuerySingleAsync<int>(@"
                        INSERT INTO HygieneReports (
                            created_at, supervisor_name, employee_count, approval_status, branch_id, branch_name, shift_id, shift_name
                        ) VALUES (
                            @CreatedAt, @SupervisorName, @EmployeeCount, @ApprovalStatus, @BranchId, @BranchName, @ShiftId, @ShiftName
                        );
                        SELECT CAST(SCOPE_IDENTITY() AS INT);",
                        new { CreatedAt = createdAt, SupervisorName = supervisorName, EmployeeCount = employeeCount, ApprovalStatus = approvalStatus, BranchId = branchId, BranchName = branchName, ShiftId = shiftId, ShiftName = shiftName }, trans);
                }

                if (body.TryGetProperty("details", out var detailsProp) && detailsProp.ValueKind == JsonValueKind.Array)
                {
                    var groupedByEmp = new Dictionary<int, List<JsonElement>>();
                    foreach (var d in detailsProp.EnumerateArray())
                    {
                        int empId = d.GetInt("employeeId", "employee_id", 0);
                        if (!groupedByEmp.ContainsKey(empId))
                            groupedByEmp[empId] = new List<JsonElement>();
                        groupedByEmp[empId].Add(d);
                    }

                    foreach (var kvp in groupedByEmp)
                    {
                        int empId = kvp.Key;
                        int logId = await conn.QuerySingleAsync<int>(@"
                            INSERT INTO HygieneLogs (report_id, branch_id, shift_id, shift_name, checklist_id, employee_id, created_at, approval_status)
                            VALUES (@ReportId, @BranchId, @ShiftId, @ShiftName, 1, @EmployeeId, @CreatedAt, @ApprovalStatus);
                            SELECT CAST(SCOPE_IDENTITY() AS INT);",
                            new { ReportId = reportId, BranchId = branchId, ShiftId = shiftId, ShiftName = shiftName, EmployeeId = empId, CreatedAt = createdAt, ApprovalStatus = approvalStatus }, trans);

                        foreach (var item in kvp.Value)
                        {
                            int critId = item.GetInt("checklistItemId", "checklist_item_id", 0);
                            bool isCompliant = item.GetBool("isCompliant", "is_compliant", true);
                            string? notes = item.GetStringOrNull("notes");

                            await conn.ExecuteAsync(@"
                                INSERT INTO HygieneLogDetails (hygiene_log_id, checklist_item_id, is_compliant, notes)
                                VALUES (@LogId, @ChecklistItemId, @IsCompliant, @Notes)",
                                new { LogId = logId, ChecklistItemId = critId, IsCompliant = isCompliant ? 1 : 0, Notes = notes }, trans);
                        }
                    }
                }

                trans.Commit();
                return Result<int>.Ok(reportId, "تم حفظ تقرير النظافة بنجاح.");
            }
            catch (Exception ex)
            {
                trans.Rollback();
                return Result<int>.Fail($"حدث خطأ أثناء حفظ التقرير: {ex.Message}");
            }
        }

        public async Task<Result<bool>> UpdateReportStatusAsync(int id, UpdateStatusRequest dto)
        {
            using var conn = _db.CreateConnection();
            string status = dto.Status;
            string? approvedBy = dto.ApprovedBy;
            string? rejectionReason = dto.RejectionReason;
            string? gmDecision = dto.GmDecision;
            string? gmDecisionBy = dto.GmDecisionBy;
            string? gmDecisionNotes = dto.GmDecisionNotes;

            string sql = "UPDATE HygieneReports SET approval_status = @Status";

            if (status == "Approved_QM" || status == "Pending_GM" || status == "Approved_Stage1_QM"
                || status == "Approved_Micro_QM" || status == "Pending_Micro_QM")
            {
                sql += ", qm_approved_by = @ApprovedBy, qm_approved_at = @Now";
            }
            else if (status == "Approved_GM" || status == "Approved" || status == "Approved_Stage1_GM"
                || status == "Approved_Micro_GM" || status == "Pending_Micro_GM")
            {
                sql += ", gm_approved_by = @ApprovedBy, gm_approved_at = @Now";
            }
            else if (status == "Closed")
            {
                sql += ", gm_approved_by = COALESCE(@ApprovedBy, gm_approved_by), gm_approved_at = @Now, gm_decision = COALESCE(@GmDecision, 'Approved'), gm_decision_by = COALESCE(@GmDecisionBy, @ApprovedBy), gm_decision_at = @Now";
                if (!string.IsNullOrEmpty(gmDecisionNotes) || !string.IsNullOrEmpty(rejectionReason))
                {
                    sql += ", gm_decision_notes = COALESCE(@GmDecisionNotes, @RejectionReason)";
                }
            }
            else if (status == "Rejected_By_GM")
            {
                sql += ", gm_decision = COALESCE(@GmDecision, 'Rejected'), gm_decision_by = COALESCE(@GmDecisionBy, @ApprovedBy), gm_decision_at = @Now, gm_decision_notes = COALESCE(@GmDecisionNotes, @RejectionReason)";
            }
            else if (status == "Pending_GM_Decision")
            {
                // اعتماد الإجراء التصحيحي من مدير الجودة وتحويله لقرار مدير المصنع
                if (!string.IsNullOrEmpty(approvedBy))
                    sql += ", qm_approved_by = COALESCE(@ApprovedBy, qm_approved_by), qm_approved_at = @Now";
            }
            else if (status == "Pending_QM_Decision")
            {
                // رفع الإجراء التصحيحي من أخصائي الجودة لمراجعة مدير الجودة — لا نغير rejected_by الأصلي
            }
            else if (status == "Rejected" || status == "Corrective_In_Progress" || status == "Returned")
            {
                if (!string.IsNullOrEmpty(rejectionReason)) sql += ", rejection_reason = @RejectionReason";
                if (!string.IsNullOrEmpty(approvedBy) && (status == "Rejected" || status == "Returned" || !string.IsNullOrEmpty(rejectionReason)))
                {
                    sql += ", rejected_by = COALESCE(rejected_by, @ApprovedBy), rejected_at = COALESCE(rejected_at, @Now)";
                }
            }

            // تحديث تفاصيل الإجراء التصحيحي والرفض عند توفرها
            if (!string.IsNullOrEmpty(dto.RejectionCategory)) sql += ", rejection_category = @RejectionCategory";
            if (!string.IsNullOrEmpty(dto.SeverityLevel)) sql += ", severity_level = @SeverityLevel";
            if (!string.IsNullOrEmpty(dto.RejectionNotes)) sql += ", rejection_notes = @RejectionNotes";
            if (!string.IsNullOrEmpty(dto.AffectedProducts)) sql += ", affected_products = @AffectedProducts";
            if (!string.IsNullOrEmpty(dto.ImmediateAction)) sql += ", immediate_action = @ImmediateAction";
            if (!string.IsNullOrEmpty(dto.CorrectiveAction)) sql += ", corrective_action = @CorrectiveAction";
            if (!string.IsNullOrEmpty(dto.CorrectiveOwner)) sql += ", corrective_owner = @CorrectiveOwner";
            if (!string.IsNullOrEmpty(dto.CorrectiveDueDate)) sql += ", corrective_due_date = @CorrectiveDueDate";

            if (!string.IsNullOrEmpty(gmDecision) && status != "Closed" && status != "Rejected_By_GM")
            {
                sql += ", gm_decision = @GmDecision, gm_decision_by = @GmDecisionBy, gm_decision_at = @Now, gm_decision_notes = @GmDecisionNotes";
            }

            sql += " WHERE id = @Id";

            await conn.ExecuteAsync(sql, new
            {
                Status = status,
                ApprovedBy = approvedBy,
                RejectionReason = rejectionReason,
                RejectionCategory = dto.RejectionCategory,
                SeverityLevel = dto.SeverityLevel,
                RejectionNotes = dto.RejectionNotes,
                AffectedProducts = dto.AffectedProducts,
                ImmediateAction = dto.ImmediateAction,
                CorrectiveAction = dto.CorrectiveAction,
                CorrectiveOwner = dto.CorrectiveOwner,
                CorrectiveDueDate = dto.CorrectiveDueDate,
                GmDecision = gmDecision,
                GmDecisionBy = gmDecisionBy,
                GmDecisionNotes = gmDecisionNotes,
                Now = DateTime.UtcNow.ToString("o"),
                Id = id
            });

            await conn.ExecuteAsync(
                "UPDATE HygieneLogs SET approval_status = @Status WHERE report_id = @Id",
                new { Status = status, Id = id });

            return Result<bool>.Ok(true, "تم تحديث حالة تقرير النظافة وسجل الاعتماد والإجراء التصحيحي بنجاح.");
        }

        public async Task<Result<bool>> DeleteReportAsync(int id)
        {
            using var conn = _db.CreateConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();

            try
            {
                await conn.ExecuteAsync(@"
                    DELETE FROM HygieneLogDetails
                    WHERE hygiene_log_id IN (SELECT id FROM HygieneLogs WHERE report_id = @Id)", new { Id = id }, trans);

                await conn.ExecuteAsync("DELETE FROM HygieneLogs WHERE report_id = @Id", new { Id = id }, trans);
                await conn.ExecuteAsync("DELETE FROM HygieneReports WHERE id = @Id", new { Id = id }, trans);

                trans.Commit();
                return Result<bool>.Ok(true, "تم حذف التقرير بنجاح.");
            }
            catch (Exception ex)
            {
                trans.Rollback();
                return Result<bool>.Fail($"حدث خطأ أثناء الحذف: {ex.Message}");
            }
        }

        public async Task<IEnumerable<dynamic>> GetChecklistsAsync()
        {
            using var conn = _db.CreateConnection();
            var checklists = await conn.QueryAsync<dynamic>("SELECT * FROM Checklists ORDER BY id");
            var items = await conn.QueryAsync<dynamic>("SELECT * FROM ChecklistItems ORDER BY id");

            var result = checklists.Select(c => new
            {
                Id = c.id,
                Title = c.title,
                Type = c.type,
                Items = items.Where(i => (int)i.checklist_id == (int)c.id).ToList()
            });

            return result;
        }

        public async Task<IEnumerable<dynamic>> GetHygieneLogsAsync(int? reportId, int? branchId)
        {
            using var conn = _db.CreateConnection();
            string sql = "SELECT * FROM HygieneLogs WHERE 1=1";
            if (reportId.HasValue) sql += " AND report_id = @ReportId";
            if (branchId.HasValue) sql += " AND branch_id = @BranchId";
            sql += " ORDER BY id DESC";
            return await conn.QueryAsync<dynamic>(sql, new { ReportId = reportId, BranchId = branchId });
        }
    }
}
