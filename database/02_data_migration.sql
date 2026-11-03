-- ==============================================================================
-- سكريبت ترحيل البيانات من SQLite إلى Microsoft SQL Server
-- تاريخ التوليد: 2026-10-06 00:56:04
-- المصدر: wadialain_quality_v10.db
-- ==============================================================================

USE [WadiAlAin_QualityDB];
GO

-- تعطيل القيود مؤقتاً لضمان سرعة ومرونة إدخال البيانات
EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL';
GO

-- -------------------------------------------------------------
-- إدراج بيانات الجدول: dbo.[Branches] (2 صف)
-- -------------------------------------------------------------
SET IDENTITY_INSERT dbo.[Branches] ON;
GO

INSERT INTO dbo.[Branches] ([id], [name], [code], [location], [is_active])
VALUES
  (1, N'مصنع أرتل (المصنع الرئيسي)', N'plant_1', N'الموقع الرئيسي', 1),
  (2, N'مصنع سنحان (المصنع الثاني)', N'plant_2', N'الموقع الثاني', 1);

SET IDENTITY_INSERT dbo.[Branches] OFF;
GO

-- -------------------------------------------------------------
-- إدراج بيانات الجدول: dbo.[departments] (7 صف)
-- -------------------------------------------------------------
SET IDENTITY_INSERT dbo.[departments] ON;
GO

INSERT INTO dbo.[departments] ([id], [name], [code], [description], [parent_id], [branch_id], [is_active], [sort_order], [created_at], [updated_at])
VALUES
  (1, N'إدارة الجودة ورقابة الجودة', N'QA_QC', N'قسم توكيد ورقابة الجودة', NULL, NULL, 1, 1, N'2026-10-04T23:15:17.249549', N'2026-10-04T23:15:17.249549'),
  (2, N'الإنتاج', N'PROD', N'قسم الإنتاج والتصنيع', NULL, NULL, 1, 2, N'2026-10-04T23:15:17.249549', N'2026-10-04T23:15:17.249549'),
  (3, N'المختبر', N'LAB', N'قسم المختبر والتحاليل', NULL, NULL, 1, 3, N'2026-10-04T23:15:17.249549', N'2026-10-04T23:15:17.249549'),
  (4, N'الصيانة', N'MAINT', N'قسم الصيانة والتشغيل', NULL, NULL, 1, 4, N'2026-10-04T23:15:17.249549', N'2026-10-04T23:15:17.249549'),
  (5, N'المستودعات', N'STORE', N'قسم المستودعات والتخزين', NULL, NULL, 1, 5, N'2026-10-04T23:15:17.249549', N'2026-10-04T23:15:17.249549'),
  (6, N'سلامة الغذاء', N'FSMS', N'قسم سلامة الغذاء والصحة', NULL, NULL, 1, 6, N'2026-10-04T23:15:17.249549', N'2026-10-04T23:15:17.249549'),
  (7, N'الإدارة العليا', N'MGMT', N'الإدارة العامة والتنفيذية', NULL, NULL, 1, 7, N'2026-10-04T23:15:17.249549', N'2026-10-04T23:15:17.249549');

SET IDENTITY_INSERT dbo.[departments] OFF;
GO

-- -------------------------------------------------------------
-- إدراج بيانات الجدول: dbo.[positions] (10 صف)
-- -------------------------------------------------------------
SET IDENTITY_INSERT dbo.[positions] ON;
GO

INSERT INTO dbo.[positions] ([id], [name], [code], [department_id], [level], [description], [is_active], [created_at], [updated_at])
VALUES
  (1, N'مدير عام المصنع', N'GM', 7, 1, NULL, 1, N'2026-10-04T23:15:17.256100', N'2026-10-04T23:15:17.256100'),
  (2, N'مدير الجودة', N'QM', 1, 2, NULL, 1, N'2026-10-04T23:15:17.256100', N'2026-10-04T23:15:17.256100'),
  (3, N'مشرف الجودة', N'QS', 1, 3, NULL, 1, N'2026-10-04T23:15:17.256100', N'2026-10-04T23:15:17.256100'),
  (4, N'أخصائي جودة / فاحص', N'QI', 1, 4, NULL, 1, N'2026-10-04T23:15:17.256100', N'2026-10-04T23:15:17.256100'),
  (5, N'مدير الإنتاج', N'PM', 2, 2, NULL, 1, N'2026-10-04T23:15:17.256100', N'2026-10-04T23:15:17.256100'),
  (6, N'مشرف الإنتاج', N'PS', 2, 3, NULL, 1, N'2026-10-04T23:15:17.256100', N'2026-10-04T23:15:17.256100'),
  (7, N'فني مختبر', N'LT', 3, 4, NULL, 1, N'2026-10-04T23:15:17.256100', N'2026-10-04T23:15:17.256100'),
  (8, N'مسؤول سلامة الغذاء', N'FSO', 6, 3, NULL, 1, N'2026-10-04T23:15:17.256100', N'2026-10-04T23:15:17.256100'),
  (9, N'مسؤول المستودع', N'SM', 5, 3, NULL, 1, N'2026-10-04T23:15:17.256100', N'2026-10-04T23:15:17.256100'),
  (10, N'مدير تقنية المعلومات', N'IT', 7, 2, NULL, 1, N'2026-10-04T23:15:17.256100', N'2026-10-04T23:15:17.256100');

SET IDENTITY_INSERT dbo.[positions] OFF;
GO

-- -------------------------------------------------------------
-- إدراج بيانات الجدول: dbo.[CustomRoles] (6 صف)
-- -------------------------------------------------------------
SET IDENTITY_INSERT dbo.[CustomRoles] ON;
GO

