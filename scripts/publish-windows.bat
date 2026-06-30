@echo off
cd /d "%~dp0\.."
dotnet publish src\Accyourate.App\Accyourate.App.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish\win-x64
pause
