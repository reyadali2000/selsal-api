using Dapper;
using WadiAlAin.Application.Common.Interfaces;
using WadiAlAin.Application.Common.Models;
using WadiAlAin.Application.Interfaces;
using WadiAlAin.Application.Common.Helpers;
using WadiAlAin.Application.DTOs.Common;
using System.Text.Json;

namespace WadiAlAin.Infrastructure.Services
{
    public class RawMaterialsService : IRawMaterialsService
    {
        private readonly IDbConnectionFactory _db;

        public RawMaterialsService(IDbConnectionFactory db)
        {
            _db = db;
        }

        public async Task<IEnumerable<dynamic>> GetReportsAsync(int? branchId)
        {
            using var conn = _db.CreateConnection();
            string sql = "SELECT * FROM RawMaterialReports";
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

        public async Task<Result<dynamic>> GetReportDetailsAsync(int id)
        {
            using var conn = _db.CreateConnection();
            var report = await conn.QuerySingleOrDefaultAsync<dynamic>(
                "SELECT * FROM RawMaterialReports WHERE id = @Id", new { Id = id });

            if (report == null)
                return Result<dynamic>.Fail("لم يتم العثور على التقرير.");

            var general = await conn.QueryAsync<dynamic>(
                "SELECT * FROM RawMaterialGeneralResults WHERE report_id = @ReportId", new { ReportId = id });

            var annex = await conn.QueryAsync<dynamic>(
                "SELECT * FROM RawMaterialAnnexResults WHERE report_id = @ReportId", new { ReportId = id });

            return Result<dynamic>.Ok(new { Report = report, GeneralResults = general, AnnexResults = annex });
        }

        public async Task<Result<int>> SaveReportAsync(System.Text.Json.JsonElement body)
        {
            using var conn = _db.CreateConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();

            try
            {
                System.Text.Json.JsonElement reportElem = body;
                if (body.ValueKind == System.Text.Json.JsonValueKind.Object && body.TryGetProperty("report", out var nestedRep))
                {
                    reportElem = nestedRep;
                }

                int reportId = reportElem.GetInt("id", null, 0);
                string createdAt = reportElem.GetString("createdAt", "created_at", null, DateTime.UtcNow.ToString("o"));
                string supplierName = reportElem.GetString("supplierName", "supplier_name", null, "");
                string poNumber = reportElem.GetString("poNumber", "po_number", null, "");
                string materialName = reportElem.GetString("materialName", "material_name", "material_type", "");
                string receiptNumber = reportElem.GetString("receiptNumber", "receipt_number", "invoice_number", "");
                string receivedQuantity = reportElem.GetString("receivedQuantity", "received_quantity", null, "");
                string inspectedQuantity = reportElem.GetString("inspectedQuantity", "inspected_quantity", "sample_size", "");
                string acceptedQuantity = reportElem.GetString("acceptedQuantity", "accepted_quantity", null, "");
                string rejectedQuantity = reportElem.GetString("rejectedQuantity", "rejected_quantity", null, "");
                string inspectorName = reportElem.GetString("inspectorName", "inspector_name", null, "");
                string batchNumber = reportElem.GetString("batchNumber", "batch_number", "lot_number", "");
                string qualityDecision = reportElem.GetString("qualityDecision", "quality_decision", null, "Accepted");
                bool actionHold = reportElem.GetBool("actionHold", "action_hold", false);
                bool actionReject = reportElem.GetBool("actionReject", "action_reject", false);
                bool actionReturnToSupplier = reportElem.GetBool("actionReturnToSupplier", "action_return_to_supplier", false);
                bool actionNcr = reportElem.GetBool("actionNcr", "action_ncr", false);
                string? ncrNumber = reportElem.GetStringOrNull("ncrNumber", "ncr_number");
                string generalNotes = reportElem.GetString("generalNotes", "general_notes", null, "");
                string annexNotes = reportElem.GetString("annexNotes", "annex_notes", null, "");
                string approvalStatus = reportElem.GetString("approvalStatus", "approval_status", null, "Pending_QM");
                string categoryKey = reportElem.GetString("categoryKey", "category_key", null, "raw_material");
                string annexCode = reportElem.GetString("annexCode", "annex_code", null, "");
                string annexTitle = reportElem.GetString("annexTitle", "annex_title", null, "");
                string dateStr = reportElem.GetString("dateStr", "date_str", "received_date", "");
                string timeStr = reportElem.GetString("timeStr", "time_str", null, "");
                int branchId = reportElem.GetInt("branchId", "branch_id", 1);
                string branchName = reportElem.GetString("branchName", "branch_name", null, "مصنع أرتل");

                string? labResponsibleName = reportElem.GetStringOrNull("labResponsibleName", "lab_responsible_name");
                string? labResponsibleAt = reportElem.GetStringOrNull("labResponsibleAt", "lab_responsible_at");
                string? qmApprovedBy = reportElem.GetStringOrNull("qmApprovedBy", "qm_approved_by");
                string? qmApprovedAt = reportElem.GetStringOrNull("qmApprovedAt", "qm_approved_at");
                string? gmApprovedBy = reportElem.GetStringOrNull("gmApprovedBy", "gm_approved_by");
                string? gmApprovedAt = reportElem.GetStringOrNull("gmApprovedAt", "gm_approved_at");
                string? rejectedBy = reportElem.GetStringOrNull("rejectedBy", "rejected_by");
                string? rejectionReason = reportElem.GetStringOrNull("rejectionReason", "rejection_reason");
                string? rejectionCategory = reportElem.GetStringOrNull("rejectionCategory", "rejection_category");
                string? severityLevel = reportElem.GetStringOrNull("severityLevel", "severity_level");
                string? rejectionNotes = reportElem.GetStringOrNull("rejectionNotes", "rejection_notes");
                string? affectedProducts = reportElem.GetStringOrNull("affectedProducts", "affected_products");
                string? immediateAction = reportElem.GetStringOrNull("immediateAction", "immediate_action");
                string? correctiveAction = reportElem.GetStringOrNull("correctiveAction", "corrective_action");
                string? correctiveOwner = reportElem.GetStringOrNull("correctiveOwner", "corrective_owner");
                string? correctiveDueDate = reportElem.GetStringOrNull("correctiveDueDate", "corrective_due_date");
                string? gmDecision = reportElem.GetStringOrNull("gmDecision", "gm_decision");
                string? gmDecisionBy = reportElem.GetStringOrNull("gmDecisionBy", "gm_decision_by");
                string? gmDecisionAt = reportElem.GetStringOrNull("gmDecisionAt", "gm_decision_at");
                string? gmDecisionNotes = reportElem.GetStringOrNull("gmDecisionNotes", "gm_decision_notes");

                bool existsInDb = reportId > 0 &&
                    (await conn.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM RawMaterialReports WHERE id = @reportId", new { reportId }, trans)) > 0;

                if (existsInDb)
                {
                    await conn.ExecuteAsync(@"
                        UPDATE RawMaterialReports SET
                            category_key = @categoryKey, annex_code = @annexCode, annex_title = @annexTitle,
                            date_str = @dateStr, time_str = @timeStr, supplier_name = @supplierName, po_number = @poNumber,
                            material_name = @materialName, receipt_number = @receiptNumber, received_quantity = @receivedQuantity,
                            inspected_quantity = @inspectedQuantity, accepted_quantity = @acceptedQuantity, rejected_quantity = @rejectedQuantity,
                            inspector_name = @inspectorName, batch_number = @batchNumber, quality_decision = @qualityDecision,
                            action_hold = @actionHold, action_reject = @actionReject, action_return_to_supplier = @actionReturnToSupplier, action_ncr = @actionNcr,
                            ncr_number = @ncrNumber, general_notes = @generalNotes, annex_notes = @annexNotes,
                            approval_status = @approvalStatus, branch_id = @branchId, branch_name = @branchName,
                            lab_responsible_name = COALESCE(@labResponsibleName, lab_responsible_name),
                            lab_responsible_at = COALESCE(@labResponsibleAt, lab_responsible_at),
                            qm_approved_by = COALESCE(@qmApprovedBy, qm_approved_by),
                            qm_approved_at = COALESCE(@qmApprovedAt, qm_approved_at),
                            gm_approved_by = COALESCE(@gmApprovedBy, gm_approved_by),
                            gm_approved_at = COALESCE(@gmApprovedAt, gm_approved_at),
                            rejected_by = COALESCE(@rejectedBy, rejected_by),
                            rejection_reason = COALESCE(@rejectionReason, rejection_reason),
                            rejection_category = COALESCE(@rejectionCategory, rejection_category),
                            severity_level = COALESCE(@severityLevel, severity_level),
                            rejection_notes = COALESCE(@rejectionNotes, rejection_notes),
                            affected_products = COALESCE(@affectedProducts, affected_products),
                            immediate_action = COALESCE(@immediateAction, immediate_action),
                            corrective_action = COALESCE(@correctiveAction, corrective_action),
                            corrective_owner = COALESCE(@correctiveOwner, corrective_owner),
                            corrective_due_date = COALESCE(@correctiveDueDate, corrective_due_date),
                            gm_decision = COALESCE(@gmDecision, gm_decision),
                            gm_decision_by = COALESCE(@gmDecisionBy, gm_decision_by),
                            gm_decision_at = COALESCE(@gmDecisionAt, gm_decision_at),
                            gm_decision_notes = COALESCE(@gmDecisionNotes, gm_decision_notes)
                        WHERE id = @reportId",
                        new {
                            categoryKey, annexCode, annexTitle, dateStr, timeStr, supplierName, poNumber, materialName, receiptNumber,
                            receivedQuantity, inspectedQuantity, acceptedQuantity, rejectedQuantity, inspectorName, batchNumber, qualityDecision,
                            actionHold, actionReject, actionReturnToSupplier, actionNcr, ncrNumber, generalNotes, annexNotes, approvalStatus,
                            branchId, branchName, labResponsibleName, labResponsibleAt, qmApprovedBy, qmApprovedAt, gmApprovedBy, gmApprovedAt,
                            rejectedBy, rejectionReason, rejectionCategory, severityLevel, rejectionNotes, affectedProducts, immediateAction,
                            correctiveAction, correctiveOwner, correctiveDueDate, gmDecision, gmDecisionBy, gmDecisionAt, gmDecisionNotes, reportId
                        }, trans);

                    await conn.ExecuteAsync("DELETE FROM RawMaterialGeneralResults WHERE report_id = @reportId", new { reportId }, trans);
                    await conn.ExecuteAsync("DELETE FROM RawMaterialAnnexResults WHERE report_id = @reportId", new { reportId }, trans);
                }
                else
                {
                    reportId = await conn.QuerySingleAsync<int>(@"
                        INSERT INTO RawMaterialReports (
                            category_key, annex_code, annex_title, created_at, date_str, time_str, supplier_name, po_number,
                            material_name, receipt_number, received_quantity, inspected_quantity, accepted_quantity, rejected_quantity,
                            inspector_name, batch_number, quality_decision, action_hold, action_reject, action_return_to_supplier, action_ncr,
                            ncr_number, general_notes, annex_notes, approval_status, branch_id, branch_name,
                            lab_responsible_name, lab_responsible_at, qm_approved_by, qm_approved_at, gm_approved_by, gm_approved_at,
                            rejected_by, rejection_reason, rejection_category, severity_level, rejection_notes, affected_products,
                            immediate_action, corrective_action, corrective_owner, corrective_due_date,
                            gm_decision, gm_decision_by, gm_decision_at, gm_decision_notes
                        ) VALUES (
                            @categoryKey, @annexCode, @annexTitle, @createdAt, @dateStr, @timeStr, @supplierName, @poNumber,
                            @materialName, @receiptNumber, @receivedQuantity, @inspectedQuantity, @acceptedQuantity, @rejectedQuantity,
                            @inspectorName, @batchNumber, @qualityDecision, @actionHold, @actionReject, @actionReturnToSupplier, @actionNcr,
                            @ncrNumber, @generalNotes, @annexNotes, @approvalStatus, @branchId, @branchName,
                            @labResponsibleName, @labResponsibleAt, @qmApprovedBy, @qmApprovedAt, @gmApprovedBy, @gmApprovedAt,
                            @rejectedBy, @rejectionReason, @rejectionCategory, @severityLevel, @rejectionNotes, @affectedProducts,
                            @immediateAction, @correctiveAction, @correctiveOwner, @correctiveDueDate,
                            @gmDecision, @gmDecisionBy, @gmDecisionAt, @gmDecisionNotes
                        );
                        SELECT CAST(SCOPE_IDENTITY() AS INT);",
                        new {
                            categoryKey, annexCode, annexTitle, createdAt, dateStr, timeStr, supplierName, poNumber, materialName, receiptNumber,
                            receivedQuantity, inspectedQuantity, acceptedQuantity, rejectedQuantity, inspectorName, batchNumber, qualityDecision,
                            actionHold, actionReject, actionReturnToSupplier, actionNcr, ncrNumber, generalNotes, annexNotes, approvalStatus,
                            branchId, branchName, labResponsibleName, labResponsibleAt, qmApprovedBy, qmApprovedAt, gmApprovedBy, gmApprovedAt,
                            rejectedBy, rejectionReason, rejectionCategory, severityLevel, rejectionNotes, affectedProducts, immediateAction,
                            correctiveAction, correctiveOwner, correctiveDueDate, gmDecision, gmDecisionBy, gmDecisionAt, gmDecisionNotes
                        }, trans);
                }

                System.Text.Json.JsonElement itemsElem = default;
                if (body.TryGetProperty("items", out var itArr) && itArr.ValueKind == System.Text.Json.JsonValueKind.Array) itemsElem = itArr;
                else if (reportElem.TryGetProperty("items", out var itArr2) && itArr2.ValueKind == System.Text.Json.JsonValueKind.Array) itemsElem = itArr2;

                if (itemsElem.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    foreach (var item in itemsElem.EnumerateArray())
                    {
                        int itemId = item.GetInt("itemId", "item_id", 0);
                        string itemName = item.GetString("itemName", "item_name", null, "");
                        string acceptanceCriteria = item.GetString("acceptanceCriteria", "acceptance_criteria", null, "");
                        bool isCompliant = item.GetBool("isCompliant", "is_compliant", true);
                        string? resultValue = item.GetStringOrNull("resultValue", "result_value");
                        string? notes = item.GetStringOrNull("notes");
                        string itemType = item.GetString("itemType", "item_type", null, "general");

                        if (itemType.ToLower() == "annex" || itemType.ToLower() == "special")
                        {
                            await conn.ExecuteAsync(@"
                                INSERT INTO RawMaterialAnnexResults (report_id, item_id, item_name, acceptance_criteria, is_compliant, result_value, notes)
                                VALUES (@ReportId, @ItemId, @ItemName, @AcceptanceCriteria, @IsCompliant, @ResultValue, @Notes)",
                                new { ReportId = reportId, ItemId = itemId, ItemName = itemName, AcceptanceCriteria = acceptanceCriteria, IsCompliant = isCompliant ? 1 : 0, ResultValue = resultValue, Notes = notes }, trans);
                        }
                        else
                        {
                            await conn.ExecuteAsync(@"
                                INSERT INTO RawMaterialGeneralResults (report_id, item_id, item_name, acceptance_criteria, is_compliant, result_value, notes)
                                VALUES (@ReportId, @ItemId, @ItemName, @AcceptanceCriteria, @IsCompliant, @ResultValue, @Notes)",
                                new { ReportId = reportId, ItemId = itemId, ItemName = itemName, AcceptanceCriteria = acceptanceCriteria, IsCompliant = isCompliant ? 1 : 0, ResultValue = resultValue, Notes = notes }, trans);
                        }
                    }
                }

                trans.Commit();
                return Result<int>.Ok(reportId, "تم حفظ تقرير المواد الخام بنجاح.");
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

            string sql = "UPDATE RawMaterialReports SET approval_status = @Status";

            if (status == "Pending_QM")
            {
                // اعتماد مشرف الجودة وتحويله لمدير الجودة
                sql += ", lab_responsible_name = COALESCE(@ApprovedBy, lab_responsible_name), lab_responsible_at = @Now";
            }
            else if (status == "Approved_QM" || status == "Pending_GM" || status == "Approved_Stage1_QM" || status == "Approved_Micro_QM" || status == "Pending_Micro_QM")
            {
                // اعتماد مدير الجودة وتحويله لمدير المصنع
                sql += ", qm_approved_by = @ApprovedBy, qm_approved_at = @Now";
            }
            else if (status == "Approved_GM" || status == "Approved" || status == "Approved_Stage1_GM" || status == "Approved_Micro_GM" || status == "Pending_Micro_GM")
            {
                // الاعتماد النهائي من مدير المصنع
                sql += ", gm_approved_by = @ApprovedBy, gm_approved_at = @Now";
            }
            else if (status == "Closed")
            {
                // اعتماد الإجراء التصحيحي وإغلاق التقرير نهائياً من مدير المصنع
                sql += ", gm_approved_by = COALESCE(@ApprovedBy, gm_approved_by), gm_approved_at = @Now, gm_decision = COALESCE(@GmDecision, 'Approved'), gm_decision_by = COALESCE(@GmDecisionBy, @ApprovedBy), gm_decision_at = @Now";
                if (!string.IsNullOrEmpty(dto.GmDecisionNotes) || !string.IsNullOrEmpty(rejectionReason))
                {
                    sql += ", gm_decision_notes = COALESCE(@GmDecisionNotes, @RejectionReason)";
                }
            }
            else if (status == "Rejected_By_GM")
            {
                // رفض الإجراء التصحيحي من قبل مدير المصنع
                sql += ", gm_decision = COALESCE(@GmDecision, 'Rejected'), gm_decision_by = COALESCE(@GmDecisionBy, @ApprovedBy), gm_decision_at = @Now, gm_decision_notes = COALESCE(@GmDecisionNotes, @RejectionReason)";
            }
            else if (status == "Rejected_Final")
            {
                // الرفض النهائي القاطع
                sql += ", gm_decision = COALESCE(@GmDecision, 'Rejected'), gm_decision_by = COALESCE(@GmDecisionBy, @ApprovedBy), gm_decision_at = @Now, gm_decision_notes = COALESCE(@GmDecisionNotes, @RejectionReason)";
                if (!string.IsNullOrEmpty(rejectionReason)) sql += ", rejection_reason = COALESCE(rejection_reason, @RejectionReason)";
                if (!string.IsNullOrEmpty(approvedBy)) sql += ", rejected_by = COALESCE(rejected_by, @ApprovedBy), rejected_at = COALESCE(rejected_at, @Now)";
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

            if (!string.IsNullOrEmpty(dto.GmDecision) && status != "Closed" && status != "Rejected_By_GM" && status != "Rejected_Final")
            {
                sql += ", gm_decision = @GmDecision, gm_decision_by = @GmDecisionBy, gm_decision_at = @Now, gm_decision_notes = @GmDecisionNotes";
            }

            if (dto.RejectionCount.HasValue) sql += ", rejection_count = @RejectionCount";

            sql += " WHERE id = @Id";

            await conn.ExecuteAsync(sql, new
            {
                Status = status,
                ApprovedBy = approvedBy,
                RejectionReason = rejectionReason,
                RejectionCount = dto.RejectionCount,
                RejectionCategory = dto.RejectionCategory,
                SeverityLevel = dto.SeverityLevel,
                RejectionNotes = dto.RejectionNotes,
                AffectedProducts = dto.AffectedProducts,
                ImmediateAction = dto.ImmediateAction,
                CorrectiveAction = dto.CorrectiveAction,
                CorrectiveOwner = dto.CorrectiveOwner,
                CorrectiveDueDate = dto.CorrectiveDueDate,
                GmDecision = dto.GmDecision,
                GmDecisionBy = dto.GmDecisionBy,
                GmDecisionNotes = dto.GmDecisionNotes,
                Now = DateTime.UtcNow.ToString("o"),
                Id = id
            });

            return Result<bool>.Ok(true, "تم تحديث حالة تقرير المواد الأولية وسجل الاعتماد بنجاح.");
        }

        public async Task<Result<bool>> DeleteReportAsync(int id)
        {
            using var conn = _db.CreateConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();
            try
            {
                await conn.ExecuteAsync("DELETE FROM RawMaterialGeneralResults WHERE report_id = @Id", new { Id = id }, trans);
                await conn.ExecuteAsync("DELETE FROM RawMaterialAnnexResults WHERE report_id = @Id", new { Id = id }, trans);
                await conn.ExecuteAsync("DELETE FROM RawMaterialReports WHERE id = @Id", new { Id = id }, trans);
                trans.Commit();
                return Result<bool>.Ok(true, "تم حذف التقرير بنجاح.");
            }
            catch (Exception ex)
            {
                trans.Rollback();
                return Result<bool>.Fail($"حدث خطأ أثناء الحذف: {ex.Message}");
            }
        }
    }
}