INSERT INTO dbo.[CustomRoles] ([id], [code], [title], [description], [color], [icon_name], [permissions_json], [is_system], [created_at])
VALUES
  (1, N'admin', N'مسؤول النظام (المدير التقني)', N'كامل الصلاحيات الفنية والإدارية وإدارة المستخدمين والأدوار', 4281812815, N'admin', N'["dept_quality","enabled_rpt_raw_material","fill_rpt_raw_material","view_rpt_raw_material","print_rpt_raw_material","enabled_rpt_final_product","fill_rpt_final_product","view_rpt_final_product","print_rpt_final_product","enabled_rpt_raw_water","fill_rpt_raw_water","view_rpt_raw_water","print_rpt_raw_water","create_water_reports","create_raw_material_reports","create_hygiene_reports","fill_quality_dynamic_reports","fill_food_safety_dynamic_reports","view_reports","edit_reports","delete_reports","approve_qm","approve_gm","reject_reports","design_dynamic_reports","dept_food_safety","enabled_rpt_hygiene","fill_rpt_hygiene","view_rpt_hygiene","print_rpt_hygiene","fill_health_reports","view_health_reports","view_shifts","manage_shifts","manage_users","manage_roles","export_pdf","manage_settings","manage_products","manage_daily_workers","manage_signature","change_password"]', 1, N'2026-10-04T23:15:17.156778'),
  (2, N'general_manager', N'المدير العام', N'أعلى سلطة في الشركة، صلاحيات شاملة واستعراض كل المصانع', 4279060385, N'gmanager', N'["dept_quality","enabled_rpt_raw_material","fill_rpt_raw_material","view_rpt_raw_material","print_rpt_raw_material","enabled_rpt_final_product","fill_rpt_final_product","view_rpt_final_product","print_rpt_final_product","enabled_rpt_raw_water","fill_rpt_raw_water","view_rpt_raw_water","print_rpt_raw_water","create_water_reports","create_raw_material_reports","create_hygiene_reports","fill_quality_dynamic_reports","fill_food_safety_dynamic_reports","view_reports","edit_reports","delete_reports","approve_qm","approve_gm","reject_reports","design_dynamic_reports","dept_food_safety","enabled_rpt_hygiene","fill_rpt_hygiene","view_rpt_hygiene","print_rpt_hygiene","fill_health_reports","view_health_reports","view_shifts","manage_shifts","manage_users","manage_roles","export_pdf","manage_settings","manage_products","manage_daily_workers","manage_signature","change_password"]', 1, N'2026-10-04T23:15:17.157775'),
  (3, N'quality_manager', N'مدير الجودة', N'إدخال الفحوصات، اعتماد المستوى الأول QM، وتصميم النماذج', 4281236786, N'qm', N'["dept_quality","enabled_rpt_final_product","fill_rpt_final_product","view_rpt_final_product","print_rpt_final_product","enabled_rpt_raw_water","fill_rpt_raw_water","view_rpt_raw_water","print_rpt_raw_water","enabled_rpt_raw_material","fill_rpt_raw_material","view_rpt_raw_material","print_rpt_raw_material","create_water_reports","create_raw_material_reports","fill_quality_dynamic_reports","view_reports","edit_reports","approve_qm","reject_reports","design_dynamic_reports","view_shifts","manage_products","manage_daily_workers","export_pdf","manage_signature","change_password"]', 1, N'2026-10-04T23:15:17.158773'),
  (4, N'factory_manager', N'مدير المصنع', N'استعراض التقارير والاعتماد النهائي GM وتصدير الوثائق', 4285143962, N'gm', N'["dept_quality","dept_food_safety","enabled_rpt_final_product","view_rpt_final_product","print_rpt_final_product","enabled_rpt_raw_water","view_rpt_raw_water","print_rpt_raw_water","enabled_rpt_raw_material","view_rpt_raw_material","print_rpt_raw_material","enabled_rpt_hygiene","view_rpt_hygiene","print_rpt_hygiene","view_reports","approve_gm","reject_reports","view_health_reports","view_shifts","manage_daily_workers","export_pdf","manage_signature","change_password"]', 1, N'2026-10-04T23:15:17.160781'),
  (5, N'quality_specialist', N'أخصائي الجودة / منفذ الفحص', N'إدخال قراءات وفحوصات المياه وتعبئة السجلات اليومية', 4278220689, N'inspector', N'["dept_quality","enabled_rpt_final_product","fill_rpt_final_product","view_rpt_final_product","print_rpt_final_product","enabled_rpt_raw_water","fill_rpt_raw_water","view_rpt_raw_water","print_rpt_raw_water","enabled_rpt_raw_material","fill_rpt_raw_material","view_rpt_raw_material","print_rpt_raw_material","create_water_reports","create_raw_material_reports","fill_quality_dynamic_reports","view_reports","edit_reports","export_pdf","manage_signature","change_password"]', 1, N'2026-10-04T23:15:17.161777'),
  (6, N'food_safety_engineer', N'مهندس سلامة الغذاء', N'تعبئة نماذج سلامة الغذاء والنظافة وتقارير المواد الخام ومراجعتها', 4293284096, N'foodsafety', N'["dept_food_safety","enabled_rpt_hygiene","fill_rpt_hygiene","view_rpt_hygiene","print_rpt_hygiene","create_hygiene_reports","fill_food_safety_dynamic_reports","edit_reports","fill_health_reports","view_health_reports","export_pdf","manage_signature","change_password"]', 1, N'2026-10-04T23:15:17.162774');

SET IDENTITY_INSERT dbo.[CustomRoles] OFF;
GO

-- -------------------------------------------------------------
-- إدراج بيانات الجدول: dbo.[Users] (11 صف)
-- -------------------------------------------------------------
SET IDENTITY_INSERT dbo.[Users] ON;
GO

