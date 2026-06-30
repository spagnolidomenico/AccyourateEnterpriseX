@echo off
cd /d "%~dp0\.."
dotnet clean AccyourateEnterpriseX.sln
dotnet restore AccyourateEnterpriseX.sln
if errorlevel 1 pause & exit /b 1
dotnet build AccyourateEnterpriseX.sln
if errorlevel 1 pause & exit /b 1
dotnet run --project src\Accyourate.App\Accyourate.App.csproj
pause
