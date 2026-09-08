@echo off
chcp 65001 >nul

:loop

echo ==================================================
echo 🚀 ShardingSphere BDD .NET 10 整合測試啟動器
echo ==================================================
echo.

:: 1. 啟動 Docker 容器
echo 🐳 [Step 1] 正在啟動 Docker 容器 (MySQL 8.0 與 Sharding-App)...
echo 這可能需要幾分鐘來下載 MySQL 映像檔並在 Docker 容器內編譯/打包 Java 專案...
docker-compose up --build -d
if %ERRORLEVEL% neq 0 (
    echo.
    echo ❌ 錯誤: 無法啟動 Docker 容器，請確認 Docker Desktop 已正常啟動。
    pause
    exit /b %ERRORLEVEL%
)
echo ✅ Docker 容器已啟動。
echo.

:: 等待 Spring Boot 服務就緒
echo ⏳ 正在等待 Spring Boot 服務就緒 (http://localhost:8080)...
:wait_loop
curl -s -I http://localhost:8080/swagger-ui/index.html >nul
if %ERRORLEVEL% neq 0 (
    timeout /t 5 /nobreak >nul
    goto wait_loop
)
echo ✅ Spring Boot 服務已就緒！
echo.

:: 2. 建置 .NET 10 測試專案
echo 📦 [Step 2] 正在建置 C# .NET 10 BDD 測試專案...
cd testCase
dotnet build
if %ERRORLEVEL% neq 0 (
    echo.
    echo ❌ 錯誤: C# 測試專案建置失敗！
    cd ..
    pause
    exit /b %ERRORLEVEL%
)
echo ✅ C# 測試專案建置成功。
echo.

:: 3. 執行 .NET BDD 測試
echo 🏃 [Step 3] 正在執行 C# .NET 10 BDD 整合測試場景...
dotnet test --logger "console;verbosity=normal"
set TEST_STATUS=%ERRORLEVEL%
echo.

cd ..

if %TEST_STATUS% equ 0 (
    echo ==================================================
    echo 🎉 恭喜！所有 ShardingSphere BDD 整合測試場景皆已順利通過！
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
