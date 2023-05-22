@echo off
start /b /wait build-api.bat
call deploy-api.bat
exit 0
