using Dapper;
using WadiAlAin.Application.Common.Interfaces;
using WadiAlAin.Application.Common.Models;
using WadiAlAin.Application.Interfaces;
using WadiAlAin.Application.Common.Helpers;
using System.Text.Json;

namespace WadiAlAin.Infrastructure.Services
{
    public class DynamicReportService : IDynamicReportService
    {
        private readonly IDbConnectionFactory _db;

        public DynamicReportService(IDbConnectionFactory db)
        {
            _db = db;
        }

        public async Task<IEnumerable<dynamic>> GetCategoriesAsync()
        {
            using var conn = _db.CreateConnection();
            return await conn.QueryAsync<dynamic>("SELECT * FROM dr_categories WHERE is_active = 1 ORDER BY sort_order, id");
        }

        public async Task<IEnumerable<dynamic>> GetTemplatesAsync(int? categoryId)
        {
            using var conn = _db.CreateConnection();
            string sql = "SELECT * FROM dr_reports WHERE is_active = 1";
            if (categoryId.HasValue) sql += " AND category_id = @CategoryId";
            sql += " ORDER BY sort_order, id";
            return await conn.QueryAsync<dynamic>(sql, new { CategoryId = categoryId });
        }

        public async Task<Result<dynamic>> GetTemplateDetailsAsync(int id)
        {
            using var conn = _db.CreateConnection();
            var report = await conn.QuerySingleOrDefaultAsync<dynamic>(
                "SELECT * FROM dr_reports WHERE id = @Id", new { Id = id });

            if (report == null)
                return Result<dynamic>.Fail("لم يتم العثور على القالب.");

            var sections = await conn.QueryAsync<dynamic>(
                "SELECT * FROM dr_sections WHERE report_id = @ReportId ORDER BY sort_order, id", new { ReportId = id });

            var fields = await conn.QueryAsync<dynamic>(
                "SELECT * FROM dr_fields WHERE report_id = @ReportId ORDER BY sort_order, id", new { ReportId = id });

            var versions = await conn.QueryAsync<dynamic>(
                "SELECT * FROM dr_report_versions WHERE report_id = @ReportId ORDER BY version_number DESC", new { ReportId = id });

            return Result<dynamic>.Ok(new
            {
                Template = report,
                Report = report,
                Sections = sections,
                Fields = fields,
                Versions = versions
            });
        }

        public async Task<IEnumerable<dynamic>> GetRecordsAsync(int? reportId, int? branchId)
        {
            using var conn = _db.CreateConnection();
            string sql = @"
                SELECT r.*, COALESCE(u.full_name, r.created_by) AS created_by
                FROM dr_records r
                LEFT JOIN Users u ON r.created_by = u.username
                WHERE 1=1";
            if (reportId.HasValue) sql += " AND r.report_id = @ReportId";
            if (branchId.HasValue)
            {
                if (branchId.Value == 1)
                    sql += " AND (r.branch_id = 1 OR r.branch_id IS NULL OR r.branch_id = 0)";
                else
                    sql += " AND r.branch_id = @BranchId";
            }
            sql += " ORDER BY r.id DESC";
            return await conn.QueryAsync<dynamic>(sql, new { ReportId = reportId, BranchId = branchId });
        }

        public async Task<object> GetDynamicReportStatsAsync(string? timeframe, int? branchId)
        {
            using var conn = _db.CreateConnection();
            int totalReports = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM dr_reports WHERE is_active = 1");

            string whereSql = "WHERE 1=1";
            if (branchId.HasValue && branchId.Value > 0)
            {
                if (branchId.Value == 1)
                    whereSql += " AND (branch_id = 1 OR branch_id IS NULL OR branch_id = 0)";
                else
                    whereSql += " AND branch_id = @BranchId";
            }

            DateTime? minDate = null;
            string tf = (timeframe ?? "all").Trim().ToLowerInvariant();
            if (tf == "today" || tf == "day") minDate = DateTime.UtcNow.Date;
            else if (tf == "week") minDate = DateTime.UtcNow.Date.AddDays(-7);
            else if (tf == "month") minDate = DateTime.UtcNow.Date.AddDays(-30);

            if (minDate.HasValue)
            {
                whereSql += " AND created_at >= @MinDate";
            }

            string statsSql = $@"
                SELECT
                    COUNT(*) AS TotalRecords,
                    SUM(CASE WHEN approval_status IN ('Approved', 'Closed', 'Approved_GM', 'Approved_QM') THEN 1 ELSE 0 END) AS ApprovedRecords,
                    SUM(CASE WHEN approval_status = 'Pending_QS' THEN 1 ELSE 0 END) AS PendingQS,
                    SUM(CASE WHEN approval_status = 'Pending_QM' THEN 1 ELSE 0 END) AS PendingQM,
                    SUM(CASE WHEN approval_status = 'Pending_GM' THEN 1 ELSE 0 END) AS PendingGM,
                    SUM(CASE WHEN approval_status IN ('Rejected', 'Rejected_By_GM', 'Rejected_Final') THEN 1 ELSE 0 END) AS RejectedRecords,
                    SUM(CASE WHEN approval_status = 'Corrective_In_Progress' THEN 1 ELSE 0 END) AS CorrectiveInProgress,
                    SUM(CASE WHEN approval_status = 'Pending_QM_Decision' THEN 1 ELSE 0 END) AS PendingQMDecision,
                    SUM(CASE WHEN approval_status = 'Pending_GM_Decision' THEN 1 ELSE 0 END) AS PendingGMDecision,
                    SUM(CASE WHEN COALESCE(is_compliant, 1) = 1 AND approval_status NOT IN ('Rejected', 'Rejected_By_GM', 'Rejected_Final', 'Corrective_In_Progress') THEN 1 ELSE 0 END) AS CompliantRecords
                FROM dr_records
                {whereSql}";

            var row = await conn.QuerySingleOrDefaultAsync<dynamic>(statsSql, new
            {
                BranchId = branchId,
                MinDate = minDate?.ToString("yyyy-MM-dd")
            });

            return new
            {
                totalReports,
                totalRecords = (int?)(row?.TotalRecords) ?? 0,
                approvedRecords = (int?)(row?.ApprovedRecords) ?? 0,
                pendingQS = (int?)(row?.PendingQS) ?? 0,
                pendingQM = (int?)(row?.PendingQM) ?? 0,
                pendingGM = (int?)(row?.PendingGM) ?? 0,
                rejectedRecords = (int?)(row?.RejectedRecords) ?? 0,
                correctiveInProgress = (int?)(row?.CorrectiveInProgress) ?? 0,
                pendingQMDecision = (int?)(row?.PendingQMDecision) ?? 0,
                pendingGMDecision = (int?)(row?.PendingGMDecision) ?? 0,
                compliantRecords = (int?)(row?.CompliantRecords) ?? 0
            };
        }

