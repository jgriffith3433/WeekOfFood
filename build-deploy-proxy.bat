@echo off
start /b /wait build-proxy.bat
call deploy-proxy.bat
exit 0