INSERT INTO dbo.[Users] ([id], [branch_id], [full_name], [username], [role], [email], [password_hash], [phone], [job_title], [is_active], [avatar_color], [created_at], [last_login])
VALUES
  (1, NULL, N'مسؤول النظام (المدير التقني)', N'admin', N'Admin', N'admin@wadialain.com', N'217d9cb702a36fed3a5812ddd596dab9e0984d61d2f3d245f63cc3d4db1b06a7', N'0500000001', N'مدير تقنية المعلومات والنظام', 1, 4281812815, N'2026-10-04T23:15:16.996827', NULL),
  (2, NULL, N'أ. مازن الجلال', N'gmanager', N'GeneralManager', N'mazen@wadialain.com', N'217d9cb702a36fed3a5812ddd596dab9e0984d61d2f3d245f63cc3d4db1b06a7', N'0500000010', N'المدير العام', 1, 4279060385, N'2026-10-04T23:15:16.999821', NULL),
  (3, NULL, N'أ. حمود يسر', N'qm', N'QualityManager', N'hamoud@wadialain.com', N'217d9cb702a36fed3a5812ddd596dab9e0984d61d2f3d245f63cc3d4db1b06a7', N'0555566778', N'مدير الرقابة وتوكيد الجودة', 1, 4281236786, N'2026-10-04T23:15:17.000818', NULL),
  (4, NULL, N'م. نبيل مراد', N'nabil', N'QualityManager', N'nabil@wadialain.com', N'217d9cb702a36fed3a5812ddd596dab9e0984d61d2f3d245f63cc3d4db1b06a7', N'0555566779', N'مهندس توكيد ورقابة الجودة', 1, 4281236786, N'2026-10-04T23:15:17.000818', NULL),
  (5, 2, N'أ. محمد الحاج', N'gm2', N'FactoryManager', N'mohammed.h@wadialain.com', N'217d9cb702a36fed3a5812ddd596dab9e0984d61d2f3d245f63cc3d4db1b06a7', N'0559988772', N'مدير المصنع (سنحان)', 1, 4283045004, N'2026-10-04T23:15:17.000818', NULL),
  (6, 1, N'د. خالد العمري', N'gm', N'FactoryManager', N'khaled@wadialain.com', N'217d9cb702a36fed3a5812ddd596dab9e0984d61d2f3d245f63cc3d4db1b06a7', N'0559988776', N'مدير المصنع (أرتل)', 1, 4285143962, N'2026-10-04T23:15:17.000818', NULL),
  (7, 1, N'م. صلاح الجابري', N'salah', N'QualitySpecialist', N'salah@wadialain.com', N'217d9cb702a36fed3a5812ddd596dab9e0984d61d2f3d245f63cc3d4db1b06a7', N'0551122334', N'مهندس جودة (أرتل)', 1, 4278220689, N'2026-10-04T23:15:17.001815', NULL),
  (8, 2, N'م. أسامة الشجاع', N'osama', N'QualitySpecialist', N'osama@wadialain.com', N'217d9cb702a36fed3a5812ddd596dab9e0984d61d2f3d245f63cc3d4db1b06a7', N'0551122335', N'مهندس جودة (سنحان)', 1, 4278220689, N'2026-10-04T23:15:17.001815', NULL),
  (9, 2, N'م. محمد الرجامي', N'mohammed_r', N'QualitySpecialist', N'mohammed.r@wadialain.com', N'217d9cb702a36fed3a5812ddd596dab9e0984d61d2f3d245f63cc3d4db1b06a7', N'0551122337', N'مهندس جودة (سنحان)', 1, 4278220689, N'2026-10-04T23:15:17.001815', NULL),
  (10, 1, N'م. بشير القدسي', N'basheer_fs', N'FoodSafetyEngineer', N'basheer.fs@wadialain.com', N'217d9cb702a36fed3a5812ddd596dab9e0984d61d2f3d245f63cc3d4db1b06a7', N'0551122346', N'مهندس سلامة الغذاء (أرتل)', 1, 4293284096, N'2026-10-04T23:15:17.001815', NULL),
  (11, 2, N'م. أمجد الخدشي', N'amjad_fs', N'FoodSafetyEngineer', N'amjad.fs@wadialain.com', N'217d9cb702a36fed3a5812ddd596dab9e0984d61d2f3d245f63cc3d4db1b06a7', N'0551122347', N'مهندس سلامة الغذاء (سنحان)', 1, 4293284096, N'2026-10-04T23:15:17.001815', NULL);

SET IDENTITY_INSERT dbo.[Users] OFF;
GO

-- -------------------------------------------------------------
-- إدراج بيانات الجدول: dbo.[Shifts] (4 صف)
-- -------------------------------------------------------------
SET IDENTITY_INSERT dbo.[Shifts] ON;
GO

INSERT INTO dbo.[Shifts] ([id], [branch_id], [name], [start_time], [end_time], [season_name], [is_active])
VALUES
  (1, 1, N'الوردية الصباحية', N'06:00', N'14:00', N'التوقيت الصيفي', 1),
  (2, 1, N'الوردية المسائية', N'14:00', N'22:00', N'التوقيت الصيفي', 1),
  (3, 2, N'الوردية الأولى (الصباحية)', N'07:00', N'15:30', N'التوقيت الصيفي', 1),
  (4, 2, N'الوردية الثانية (المسائية)', N'15:30', N'00:00', N'التوقيت الصيفي', 1);

SET IDENTITY_INSERT dbo.[Shifts] OFF;
GO

-- -------------------------------------------------------------
-- إدراج بيانات الجدول: dbo.[Products] (8 صف)
-- -------------------------------------------------------------
SET IDENTITY_INSERT dbo.[Products] ON;
GO

INSERT INTO dbo.[Products] ([id], [branch_id], [name], [volume_liters], [is_active])
VALUES
  (1, 1, N'عبوة 5 لتر', 5.0, 1),
  (2, 1, N'عبوة 3 لتر', 3.0, 1),
  (3, 1, N'عبوة 1.8 لتر', 1.8, 1),
  (4, 1, N'عبوة 1.2 لتر', 1.2, 1),
  (5, 1, N'عبوة 330 مل', 0.33, 1),
  (6, 2, N'عبوة 600 مل', 0.6, 1),
  (7, 2, N'عبوة 1.5 لتر', 1.5, 1),
  (8, 2, N'عبوة 19 لتر', 19.0, 1);

