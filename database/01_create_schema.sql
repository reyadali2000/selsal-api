-- ==============================================================================
-- مشروع: نظام إدارة الجودة وسلامة الغذاء - مياه وادي العين
-- سكربت: 01_create_schema.sql
-- الهدف: إنشاء قاعدة البيانات والجداول والعلاقات والفهارس على Microsoft SQL Server
-- التوافق: SQL Server 2016 / 2019 / 2022 / Azure SQL
-- الترميز: Arabic_100_CI_AS لدعم كامل ودقيق للغة العربية والبحث
-- ==============================================================================

-- 1. إنشاء قاعدة البيانات
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'WadiAlAin_QualityDB')
BEGIN
    CREATE DATABASE [WadiAlAin_QualityDB]
    COLLATE Arabic_100_CI_AS;
    PRINT '>> Database [WadiAlAin_QualityDB] created successfully.';
END
ELSE
BEGIN
    PRINT '>> Database [WadiAlAin_QualityDB] already exists.';
END
GO

USE [WadiAlAin_QualityDB];
GO

-- ==============================================================================
-- القسم الأول: الهيكل الإداري والمستخدمين والفروع (Administrative & Master Data)
-- ==============================================================================

-- 1. جدول الفروع والمصانع (Branches)
IF OBJECT_ID(N'dbo.Branches', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Branches (
        id INT IDENTITY(1,1) PRIMARY KEY,
        name NVARCHAR(200) NOT NULL,
        code NVARCHAR(50) UNIQUE NULL,
        location NVARCHAR(300) NULL,
        is_active BIT NOT NULL DEFAULT 1
    );
    PRINT 'Created table: dbo.Branches';
END
GO

-- 2. جدول الأقسام الإدارية (Departments)
IF OBJECT_ID(N'dbo.departments', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.departments (
        id INT IDENTITY(1,1) PRIMARY KEY,
        name NVARCHAR(200) NOT NULL,
        code NVARCHAR(50) UNIQUE NULL,
        description NVARCHAR(500) NULL,
        parent_id INT NULL FOREIGN KEY REFERENCES dbo.departments(id),
        branch_id INT NULL FOREIGN KEY REFERENCES dbo.Branches(id),
        is_active BIT NOT NULL DEFAULT 1,
        sort_order INT NOT NULL DEFAULT 0,
        created_at NVARCHAR(50) NULL,
        updated_at NVARCHAR(50) NULL
    );
    PRINT 'Created table: dbo.departments';
END
GO

-- 3. جدول المناصب والوظائف (Positions)
IF OBJECT_ID(N'dbo.positions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.positions (
        id INT IDENTITY(1,1) PRIMARY KEY,
        name NVARCHAR(200) NOT NULL,
        code NVARCHAR(50) UNIQUE NULL,
        department_id INT NULL FOREIGN KEY REFERENCES dbo.departments(id),
        level INT NOT NULL DEFAULT 0,
        description NVARCHAR(500) NULL,
        is_active BIT NOT NULL DEFAULT 1,
        created_at NVARCHAR(50) NULL,
        updated_at NVARCHAR(50) NULL
    );
    PRINT 'Created table: dbo.positions';
END
GO

-- 4. جدول الأدوار والصلاحيات المخصصة (CustomRoles)
IF OBJECT_ID(N'dbo.CustomRoles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CustomRoles (
        id INT IDENTITY(1,1) PRIMARY KEY,
        code NVARCHAR(50) UNIQUE NOT NULL,
        title NVARCHAR(200) NOT NULL,
        description NVARCHAR(500) NULL,
        color BIGINT NULL,
        icon_name NVARCHAR(100) NULL,
        permissions_json NVARCHAR(MAX) NOT NULL,
        is_system BIT NOT NULL DEFAULT 0,
        created_at NVARCHAR(50) NULL
    );
    PRINT 'Created table: dbo.CustomRoles';
END
GO

-- 5. جدول المستخدمين (Users)
IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users (
        id INT IDENTITY(1,1) PRIMARY KEY,
        branch_id INT NULL FOREIGN KEY REFERENCES dbo.Branches(id),
        full_name NVARCHAR(200) NOT NULL,
        username NVARCHAR(100) UNIQUE NOT NULL,
        role NVARCHAR(50) NULL,
        email NVARCHAR(200) NULL,
        password_hash NVARCHAR(255) NULL,
        phone NVARCHAR(50) NULL,
        job_title NVARCHAR(200) NULL,
        is_active BIT NOT NULL DEFAULT 1,
        avatar_color BIGINT NULL,
        created_at NVARCHAR(50) NULL,
        last_login NVARCHAR(50) NULL
    );
    PRINT 'Created table: dbo.Users';
END
GO

-- 6. جدول التوقيعات الرقمية للمستخدمين (user_signatures)
IF OBJECT_ID(N'dbo.user_signatures', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.user_signatures (
        id INT IDENTITY(1,1) PRIMARY KEY,
        user_id INT NOT NULL FOREIGN KEY REFERENCES dbo.Users(id) ON DELETE CASCADE,
        signature_type NVARCHAR(50) NOT NULL DEFAULT 'drawn_png',
        signature_data NVARCHAR(MAX) NOT NULL,
        signature_hash NVARCHAR(255) NULL,
        is_active BIT NOT NULL DEFAULT 1,
        created_at NVARCHAR(50) NULL,
        updated_at NVARCHAR(50) NULL
    );
    CREATE INDEX idx_user_signatures_user ON dbo.user_signatures(user_id, is_active);
    PRINT 'Created table: dbo.user_signatures';
END
GO

-- 7. جدول الورديات التشغيلية (Shifts)
IF OBJECT_ID(N'dbo.Shifts', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Shifts (
        id INT IDENTITY(1,1) PRIMARY KEY,
        branch_id INT NOT NULL FOREIGN KEY REFERENCES dbo.Branches(id),
        name NVARCHAR(200) NOT NULL,
        start_time NVARCHAR(20) NOT NULL,
        end_time NVARCHAR(20) NOT NULL,
        season_name NVARCHAR(100) NOT NULL DEFAULT N'التوقيت الصيفي',
        is_active BIT NOT NULL DEFAULT 1
    );
    PRINT 'Created table: dbo.Shifts';
END
GO

-- ==============================================================================
-- القسم الثاني: إدارة المنتجات ومعايير الجودة (Products & Standards)
-- ==============================================================================

-- 8. جدول المنتجات والعبوات (Products)
IF OBJECT_ID(N'dbo.Products', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Products (
        id INT IDENTITY(1,1) PRIMARY KEY,
        branch_id INT NULL FOREIGN KEY REFERENCES dbo.Branches(id),
        name NVARCHAR(200) NOT NULL,
        volume_liters DECIMAL(10,3) NULL,
        is_active BIT NOT NULL DEFAULT 1
    );
    PRINT 'Created table: dbo.Products';
END
GO

-- 9. جدول معايير الفحص وعناصره (TestParameters)
IF OBJECT_ID(N'dbo.TestParameters', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TestParameters (
        id INT IDENTITY(1,1) PRIMARY KEY,
        category NVARCHAR(100) NULL,
        name_ar NVARCHAR(200) NOT NULL,
        unit NVARCHAR(50) NULL,
        input_type NVARCHAR(50) NULL
    );
    PRINT 'Created table: dbo.TestParameters';
END
GO

-- 10. جدول المعايير والحدود القياسية للجودة (QualityStandards)
IF OBJECT_ID(N'dbo.QualityStandards', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.QualityStandards (
        id INT IDENTITY(1,1) PRIMARY KEY,
        product_id INT NULL FOREIGN KEY REFERENCES dbo.Products(id),
        water_type NVARCHAR(50) NULL,
        parameter_id INT NULL FOREIGN KEY REFERENCES dbo.TestParameters(id),
        min_value DECIMAL(18,4) NULL,
        max_value DECIMAL(18,4) NULL,
        expected_text NVARCHAR(255) NULL
    );
    PRINT 'Created table: dbo.QualityStandards';
END
GO

-- ==============================================================================
-- القسم الثالث: سجلات فحص جودة المياه (Water Quality Logs & Results)
-- ==============================================================================

-- 11. سجل فحص المياه الرئيسي (WaterQualityLogs)
IF OBJECT_ID(N'dbo.WaterQualityLogs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.WaterQualityLogs (
        id INT IDENTITY(1,1) PRIMARY KEY,
        branch_id INT NOT NULL DEFAULT 1 FOREIGN KEY REFERENCES dbo.Branches(id),
        branch_name NVARCHAR(200) NOT NULL DEFAULT N'مصنع أرتل',
        shift_id INT NULL FOREIGN KEY REFERENCES dbo.Shifts(id),
        shift_name NVARCHAR(200) NULL,
        created_by INT NULL FOREIGN KEY REFERENCES dbo.Users(id),
        report_category NVARCHAR(100) NULL,
        water_type NVARCHAR(100) NULL,
        batch_number NVARCHAR(100) NULL,
        is_compliant BIT NULL,
        general_notes NVARCHAR(MAX) NULL,
        created_at NVARCHAR(50) NULL,
        approval_status NVARCHAR(50) NULL,
        approved_by INT NULL FOREIGN KEY REFERENCES dbo.Users(id),
        approved_at NVARCHAR(50) NULL,
        rejection_reason NVARCHAR(MAX) NULL,
        water_source NVARCHAR(200) NULL,
        well_number NVARCHAR(100) NULL,
        sample_taker NVARCHAR(200) NULL,
        manager_notified BIT NULL,
        source_suspended BIT NULL,
        ncr_number NVARCHAR(100) NULL,
        qm_approved_by NVARCHAR(200) NULL,
        qm_approved_at NVARCHAR(50) NULL,
        gm_approved_by NVARCHAR(200) NULL,
        gm_approved_at NVARCHAR(50) NULL,
        rejected_by NVARCHAR(200) NULL,
        rejected_at NVARCHAR(100) NULL,
        product_name NVARCHAR(200) NULL,
        bottle_size NVARCHAR(100) NULL,
        production_date NVARCHAR(50) NULL,
        sampling_time NVARCHAR(50) NULL,
        sampling_location NVARCHAR(200) NULL,
        produced_quantity NVARCHAR(100) NULL,
        is_isolated BIT NULL,
        is_hold_issued BIT NULL,
        is_reject_issued BIT NULL,
        micro_results_pending BIT NOT NULL DEFAULT 0,
        micro_entered_by NVARCHAR(200) NULL,
        micro_entered_at NVARCHAR(50) NULL,
        stage1_qm_approved_by NVARCHAR(200) NULL,
        stage1_qm_approved_at NVARCHAR(50) NULL,
        stage1_gm_approved_by NVARCHAR(200) NULL,
        stage1_gm_approved_at NVARCHAR(50) NULL,
        micro_qm_approved_by NVARCHAR(200) NULL,
        micro_qm_approved_at NVARCHAR(50) NULL,
        micro_gm_approved_by NVARCHAR(200) NULL,
        micro_gm_approved_at NVARCHAR(50) NULL,
        rejection_category NVARCHAR(100) NULL,
        severity_level NVARCHAR(100) NULL,
        rejection_notes NVARCHAR(MAX) NULL,
        affected_products NVARCHAR(MAX) NULL,
        immediate_action NVARCHAR(MAX) NULL,
        corrective_action NVARCHAR(MAX) NULL,
        corrective_owner NVARCHAR(200) NULL,
        corrective_due_date NVARCHAR(50) NULL,
        gm_decision NVARCHAR(100) NULL,
        gm_decision_by NVARCHAR(200) NULL,
        gm_decision_at NVARCHAR(50) NULL,
        gm_decision_notes NVARCHAR(MAX) NULL,
        rejection_count INT NOT NULL DEFAULT 0
    );
    CREATE INDEX idx_water_logs_branch_date ON dbo.WaterQualityLogs(branch_id, created_at);
    PRINT 'Created table: dbo.WaterQualityLogs';
END
GO

-- 12. تفاصيل نتائج فحص المياه لكل عنصر (WaterQualityResults)
IF OBJECT_ID(N'dbo.WaterQualityResults', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.WaterQualityResults (
        id INT IDENTITY(1,1) PRIMARY KEY,
        log_id INT NOT NULL FOREIGN KEY REFERENCES dbo.WaterQualityLogs(id) ON DELETE CASCADE,
        parameter_id INT NULL FOREIGN KEY REFERENCES dbo.TestParameters(id),
        numeric_result DECIMAL(18,4) NULL,
        text_result NVARCHAR(255) NULL,
        is_compliant BIT NULL,
        notes NVARCHAR(MAX) NULL
    );
    CREATE INDEX idx_water_results_log ON dbo.WaterQualityResults(log_id);
    PRINT 'Created table: dbo.WaterQualityResults';
END
GO

-- 13. تقارير جودة المياه اليومية الملخصة (daily_water_reports)
IF OBJECT_ID(N'dbo.daily_water_reports', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.daily_water_reports (
        id INT IDENTITY(1,1) PRIMARY KEY,
        date NVARCHAR(50) NULL,
        day NVARCHAR(50) NULL
    );
    PRINT 'Created table: dbo.daily_water_reports';
END
GO

-- 14. بنود التقرير اليومي للمياه (daily_water_report_items)
IF OBJECT_ID(N'dbo.daily_water_report_items', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.daily_water_report_items (
        id INT IDENTITY(1,1) PRIMARY KEY,
        report_id INT NOT NULL FOREIGN KEY REFERENCES dbo.daily_water_reports(id) ON DELETE CASCADE,
        parameter_name NVARCHAR(200) NULL,
        raw_shift NVARCHAR(100) NULL,
        raw_standard_limits NVARCHAR(200) NULL,
        raw_remark NVARCHAR(255) NULL,
        end_shift NVARCHAR(100) NULL,
        end_standard_limits NVARCHAR(200) NULL,
        end_remark NVARCHAR(255) NULL
    );
    PRINT 'Created table: dbo.daily_water_report_items';
END
GO

-- ==============================================================================
-- القسم الرابع: النظافة الشخصية والعمالة والمواد الأولية
-- ==============================================================================

-- 15. سجل العمالة اليومية الدائم (DailyWorkers)
IF OBJECT_ID(N'dbo.DailyWorkers', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DailyWorkers (
        id INT IDENTITY(1,1) PRIMARY KEY,
        branch_id INT NOT NULL DEFAULT 1 FOREIGN KEY REFERENCES dbo.Branches(id),
        full_name NVARCHAR(200) NOT NULL,
        name_key NVARCHAR(200) NOT NULL,
        job_title NVARCHAR(200) NULL,
        is_active BIT NOT NULL DEFAULT 1,
        created_at NVARCHAR(50) NULL,
        CONSTRAINT UQ_DailyWorkers_branch_name UNIQUE(branch_id, name_key)
    );
    CREATE INDEX idx_daily_workers_name_key ON dbo.DailyWorkers(name_key);
    CREATE INDEX idx_daily_workers_branch ON dbo.DailyWorkers(branch_id);
    PRINT 'Created table: dbo.DailyWorkers';
END
GO

-- 16. جدول الموظفين (Employees)
IF OBJECT_ID(N'dbo.Employees', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Employees (
        id INT IDENTITY(1,1) PRIMARY KEY,
        branch_id INT NULL FOREIGN KEY REFERENCES dbo.Branches(id),
        full_name NVARCHAR(200) NOT NULL,
        job_title NVARCHAR(200) NULL,
        is_active BIT NOT NULL DEFAULT 1
    );
    PRINT 'Created table: dbo.Employees';
END
GO

-- 17. قوائم التحقق والتفتيش (Checklists)
IF OBJECT_ID(N'dbo.Checklists', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Checklists (
        id INT IDENTITY(1,1) PRIMARY KEY,
        title NVARCHAR(200) NOT NULL,
        type NVARCHAR(100) NULL
    );
    PRINT 'Created table: dbo.Checklists';
END
GO

-- 18. بنود قوائم التحقق (ChecklistItems)
IF OBJECT_ID(N'dbo.ChecklistItems', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ChecklistItems (
        id INT IDENTITY(1,1) PRIMARY KEY,
        checklist_id INT NOT NULL FOREIGN KEY REFERENCES dbo.Checklists(id) ON DELETE CASCADE,
        item_name NVARCHAR(255) NOT NULL
    );
    PRINT 'Created table: dbo.ChecklistItems';
END
GO

-- 19. تقارير النظافة اليومية (HygieneReports)
IF OBJECT_ID(N'dbo.HygieneReports', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.HygieneReports (
        id INT IDENTITY(1,1) PRIMARY KEY,
        created_at NVARCHAR(50) NOT NULL,
        supervisor_name NVARCHAR(200) NULL,
        employee_count INT NOT NULL DEFAULT 0,
        approval_status NVARCHAR(50) NOT NULL DEFAULT 'Pending_QM',
        rejection_reason NVARCHAR(MAX) NULL,
        rejected_by NVARCHAR(200) NULL,
        rejected_at NVARCHAR(100) NULL,
        qm_approved_by NVARCHAR(200) NULL,
        qm_approved_at NVARCHAR(50) NULL,
        gm_approved_by NVARCHAR(200) NULL,
        gm_approved_at NVARCHAR(50) NULL,
        branch_id INT NULL,
        branch_name NVARCHAR(200) NULL,
        shift_id INT NOT NULL DEFAULT 1,
        shift_name NVARCHAR(200) NOT NULL DEFAULT N'وردية أولى (صباحية)',
        rejection_category NVARCHAR(100) NULL,
        severity_level NVARCHAR(100) NULL,
        rejection_notes NVARCHAR(MAX) NULL,
        affected_products NVARCHAR(MAX) NULL,
        immediate_action NVARCHAR(MAX) NULL,
        corrective_action NVARCHAR(MAX) NULL,
        corrective_owner NVARCHAR(200) NULL,
        corrective_due_date NVARCHAR(50) NULL,
        gm_decision NVARCHAR(100) NULL,
        gm_decision_by NVARCHAR(200) NULL,
        gm_decision_at NVARCHAR(50) NULL,
        gm_decision_notes NVARCHAR(MAX) NULL,
        rejection_count INT NOT NULL DEFAULT 0
    );
    PRINT 'Created table: dbo.HygieneReports';
END
GO

-- 20. سجلات فحص النظافة للعمال (HygieneLogs)
IF OBJECT_ID(N'dbo.HygieneLogs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.HygieneLogs (
        id INT IDENTITY(1,1) PRIMARY KEY,
        report_id INT NULL FOREIGN KEY REFERENCES dbo.HygieneReports(id),
        branch_id INT NULL FOREIGN KEY REFERENCES dbo.Branches(id),
        shift_id INT NOT NULL DEFAULT 1 FOREIGN KEY REFERENCES dbo.Shifts(id),
        shift_name NVARCHAR(200) NOT NULL DEFAULT N'وردية أولى (صباحية)',
        checklist_id INT NULL FOREIGN KEY REFERENCES dbo.Checklists(id),
        employee_id INT NULL FOREIGN KEY REFERENCES dbo.Employees(id),
        created_by INT NULL FOREIGN KEY REFERENCES dbo.Users(id),
        created_at NVARCHAR(50) NULL,
        approval_status NVARCHAR(50) NULL,
        approved_by INT NULL FOREIGN KEY REFERENCES dbo.Users(id),
        approved_at NVARCHAR(50) NULL,
        rejection_reason NVARCHAR(MAX) NULL
    );
    PRINT 'Created table: dbo.HygieneLogs';
END
GO

-- 21. تفاصيل فحص كل بند نظافة لكل عامل (HygieneLogDetails)
IF OBJECT_ID(N'dbo.HygieneLogDetails', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.HygieneLogDetails (
        id INT IDENTITY(1,1) PRIMARY KEY,
        hygiene_log_id INT NOT NULL FOREIGN KEY REFERENCES dbo.HygieneLogs(id) ON DELETE CASCADE,
        checklist_item_id INT NULL FOREIGN KEY REFERENCES dbo.ChecklistItems(id),
        is_compliant BIT NULL,
        photo_url NVARCHAR(MAX) NULL,
        notes NVARCHAR(MAX) NULL
    );
    PRINT 'Created table: dbo.HygieneLogDetails';
END
GO

-- 22. تقارير فحص المواد الأولية ومواد التعبئة (RawMaterialReports)
IF OBJECT_ID(N'dbo.RawMaterialReports', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RawMaterialReports (
        id INT IDENTITY(1,1) PRIMARY KEY,
        category_key NVARCHAR(100) NOT NULL,
        annex_code NVARCHAR(50) NOT NULL,
        annex_title NVARCHAR(255) NOT NULL,
        created_at NVARCHAR(50) NOT NULL,
        date_str NVARCHAR(50) NOT NULL,
        time_str NVARCHAR(50) NOT NULL,
        supplier_name NVARCHAR(200) NOT NULL,
        po_number NVARCHAR(100) NOT NULL,
        material_name NVARCHAR(200) NOT NULL,
        receipt_number NVARCHAR(100) NOT NULL,
        received_quantity NVARCHAR(100) NOT NULL,
        inspected_quantity NVARCHAR(100) NOT NULL,
        inspector_name NVARCHAR(200) NOT NULL,
        batch_number NVARCHAR(100) NOT NULL,
        quality_decision NVARCHAR(100) NOT NULL,
        action_hold BIT NOT NULL DEFAULT 0,
        action_reject BIT NOT NULL DEFAULT 0,
        action_return_to_supplier BIT NOT NULL DEFAULT 0,
        action_ncr BIT NOT NULL DEFAULT 0,
        ncr_number NVARCHAR(100) NULL,
        general_notes NVARCHAR(MAX) NULL,
        annex_notes NVARCHAR(MAX) NULL,
        approval_status NVARCHAR(50) NOT NULL DEFAULT 'Pending_QM',
        qm_approved_by NVARCHAR(200) NULL,
        qm_approved_at NVARCHAR(50) NULL,
        gm_approved_by NVARCHAR(200) NULL,
        gm_approved_at NVARCHAR(50) NULL,
        lab_responsible_name NVARCHAR(200) NULL,
        lab_responsible_at NVARCHAR(50) NULL,
        rejected_by NVARCHAR(200) NULL,
        rejected_at NVARCHAR(100) NULL,
        rejection_reason NVARCHAR(MAX) NULL,
        rejection_category NVARCHAR(100) NULL,
        severity_level NVARCHAR(100) NULL,
        rejection_notes NVARCHAR(MAX) NULL,
        affected_products NVARCHAR(MAX) NULL,
        immediate_action NVARCHAR(MAX) NULL,
        corrective_action NVARCHAR(MAX) NULL,
        corrective_owner NVARCHAR(200) NULL,
        corrective_due_date NVARCHAR(50) NULL,
        gm_decision NVARCHAR(100) NULL,
        gm_decision_by NVARCHAR(200) NULL,
        gm_decision_at NVARCHAR(50) NULL,
        gm_decision_notes NVARCHAR(MAX) NULL,
        accepted_quantity NVARCHAR(100) NULL,
        rejected_quantity NVARCHAR(100) NULL,
        branch_id INT NULL,
        branch_name NVARCHAR(200) NULL,
        rejection_count INT NOT NULL DEFAULT 0
    );
    PRINT 'Created table: dbo.RawMaterialReports';
END
GO

-- 23. نتائج الفحص العام للمواد الأولية (RawMaterialGeneralResults)
IF OBJECT_ID(N'dbo.RawMaterialGeneralResults', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RawMaterialGeneralResults (
        id INT IDENTITY(1,1) PRIMARY KEY,
        report_id INT NOT NULL FOREIGN KEY REFERENCES dbo.RawMaterialReports(id) ON DELETE CASCADE,
        item_id INT NOT NULL,
        item_name NVARCHAR(255) NOT NULL,
        acceptance_criteria NVARCHAR(MAX) NOT NULL,
        is_compliant BIT NULL,
        result_value NVARCHAR(MAX) NULL,
        notes NVARCHAR(MAX) NULL
    );
    PRINT 'Created table: dbo.RawMaterialGeneralResults';
END
GO

-- 24. نتائج الفحص التخصصي بالملحق للمواد الأولية (RawMaterialAnnexResults)
IF OBJECT_ID(N'dbo.RawMaterialAnnexResults', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RawMaterialAnnexResults (
        id INT IDENTITY(1,1) PRIMARY KEY,
        report_id INT NOT NULL FOREIGN KEY REFERENCES dbo.RawMaterialReports(id) ON DELETE CASCADE,
        item_id INT NOT NULL,
        item_name NVARCHAR(255) NOT NULL,
        acceptance_criteria NVARCHAR(MAX) NOT NULL,
        is_compliant BIT NULL,
        result_value NVARCHAR(MAX) NULL,
        notes NVARCHAR(MAX) NULL
    );
    PRINT 'Created table: dbo.RawMaterialAnnexResults';
END
GO

-- ==============================================================================
-- القسم الخامس: محرك النماذج والتقارير الديناميكية (Dynamic Forms Engine)
-- ==============================================================================

-- 25. تصنيفات التقارير الديناميكية (dr_categories)
IF OBJECT_ID(N'dbo.dr_categories', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.dr_categories (
        id INT IDENTITY(1,1) PRIMARY KEY,
        name NVARCHAR(200) NOT NULL,
        description NVARCHAR(500) NULL,
        icon NVARCHAR(100) NULL,
        sort_order INT NOT NULL DEFAULT 0,
        is_active BIT NOT NULL DEFAULT 1,
        created_at NVARCHAR(50) NULL,
        updated_at NVARCHAR(50) NULL
    );
    PRINT 'Created table: dbo.dr_categories';
END
GO

-- 26. قوالب التقارير الديناميكية (dr_reports)
IF OBJECT_ID(N'dbo.dr_reports', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.dr_reports (
        id INT IDENTITY(1,1) PRIMARY KEY,
        category_id INT NULL FOREIGN KEY REFERENCES dbo.dr_categories(id) ON DELETE SET NULL,
        name NVARCHAR(200) NOT NULL,
        description NVARCHAR(500) NULL,
        icon NVARCHAR(100) NULL,
        icon_name NVARCHAR(100) NULL,
        sort_order INT NOT NULL DEFAULT 0,
        is_active BIT NOT NULL DEFAULT 1,
        approval_type NVARCHAR(50) NOT NULL DEFAULT 'two_level',
        orientation NVARCHAR(50) NOT NULL DEFAULT 'portrait',
        doc_number NVARCHAR(100) NULL,
        version_number NVARCHAR(50) NULL,
        issue_date NVARCHAR(50) NULL,
        primary_department_id INT NULL,
        workflow_id INT NULL,
        current_version_id INT NULL,
        code NVARCHAR(100) NULL,
        created_at NVARCHAR(50) NULL,
        updated_at NVARCHAR(50) NULL
    );
    PRINT 'Created table: dbo.dr_reports';
END
GO

-- 27. إصدارات النماذج المؤرخة (dr_report_versions)
IF OBJECT_ID(N'dbo.dr_report_versions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.dr_report_versions (
        id INT IDENTITY(1,1) PRIMARY KEY,
        report_id INT NOT NULL FOREIGN KEY REFERENCES dbo.dr_reports(id) ON DELETE CASCADE,
        version_number INT NOT NULL DEFAULT 1,
        version_label NVARCHAR(100) NULL,
        schema_snapshot_json NVARCHAR(MAX) NULL,
        signature_layout_json NVARCHAR(MAX) NULL,
        published_at NVARCHAR(50) NULL,
        published_by NVARCHAR(200) NULL,
        change_notes NVARCHAR(MAX) NULL,
        is_current BIT NOT NULL DEFAULT 1,
        created_at NVARCHAR(50) NULL
    );
    PRINT 'Created table: dbo.dr_report_versions';
END
GO

-- 28. أقسام النموذج الديناميكي (dr_sections)
IF OBJECT_ID(N'dbo.dr_sections', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.dr_sections (
        id INT IDENTITY(1,1) PRIMARY KEY,
        report_id INT NOT NULL FOREIGN KEY REFERENCES dbo.dr_reports(id) ON DELETE CASCADE,
        report_version_id INT NULL,
        title NVARCHAR(255) NOT NULL,
        description NVARCHAR(500) NULL,
        sort_order INT NOT NULL DEFAULT 0,
        is_collapsible BIT NOT NULL DEFAULT 1,
        visibility_json NVARCHAR(MAX) NULL,
        created_at NVARCHAR(50) NULL,
        updated_at NVARCHAR(50) NULL
    );
    PRINT 'Created table: dbo.dr_sections';
END
GO

-- 29. حقول النموذج الديناميكي (dr_fields)
IF OBJECT_ID(N'dbo.dr_fields', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.dr_fields (
        id INT IDENTITY(1,1) PRIMARY KEY,
        report_id INT NOT NULL,
        section_id INT NOT NULL,
        report_version_id INT NULL,
        field_key NVARCHAR(100) NOT NULL,
        label NVARCHAR(255) NOT NULL,
        hint NVARCHAR(255) NULL,
        field_type NVARCHAR(50) NOT NULL,
        is_required BIT NOT NULL DEFAULT 0,
        sort_order INT NOT NULL DEFAULT 0,
        default_value NVARCHAR(MAX) NULL,
        options_json NVARCHAR(MAX) NULL,
        validation_json NVARCHAR(MAX) NULL,
        min_value DECIMAL(18,4) NULL,
        max_value DECIMAL(18,4) NULL,
        unit NVARCHAR(50) NULL,
        is_visible BIT NOT NULL DEFAULT 1,
        parameter_id INT NULL,
        standard_id INT NULL,
        standard_snapshot_json NVARCHAR(MAX) NULL,
        visibility_json NVARCHAR(MAX) NULL,
        calculation_json NVARCHAR(MAX) NULL,
        created_at NVARCHAR(50) NULL,
        updated_at NVARCHAR(50) NULL,
        CONSTRAINT FK_dr_fields_report FOREIGN KEY (report_id) REFERENCES dbo.dr_reports(id) ON DELETE CASCADE,
        CONSTRAINT FK_dr_fields_section FOREIGN KEY (section_id) REFERENCES dbo.dr_sections(id) ON DELETE NO ACTION
    );
    PRINT 'Created table: dbo.dr_fields';
END
GO

-- 30. سجلات النماذج الديناميكية المعبأة (dr_records)
IF OBJECT_ID(N'dbo.dr_records', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.dr_records (
        id INT IDENTITY(1,1) PRIMARY KEY,
        report_id INT NOT NULL FOREIGN KEY REFERENCES dbo.dr_reports(id) ON DELETE CASCADE,
        branch_id INT NULL,
        branch_name NVARCHAR(200) NULL,
        shift_id INT NULL,
        shift_name NVARCHAR(200) NULL,
        created_by NVARCHAR(200) NULL,
        created_at NVARCHAR(50) NULL,
        updated_at NVARCHAR(50) NULL,
        status NVARCHAR(50) NULL,
        approval_status NVARCHAR(50) NOT NULL DEFAULT 'Pending_QM',
        qm_approved_by NVARCHAR(200) NULL,
        qm_approved_at NVARCHAR(50) NULL,
        gm_approved_by NVARCHAR(200) NULL,
        gm_approved_at NVARCHAR(50) NULL,
        rejection_reason NVARCHAR(MAX) NULL,
        rejected_by NVARCHAR(200) NULL,
        rejected_at NVARCHAR(50) NULL,
        rejection_category NVARCHAR(100) NULL,
        severity_level NVARCHAR(100) NULL,
        rejection_notes NVARCHAR(MAX) NULL,
        affected_products NVARCHAR(MAX) NULL,
        immediate_action NVARCHAR(MAX) NULL,
        corrective_action NVARCHAR(MAX) NULL,
        corrective_owner NVARCHAR(200) NULL,
        corrective_due_date NVARCHAR(50) NULL,
        gm_decision NVARCHAR(100) NULL,
        gm_decision_by NVARCHAR(200) NULL,
        gm_decision_at NVARCHAR(50) NULL,
        gm_decision_notes NVARCHAR(MAX) NULL,
        client_uuid NVARCHAR(64) NULL,
        is_compliant BIT NULL DEFAULT 1,
        rejection_count INT NOT NULL DEFAULT 0
    );
    CREATE INDEX idx_dr_records_report ON dbo.dr_records(report_id, created_at);
    CREATE UNIQUE INDEX IX_dr_records_client_uuid ON dbo.dr_records(client_uuid) WHERE client_uuid IS NOT NULL;
    PRINT 'Created table: dbo.dr_records';
END
GO

-- 31. قيم حقول النماذج المعبأة (dr_values)
IF OBJECT_ID(N'dbo.dr_values', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.dr_values (
        id INT IDENTITY(1,1) PRIMARY KEY,
        record_id INT NOT NULL FOREIGN KEY REFERENCES dbo.dr_records(id) ON DELETE CASCADE,
        field_id INT NOT NULL FOREIGN KEY REFERENCES dbo.dr_fields(id) ON DELETE NO ACTION,
        value NVARCHAR(MAX) NULL
    );
    CREATE INDEX idx_dr_values_record ON dbo.dr_values(record_id);
    PRINT 'Created table: dbo.dr_values';
END
GO

-- ==============================================================================
-- القسم السادس: مسارات الاعتماد وسجل التدقيق والإشعارات
-- ==============================================================================

-- 32. مسارات الاعتماد المعرفة (report_workflows)
IF OBJECT_ID(N'dbo.report_workflows', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.report_workflows (
        id INT IDENTITY(1,1) PRIMARY KEY,
        name NVARCHAR(200) NOT NULL,
        code NVARCHAR(50) UNIQUE NULL,
        description NVARCHAR(500) NULL,
        is_active BIT NOT NULL DEFAULT 1,
        created_at NVARCHAR(50) NULL,
        updated_at NVARCHAR(50) NULL
    );
    PRINT 'Created table: dbo.report_workflows';
END
GO

-- 33. خطوات مسار الاعتماد (report_workflow_steps)
IF OBJECT_ID(N'dbo.report_workflow_steps', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.report_workflow_steps (
        id INT IDENTITY(1,1) PRIMARY KEY,
        workflow_id INT NOT NULL FOREIGN KEY REFERENCES dbo.report_workflows(id) ON DELETE CASCADE,
        step_order INT NOT NULL,
        step_name NVARCHAR(200) NOT NULL,
        required_role NVARCHAR(50) NULL,
        required_position_id INT NULL FOREIGN KEY REFERENCES dbo.positions(id) ON DELETE SET NULL,
        action_type NVARCHAR(50) NOT NULL DEFAULT 'approve',
        is_optional BIT NOT NULL DEFAULT 0,
        created_at NVARCHAR(50) NULL
    );
    PRINT 'Created table: dbo.report_workflow_steps';
END
GO

-- 34. سجل قرارات الاعتماد والتوقيع المتراكب (report_approvals)
IF OBJECT_ID(N'dbo.report_approvals', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.report_approvals (
        id INT IDENTITY(1,1) PRIMARY KEY,
        record_id INT NOT NULL FOREIGN KEY REFERENCES dbo.dr_records(id) ON DELETE CASCADE,
        step_id INT NULL FOREIGN KEY REFERENCES dbo.report_workflow_steps(id) ON DELETE SET NULL,
        step_name NVARCHAR(200) NULL,
        action NVARCHAR(50) NOT NULL,
        acted_by NVARCHAR(200) NULL,
        acted_by_user_id INT NULL FOREIGN KEY REFERENCES dbo.Users(id) ON DELETE SET NULL,
        acted_at NVARCHAR(50) NULL,
        comments NVARCHAR(MAX) NULL,
        previous_status NVARCHAR(50) NULL,
        new_status NVARCHAR(50) NULL,
        signature_snapshot_blob VARBINARY(MAX) NULL,
        signature_snapshot_path NVARCHAR(MAX) NULL,
        signature_hash NVARCHAR(255) NULL,
        signature_type NVARCHAR(50) NULL,
        action_by_position NVARCHAR(200) NULL,
        document_hash NVARCHAR(255) NULL
    );
    CREATE INDEX idx_report_approvals_record ON dbo.report_approvals(record_id);
    PRINT 'Created table: dbo.report_approvals';
END
GO

-- 35. سجل التدقيق والمراجعة الشامل (audit_logs)
IF OBJECT_ID(N'dbo.audit_logs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.audit_logs (
        id INT IDENTITY(1,1) PRIMARY KEY,
        entity_type NVARCHAR(100) NOT NULL,
        entity_id INT NOT NULL,
        action NVARCHAR(50) NOT NULL,
        user_id INT NULL FOREIGN KEY REFERENCES dbo.Users(id) ON DELETE SET NULL,
        user_name NVARCHAR(200) NULL,
        old_value_json NVARCHAR(MAX) NULL,
        new_value_json NVARCHAR(MAX) NULL,
        ip_address NVARCHAR(100) NULL,
        device_info NVARCHAR(255) NULL,
        created_at NVARCHAR(50) NULL
    );
    CREATE INDEX idx_audit_logs_entity ON dbo.audit_logs(entity_type, entity_id);
    PRINT 'Created table: dbo.audit_logs';
END
GO

-- 36. جدول الإشعارات (Notifications)
IF OBJECT_ID(N'dbo.Notifications', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Notifications (
        id INT IDENTITY(1,1) PRIMARY KEY,
        branch_id INT NULL,
        title NVARCHAR(200) NULL,
        body NVARCHAR(MAX) NULL,
        report_id INT NULL,
        report_category NVARCHAR(100) NULL,
        target_role NVARCHAR(50) NULL,
        is_read BIT NOT NULL DEFAULT 0,
        created_at NVARCHAR(50) NULL
    );
    CREATE INDEX idx_notifications_role ON dbo.Notifications(target_role, is_read);
    PRINT 'Created table: dbo.Notifications';
END
GO

-- 37. الأرشفة الرقمية للتقارير (ArchivedReports)
IF OBJECT_ID(N'dbo.ArchivedReports', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ArchivedReports (
        id INT IDENTITY(1,1) PRIMARY KEY,
        log_id INT NULL,
        log_type NVARCHAR(100) NULL,
        generated_by INT NULL FOREIGN KEY REFERENCES dbo.Users(id),
        approved_by INT NULL FOREIGN KEY REFERENCES dbo.Users(id),
        file_url NVARCHAR(MAX) NULL,
        file_hash NVARCHAR(255) NULL,
        archived_at NVARCHAR(50) NULL
    );
    PRINT 'Created table: dbo.ArchivedReports';
END
GO

PRINT '==============================================================================';
PRINT '>> [SUCCESS] All 37 tables, constraints, and indexes created successfully on SQL Server!';
PRINT '==============================================================================';
