@echo off
cd /d "%~dp0\..\.."

git init
git add .
git commit -m "Initial stable baseline v4.0.0"
git branch -M main
git tag v4.0.0-stable

echo Repository locale creato.
echo Collega GitHub con:
echo git remote add origin https://github.com/TUO-ACCOUNT/AccyourateEnterpriseX.git
echo git push -u origin main
echo git push origin v4.0.0-stable
pause