SET IDENTITY_INSERT dbo.[Products] OFF;
GO

-- -------------------------------------------------------------
-- إدراج بيانات الجدول: dbo.[TestParameters] (69 صف)
-- -------------------------------------------------------------
SET IDENTITY_INSERT dbo.[TestParameters] ON;
GO

INSERT INTO dbo.[TestParameters] ([id], [category], [name_ar], [unit], [input_type])
VALUES
  (1, N'Physiochemical', N'pH', N'', N'Numeric'),
  (2, N'Physiochemical', N'TDS', N'ppm', N'Numeric'),
  (3, N'Physiochemical', N'EC', N'µS/cm', N'Numeric'),
  (4, N'Physiochemical', N'Temp', N'°C', N'Numeric'),
  (5, N'Physiochemical', N'Turbidity', N'NTU', N'Numeric'),
  (6, N'Physiochemical', N'Total Hardness', N'mg/L', N'Numeric'),
  (7, N'Physiochemical', N'Total Alkalinity', N'mg/L', N'Numeric'),
  (8, N'Chemical', N'Ca', N'mg/L', N'Numeric'),
  (9, N'Chemical', N'Mg', N'mg/L', N'Numeric'),
  (10, N'Chemical', N'Cl', N'mg/L', N'Numeric'),
  (11, N'Chemical', N'SO4', N'mg/L', N'Numeric'),
  (12, N'Chemical', N'NO3', N'mg/L', N'Numeric'),
  (13, N'Chemical', N'Fe', N'mg/L', N'Numeric'),
  (14, N'Chemical', N'K', N'mg/L', N'Numeric'),
  (15, N'Chemical', N'Na', N'mg/L', N'Numeric'),
  (16, N'Chemical', N'F', N'mg/L', N'Numeric'),
  (17, N'Chemical', N'HCO3', N'mg/L', N'Numeric'),
  (18, N'Chemical', N'Chlorine', N'ppm', N'Numeric'),
  (19, N'Physical', N'Color', N'', N'Text'),
  (20, N'Physical', N'Taste', N'', N'Text'),
  (21, N'Physical', N'Odor', N'', N'Text'),
  (22, N'Physiochemical', N'Ozone', N'ppm', N'Numeric'),
  (23, N'Physiochemical', N'Residual Cl', N'ppm', N'Numeric'),
  (24, N'Physiochemical', N'UV', N'mj/cm2', N'Numeric'),
  (25, N'Microbiological', N'Total Plate Count', N'', N'Text'),
  (26, N'Microbiological', N'Total Coliforms', N'', N'Text'),
  (27, N'Microbiological', N'E. coli', N'', N'Text'),
  (28, N'Microbiological', N'Yeasts & Molds', N'', N'Text'),
  (29, N'Physical', N'Appearance', N'', N'Text'),
  (30, N'Physical', N'Volume', N'ml', N'Numeric'),
  (31, N'Physical', N'Sealing', N'', N'Text'),
  (32, N'Physical', N'Labeling', N'', N'Text'),
  (33, N'Physical', N'Date Printing', N'', N'Text'),
  (34, N'Sensory', N'اللون', N'', N'Text'),
  (35, N'Sensory', N'الطعم', N'', N'Text'),
  (36, N'Sensory', N'الرائحة', N'', N'Text'),
  (37, N'Sensory', N'العكارة', N'NTU', N'Numeric'),
  (38, N'Sensory', N'المواد الغريبة أو الشوائب', N'', N'Text'),
  (39, N'Sensory', N'المظهر', N'', N'Text'),
  (40, N'Sensory', N'سلامة العبوة', N'', N'Text'),
  (41, N'Sensory', N'إحكام الغلق', N'', N'Text'),
  (42, N'Sensory', N'نظافة العبوة', N'', N'Text'),
  (43, N'Sensory', N'جودة الطباعة', N'', N'Text'),
  (44, N'Sensory', N'رقم التشغيلة', N'', N'Text'),
  (45, N'Sensory', N'تاريخ الإنتاج', N'', N'Text'),
  (46, N'Sensory', N'تاريخ الانتهاء', N'', N'Text'),
  (47, N'Sensory', N'البطاقة التعريفية', N'', N'Text'),
  (48, N'Sensory', N'التغليف النهائي', N'', N'Text'),
  (49, N'Physiochemical', N'درجة الحرارة', N'°C', N'Numeric'),
  (50, N'Physiochemical', N'الرقم الهيدروجيني (pH)', N'', N'Numeric');

INSERT INTO dbo.[TestParameters] ([id], [category], [name_ar], [unit], [input_type])
VALUES
  (51, N'Physiochemical', N'المواد الصلبة الذائبة (TDS)', N'mg/l', N'Numeric'),
  (52, N'Physiochemical', N'التوصيلية الكهربائية', N'µS/cm', N'Numeric'),
  (53, N'Chemical', N'العسر الكلي', N'mg/l', N'Numeric'),
  (54, N'Chemical', N'القلوية الكلية', N'mg/l', N'Numeric'),
  (55, N'Chemical', N'الكالسيوم', N'mg/l', N'Numeric'),
  (56, N'Chemical', N'المغنسيوم', N'mg/l', N'Numeric'),
  (57, N'Chemical', N'البوتاسيوم', N'mg/l', N'Numeric'),
  (58, N'Chemical', N'الصوديوم', N'mg/l', N'Numeric'),
  (59, N'Chemical', N'الفلورايد', N'mg/l', N'Numeric'),
  (60, N'Chemical', N'الكلورايد', N'mg/l', N'Numeric'),
  (61, N'Chemical', N'الحديد', N'mg/l', N'Numeric'),
  (62, N'Chemical', N'النترات', N'mg/l', N'Numeric'),
  (63, N'Chemical', N'الكبريتات', N'mg/l', N'Numeric'),
  (64, N'Chemical', N'البيكربونات', N'mg/l', N'Numeric'),
  (65, N'Physiochemical', N'الأوزون المتبقي (إن وجد)', N'mg/l', N'Numeric'),
  (66, N'Physiochemical', N'الكلور المتبقي (إن وجد)', N'ppm', N'Numeric'),
  (67, N'Microbiological', N'العدد الكلي للبكتيريا', N'', N'Text'),
  (68, N'Microbiological', N'القولونيات الكلية', N'', N'Text'),
  (69, N'Microbiological', N'الخمائر والفطريات', N'', N'Text');

