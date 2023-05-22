@echo off
start /b /wait build-api.bat
start /b /wait build-client.bat
start /b /wait build-proxy.bat
start cmd /c deploy-all.bat
exit 0
