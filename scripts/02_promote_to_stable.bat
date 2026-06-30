@echo off
title Promote to Stable
cd /d "%~dp0\.."

echo Questo script prepara una cartella Stable locale.
echo Usare solo dopo collaudo RC positivo.
echo.

if not exist releases\stable mkdir releases\stable

set name=AccyourateEnterpriseX_1_1_0_Stable

if exist "releases\stable\%name%" rmdir /s /q "releases\stable\%name%"
mkdir "releases\stable\%name%"

xcopy src "releases\stable\%name%\src" /E /I /Y
xcopy docs "releases\stable\%name%\docs" /E /I /Y
xcopy scripts "releases\stable\%name%\scripts" /E /I /Y
copy AccyourateEnterpriseX.sln "releases\stable\%name%\"
copy README.md "releases\stable\%name%\"
copy VERSION.txt "releases\stable\%name%\"
copy manifest.json "releases\stable\%name%\"

echo.
echo Stable preparata in releases\stable\%name%
pause