SET IDENTITY_INSERT dbo.[TestParameters] OFF;
GO

-- -------------------------------------------------------------
-- إدراج بيانات الجدول: dbo.[QualityStandards] (216 صف)
-- -------------------------------------------------------------
SET IDENTITY_INSERT dbo.[QualityStandards] ON;
GO

INSERT INTO dbo.[QualityStandards] ([id], [product_id], [water_type], [parameter_id], [min_value], [max_value], [expected_text])
VALUES
  (1, 1, N'Final Product', 19, NULL, NULL, N'عديم اللون'),
  (2, 1, N'Final Product', 20, NULL, NULL, N'مستساغ'),
  (3, 1, N'Final Product', 21, NULL, NULL, N'مقبولة'),
  (4, 1, N'Final Product', 5, 0.0, 5.0, NULL),
  (5, 1, N'Final Product', 4, NULL, NULL, NULL),
  (6, 1, N'Final Product', 1, 6.5, 8.5, NULL),
  (7, 1, N'Final Product', 2, 100.0, 500.0, NULL),
  (8, 1, N'Final Product', 3, NULL, NULL, NULL),
  (9, 1, N'Final Product', 6, 0.0, 500.0, NULL),
  (10, 1, N'Final Product', 7, 0.0, 200.0, NULL),
  (11, 1, N'Final Product', 8, 0.0, 350.0, NULL),
  (12, 1, N'Final Product', 9, 0.0, 150.0, NULL),
  (13, 1, N'Final Product', 14, 1.0, 10.0, NULL),
  (14, 1, N'Final Product', 15, 0.0, 200.0, NULL),
  (15, 1, N'Final Product', 16, 0.7, 1.5, NULL),
  (16, 1, N'Final Product', 10, 0.0, 250.0, NULL),
  (17, 1, N'Final Product', 13, 0.0, 0.3, NULL),
  (18, 1, N'Final Product', 12, 0.0, 50.0, NULL),
  (19, 1, N'Final Product', 11, 0.0, 250.0, NULL),
  (20, 1, N'Final Product', 17, 0.0, 250.0, NULL),
  (21, 1, N'Final Product', 22, 0.25, 0.45, NULL),
  (22, 1, N'Final Product', 23, 0.2, 0.5, NULL),
  (23, 1, N'Final Product', 25, NULL, NULL, N'خالية تماماً'),
  (24, 1, N'Final Product', 26, NULL, NULL, N'خالية تماماً'),
  (25, 1, N'Final Product', 27, NULL, NULL, N'خالية تماماً'),
  (26, 1, N'Final Product', 28, NULL, NULL, N'خالية تماماً'),
  (27, 1, N'Final Product', 29, NULL, NULL, N'Clear'),
  (28, 1, N'Final Product', 31, NULL, NULL, N'Good'),
  (29, 1, N'Final Product', 32, NULL, NULL, N'Good'),
  (30, 1, N'Final Product', 33, NULL, NULL, N'Clear'),
  (31, 1, N'Final Product', 30, 4950.0, 5050.0, NULL),
  (32, 2, N'Final Product', 19, NULL, NULL, N'عديم اللون'),
  (33, 2, N'Final Product', 20, NULL, NULL, N'مستساغ'),
  (34, 2, N'Final Product', 21, NULL, NULL, N'مقبولة'),
  (35, 2, N'Final Product', 5, 0.0, 5.0, NULL),
  (36, 2, N'Final Product', 4, NULL, NULL, NULL),
  (37, 2, N'Final Product', 1, 6.5, 8.5, NULL),
  (38, 2, N'Final Product', 2, 100.0, 500.0, NULL),
  (39, 2, N'Final Product', 3, NULL, NULL, NULL),
  (40, 2, N'Final Product', 6, 0.0, 500.0, NULL),
  (41, 2, N'Final Product', 7, 0.0, 200.0, NULL),
  (42, 2, N'Final Product', 8, 0.0, 350.0, NULL),
  (43, 2, N'Final Product', 9, 0.0, 150.0, NULL),
  (44, 2, N'Final Product', 14, 1.0, 10.0, NULL),
  (45, 2, N'Final Product', 15, 0.0, 200.0, NULL),
  (46, 2, N'Final Product', 16, 0.7, 1.5, NULL),
  (47, 2, N'Final Product', 10, 0.0, 250.0, NULL),
  (48, 2, N'Final Product', 13, 0.0, 0.3, NULL),
  (49, 2, N'Final Product', 12, 0.0, 50.0, NULL),
  (50, 2, N'Final Product', 11, 0.0, 250.0, NULL);

