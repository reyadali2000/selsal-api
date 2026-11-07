using Dapper;
using WadiAlAin.Application.Common.Interfaces;
using WadiAlAin.Application.Common.Models;
using WadiAlAin.Application.Interfaces;
using WadiAlAin.Domain.Entities;
using WadiAlAin.Application.Common.Helpers;
using System.Text.Json;

namespace WadiAlAin.Infrastructure.Services
{
    public class WaterQualityService : IWaterQualityService
    {
        private readonly IDbConnectionFactory _db;

        public WaterQualityService(IDbConnectionFactory db)
        {
            _db = db;
        }

        public async Task<IEnumerable<WaterQualityReport>> GetLogsAsync(int? branchId, string? category, string? fromDate, string? toDate)
        {
            using var conn = _db.CreateConnection();
            string sql = @"
                SELECT 
                    id, branch_id AS BranchId, branch_name AS BranchName, shift_id AS ShiftId, shift_name AS ShiftName,
                    created_by AS CreatedBy, report_category AS ReportCategory, water_type AS WaterType, batch_number AS BatchNumber,
                    is_compliant AS IsCompliant, general_notes AS GeneralNotes, created_at AS CreatedAt, approval_status AS ApprovalStatus,
                    approved_by AS ApprovedBy, approved_at AS ApprovedAt, rejection_reason AS RejectionReason,
                    water_source AS WaterSource, well_number AS WellNumber, sample_taker AS SampleTaker,
                    manager_notified AS ManagerNotified, source_suspended AS SourceSuspended, ncr_number AS NcrNumber,
                    qm_approved_by AS QmApprovedBy, qm_approved_at AS QmApprovedAt, gm_approved_by AS GmApprovedBy, gm_approved_at AS GmApprovedAt,
                    rejected_by AS RejectedBy, product_name AS ProductName, bottle_size AS BottleSize, production_date AS ProductionDate,
                    sampling_time AS SamplingTime, sampling_location AS SamplingLocation, produced_quantity AS ProducedQuantity,
                    is_isolated AS IsIsolated, is_hold_issued AS IsHoldIssued, is_reject_issued AS IsRejectIssued,
                    micro_results_pending AS MicroResultsPending, micro_entered_by AS MicroEnteredBy, micro_entered_at AS MicroEnteredAt,
                    stage1_qm_approved_by AS Stage1QmApprovedBy, stage1_qm_approved_at AS Stage1QmApprovedAt,
                    stage1_gm_approved_by AS Stage1GmApprovedBy, stage1_gm_approved_at AS Stage1GmApprovedAt,
                    micro_qm_approved_by AS MicroQmApprovedBy, micro_qm_approved_at AS MicroQmApprovedAt,
                    micro_gm_approved_by AS MicroGmApprovedBy, micro_gm_approved_at AS MicroGmApprovedAt,
                    rejection_category AS RejectionCategory, severity_level AS SeverityLevel, rejection_notes AS RejectionNotes,
                    affected_products AS AffectedProducts, immediate_action AS ImmediateAction, corrective_action AS CorrectiveAction,
                    corrective_owner AS CorrectiveOwner, corrective_due_date AS CorrectiveDueDate,
                    gm_decision AS GmDecision, gm_decision_by AS GmDecisionBy, gm_decision_at AS GmDecisionAt, gm_decision_notes AS GmDecisionNotes,
                    micro_rejection_reason AS MicroRejectionReason, micro_rejected_by AS MicroRejectedBy, micro_rejected_at AS MicroRejectedAt, micro_rejection_action AS MicroRejectionAction
                FROM WaterQualityLogs
                WHERE 1=1";

            if (branchId.HasValue && branchId.Value > 0)
            {
                if (branchId.Value == 1)
                    sql += " AND (branch_id = 1 OR branch_id IS NULL OR branch_id = 0)";
                else
                    sql += " AND branch_id = @BranchId";
            }
            if (!string.IsNullOrWhiteSpace(category)) sql += " AND report_category = @Category";
            if (!string.IsNullOrWhiteSpace(fromDate)) sql += " AND created_at >= @FromDate";
            if (!string.IsNullOrWhiteSpace(toDate)) sql += " AND created_at <= @ToDate";

            sql += " ORDER BY id DESC";

            return await conn.QueryAsync<WaterQualityReport>(sql, new { BranchId = branchId, Category = category, FromDate = fromDate, ToDate = toDate });
        }

