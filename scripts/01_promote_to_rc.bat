@echo off
title Promote to RC
cd /d "%~dp0\.."

echo Questo script prepara una cartella Release Candidate locale.
echo Eseguire prima scripts\00_test_full.bat
echo.

if not exist releases\rc mkdir releases\rc

for /f "tokens=1-3 delims=/ " %%a in ("%date%") do set d=%%c-%%b-%%a
set name=AccyourateEnterpriseX_1_1_0_RC_%d%

if exist "releases\rc\%name%" rmdir /s /q "releases\rc\%name%"
mkdir "releases\rc\%name%"

xcopy src "releases\rc\%name%\src" /E /I /Y
xcopy docs "releases\rc\%name%\docs" /E /I /Y
xcopy scripts "releases\rc\%name%\scripts" /E /I /Y
copy AccyourateEnterpriseX.sln "releases\rc\%name%\"
copy README.md "releases\rc\%name%\"
copy VERSION.txt "releases\rc\%name%\"
copy manifest.json "releases\rc\%name%\"

echo.
echo RC preparata in releases\rc\%name%
pause