INSERT INTO dbo.[QualityStandards] ([id], [product_id], [water_type], [parameter_id], [min_value], [max_value], [expected_text])
VALUES
  (51, 2, N'Final Product', 17, 0.0, 250.0, NULL),
  (52, 2, N'Final Product', 22, 0.25, 0.45, NULL),
  (53, 2, N'Final Product', 23, 0.2, 0.5, NULL),
  (54, 2, N'Final Product', 25, NULL, NULL, N'خالية تماماً'),
  (55, 2, N'Final Product', 26, NULL, NULL, N'خالية تماماً'),
  (56, 2, N'Final Product', 27, NULL, NULL, N'خالية تماماً'),
  (57, 2, N'Final Product', 28, NULL, NULL, N'خالية تماماً'),
  (58, 2, N'Final Product', 29, NULL, NULL, N'Clear'),
  (59, 2, N'Final Product', 31, NULL, NULL, N'Good'),
  (60, 2, N'Final Product', 32, NULL, NULL, N'Good'),
  (61, 2, N'Final Product', 33, NULL, NULL, N'Clear'),
  (62, 2, N'Final Product', 30, 2950.0, 3050.0, NULL),
  (63, 3, N'Final Product', 19, NULL, NULL, N'عديم اللون'),
  (64, 3, N'Final Product', 20, NULL, NULL, N'مستساغ'),
  (65, 3, N'Final Product', 21, NULL, NULL, N'مقبولة'),
  (66, 3, N'Final Product', 5, 0.0, 5.0, NULL),
  (67, 3, N'Final Product', 4, NULL, NULL, NULL),
  (68, 3, N'Final Product', 1, 6.5, 8.5, NULL),
  (69, 3, N'Final Product', 2, 100.0, 500.0, NULL),
  (70, 3, N'Final Product', 3, NULL, NULL, NULL),
  (71, 3, N'Final Product', 6, 0.0, 500.0, NULL),
  (72, 3, N'Final Product', 7, 0.0, 200.0, NULL),
  (73, 3, N'Final Product', 8, 0.0, 350.0, NULL),
  (74, 3, N'Final Product', 9, 0.0, 150.0, NULL),
  (75, 3, N'Final Product', 14, 1.0, 10.0, NULL),
  (76, 3, N'Final Product', 15, 0.0, 200.0, NULL),
  (77, 3, N'Final Product', 16, 0.7, 1.5, NULL),
  (78, 3, N'Final Product', 10, 0.0, 250.0, NULL),
  (79, 3, N'Final Product', 13, 0.0, 0.3, NULL),
  (80, 3, N'Final Product', 12, 0.0, 50.0, NULL),
  (81, 3, N'Final Product', 11, 0.0, 250.0, NULL),
  (82, 3, N'Final Product', 17, 0.0, 250.0, NULL),
  (83, 3, N'Final Product', 22, 0.25, 0.45, NULL),
  (84, 3, N'Final Product', 23, 0.2, 0.5, NULL),
  (85, 3, N'Final Product', 25, NULL, NULL, N'خالية تماماً'),
  (86, 3, N'Final Product', 26, NULL, NULL, N'خالية تماماً'),
  (87, 3, N'Final Product', 27, NULL, NULL, N'خالية تماماً'),
  (88, 3, N'Final Product', 28, NULL, NULL, N'خالية تماماً'),
  (89, 3, N'Final Product', 29, NULL, NULL, N'Clear'),
  (90, 3, N'Final Product', 31, NULL, NULL, N'Good'),
  (91, 3, N'Final Product', 32, NULL, NULL, N'Good'),
  (92, 3, N'Final Product', 33, NULL, NULL, N'Clear'),
  (93, 3, N'Final Product', 30, 1750.0, 1850.0, NULL),
  (94, 4, N'Final Product', 19, NULL, NULL, N'عديم اللون'),
  (95, 4, N'Final Product', 20, NULL, NULL, N'مستساغ'),
  (96, 4, N'Final Product', 21, NULL, NULL, N'مقبولة'),
  (97, 4, N'Final Product', 5, 0.0, 5.0, NULL),
  (98, 4, N'Final Product', 4, NULL, NULL, NULL),
  (99, 4, N'Final Product', 1, 6.5, 8.5, NULL),
  (100, 4, N'Final Product', 2, 100.0, 500.0, NULL);

INSERT INTO dbo.[QualityStandards] ([id], [product_id], [water_type], [parameter_id], [min_value], [max_value], [expected_text])
VALUES
  (101, 4, N'Final Product', 3, NULL, NULL, NULL),
  (102, 4, N'Final Product', 6, 0.0, 500.0, NULL),
  (103, 4, N'Final Product', 7, 0.0, 200.0, NULL),
  (104, 4, N'Final Product', 8, 0.0, 350.0, NULL),
  (105, 4, N'Final Product', 9, 0.0, 150.0, NULL),
  (106, 4, N'Final Product', 14, 1.0, 10.0, NULL),
  (107, 4, N'Final Product', 15, 0.0, 200.0, NULL),
  (108, 4, N'Final Product', 16, 0.7, 1.5, NULL),
  (109, 4, N'Final Product', 10, 0.0, 250.0, NULL),
  (110, 4, N'Final Product', 13, 0.0, 0.3, NULL),
  (111, 4, N'Final Product', 12, 0.0, 50.0, NULL),
  (112, 4, N'Final Product', 11, 0.0, 250.0, NULL),
  (113, 4, N'Final Product', 17, 0.0, 250.0, NULL),
  (114, 4, N'Final Product', 22, 0.25, 0.45, NULL),
  (115, 4, N'Final Product', 23, 0.2, 0.5, NULL),
  (116, 4, N'Final Product', 25, NULL, NULL, N'خالية تماماً'),
  (117, 4, N'Final Product', 26, NULL, NULL, N'خالية تماماً'),
  (118, 4, N'Final Product', 27, NULL, NULL, N'خالية تماماً'),
  (119, 4, N'Final Product', 28, NULL, NULL, N'خالية تماماً'),
  (120, 4, N'Final Product', 29, NULL, NULL, N'Clear'),
  (121, 4, N'Final Product', 31, NULL, NULL, N'Good'),
  (122, 4, N'Final Product', 32, NULL, NULL, N'Good'),
  (123, 4, N'Final Product', 33, NULL, NULL, N'Clear'),
  (124, 4, N'Final Product', 30, 1150.0, 1250.0, NULL),
  (125, 5, N'Final Product', 19, NULL, NULL, N'عديم اللون'),
  (126, 5, N'Final Product', 20, NULL, NULL, N'مستساغ'),
  (127, 5, N'Final Product', 21, NULL, NULL, N'مقبولة'),
  (128, 5, N'Final Product', 5, 0.0, 5.0, NULL),
  (129, 5, N'Final Product', 4, NULL, NULL, NULL),
  (130, 5, N'Final Product', 1, 6.5, 8.5, NULL),
  (131, 5, N'Final Product', 2, 100.0, 500.0, NULL),
  (132, 5, N'Final Product', 3, NULL, NULL, NULL),
  (133, 5, N'Final Product', 6, 0.0, 500.0, NULL),
  (134, 5, N'Final Product', 7, 0.0, 200.0, NULL),
  (135, 5, N'Final Product', 8, 0.0, 350.0, NULL),
  (136, 5, N'Final Product', 9, 0.0, 150.0, NULL),
  (137, 5, N'Final Product', 14, 1.0, 10.0, NULL),
  (138, 5, N'Final Product', 15, 0.0, 200.0, NULL),
  (139, 5, N'Final Product', 16, 0.7, 1.5, NULL),
  (140, 5, N'Final Product', 10, 0.0, 250.0, NULL),
  (141, 5, N'Final Product', 13, 0.0, 0.3, NULL),
  (142, 5, N'Final Product', 12, 0.0, 50.0, NULL),
  (143, 5, N'Final Product', 11, 0.0, 250.0, NULL),
  (144, 5, N'Final Product', 17, 0.0, 250.0, NULL),
  (145, 5, N'Final Product', 22, 0.25, 0.45, NULL),
  (146, 5, N'Final Product', 23, 0.2, 0.5, NULL),
  (147, 5, N'Final Product', 25, NULL, NULL, N'خالية تماماً'),
  (148, 5, N'Final Product', 26, NULL, NULL, N'خالية تماماً'),
  (149, 5, N'Final Product', 27, NULL, NULL, N'خالية تماماً'),
  (150, 5, N'Final Product', 28, NULL, NULL, N'خالية تماماً');

