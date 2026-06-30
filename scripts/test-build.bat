@echo off
cd /d "%~dp0\.."
dotnet restore AccyourateEnterpriseX.sln
if errorlevel 1 pause & exit /b 1
dotnet build AccyourateEnterpriseX.sln
pause