        public async Task<Result<dynamic>> GetRecordDetailsAsync(int id, int? effectiveBranchId = null)
        {
            using var conn = _db.CreateConnection();
            var record = await conn.QuerySingleOrDefaultAsync<dynamic>(@"
                SELECT r.*, COALESCE(u.full_name, r.created_by) AS created_by
                FROM dr_records r
                LEFT JOIN Users u ON r.created_by = u.username
                WHERE r.id = @Id", new { Id = id });

            if (record == null)
                return Result<dynamic>.Fail("لم يتم العثور على السجل.");

            if (effectiveBranchId.HasValue)
            {
                var dict = (IDictionary<string, object>)record;
                int recBranch = dict.TryGetValue("branch_id", out var bVal) && bVal != null && int.TryParse(bVal.ToString(), out int b) && b > 0 ? b : 1;
                if (recBranch != effectiveBranchId.Value)
                {
                    return Result<dynamic>.Fail("غير مصرح لك بالاطلاع على سجلات تابعة لمصنع آخر.");
                }
            }

            var values = await conn.QueryAsync<dynamic>(@"
                SELECT v.*, f.field_key, f.label, f.field_type, f.unit
                FROM dr_values v
                INNER JOIN dr_fields f ON v.field_id = f.id
                WHERE v.record_id = @RecordId", new { RecordId = id });

            var approvals = await conn.QueryAsync<dynamic>(
                "SELECT * FROM report_approvals WHERE record_id = @RecordId ORDER BY id", new { RecordId = id });

            return Result<dynamic>.Ok(new { Record = record, Values = values, Approvals = approvals });
        }

        public async Task<Result<int>> SaveRecordAsync(int userId, string username, string role, JsonElement body)
        {
            using var conn = _db.CreateConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();

            try
            {
                int recordId = body.GetInt("id", null, 0);
                int reportId = body.GetInt("report_id", "reportId", 0);
                int? branchId = body.GetIntOrNull("branch_id", "branchId");
                string? branchName = body.GetStringOrNull("branch_name", "branchName");
                int? shiftId = body.GetIntOrNull("shift_id", "shiftId");
                string? shiftName = body.GetStringOrNull("shift_name", "shiftName");
                int? reportVersionId = body.GetIntOrNull("report_version_id", "reportVersionId");
                string? dataJson = body.GetStringOrNull("data_json", "dataJson");
                string? title = body.GetStringOrNull("title");
                string? notes = body.GetStringOrNull("notes");
                string? clientUuid = body.GetStringOrNull("client_uuid", "clientUuid");
                int isCompliant = (body.GetBoolOrNull("is_compliant", "isCompliant") ?? (body.GetIntOrNull("is_compliant", "isCompliant") != 0)) ? 1 : 0;
                string createdAt = body.GetString("created_at", "createdAt", null, DateTime.UtcNow.ToString("o"));
                string status = body.GetString("status", null, null, "Draft");
                string approvalStatus = body.GetString("approval_status", "approvalStatus", null, status);

                var creatorUser = await conn.QuerySingleOrDefaultAsync<dynamic>(
                    "SELECT full_name FROM Users WHERE id = @UserId", new { UserId = userId }, trans);
                string creatorName = (string?)creatorUser?.full_name 
                    ?? body.GetStringOrNull("created_by", "createdBy") 
                    ?? username;

                var report = await conn.QuerySingleOrDefaultAsync<dynamic>(
                    "SELECT id, current_version_id FROM dr_reports WHERE id = @ReportId AND is_active = 1",
                    new { ReportId = reportId }, trans);

                if (report == null)
                    return Result<int>.Fail("لم يتم العثور على النموذج الديناميكي أو غير نشط.");

                reportVersionId ??= (int?)report.current_version_id;

                bool existsInDb = recordId > 0 &&
                    (await conn.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM dr_records WHERE id = @recordId", new { recordId }, trans)) > 0;

                // منع التكرار (Idempotency): إذا تم إرسال client_uuid وكان موجوداً مسبقاً، نقوم بتحديث نفس السجل بدلاً من إنشاء مكرر
                if (!existsInDb && !string.IsNullOrWhiteSpace(clientUuid))
                {
                    var existingIdByUuid = await conn.QuerySingleOrDefaultAsync<int?>(
                        "SELECT id FROM dr_records WHERE client_uuid = @clientUuid",
                        new { clientUuid }, trans);
                    if (existingIdByUuid.HasValue && existingIdByUuid.Value > 0)
                    {
                        recordId = existingIdByUuid.Value;
                        existsInDb = true;
                    }
                }

                if (existsInDb)
                {
                    await conn.ExecuteAsync(@"
                        UPDATE dr_records SET
                            report_id = @reportId,
                            branch_id = COALESCE(@branchId, branch_id),
                            branch_name = COALESCE(@branchName, branch_name),
                            shift_id = COALESCE(@shiftId, shift_id),
                            shift_name = COALESCE(@shiftName, shift_name),
                            report_version_id = COALESCE(@reportVersionId, report_version_id),
                            data_json = COALESCE(@dataJson, data_json),
                            title = COALESCE(@title, title),
                            notes = COALESCE(@notes, notes),
                            client_uuid = COALESCE(client_uuid, @clientUuid),
                            is_compliant = @isCompliant,
                            status = @status,
                            approval_status = @approvalStatus,
                            created_by = CASE WHEN created_by = @username OR created_by IS NULL THEN @creatorName ELSE created_by END,
                            updated_at = @now
                        WHERE id = @recordId",
                        new { recordId, reportId, branchId, branchName, shiftId, shiftName, reportVersionId, dataJson, title, notes, clientUuid, isCompliant, status, approvalStatus, username, creatorName, now = DateTime.UtcNow.ToString("o") }, trans);

                    await conn.ExecuteAsync("DELETE FROM dr_values WHERE record_id = @recordId", new { recordId }, trans);
                }
                else
                {
                    recordId = await conn.QuerySingleAsync<int>(@"
                        INSERT INTO dr_records (
                            report_id, branch_id, branch_name, shift_id, shift_name,
                            report_version_id, data_json, title, notes, client_uuid, is_compliant,
                            created_by, created_at, status, approval_status)
                        VALUES (
                            @reportId, @branchId, @branchName, @shiftId, @shiftName,
                            @reportVersionId, @dataJson, @title, @notes, @clientUuid, @isCompliant,
                            @createdBy, @createdAt, @status, @approvalStatus);
                        SELECT CAST(SCOPE_IDENTITY() AS INT);",
                        new { reportId, branchId, branchName, shiftId, shiftName, reportVersionId, dataJson, title, notes, clientUuid, isCompliant, createdBy = creatorName, createdAt, status, approvalStatus }, trans);
                }

                if (body.TryGetProperty("values", out var valArr) && valArr.ValueKind == JsonValueKind.Array)
                {
                    var validFieldIds = (await conn.QueryAsync<int>(
                        "SELECT id FROM dr_fields WHERE report_id = @ReportId", new { ReportId = reportId }, trans)).ToHashSet();

                    foreach (var val in valArr.EnumerateArray())
                    {
                        int fieldId = val.GetInt("field_id", "fieldId", 0);
                        if (fieldId <= 0 || !validFieldIds.Contains(fieldId))
                            continue;

                        string? strVal = val.GetStringOrNull("string_value", "stringValue") ?? val.GetStringOrNull("value");
                        decimal? numVal = val.GetDecimalOrNull("numeric_value", "numericValue");
                        bool? boolVal = val.GetBoolOrNull("bool_value", "boolValue");
                        string? dateVal = val.GetStringOrNull("date_value", "dateValue");
                        string? jsonVal = val.GetStringOrNull("json_value", "jsonValue");

                        await conn.ExecuteAsync(@"
                            INSERT INTO dr_values (record_id, field_id, value, string_value, numeric_value, bool_value, date_value, json_value)
                            VALUES (@recordId, @fieldId, @strVal, @strVal, @numVal, @boolVal, @dateVal, @jsonVal)",
                            new { recordId, fieldId, strVal, numVal, boolVal, dateVal, jsonVal }, trans);
                    }
                }

                trans.Commit();
                return Result<int>.Ok(recordId, "تم حفظ السجل بنجاح.");
            }
            catch (Exception ex)
            {
                trans.Rollback();
                return Result<int>.Fail($"حدث خطأ أثناء حفظ السجل: {ex.Message}");
            }
        }

        public async Task<IEnumerable<dynamic>> GetWorkflowsAsync()
        {
            using var conn = _db.CreateConnection();
            var workflows = await conn.QueryAsync<dynamic>("SELECT * FROM report_workflows WHERE is_active = 1 ORDER BY id");
            var steps = await conn.QueryAsync<dynamic>("SELECT * FROM report_workflow_steps ORDER BY workflow_id, step_order");

            return workflows.Select(w => new
            {
                Id = w.id,
                Name = w.name,
                Code = w.code,
                Description = w.description,
                Steps = steps.Where(s => (int)s.workflow_id == (int)w.id).ToList()
            });
        }

        public async Task<IEnumerable<dynamic>> GetRecordApprovalsAsync(int recordId)
        {
            using var conn = _db.CreateConnection();
            return await conn.QueryAsync<dynamic>("SELECT * FROM report_approvals WHERE record_id = @RecordId ORDER BY id", new { RecordId = recordId });
        }

        public async Task<IEnumerable<dynamic>> GetPendingRecordsForRoleAsync(string roleStatus, int? branchId)
        {
            using var conn = _db.CreateConnection();
            string sql = @"
                SELECT r.id AS record_id, r.report_id, rep.name AS report_name, rep.category_id,
                       r.branch_id, r.created_at, COALESCE(u.full_name, r.created_by) AS created_by,
                       r.approval_status
                FROM dr_records r
                INNER JOIN dr_reports rep ON r.report_id = rep.id
                LEFT JOIN Users u ON r.created_by = u.username
                WHERE r.approval_status = @RoleStatus";

            if (branchId.HasValue && branchId.Value > 0)
            {
                if (branchId.Value == 1)
                    sql += " AND (r.branch_id = 1 OR r.branch_id IS NULL)";
                else
                    sql += " AND r.branch_id = @BranchId";
            }
            sql += " ORDER BY r.id DESC";

            return await conn.QueryAsync<dynamic>(sql, new { RoleStatus = roleStatus, BranchId = branchId });
        }

        public async Task<IEnumerable<dynamic>> GetCorrectiveActionRecordsAsync(int? branchId)
        {
            using var conn = _db.CreateConnection();
            string sql = @"
                SELECT r.id AS record_id, r.report_id, rep.name AS report_name, rep.category_id,
                       r.branch_id, r.created_at, COALESCE(u.full_name, r.created_by) AS created_by,
                       r.approval_status, r.rejection_reason, r.rejected_by, r.rejected_at,
                       r.rejection_category, r.severity_level, r.rejection_notes,
                       r.affected_products, r.immediate_action,
                       r.corrective_action, r.corrective_owner, r.corrective_due_date,
                       r.gm_decision, r.gm_decision_by, r.gm_decision_at, r.gm_decision_notes
                FROM dr_records r
                INNER JOIN dr_reports rep ON r.report_id = rep.id
                LEFT JOIN Users u ON r.created_by = u.username
                WHERE r.approval_status IN ('Rejected', 'Corrective_In_Progress', 'Pending_QM_Decision', 'Pending_GM_Decision', 'Rejected_By_GM', 'Rejected_Final')";

            if (branchId.HasValue && branchId.Value > 0)
            {
                if (branchId.Value == 1)
                    sql += " AND (r.branch_id = 1 OR r.branch_id IS NULL)";
                else
                    sql += " AND r.branch_id = @BranchId";
            }
            sql += " ORDER BY r.id DESC";

            return await conn.QueryAsync<dynamic>(sql, new { BranchId = branchId });
        }

        public async Task<Result<bool>> AddApprovalDecisionAsync(int id, int userId, string username, string role, JsonElement body)
        {
            using var conn = _db.CreateConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();

            try
            {
                var record = await conn.QuerySingleOrDefaultAsync<dynamic>(@"
                    SELECT r.id, r.report_id, r.status, r.approval_status, r.report_version_id,
                           rep.workflow_id, rep.approval_type
                    FROM dr_records r
                    INNER JOIN dr_reports rep ON r.report_id = rep.id
                    WHERE r.id = @Id", new { Id = id }, trans);

                if (record == null)
                    return Result<bool>.Fail("لم يتم العثور على السجل.");

                string rawAction = body.TryGetProperty("action", out var ac) ? (ac.GetString() ?? "Approved") : "Approved";
                string action = rawAction.ToLowerInvariant() switch
                {
                    "reject" or "rejected" => "Rejected",
                    "return" or "returned" => "Returned",
                    _ => "Approved"
                };
                string? comments = body.TryGetProperty("comments", out var cm) ? cm.GetString() : null;
                string? signatureHash = body.TryGetProperty("signature_hash", out var sh) ? sh.GetString() : null;
                string? signatureType = body.TryGetProperty("signature_type", out var st) ? st.GetString() : null;

                var workflowId = (int?)record.workflow_id;
                var currentStatus = (string?)record.status ?? "Draft";

                string stepName = action.ToLowerInvariant() switch
                {
                    "approve" or "approved" => "اعتماد",
                    "reject" or "rejected" => "رفض",
                    "return" or "returned" => "إرجاع",
                    _ => "اعتماد"
                };

                string approvalType = ((string?)record.approval_type)?.Trim().ToLowerInvariant() ?? "two_level";
                bool isSingleLevel = approvalType == "single_level" || approvalType == "three_level_qm" || approvalType == "none";

                string requestedAppStatus = body.TryGetProperty("approval_status", out var reqSt) ? (reqSt.GetString() ?? "") : "";
                string currentAppStatus = (string?)record.approval_status ?? currentStatus;

                // Determine the current step order based on the approval status
                // so that role validation targets the correct step in multi-level workflows.
                int currentStepOrder = currentAppStatus switch
                {
                    "Pending_QM"          => 2,
                    "Pending_QM_Decision" => 2,
                    "Pending_GM"          => 3,
                    "Pending_GM_Decision" => 4,
                    _                     => 1  // Draft, Pending_QS, Corrective_In_Progress, Rejected, or any unknown state
                };

                bool isSpecialistSubmittingCorrective =
                    (currentAppStatus == "Rejected" || currentAppStatus == "Corrective_In_Progress" || currentAppStatus == "Rejected_By_GM" || currentAppStatus == "Rejected_Final")
                    && (requestedAppStatus == "Pending_QM_Decision" || requestedAppStatus == "Pending_GM_Decision" || requestedAppStatus == "Corrective_In_Progress")
                    && action != "Rejected";

                if (workflowId.HasValue && !isSpecialistSubmittingCorrective)
                {
                    // Fetch the step matching the CURRENT approval stage, not always step 1.
                    var step = await conn.QuerySingleOrDefaultAsync<dynamic>(@"
                        SELECT TOP 1 s.id, s.step_name, s.required_role, s.step_order
                        FROM report_workflow_steps s
                        WHERE s.workflow_id = @WorkflowId
                          AND s.step_order = @StepOrder
                        ORDER BY s.step_order",
                        new { WorkflowId = workflowId.Value, StepOrder = currentStepOrder }, trans);

                    // Fallback: if no exact step found for this order, skip role validation
                    // (avoids blocking Admin/GeneralManager on non-standard workflows).
                    if (step != null && !string.IsNullOrEmpty((string?)step.required_role))
                    {
                        var requiredRole = (string)step.required_role;
                        bool roleMatches =
                            string.Equals(role, requiredRole, StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(role.Replace("_", ""), requiredRole.Replace("_", ""), StringComparison.OrdinalIgnoreCase) ||
                            role == "Admin" || role == "GeneralManager";

                        if (!roleMatches)
                            return Result<bool>.Fail($"دور المستخدم ({role}) لا يملك صلاحية الاعتماد في هذه الخطوة ({requiredRole}).");
                    }
                }
                string previousStatus = currentAppStatus;
                string newStatus;

                if (action == "Rejected" || requestedAppStatus == "Rejected_By_GM" || requestedAppStatus == "Rejected_Final" || requestedAppStatus == "Rejected")
                {
                    if (requestedAppStatus == "Corrective_In_Progress")
                    {
                        newStatus = "Corrective_In_Progress";
                    }
                    else if (requestedAppStatus == "Rejected_Final")
                    {
                        newStatus = "Rejected_Final";
                    }
                    else if (currentAppStatus == "Pending_GM_Decision" || requestedAppStatus == "Rejected_By_GM")
                    {
                        newStatus = "Rejected_By_GM";
                    }
                    else if (isSingleLevel && currentAppStatus == "Pending_QM_Decision")
                    {
                        newStatus = "Rejected_Final";
                    }
                    else
                    {
                        newStatus = "Rejected";
                    }
                }
                else if (action == "Returned")
                {
                    newStatus = "Returned";
                }
                else
                {
                    if (!string.IsNullOrEmpty(requestedAppStatus) && requestedAppStatus != "Rejected")
                    {
                        if (isSingleLevel && currentAppStatus == "Pending_QM" && requestedAppStatus == "Pending_GM")
                        {
                            newStatus = "Approved";
                        }
                        else if (isSingleLevel && currentAppStatus == "Pending_QM_Decision" && requestedAppStatus == "Pending_GM_Decision")
                        {
                            newStatus = "Closed";
                        }
                        else
                        {
                            newStatus = requestedAppStatus;
                        }
                    }
                    else if (isSingleLevel)
                    {
                        if (currentAppStatus == "Pending_QS")
                            newStatus = "Pending_QM";
                        else if (currentAppStatus == "Pending_QM_Decision")
                            newStatus = "Closed";
                        else
                            newStatus = "Approved";
                    }
                    else
                    {
                        newStatus = (role == "FactoryManager" || role == "GM" || role == "GeneralManager")
                            ? (currentAppStatus == "Pending_GM_Decision" ? "Closed" : "Approved")
                            : (role == "QualitySupervisor" || role == "QS" || currentAppStatus == "Pending_QS")
                                ? "Pending_QM"
                                : (currentAppStatus == "Pending_QM_Decision" ? "Pending_GM_Decision" : "Pending_GM");
                    }
                }

                var actor = await conn.QuerySingleOrDefaultAsync<dynamic>(
                    "SELECT full_name, job_title FROM Users WHERE id = @UserId", new { UserId = userId }, trans);

                string actorName = actor?.full_name ?? username;
                string actorPosition = actor?.job_title ?? role;

                var stepId = workflowId.HasValue
                    ? await conn.QuerySingleOrDefaultAsync<int?>(
                        "SELECT TOP 1 s.id FROM report_workflow_steps s WHERE s.workflow_id = @W AND s.step_order = @StepOrder ORDER BY s.step_order",
                        new { W = workflowId.Value, StepOrder = currentStepOrder }, trans)
                    : null;

                await conn.ExecuteAsync(@"
                    INSERT INTO report_approvals (
                        record_id, step_id, step_name, action,
                        acted_by, acted_by_user_id, action_by_position,
                        actor_id, actor_name, role,
                        acted_at, comments, previous_status, new_status,
                        signature_hash, signature_type)
                    VALUES (
                        @recordId, @stepId, @stepName, @action,
                        @actorName, @userId, @actorPosition,
                        @userId, @actorName, @role,
                        @actedAt, @comments, @previousStatus, @newStatus,
                        @signatureHash, @signatureType)",
                    new { recordId = id, stepId, stepName, action, actorName, userId, actorPosition, role, actedAt = DateTime.UtcNow.ToString("o"), comments, previousStatus, newStatus, signatureHash, signatureType }, trans);

                string nowIso = DateTime.UtcNow.ToString("o");

                // Always resolve names from the Users table (actorName/actorPosition)
                // to guarantee the real person's name is stored, not a role label.
                bool isQsApprove = currentAppStatus == "Pending_QS" && action == "Approved" && newStatus != "Rejected" && newStatus != "Returned";
                string? qsApprovedBy = isQsApprove ? $"{actorName} ({actorPosition})" : null;
                string? qsApprovedAt = isQsApprove ? nowIso : null;

                bool isQmApprove = (currentAppStatus == "Pending_QM" || currentAppStatus == "Pending_QM_Decision")
                    && action == "Approved" && newStatus != "Rejected" && newStatus != "Returned" && newStatus != "Corrective_In_Progress";
                string? qmApprovedBy = isQmApprove ? $"{actorName} ({actorPosition})" : null;
                string? qmApprovedAt = isQmApprove ? nowIso : null;

                bool isGmApprove = !isSingleLevel &&
                    (currentAppStatus == "Pending_GM" || currentAppStatus == "Pending_GM_Decision") &&
                    action == "Approved" && (newStatus == "Approved" || newStatus == "Closed");
                string? gmApprovedBy = isGmApprove ? $"{actorName} ({actorPosition})" : null;
                string? gmApprovedAt = isGmApprove ? nowIso : null;

                string? correctiveAction = body.GetStringOrNull("corrective_action", "correctiveAction");
                string? correctiveOwner = body.GetStringOrNull("corrective_owner", "correctiveOwner");
                string? correctiveDueDate = body.GetStringOrNull("corrective_due_date", "correctiveDueDate");
                string? rejectionCategory = body.GetStringOrNull("rejection_category", "rejectionCategory");
                string? severityLevel = body.GetStringOrNull("severity_level", "severityLevel");
                string? rejectionNotes = body.GetStringOrNull("rejection_notes", "rejectionNotes");
                string? affectedProducts = body.GetStringOrNull("affected_products", "affectedProducts");
                string? immediateAction = body.GetStringOrNull("immediate_action", "immediateAction");

                string? rejectionReason = body.GetStringOrNull("rejection_reason", "rejectionReason")
                    ?? (action == "Rejected" ? comments : null);

                bool isInitialRejection = action == "Rejected" && currentAppStatus != "Pending_QM_Decision" && currentAppStatus != "Pending_GM_Decision";
                string? rejectedBy = body.GetStringOrNull("rejected_by", "rejectedBy")
                    ?? (isInitialRejection ? $"{actorName} ({actorPosition})" : null);
                string? rejectedAt = body.GetStringOrNull("rejected_at", "rejectedAt")
                    ?? (isInitialRejection ? nowIso : null);

                bool isFinalDecisionStage = currentAppStatus == "Pending_GM_Decision"
                    || (isSingleLevel && currentAppStatus == "Pending_QM_Decision")
                    || newStatus == "Closed"
                    || newStatus == "Rejected_By_GM"
                    || newStatus == "Rejected_Final";

                string? gmDecision = body.GetStringOrNull("gm_decision", "gmDecision")
                    ?? (isFinalDecisionStage ? (newStatus == "Rejected_By_GM" || newStatus == "Rejected_Final" ? "Rejected" : "Approved") : null);
                string? gmDecisionBy = body.GetStringOrNull("gm_decision_by", "gmDecisionBy")
                    ?? (isFinalDecisionStage ? $"{actorName} ({actorPosition})" : null);
                string? gmDecisionAt = body.GetStringOrNull("gm_decision_at", "gmDecisionAt")
                    ?? (isFinalDecisionStage ? nowIso : null);
                string? gmDecisionNotes = body.GetStringOrNull("gm_decision_notes", "gmDecisionNotes")
                    ?? (isFinalDecisionStage ? comments : null);
                int? rejectionCount = body.GetIntOrNull("rejection_count", "rejectionCount");

                await conn.ExecuteAsync(@"
                    UPDATE dr_records SET
                        status = @newStatus,
                        approval_status = @newStatus,
                        rejection_count = COALESCE(@rejectionCount, rejection_count),
                        corrective_action = COALESCE(@correctiveAction, corrective_action),
                        corrective_owner = COALESCE(@correctiveOwner, corrective_owner),
                        corrective_due_date = COALESCE(@correctiveDueDate, corrective_due_date),
                        rejection_reason = COALESCE(@rejectionReason, rejection_reason),
                        rejected_by = COALESCE(@rejectedBy, rejected_by),
                        rejected_at = COALESCE(@rejectedAt, rejected_at),
                        rejection_category = COALESCE(@rejectionCategory, rejection_category),
                        severity_level = COALESCE(@severityLevel, severity_level),
                        rejection_notes = COALESCE(@rejectionNotes, rejection_notes),
                        affected_products = COALESCE(@affectedProducts, affected_products),
                        immediate_action = COALESCE(@immediateAction, immediate_action),
                        gm_decision = COALESCE(@gmDecision, gm_decision),
                        gm_decision_by = COALESCE(@gmDecisionBy, gm_decision_by),
                        gm_decision_at = COALESCE(@gmDecisionAt, gm_decision_at),
                        gm_decision_notes = COALESCE(@gmDecisionNotes, gm_decision_notes),
                        qs_approved_by = COALESCE(@qsApprovedBy, qs_approved_by),
                        qs_approved_at = COALESCE(@qsApprovedAt, qs_approved_at),
                        qm_approved_by = COALESCE(@qmApprovedBy, qm_approved_by),
                        qm_approved_at = COALESCE(@qmApprovedAt, qm_approved_at),
                        gm_approved_by = COALESCE(@gmApprovedBy, gm_approved_by),
                        gm_approved_at = COALESCE(@gmApprovedAt, gm_approved_at),
                        updated_at = @actedAt
                    WHERE id = @Id",
                    new {
                        Id = id,
                        newStatus,
                        correctiveAction,
                        correctiveOwner,
                        correctiveDueDate,
                        rejectionReason,
                        rejectedBy,
                        rejectedAt,
                        rejectionCategory,
                        severityLevel,
                        rejectionNotes,
                        affectedProducts,
                        immediateAction,
                        gmDecision,
                        gmDecisionBy,
                        gmDecisionAt,
                        gmDecisionNotes,
                        qsApprovedBy,
                        qsApprovedAt,
                        qmApprovedBy,
                        qmApprovedAt,
                        gmApprovedBy,
                        gmApprovedAt,
                        rejectionCount,
                        actedAt = nowIso
                    }, trans);

                trans.Commit();
                return Result<bool>.Ok(true, "تم تسجيل قرار الاعتماد بنجاح.");
            }
            catch (Exception ex)
            {
                trans.Rollback();
                return Result<bool>.Fail($"حدث خطأ: {ex.Message}");
            }
        }

        public async Task<Result<bool>> DeleteTemplateAsync(int id)
        {
            using var conn = _db.CreateConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();
            try
            {
                var report = await conn.QuerySingleOrDefaultAsync<dynamic>(
                    "SELECT id FROM dr_reports WHERE id = @Id", new { Id = id }, trans);

                if (report == null)
                    return Result<bool>.Fail("لم يتم العثور على التقرير.");

                // 1. Delete values for all records belonging to this report
                await conn.ExecuteAsync(@"
                    DELETE FROM dr_values 
                    WHERE record_id IN (SELECT id FROM dr_records WHERE report_id = @Id)",
                    new { Id = id }, trans);

                // 2. Delete approvals for all records belonging to this report
                await conn.ExecuteAsync(@"
                    DELETE FROM report_approvals 
                    WHERE record_id IN (SELECT id FROM dr_records WHERE report_id = @Id)",
                    new { Id = id }, trans);

                // 3. Delete records belonging to this report
                await conn.ExecuteAsync(@"
                    DELETE FROM dr_records WHERE report_id = @Id",
                    new { Id = id }, trans);

                // 4. Delete any remaining values referencing fields of this report
                await conn.ExecuteAsync(@"
                    DELETE FROM dr_values 
                    WHERE field_id IN (SELECT id FROM dr_fields WHERE report_id = @Id)",
                    new { Id = id }, trans);

                // 5. Delete fields belonging to this report
                await conn.ExecuteAsync(@"
                    DELETE FROM dr_fields WHERE report_id = @Id",
                    new { Id = id }, trans);

                // 6. Delete sections belonging to this report
                await conn.ExecuteAsync(@"
                    DELETE FROM dr_sections WHERE report_id = @Id",
                    new { Id = id }, trans);

                // 7. Break circular foreign key between dr_reports and dr_report_versions
                await conn.ExecuteAsync(@"
                    UPDATE dr_reports SET current_version_id = NULL WHERE id = @Id",
                    new { Id = id }, trans);

                // 8. Delete report versions
                await conn.ExecuteAsync(@"
                    DELETE FROM dr_report_versions WHERE report_id = @Id",
                    new { Id = id }, trans);

                // 9. Delete the report itself
                await conn.ExecuteAsync(@"
                    DELETE FROM dr_reports WHERE id = @Id",
                    new { Id = id }, trans);

                trans.Commit();
                return Result<bool>.Ok(true, "تم حذف التقرير وكافة بياناته وسجلاته بنجاح.");
            }
            catch (Exception ex)
            {
                trans.Rollback();
                return Result<bool>.Fail($"حدث خطأ أثناء حذف التقرير: {ex.Message}");
            }
        }

        public async Task<Result<bool>> DeleteRecordAsync(int id)
        {
            using var conn = _db.CreateConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();
            try
            {
                var record = await conn.QuerySingleOrDefaultAsync<dynamic>(
                    "SELECT id FROM dr_records WHERE id = @Id", new { Id = id }, trans);

                if (record == null)
                    return Result<bool>.Fail("لم يتم العثور على السجل.");

                // 1. Delete values for this record
                await conn.ExecuteAsync(@"
                    DELETE FROM dr_values WHERE record_id = @Id",
                    new { Id = id }, trans);

                // 2. Delete approvals for this record
                await conn.ExecuteAsync(@"
                    DELETE FROM report_approvals WHERE record_id = @Id",
                    new { Id = id }, trans);

                // 3. Delete the record itself
                await conn.ExecuteAsync(@"
                    DELETE FROM dr_records WHERE id = @Id",
                    new { Id = id }, trans);

                trans.Commit();
                return Result<bool>.Ok(true, "تم حذف سجل التقرير بنجاح.");
            }
            catch (Exception ex)
            {
                trans.Rollback();
                return Result<bool>.Fail($"حدث خطأ أثناء حذف السجل: {ex.Message}");
            }
        }

        public async Task<Result<bool>> UpdateTemplateAsync(int id, JsonElement body)
        {
            using var conn = _db.CreateConnection();
            try
            {
                var report = await conn.QuerySingleOrDefaultAsync<dynamic>(
                    "SELECT id FROM dr_reports WHERE id = @Id", new { Id = id });

                if (report == null)
                    return Result<bool>.Fail("لم يتم العثور على التقرير.");

                bool? isActive = body.GetBoolOrNull("is_active", "isActive");
                string? name = body.GetStringOrNull("name");
                string? description = body.GetStringOrNull("description");
                string? icon = body.GetStringOrNull("icon");
                string? orientation = body.GetStringOrNull("orientation");
                string? approvalType = body.GetStringOrNull("approval_type", "approvalType");
                string? docNumber = body.GetStringOrNull("doc_number", "docNumber");
                string? versionNumber = body.GetStringOrNull("version_number", "versionNumber");
                string? issueDate = body.GetStringOrNull("issue_date", "issueDate");
                string? headerConfigJson = body.GetStringOrNull("header_config_json", "headerConfigJson");
                int? sortOrder = body.GetIntOrNull("sort_order", "sortOrder");
                int? rawCatId = body.GetIntOrNull("category_id", "categoryId");
                int? categoryId = (rawCatId.HasValue && rawCatId.Value > 0) ? rawCatId : null;
                int? rawWfId = body.GetIntOrNull("workflow_id", "workflowId");
                int? workflowId = (rawWfId.HasValue && rawWfId.Value > 0) ? rawWfId : null;

                await conn.ExecuteAsync(@"
                    UPDATE dr_reports SET
                        is_active = COALESCE(@isActive, is_active),
                        name = COALESCE(@name, name),
                        description = COALESCE(@description, description),
                        icon = COALESCE(@icon, icon),
                        sort_order = COALESCE(@sortOrder, sort_order),
                        category_id = COALESCE(@categoryId, category_id),
                        workflow_id = COALESCE(@workflowId, workflow_id),
                        orientation = COALESCE(@orientation, orientation),
                        approval_type = COALESCE(@approvalType, approval_type),
                        doc_number = COALESCE(@docNumber, doc_number),
                        version_number = COALESCE(@versionNumber, version_number),
                        issue_date = COALESCE(@issueDate, issue_date),
                        header_config_json = COALESCE(@headerConfigJson, header_config_json),
                        updated_at = @now
                    WHERE id = @Id",
                    new
                    {
                        Id = id,
                        isActive,
                        name,
                        description,
                        icon,
                        sortOrder,
                        categoryId,
                        workflowId,
                        orientation,
                        approvalType,
                        docNumber,
                        versionNumber,
                        issueDate,
                        headerConfigJson,
                        now = DateTime.UtcNow.ToString("o")
                    });

                return Result<bool>.Ok(true, "تم تحديث التقرير بنجاح.");
            }
            catch (Exception ex)
            {
                return Result<bool>.Fail($"حدث خطأ أثناء تحديث التقرير: {ex.Message}");
            }
        }

        public async Task<Result<int>> SaveTemplateAsync(JsonElement body)
        {
            using var conn = _db.CreateConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();
            try
            {
                int reportId = body.GetInt("id", null, 0);
                bool isNewReport = reportId <= 0;
                int? rawCatId = body.GetIntOrNull("category_id", "categoryId");
                int? categoryId = (rawCatId.HasValue && rawCatId.Value > 0) ? rawCatId : null;
                string name = body.GetString("name", null, null, "تقرير جديد");
                string? description = body.GetStringOrNull("description");
                string? icon = body.GetStringOrNull("icon");
                int sortOrder = body.GetInt("sort_order", "sortOrder", 0);
                bool isActive = body.GetBool("is_active", "isActive", true);
                string approvalType = body.GetString("approval_type", "approvalType", null, "two_level");
                string orientation = body.GetString("orientation", null, null, "portrait");
                string? docNumber = body.GetStringOrNull("doc_number", "docNumber");
                string? versionNumber = body.GetStringOrNull("version_number", "versionNumber");
                string? issueDate = body.GetStringOrNull("issue_date", "issueDate");
                string? headerConfigJson = body.GetStringOrNull("header_config_json", "headerConfigJson");
                int? rawWfId = body.GetIntOrNull("workflow_id", "workflowId");
                int? workflowId = (rawWfId.HasValue && rawWfId.Value > 0) ? rawWfId : null;
                string now = DateTime.UtcNow.ToString("o");

                if (reportId > 0)
                {
                    await conn.ExecuteAsync(@"
                        UPDATE dr_reports SET
                            category_id = @categoryId,
                            name = @name,
                            description = @description,
                            icon = @icon,
                            sort_order = @sortOrder,
                            is_active = @isActive,
                            approval_type = @approvalType,
                            orientation = @orientation,
                            doc_number = @docNumber,
                            version_number = @versionNumber,
                            issue_date = @issueDate,
                            header_config_json = @headerConfigJson,
                            workflow_id = @workflowId,
                            updated_at = @now
                        WHERE id = @reportId",
                        new { reportId, categoryId, name, description, icon, sortOrder, isActive, approvalType, orientation, docNumber, versionNumber, issueDate, headerConfigJson, workflowId, now }, trans);
                }
                else
                {
                    reportId = await conn.QuerySingleAsync<int>(@"
                        INSERT INTO dr_reports (
                            category_id, name, description, icon, sort_order, is_active,
                            approval_type, orientation, doc_number, version_number, issue_date, header_config_json, workflow_id,
                            created_at, updated_at
                        ) VALUES (
                            @categoryId, @name, @description, @icon, @sortOrder, @isActive,
                            @approvalType, @orientation, @docNumber, @versionNumber, @issueDate, @headerConfigJson, @workflowId,
                            @now, @now
                        );
                        SELECT CAST(SCOPE_IDENTITY() AS INT);",
                        new { categoryId, name, description, icon, sortOrder, isActive, approvalType, orientation, docNumber, versionNumber, issueDate, headerConfigJson, workflowId, now }, trans);
                }

                if (body.TryGetProperty("sections", out var secArr) && secArr.ValueKind == JsonValueKind.Array)
                {
                    var existingSectionIds = (await conn.QueryAsync<int>(
                        "SELECT id FROM dr_sections WHERE report_id = @reportId", new { reportId }, trans)).ToHashSet();
                    var existingFields = (await conn.QueryAsync<(int Id, string? FieldKey)>(
                        "SELECT id AS Id, field_key AS FieldKey FROM dr_fields WHERE report_id = @reportId", new { reportId }, trans)).ToList();
                    var existingFieldIds = existingFields.Select(f => f.Id).ToHashSet();
                    var existingFieldByKey = existingFields
                        .Where(f => !string.IsNullOrWhiteSpace(f.FieldKey))
                        .GroupBy(f => f.FieldKey!, StringComparer.OrdinalIgnoreCase)
                        .ToDictionary(g => g.Key, g => g.First().Id, StringComparer.OrdinalIgnoreCase);

                    var processedSectionIds = new HashSet<int>();
                    var processedFieldIds = new HashSet<int>();

                    int secSort = 1;
                    foreach (var sec in secArr.EnumerateArray())
                    {
                        int rawSecId = sec.GetInt("id", null, 0);
                        string title = sec.GetString("title", null, null, "قسم");
                        string? secDesc = sec.GetStringOrNull("description");
                        bool isCollapsible = sec.GetBool("is_collapsible", "isCollapsible", false);

                        int sectionId;
                        if (rawSecId > 0 && existingSectionIds.Contains(rawSecId))
                        {
                            sectionId = rawSecId;
                            await conn.ExecuteAsync(@"
                                UPDATE dr_sections SET
                                    title = @title,
                                    description = @secDesc,
                                    sort_order = @secSort,
                                    is_collapsible = @isCollapsible,
                                    updated_at = @now
                                WHERE id = @sectionId AND report_id = @reportId",
                                new { sectionId, reportId, title, secDesc, secSort, isCollapsible, now }, trans);
                        }
                        else
                        {
                            sectionId = await conn.QuerySingleAsync<int>(@"
                                INSERT INTO dr_sections (report_id, title, description, sort_order, is_collapsible, created_at, updated_at)
                                VALUES (@reportId, @title, @secDesc, @secSort, @isCollapsible, @now, @now);
                                SELECT CAST(SCOPE_IDENTITY() AS INT);",
                                new { reportId, title, secDesc, secSort, isCollapsible, now }, trans);
                        }

                        processedSectionIds.Add(sectionId);
                        secSort++;

                        if (sec.TryGetProperty("fields", out var fieldsArr) && fieldsArr.ValueKind == JsonValueKind.Array)
                        {
                            int fldSort = 1;
                            foreach (var fld in fieldsArr.EnumerateArray())
                            {
                                int rawFldId = fld.GetInt("id", null, 0);
                                string fieldKey = fld.GetString("field_key", "fieldKey", null, $"f_{sectionId}_{fldSort}");
                                string label = fld.GetString("label", null, null, "حقل");
                                string? hint = fld.GetStringOrNull("hint");
                                string fieldType = fld.GetString("field_type", "fieldType", null, "Text");
                                bool isRequired = fld.GetBool("is_required", "isRequired", false);
                                string? defaultValue = fld.GetStringOrNull("default_value", "defaultValue");
                                string? optionsJson = fld.GetStringOrNull("options_json", "optionsJson");
                                string? validationJson = fld.GetStringOrNull("validation_json", "validationJson");
                                decimal? minValue = fld.GetDecimalOrNull("min_value", "minValue");
                                decimal? maxValue = fld.GetDecimalOrNull("max_value", "maxValue");
                                string? unit = fld.GetStringOrNull("unit");
                                bool isVisible = fld.GetBool("is_visible", "isVisible", true);
                                int? parameterId = fld.GetIntOrNull("parameter_id", "parameterId");
                                int? standardId = fld.GetIntOrNull("standard_id", "standardId");

                                int matchedFieldId = 0;
                                if (rawFldId > 0 && existingFieldIds.Contains(rawFldId))
                                {
                                    matchedFieldId = rawFldId;
                                }
                                else if (!string.IsNullOrWhiteSpace(fieldKey) &&
                                         existingFieldByKey.TryGetValue(fieldKey, out int byKeyId) &&
                                         !processedFieldIds.Contains(byKeyId))
                                {
                                    matchedFieldId = byKeyId;
                                }

                                int fieldId;
                                if (matchedFieldId > 0)
                                {
                                    fieldId = matchedFieldId;
                                    await conn.ExecuteAsync(@"
                                        UPDATE dr_fields SET
                                            section_id = @sectionId,
                                            field_key = @fieldKey,
                                            label = @label,
                                            hint = @hint,
                                            field_type = @fieldType,
                                            is_required = @isRequired,
                                            sort_order = @fldSort,
                                            default_value = @defaultValue,
                                            options_json = @optionsJson,
                                            validation_json = @validationJson,
                                            min_value = @minValue,
                                            max_value = @maxValue,
                                            unit = @unit,
                                            is_visible = @isVisible,
                                            parameter_id = @parameterId,
                                            standard_id = @standardId,
                                            updated_at = @now
                                        WHERE id = @fieldId AND report_id = @reportId",
                                        new { fieldId, reportId, sectionId, fieldKey, label, hint, fieldType, isRequired, fldSort, defaultValue, optionsJson, validationJson, minValue, maxValue, unit, isVisible, parameterId, standardId, now }, trans);
                                }
                                else
                                {
                                    fieldId = await conn.QuerySingleAsync<int>(@"
                                        INSERT INTO dr_fields (
                                            report_id, section_id, field_key, label, hint, field_type,
                                            is_required, sort_order, default_value, options_json,
                                            validation_json,
                                            min_value, max_value, unit, is_visible, parameter_id, standard_id,
                                            created_at, updated_at
                                        ) VALUES (
                                            @reportId, @sectionId, @fieldKey, @label, @hint, @fieldType,
                                            @isRequired, @fldSort, @defaultValue, @optionsJson,
                                            @validationJson,
                                            @minValue, @maxValue, @unit, @isVisible, @parameterId, @standardId,
                                            @now, @now
                                        );
                                        SELECT CAST(SCOPE_IDENTITY() AS INT);",
                                        new { reportId, sectionId, fieldKey, label, hint, fieldType, isRequired, fldSort, defaultValue, optionsJson, validationJson, minValue, maxValue, unit, isVisible, parameterId, standardId, now }, trans);
                                }

                                processedFieldIds.Add(fieldId);
                                fldSort++;
                            }
                        }
                    }

                    // Delete removed fields that are NOT referenced by any dr_values
                    var removedFieldIds = existingFieldIds.Except(processedFieldIds).ToList();
                    if (removedFieldIds.Any())
                    {
                        await conn.ExecuteAsync(@"
                            DELETE FROM dr_fields 
                            WHERE id IN @removedFieldIds 
                              AND id NOT IN (SELECT DISTINCT field_id FROM dr_values WHERE field_id IS NOT NULL)",
                            new { removedFieldIds }, trans);

                        // Mark any un-deletable removed fields as hidden
                        await conn.ExecuteAsync(@"
                            UPDATE dr_fields 
                            SET is_visible = 0, updated_at = @now 
                            WHERE id IN @removedFieldIds",
                            new { removedFieldIds, now }, trans);
                    }

                    // Delete removed sections that no longer contain any fields
                    var removedSectionIds = existingSectionIds.Except(processedSectionIds).ToList();
                    if (removedSectionIds.Any())
                    {
                        await conn.ExecuteAsync(@"
                            DELETE FROM dr_sections 
                            WHERE id IN @removedSectionIds 
                              AND id NOT IN (SELECT DISTINCT section_id FROM dr_fields WHERE section_id IS NOT NULL)",
                            new { removedSectionIds }, trans);
                    }
                }

                // منح صلاحيات التقرير الجديد تلقائياً للأدوار المخولة في جدول CustomRoles
                if (isNewReport && reportId > 0)
                {
                    try
                    {
                        var roles = (await conn.QueryAsync<(int Id, string? Code, string? PermissionsJson)>(
                            "SELECT id AS Id, code AS Code, permissions_json AS PermissionsJson FROM CustomRoles",
                            transaction: trans)).ToList();

                        foreach (var role in roles)
                        {
                            var perms = new List<string>();
                            if (!string.IsNullOrWhiteSpace(role.PermissionsJson))
                            {
                                try
                                {
                                    perms = JsonSerializer.Deserialize<List<string>>(role.PermissionsJson) ?? new List<string>();
                                }
                                catch { }
                            }

                            var codeLower = (role.Code ?? "").Trim().ToLowerInvariant();
                            bool isAdminOrManager = codeLower == "admin" ||
                                                    codeLower == "qualitymanager" ||
                                                    codeLower == "factorymanager" ||
                                                    codeLower == "generalmanager" ||
                                                    codeLower == "qualitysupervisor";
                            bool hasDynamicPerm = perms.Contains("design_dynamic_reports") ||
                                                  perms.Contains("fill_dynamic_reports") ||
                                                  perms.Contains("fill_quality_dynamic_reports") ||
                                                  perms.Contains("fill_food_safety_dynamic_reports") ||
                                                  perms.Contains("view_reports");

                            if (isAdminOrManager || hasDynamicPerm)
                            {
                                string[] newPerms = new[]
                                {
                                    $"enabled_dyn_rpt_{reportId}",
                                    $"view_dyn_rpt_{reportId}",
                                    $"fill_dyn_rpt_{reportId}",
                                    $"print_dyn_rpt_{reportId}"
                                };
                                bool changed = false;
                                foreach (var np in newPerms)
                                {
                                    if (!perms.Contains(np))
                                    {
                                        perms.Add(np);
                                        changed = true;
                                    }
                                }
                                if (changed)
                                {
                                    string updatedJson = JsonSerializer.Serialize(perms);
                                    await conn.ExecuteAsync(
                                        "UPDATE CustomRoles SET permissions_json = @PermissionsJson WHERE id = @RoleId",
                                        new { PermissionsJson = updatedJson, RoleId = role.Id }, trans);
                                }
                            }
                        }
                    }
                    catch { }
                }

                trans.Commit();
                return Result<int>.Ok(reportId, "تم حفظ قالب التقرير بنجاح.");
            }
            catch (Exception ex)
            {
                trans.Rollback();
                return Result<int>.Fail($"حدث خطأ أثناء حفظ القالب: {ex.Message}");
            }
        }
    }
}