INSERT INTO dbo.[QualityStandards] ([id], [product_id], [water_type], [parameter_id], [min_value], [max_value], [expected_text])
VALUES
  (151, 5, N'Final Product', 29, NULL, NULL, N'Clear'),
  (152, 5, N'Final Product', 31, NULL, NULL, N'Good'),
  (153, 5, N'Final Product', 32, NULL, NULL, N'Good'),
  (154, 5, N'Final Product', 33, NULL, NULL, N'Clear'),
  (155, 5, N'Final Product', 30, 325.0, 335.0, NULL),
  (156, NULL, N'Raw Water', 34, NULL, NULL, N'طبيعي'),
  (157, NULL, N'Raw Water', 35, NULL, NULL, N'مستساغ'),
  (158, NULL, N'Raw Water', 36, NULL, NULL, N'طبيعية'),
  (159, NULL, N'Raw Water', 37, NULL, 5.0, NULL),
  (160, NULL, N'Raw Water', 1, 6.5, 8.5, NULL),
  (161, NULL, N'Raw Water', 2, 100.0, 1000.0, NULL),
  (162, NULL, N'Raw Water', 3, NULL, 1500.0, NULL),
  (163, NULL, N'Raw Water', 4, NULL, NULL, NULL),
  (165, NULL, N'Raw Water', 6, NULL, 500.0, NULL),
  (166, NULL, N'Raw Water', 7, NULL, 200.0, NULL),
  (167, NULL, N'Raw Water', 8, NULL, 350.0, NULL),
  (168, NULL, N'Raw Water', 9, NULL, 150.0, NULL),
  (169, NULL, N'Raw Water', 10, NULL, NULL, NULL),
  (170, NULL, N'Raw Water', 11, NULL, 250.0, NULL),
  (171, NULL, N'Raw Water', 12, NULL, 50.0, NULL),
  (172, NULL, N'Raw Water', 13, NULL, 0.3, NULL),
  (173, NULL, N'Raw Water', 14, 1.0, 10.0, NULL),
  (174, NULL, N'Raw Water', 15, NULL, 200.0, NULL),
  (175, NULL, N'Raw Water', 16, NULL, 1.5, NULL),
  (176, NULL, N'Raw Water', 17, NULL, 250.0, NULL),
  (177, NULL, N'Raw Water', 18, 0.2, 0.5, NULL),
  (178, NULL, N'Raw Water', 25, NULL, NULL, N'<100 CFU'),
  (179, NULL, N'Raw Water', 26, NULL, NULL, N'خالي'),
  (180, NULL, N'Raw Water', 27, NULL, NULL, N'خالي'),
  (181, NULL, N'Raw Water', 28, NULL, NULL, N'خالي'),
  (182, NULL, N'Final Product', 38, NULL, NULL, N'خالية تماماً'),
  (183, NULL, N'Final Product', 39, NULL, NULL, N'طبيعي'),
  (184, NULL, N'Final Product', 40, NULL, NULL, N'بدون تشققات أو تشوهات'),
  (185, NULL, N'Final Product', 41, NULL, NULL, N'لا يوجد تسريب'),
  (186, NULL, N'Final Product', 42, NULL, NULL, N'نظيفة'),
  (187, NULL, N'Final Product', 43, NULL, NULL, N'واضحة'),
  (188, NULL, N'Final Product', 44, NULL, NULL, N'صحيح'),
  (189, NULL, N'Final Product', 45, NULL, NULL, N'صحيح'),
  (190, NULL, N'Final Product', 46, NULL, NULL, N'صحيح'),
  (191, NULL, N'Final Product', 47, NULL, NULL, N'مطابقة'),
  (192, NULL, N'Final Product', 48, NULL, NULL, N'مطابق'),
  (193, NULL, N'Final Product', 34, NULL, NULL, N'عديم اللون'),
  (194, NULL, N'Final Product', 35, NULL, NULL, N'مستساغ'),
  (195, NULL, N'Final Product', 36, NULL, NULL, N'مقبولة'),
  (196, NULL, N'Final Product', 37, 0.0, 5.0, NULL),
  (197, NULL, N'Final Product', 49, NULL, NULL, NULL),
  (198, NULL, N'Final Product', 50, 6.5, 8.5, NULL),
  (199, NULL, N'Final Product', 51, 100.0, 500.0, NULL),
  (200, NULL, N'Final Product', 52, NULL, NULL, NULL);

