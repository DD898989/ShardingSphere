@echo off
chcp 65001 >nul
echo ==================================================
echo 🚀 ShardingSphere BDD .NET 10 獨立整合測試執行器 (乾淨 DB)
echo ==================================================
echo.

:loop

:: 1. 進入父目錄，徹底清理 Docker 容器及 Volumes
echo 🧹 [Step 1] 正在停止並清除現有的 Docker 容器與資料卷 (Volumes)...
cd ..
docker-compose down -v
if %ERRORLEVEL% neq 0 (
    echo ❌ 警告: 清除 Docker 發生異常，但仍將嘗試繼續。
)
echo ✅ 舊有環境與資料庫磁碟已徹底清理。
echo.

:: 2. 啟動全新的 Docker 容器環境
echo 🐳 [Step 2] 正在啟動全新的 Docker 服務 (MySQL 與 Sharding-App)...
docker-compose up --build -d
if %ERRORLEVEL% neq 0 (
    echo.
    echo ❌ 錯誤: 無法啟動 Docker 容器，請確認 Docker Desktop 已正常啟動。
    cd testCase
    pause
    exit /b %ERRORLEVEL%
)
echo ✅ 全新 Docker 服務已啟動。
echo.

:: 3. 等待服務就緒
echo ⏳ [Step 3] 正在等待 Spring Boot 服務就緒 (http://localhost:8080)...
:wait_loop
curl.exe -s -I http://localhost:8080/swagger-ui/index.html >nul
if %ERRORLEVEL% neq 0 (
    timeout /t 3 /nobreak >nul
    goto wait_loop
)
echo ✅ Spring Boot 服務已就緒！
echo.

:: 4. 執行測試
echo 📦 [Step 4] 正在執行 C# BDD 測試...
cd testCase
dotnet test --logger "console;verbosity=normal"
set TEST_STATUS=%ERRORLEVEL%
echo.

if %TEST_STATUS% equ 0 (
    echo ==================================================
    echo 🎉 恭喜！在乾淨環境下，所有 BDD 整合測試皆順利通過！
    echo ==================================================
) else (
    echo ==================================================
    echo ❌ 警告：部分 BDD 測試場景失敗，請檢視上方錯誤日誌。
    echo ==================================================
)



timeout /t 2 >nul
echo 完成~~~
pause
goto loop