@echo off
chcp 65001 >nul
title سيرفر API - وادي العين
echo ======================================================
echo          جاري تشغيل سيرفر الـ API لوادي العين...
echo ======================================================
cd /d "C:\Users\REYADH\source\repos\WadiAlAin_Old_CleanArch_Backup\src\WadiAlAin.Api"
"C:\Program Files\dotnet\dotnet.exe" run
pause