INSERT INTO dbo.[QualityStandards] ([id], [product_id], [water_type], [parameter_id], [min_value], [max_value], [expected_text])
VALUES
  (201, NULL, N'Final Product', 53, 0.0, 500.0, NULL),
  (202, NULL, N'Final Product', 54, 0.0, 200.0, NULL),
  (203, NULL, N'Final Product', 55, 0.0, 350.0, NULL),
  (204, NULL, N'Final Product', 56, 0.0, 150.0, NULL),
  (205, NULL, N'Final Product', 57, 1.0, 10.0, NULL),
  (206, NULL, N'Final Product', 58, 0.0, 200.0, NULL),
  (207, NULL, N'Final Product', 59, 0.7, 1.5, NULL),
  (208, NULL, N'Final Product', 60, 0.0, 250.0, NULL),
  (209, NULL, N'Final Product', 61, 0.0, 0.3, NULL),
  (210, NULL, N'Final Product', 62, 0.0, 50.0, NULL),
  (211, NULL, N'Final Product', 63, 0.0, 250.0, NULL),
  (212, NULL, N'Final Product', 64, 0.0, 250.0, NULL),
  (213, NULL, N'Final Product', 65, 0.25, 0.45, NULL),
  (214, NULL, N'Final Product', 66, 0.2, 0.5, NULL),
  (215, NULL, N'Final Product', 67, NULL, NULL, N'خالية تماماً'),
  (216, NULL, N'Final Product', 68, NULL, NULL, N'خالية تماماً'),
  (217, NULL, N'Final Product', 69, NULL, NULL, N'خالية تماماً');

SET IDENTITY_INSERT dbo.[QualityStandards] OFF;
GO

-- -------------------------------------------------------------
-- إدراج بيانات الجدول: dbo.[dr_categories] (2 صف)
-- -------------------------------------------------------------
SET IDENTITY_INSERT dbo.[dr_categories] ON;
GO

INSERT INTO dbo.[dr_categories] ([id], [name], [description], [icon], [sort_order], [is_active], [created_at], [updated_at])
VALUES
  (1, N'إدارة الجودة', N'تقارير وفحوصات الجودة', NULL, 0, 1, N'2026-10-04T23:15:17.147802', N'2026-10-04T23:15:17.147802'),
  (2, N'سلامة الغذاء', N'تقارير وسجلات سلامة الغذاء', NULL, 0, 1, N'2026-10-04T23:15:17.147802', N'2026-10-04T23:15:17.147802');

SET IDENTITY_INSERT dbo.[dr_categories] OFF;
GO

-- -------------------------------------------------------------
-- إدراج بيانات الجدول: dbo.[report_workflows] (3 صف)
-- -------------------------------------------------------------
SET IDENTITY_INSERT dbo.[report_workflows] ON;
GO

INSERT INTO dbo.[report_workflows] ([id], [name], [code], [description], [is_active], [created_at], [updated_at])
VALUES
  (1, N'مسار الاعتماد الثنائي القياسي', N'TWO_LEVEL', N'اعتماد مدير الجودة ثم اعتماد مدير المصنع', 1, N'2026-10-04T23:15:17.260856', N'2026-10-04T23:15:17.260856'),
  (2, N'مسار الاعتماد الأحادي', N'SINGLE_LEVEL', N'اعتماد مدير الجودة فقط', 1, N'2026-10-04T23:15:17.260856', N'2026-10-04T23:15:17.260856'),
  (3, N'مسار الاعتماد الثلاثي (مختبر)', N'THREE_LEVEL_LAB', N'مسؤول المختبر → مدير الجودة → مدير المصنع', 1, N'2026-10-04T23:15:17.260856', N'2026-10-04T23:15:17.260856');

SET IDENTITY_INSERT dbo.[report_workflows] OFF;
GO

-- -------------------------------------------------------------
-- إدراج بيانات الجدول: dbo.[report_workflow_steps] (6 صف)
-- -------------------------------------------------------------
SET IDENTITY_INSERT dbo.[report_workflow_steps] ON;
GO

INSERT INTO dbo.[report_workflow_steps] ([id], [workflow_id], [step_order], [step_name], [required_role], [required_position_id], [action_type], [is_optional], [created_at])
VALUES
  (1, 1, 1, N'اعتماد مدير الجودة', N'QualityManager', 2, N'approve', 0, N'2026-10-04T23:15:17.260856'),
  (2, 1, 2, N'اعتماد مدير المصنع', N'FactoryManager', 1, N'approve', 0, N'2026-10-04T23:15:17.260856'),
  (3, 2, 1, N'اعتماد مدير الجودة', N'QualityManager', 2, N'approve', 0, N'2026-10-04T23:15:17.260856'),
  (4, 3, 1, N'اعتماد مسؤول المختبر', N'LabTechnician', 7, N'approve', 0, N'2026-10-04T23:15:17.260856'),
  (5, 3, 2, N'اعتماد مدير الجودة', N'QualityManager', 2, N'approve', 0, N'2026-10-04T23:15:17.260856'),
  (6, 3, 3, N'اعتماد مدير المصنع', N'FactoryManager', 1, N'approve', 0, N'2026-10-04T23:15:17.260856');

SET IDENTITY_INSERT dbo.[report_workflow_steps] OFF;
GO

-- إعادة تفعيل كافة القيود وفحصها
EXEC sp_MSforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL';
GO

PRINT '>> [SUCCESS] Data migration completed successfully! Total rows: 345';
GO
