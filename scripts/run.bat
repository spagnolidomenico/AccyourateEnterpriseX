@echo off
cd /d "%~dp0\.."
dotnet run --project src\Accyourate.App\Accyourate.App.csproj
pause
