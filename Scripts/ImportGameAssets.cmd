@echo off
powershell.exe -ExecutionPolicy Bypass -File "%~dp0ImportGameAssets.ps1"
if %ERRORLEVEL% NEQ 0 (
	pause
)