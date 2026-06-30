@echo off
title Accyourate Enterprise X - Full Test
cd /d "%~dp0\..\.."

dotnet clean AccyourateEnterpriseX.sln
if errorlevel 1 goto error

dotnet restore AccyourateEnterpriseX.sln
if errorlevel 1 goto error

dotnet build AccyourateEnterpriseX.sln
if errorlevel 1 goto error

dotnet run --project src\Accyourate.App\Accyourate.App.csproj
goto end

:error
echo ERRORE: test interrotto.
pause
exit /b 1

:end
pause