        public async Task<Result<dynamic>> GetLogByIdAsync(int id)
        {
            using var conn = _db.CreateConnection();
            var log = await conn.QuerySingleOrDefaultAsync<dynamic>(
                "SELECT * FROM WaterQualityLogs WHERE id = @Id", new { Id = id });

            if (log == null)
                return Result<dynamic>.Fail("لم يتم العثور على التقرير.");

            var results = await conn.QueryAsync<dynamic>(@"
                SELECT r.*, p.name_ar, p.unit, p.input_type, p.category 
                FROM WaterQualityResults r
                LEFT JOIN TestParameters p ON r.parameter_id = p.id
                WHERE r.log_id = @LogId", new { LogId = id });

            return Result<dynamic>.Ok(new { Log = log, Results = results });
        }

        public async Task<Result<int>> SaveLogAsync(JsonElement body)
        {
            using var conn = _db.CreateConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();

            try
            {
                JsonElement logElem = body;
                if (body.ValueKind == JsonValueKind.Object && body.TryGetProperty("log", out var nestedLog))
                {
                    logElem = nestedLog;
                }

                int logId = logElem.GetInt("id", null, 0);
                int branchId = logElem.GetInt("branchId", "branch_id", 1);
                string branchName = logElem.GetString("branchName", "branch_name", null, "مصنع أرتل");
                int? shiftId = logElem.GetIntOrNull("shiftId", "shift_id");
                string? shiftName = logElem.GetStringOrNull("shiftName", "shift_name");
                string createdBy = logElem.GetString("createdBy", "created_by", null, "");
                string reportCategory = logElem.GetString("reportCategory", "report_category", null, "Microbiological");
                string waterType = logElem.GetString("waterType", "water_type", null, "Treated");
                string batchNumber = logElem.GetString("batchNumber", "batch_number", null, "");
                bool isCompliant = logElem.GetBool("isCompliant", "is_compliant", true);
                string generalNotes = logElem.GetString("generalNotes", "general_notes", null, "");
                string createdAt = logElem.GetString("createdAt", "created_at", null, DateTime.UtcNow.ToString("o"));
                string approvalStatus = logElem.GetString("approvalStatus", "approval_status", null, "Pending_QM");
                string? rejectionReason = logElem.GetStringOrNull("rejectionReason", "rejection_reason");
                string waterSource = logElem.GetString("waterSource", "water_source", null, "");
                string wellNumber = logElem.GetString("wellNumber", "well_number", null, "");
                string sampleTaker = logElem.GetString("sampleTaker", "sample_taker", null, "");
                bool managerNotified = logElem.GetBool("managerNotified", "manager_notified", false);
                bool sourceSuspended = logElem.GetBool("sourceSuspended", "source_suspended", false);
                string ncrNumber = logElem.GetString("ncrNumber", "ncr_number", null, "");
                string productName = logElem.GetString("productName", "product_name", null, "");
                string bottleSize = logElem.GetString("bottleSize", "bottle_size", null, "");
                string productionDate = logElem.GetString("productionDate", "production_date", null, "");
                string samplingTime = logElem.GetString("samplingTime", "sampling_time", null, "");
                string samplingLocation = logElem.GetString("samplingLocation", "sampling_location", null, "");
                string producedQuantity = logElem.GetString("producedQuantity", "produced_quantity", null, "");
                bool isIsolated = logElem.GetBool("isIsolated", "is_isolated", false);
                bool isHoldIssued = logElem.GetBool("isHoldIssued", "is_hold_issued", false);
                bool isRejectIssued = logElem.GetBool("isRejectIssued", "is_reject_issued", false);
                bool microResultsPending = logElem.GetBool("microResultsPending", "micro_results_pending", false);
                string rejectionCategory = logElem.GetString("rejectionCategory", "rejection_category", null, "");
                string severityLevel = logElem.GetString("severityLevel", "severity_level", null, "");
                string rejectionNotes = logElem.GetString("rejectionNotes", "rejection_notes", null, "");
                string affectedProducts = logElem.GetString("affectedProducts", "affected_products", null, "");
                string immediateAction = logElem.GetString("immediateAction", "immediate_action", null, "");
                string correctiveAction = logElem.GetString("correctiveAction", "corrective_action", null, "");
                string correctiveOwner = logElem.GetString("correctiveOwner", "corrective_owner", null, "");
                string? correctiveDueDate = logElem.GetStringOrNull("correctiveDueDate", "corrective_due_date");
                if (string.IsNullOrWhiteSpace(correctiveDueDate)) correctiveDueDate = null;
                string? gmDecision = logElem.GetStringOrNull("gmDecision", "gm_decision");
                if (string.IsNullOrWhiteSpace(gmDecision)) gmDecision = null;
                string? gmDecisionBy = logElem.GetStringOrNull("gmDecisionBy", "gm_decision_by");
                if (string.IsNullOrWhiteSpace(gmDecisionBy)) gmDecisionBy = null;
                string? gmDecisionAt = logElem.GetStringOrNull("gmDecisionAt", "gm_decision_at");
                if (string.IsNullOrWhiteSpace(gmDecisionAt)) gmDecisionAt = null;
                string? gmDecisionNotes = logElem.GetStringOrNull("gmDecisionNotes", "gm_decision_notes");
                if (string.IsNullOrWhiteSpace(gmDecisionNotes)) gmDecisionNotes = null;

                // حماية من التكرار اللحظي (Idempotency): إذا لم يكن للتقرير معرّف سابق وكان رقم الدفعة موجوداً
                // وتكرر الحفظ خلال 3 دقائق بنفس الوردية والفرع والفئة، نقوم بتحديث السجل بدلاً من إنشاء سجل مكرر
                if (logId == 0 && !string.IsNullOrWhiteSpace(batchNumber))
                {
                    var existingId = await conn.QuerySingleOrDefaultAsync<int?>(@"
                        SELECT TOP 1 id FROM WaterQualityLogs 
                        WHERE batch_number = @batchNumber 
                          AND branch_id = @branchId 
                          AND report_category = @reportCategory 
                          AND created_at >= DATEADD(minute, -3, GETUTCDATE())
                        ORDER BY id DESC", new { batchNumber, branchId, reportCategory }, trans);

                    if (existingId.HasValue && existingId.Value > 0)
                    {
                        logId = existingId.Value;
                    }
                }

                bool existsInDb = logId > 0 &&
                    (await conn.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM WaterQualityLogs WHERE id = @logId", new { logId }, trans)) > 0;

                if (existsInDb)
                {
                    await conn.ExecuteAsync(@"
                        UPDATE WaterQualityLogs SET
                            branch_id = @branchId, branch_name = @branchName, shift_id = @shiftId, shift_name = @shiftName,
                            report_category = @reportCategory, water_type = @waterType, batch_number = @batchNumber,
                            is_compliant = @isCompliant, general_notes = @generalNotes, approval_status = @approvalStatus,
                            water_source = @waterSource, well_number = @wellNumber, sample_taker = @sampleTaker,
                            manager_notified = @managerNotified, source_suspended = @sourceSuspended, ncr_number = @ncrNumber,
                            product_name = @productName, bottle_size = @bottleSize, production_date = @productionDate,
                            sampling_time = @samplingTime, sampling_location = @samplingLocation, produced_quantity = @producedQuantity,
                            is_isolated = @isIsolated, is_hold_issued = @isHoldIssued, is_reject_issued = @isRejectIssued,
                            micro_results_pending = @microResultsPending, rejection_category = @rejectionCategory,
                            severity_level = @severityLevel, rejection_notes = @rejectionNotes, affected_products = @affectedProducts,
                            immediate_action = @immediateAction, corrective_action = @correctiveAction,
                            corrective_owner = @correctiveOwner, corrective_due_date = @correctiveDueDate, gm_decision = @gmDecision,
                            gm_decision_by = @gmDecisionBy, gm_decision_at = @gmDecisionAt, gm_decision_notes = @gmDecisionNotes
                        WHERE id = @logId", new
                    {
                        logId, branchId, branchName, shiftId, shiftName, createdBy, reportCategory, waterType,
                        batchNumber, isCompliant, generalNotes, approvalStatus, rejectionReason,
                        waterSource, wellNumber, sampleTaker, managerNotified, sourceSuspended, ncrNumber,
                        productName, bottleSize, productionDate, samplingTime, samplingLocation, producedQuantity,
                        isIsolated, isHoldIssued, isRejectIssued, microResultsPending,
                        rejectionCategory, severityLevel, rejectionNotes, affectedProducts, immediateAction,
                        correctiveAction, correctiveOwner, correctiveDueDate, gmDecision, gmDecisionBy, gmDecisionAt, gmDecisionNotes
                    }, trans);

                    await conn.ExecuteAsync("DELETE FROM WaterQualityResults WHERE log_id = @logId", new { logId }, trans);
                }
                else
                {
                    logId = await conn.QuerySingleAsync<int>(@"
                        INSERT INTO WaterQualityLogs (
                            branch_id, branch_name, shift_id, shift_name, created_by, report_category, water_type,
                            batch_number, is_compliant, general_notes, created_at, approval_status,
                            water_source, well_number, sample_taker, manager_notified, source_suspended, ncr_number,
                            product_name, bottle_size, production_date, sampling_time, sampling_location, produced_quantity,
                            is_isolated, is_hold_issued, is_reject_issued, micro_results_pending,
                            rejection_category, severity_level, rejection_notes, affected_products, immediate_action,
                            corrective_action, corrective_owner, corrective_due_date, gm_decision, gm_decision_by, gm_decision_at, gm_decision_notes
                        ) VALUES (
                            @branchId, @branchName, @shiftId, @shiftName, @createdBy, @reportCategory, @waterType,
                            @batchNumber, @isCompliant, @generalNotes, @createdAt, @approvalStatus,
                            @waterSource, @wellNumber, @sampleTaker, @managerNotified, @sourceSuspended, @ncrNumber,
                            @productName, @bottleSize, @productionDate, @samplingTime, @samplingLocation, @producedQuantity,
                            @isIsolated, @isHoldIssued, @isRejectIssued, @microResultsPending,
                            @rejectionCategory, @severityLevel, @rejectionNotes, @affectedProducts, @immediateAction,
                            @correctiveAction, @correctiveOwner, @correctiveDueDate, @gmDecision, @gmDecisionBy, @gmDecisionAt, @gmDecisionNotes
                        );
                        SELECT CAST(SCOPE_IDENTITY() AS INT);", new
                    {
                        branchId, branchName, shiftId, shiftName, createdBy, reportCategory, waterType,
                        batchNumber, isCompliant, generalNotes, createdAt, approvalStatus,
                        waterSource, wellNumber, sampleTaker, managerNotified, sourceSuspended, ncrNumber,
                        productName, bottleSize, productionDate, samplingTime, samplingLocation, producedQuantity,
                        isIsolated, isHoldIssued, isRejectIssued, microResultsPending,
                        rejectionCategory, severityLevel, rejectionNotes, affectedProducts, immediateAction,
                        correctiveAction, correctiveOwner, correctiveDueDate, gmDecision, gmDecisionBy, gmDecisionAt, gmDecisionNotes
                    }, trans);
                }

                JsonElement resultsElem = default;
                if (body.TryGetProperty("results", out var resArr) && resArr.ValueKind == JsonValueKind.Array)
                {
                    resultsElem = resArr;
                }
                else if (logElem.TryGetProperty("results", out var resArr2) && resArr2.ValueKind == JsonValueKind.Array)
                {
                    resultsElem = resArr2;
                }

                if (resultsElem.ValueKind == JsonValueKind.Array)
                {
                    foreach (var res in resultsElem.EnumerateArray())
                    {
                        int parameterId = res.GetInt("parameter_id", "parameterId", 0);
                        decimal? numRes = res.GetDecimalOrNull("numeric_result", "numericResult");
                        string? textRes = res.GetStringOrNull("text_result", "textResult");
                        bool? isComp = res.GetBoolOrNull("is_compliant", "isCompliant");
                        string? notes = res.GetStringOrNull("notes");

                        await conn.ExecuteAsync(@"
                            INSERT INTO WaterQualityResults (log_id, parameter_id, numeric_result, text_result, is_compliant, notes)
                            VALUES (@logId, @parameterId, @numRes, @textRes, @isComp, @notes)",
                            new { logId, parameterId, numRes, textRes, isComp, notes }, trans);
                    }
                }

                trans.Commit();
                return Result<int>.Ok(logId, "تم حفظ تقرير فحص المياه بنجاح.");
            }
            catch (Exception ex)
            {
                trans.Rollback();
                return Result<int>.Fail($"حدث خطأ أثناء حفظ التقرير: {ex.Message}");
            }
        }

        public async Task<Result<bool>> UpdateLogStatusAsync(int id, JsonElement body)
        {
            using var conn = _db.CreateConnection();
            string? status = body.TryGetProperty("status", out var stProp) ? stProp.GetString() : (body.TryGetProperty("approvalStatus", out var asProp) ? asProp.GetString() : null);
            string? approvedBy = body.TryGetProperty("approvedBy", out var abProp) ? abProp.GetString() : null;
            string? rejectionReason = body.TryGetProperty("rejectionReason", out var rrProp) ? rrProp.GetString() : null;
            string? rejectionCategory = body.TryGetProperty("rejectionCategory", out var rcProp) ? rcProp.GetString() : null;
            string? severityLevel = body.TryGetProperty("severityLevel", out var slProp) ? slProp.GetString() : null;
            string? rejectionNotes = body.TryGetProperty("rejectionNotes", out var rnProp) ? rnProp.GetString() : null;
            string? affectedProducts = body.TryGetProperty("affectedProducts", out var apProp) ? apProp.GetString() : null;
            string? immediateAction = body.TryGetProperty("immediateAction", out var iaProp) ? iaProp.GetString() : null;
            string? correctiveAction = body.TryGetProperty("correctiveAction", out var caProp) ? caProp.GetString() : null;
            string? correctiveOwner = body.TryGetProperty("correctiveOwner", out var coProp) ? coProp.GetString() : null;
            string? correctiveDueDate = body.TryGetProperty("correctiveDueDate", out var cdProp) ? cdProp.GetString() : null;
            string? gmDecision = body.TryGetProperty("gmDecision", out var gdProp) ? gdProp.GetString() : null;
            string? gmDecisionBy = body.TryGetProperty("gmDecisionBy", out var gdbProp) ? gdbProp.GetString() : null;
            string? gmDecisionNotes = body.TryGetProperty("gmDecisionNotes", out var gdnProp) ? gdnProp.GetString() : null;
            string? microRejectionReason = body.TryGetProperty("microRejectionReason", out var mrrProp) ? mrrProp.GetString() : rejectionReason;
            string? microRejectedBy = body.TryGetProperty("microRejectedBy", out var mrbProp) ? mrbProp.GetString() : approvedBy;
            string? microRejectionAction = body.TryGetProperty("microRejectionAction", out var mraProp) ? mraProp.GetString() : null;
            int? rejectionCount = body.TryGetProperty("rejectionCount", out var rCountProp) && rCountProp.TryGetInt32(out var rci)
                ? rci
                : (body.TryGetProperty("rejection_count", out var rCountProp2) && rCountProp2.TryGetInt32(out var rci2) ? rci2 : (int?)null);

            string finalStatus = status;
            if (!string.IsNullOrEmpty(gmDecision))
            {
                if (gmDecision == "Approve_Concession" || gmDecision == "Reject_Dispose" || gmDecision == "Approved")
                    finalStatus = status == "Awaiting_Micro_Results" ? "Awaiting_Micro_Results" : "Closed";
                else if (gmDecision == "Rework")
                    finalStatus = "Pending_QM";
                else if (gmDecision == "Rejected")
                    finalStatus = "Rejected_By_GM";
            }

            string sql = "UPDATE WaterQualityLogs SET approval_status = @Status";

            if (finalStatus == "Pending_Micro_QM")
            {
                sql += ", micro_results_pending = 0, micro_entered_by = @ApprovedBy, micro_entered_at = @Now";
            }
            else if (finalStatus == "Approved_QM" || finalStatus == "Pending_GM" || finalStatus == "Approved_Stage1_QM" || finalStatus == "Approved_Micro_QM" || finalStatus == "Pending_Micro_GM")
            {
                sql += ", qm_approved_by = @ApprovedBy, qm_approved_at = @Now";
                if (finalStatus == "Approved_Stage1_QM") sql += ", stage1_qm_approved_by = @ApprovedBy, stage1_qm_approved_at = @Now";
                if (finalStatus == "Approved_Micro_QM" || finalStatus == "Pending_Micro_GM") sql += ", micro_qm_approved_by = @ApprovedBy, micro_qm_approved_at = @Now";
            }
            else if (finalStatus == "Approved_GM" || finalStatus == "Approved" || finalStatus == "Approved_Stage1_GM" || finalStatus == "Approved_Micro_GM" || finalStatus == "Awaiting_Micro_Results")
            {
                sql += ", gm_approved_by = @ApprovedBy, gm_approved_at = @Now";
                if (finalStatus == "Approved_Stage1_GM" || finalStatus == "Awaiting_Micro_Results") sql += ", stage1_gm_approved_by = @ApprovedBy, stage1_gm_approved_at = @Now";
                if (finalStatus == "Approved_Micro_GM" || finalStatus == "Approved") sql += ", micro_gm_approved_by = @ApprovedBy, micro_gm_approved_at = @Now";
            }
            else if (finalStatus == "Closed")
            {
                sql += ", gm_approved_by = COALESCE(@ApprovedBy, gm_approved_by), gm_approved_at = @Now";
            }
            else if (finalStatus == "Micro_Rejected" || finalStatus == "Micro_Resample_Pending" || finalStatus == "Micro_Corrective_In_Progress")
            {
                sql += ", micro_rejected_by = @MicroRejectedBy, micro_rejection_reason = @MicroRejectionReason, micro_rejected_at = @Now";
                if (!string.IsNullOrEmpty(microRejectionAction)) sql += ", micro_rejection_action = @MicroRejectionAction";
                if (finalStatus == "Micro_Resample_Pending") sql += ", micro_results_pending = 1";
            }
            else if (finalStatus == "Pending_GM_Decision")
            {
                if (!string.IsNullOrEmpty(approvedBy))
                    sql += ", qm_approved_by = COALESCE(@ApprovedBy, qm_approved_by), qm_approved_at = @Now";
            }
            else if (finalStatus == "Pending_QM_Decision")
            {
                // رفع الإجراء التصحيحي من أخصائي الجودة لمراجعة مدير الجودة
            }
            else if (finalStatus == "Rejected" || finalStatus == "Corrective_In_Progress" || finalStatus == "Rejected_By_GM" || finalStatus == "Rejected_Final" || finalStatus == "Returned")
            {
                if (!string.IsNullOrEmpty(rejectionReason)) sql += ", rejection_reason = @RejectionReason";
                if (!string.IsNullOrEmpty(approvedBy) && (finalStatus == "Rejected" || finalStatus == "Returned" || !string.IsNullOrEmpty(rejectionReason)))
                {
                    sql += ", rejected_by = COALESCE(rejected_by, @ApprovedBy), rejected_at = COALESCE(rejected_at, @Now)";
                }
            }

            if (!string.IsNullOrEmpty(rejectionCategory)) sql += ", rejection_category = @RejectionCategory";
            if (!string.IsNullOrEmpty(severityLevel)) sql += ", severity_level = @SeverityLevel";
            if (!string.IsNullOrEmpty(rejectionNotes)) sql += ", rejection_notes = @RejectionNotes";
            if (!string.IsNullOrEmpty(affectedProducts)) sql += ", affected_products = @AffectedProducts";
            if (!string.IsNullOrEmpty(immediateAction)) sql += ", immediate_action = @ImmediateAction";
            if (!string.IsNullOrEmpty(correctiveAction)) sql += ", corrective_action = @CorrectiveAction";
            if (!string.IsNullOrEmpty(correctiveOwner)) sql += ", corrective_owner = @CorrectiveOwner";
            if (!string.IsNullOrEmpty(correctiveDueDate)) sql += ", corrective_due_date = @CorrectiveDueDate";
            if (!string.IsNullOrEmpty(gmDecision))
            {
                sql += ", gm_decision = @GmDecision, gm_decision_by = @GmDecisionBy, gm_decision_at = @Now";
                if (!string.IsNullOrEmpty(gmDecisionNotes)) sql += ", gm_decision_notes = @GmDecisionNotes";
            }
            if (rejectionCount.HasValue) sql += ", rejection_count = @RejectionCount";

            sql += " WHERE id = @Id";

            await conn.ExecuteAsync(sql, new
            {
                Status = finalStatus,
                ApprovedBy = approvedBy,
                RejectionReason = rejectionReason,
                RejectionCount = rejectionCount,
                RejectionCategory = rejectionCategory,
                SeverityLevel = severityLevel,
                RejectionNotes = rejectionNotes,
                AffectedProducts = affectedProducts,
                ImmediateAction = immediateAction,
                CorrectiveAction = correctiveAction,
                CorrectiveOwner = correctiveOwner,
                CorrectiveDueDate = correctiveDueDate,
                GmDecision = gmDecision,
                GmDecisionBy = gmDecisionBy,
                GmDecisionNotes = gmDecisionNotes,
                MicroRejectionReason = microRejectionReason,
                MicroRejectedBy = microRejectedBy,
                MicroRejectionAction = microRejectionAction,
                Now = DateTime.UtcNow.ToString("o"),
                Id = id
            });

            return Result<bool>.Ok(true, "تم تحديث الحالة بنجاح.");
        }

        public async Task<Result<bool>> SaveMicroResultsAsync(int id, JsonElement body)
        {
            using var conn = _db.CreateConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();

            try
            {
                string? enteredBy = body.GetStringOrNull("enteredBy", "EnteredBy");
                string now = DateTime.UtcNow.ToString("o");

                await conn.ExecuteAsync(@"
                    UPDATE WaterQualityLogs SET 
                        micro_results_pending = 0,
                        micro_entered_by = @EnteredBy,
                        micro_entered_at = @Now,
                        approval_status = 'Pending_Micro_QM'
                    WHERE id = @Id", new { Id = id, EnteredBy = enteredBy, Now = now }, trans);

                if (body.TryGetProperty("results", out var resArr) && resArr.ValueKind == JsonValueKind.Array)
                {
                    foreach (var res in resArr.EnumerateArray())
                    {
                        int parameterId = res.GetInt("parameter_id", "parameterId");
                        decimal? numRes = res.GetDecimalOrNull("numeric_result", "numericResult");
                        string? textRes = res.GetStringOrNull("text_result", "textResult");
                        bool? isComp = res.GetBoolOrNull("is_compliant", "isCompliant");
                        string? notes = res.GetStringOrNull("notes", "Notes");

                        await conn.ExecuteAsync(@"
                            DELETE FROM WaterQualityResults WHERE log_id = @Id AND parameter_id = @ParameterId;
                            INSERT INTO WaterQualityResults (log_id, parameter_id, numeric_result, text_result, is_compliant, notes)
                            VALUES (@Id, @ParameterId, @NumRes, @TextRes, @IsComp, @Notes)",
                            new { Id = id, ParameterId = parameterId, NumRes = numRes, TextRes = textRes, IsComp = isComp, Notes = notes }, trans);
                    }
                }

                trans.Commit();
                return Result<bool>.Ok(true, "تم حفظ نتائج الفحص الميكروبيولوجي بنجاح.");
            }
            catch (Exception ex)
            {
                trans.Rollback();
                return Result<bool>.Fail($"حدث خطأ أثناء حفظ النتائج: {ex.Message}");
            }
        }

        public async Task<Result<bool>> DeleteLogAsync(int id)
        {
            using var conn = _db.CreateConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();
            try
            {
                await conn.ExecuteAsync("DELETE FROM WaterQualityResults WHERE log_id = @Id", new { Id = id }, trans);
                await conn.ExecuteAsync("DELETE FROM WaterQualityLogs WHERE id = @Id", new { Id = id }, trans);
                trans.Commit();
                return Result<bool>.Ok(true, "تم حذف التقرير بنجاح.");
            }
            catch (Exception ex)
            {
                trans.Rollback();
                return Result<bool>.Fail($"حدث خطأ أثناء الحذف: {ex.Message}");
            }
        }

        public async Task<IEnumerable<dynamic>> GetDailyWaterReportsAsync()
        {
            using var conn = _db.CreateConnection();
            return await conn.QueryAsync<dynamic>("SELECT * FROM daily_water_reports ORDER BY id DESC");
        }

        public async Task<Result<int>> SaveDailyWaterReportAsync(JsonElement reportData)
        {
            using var conn = _db.CreateConnection();
            string date = reportData.GetString("date", null, null, DateTime.UtcNow.ToString("yyyy-MM-dd"));
            string day = reportData.GetString("day", null, null, DateTime.UtcNow.DayOfWeek.ToString());

            var id = await conn.QuerySingleAsync<int>(@"
                INSERT INTO daily_water_reports (date, day)
                VALUES (@Date, @Day);
                SELECT CAST(SCOPE_IDENTITY() as int);",
                new { Date = date, Day = day });

            return Result<int>.Ok(id, "تم حفظ التقرير اليومي بنجاح.");
        }
    }
}
