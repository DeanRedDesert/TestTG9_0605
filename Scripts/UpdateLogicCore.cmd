@ECHO off

@FOR %%A IN ("%~dp0.") DO SET RootFolder=%%~dpA

@set filename=Logic\version.txt
@set searchString=GLE:

@set foundStr=findstr /C:"%searchString%" "%RootFolder%%filename%"

@for /f "tokens=1,2 delims=:" %%i in ('FINDSTR /C:"GLE:" "%RootFolder%%filename%"') do set GLEVersion=%%j

IF "%GLEVersion:~0,9%"=="dev-team-" (
	set GLECoreSource="%LocalAppData%\Programs\GameHub\gle\Gle %GLEVersion%\coresource.zip"
	echo %GLEVersion%> "%RootFolder%\Game\LogicVersion.txt"
	@echo GLE Development Version---%GLEVersion%
) ELSE (
	set GLECoreSource="%LocalAppData%\Programs\GameHub\gle\Gle rel-v%GLEVersion%\coresource.zip"
	echo rel-v%GLEVersion%> "%RootFolder%\Game\LogicVersion.txt"
	@echo GLE Release Version---%GLEVersion%
)

@echo GLE File---%GLECoreSource%

if not exist %GLECoreSource% (
	echo Can not locate GLE Version %GLEVersion%. Please open game hub and download the specified GLE version.
	goto end
)

@del /s /q "%RootFolder%\Game\Assets\GleLogic\Logic.Core\*.cs" > nul
@del /s /q "%RootFolder%\Game\Assets\GleLogic\Gaff.Core\*.cs" > nul

echo Update Core Source to "%RootFolder%\Game\Assets\GleLogic"
tar -xf %GLECoreSource% -C "%RootFolder%\Game\Assets\GleLogic"

for /r "%RootFolder%\Game\Assets\GleLogic\Logic.Core\" %%i in (*.meta) do (
	@if not exist %%~di%%~pi%%~ni (
		@echo %%~di%%~pi%%~ni
		@del "%%i"
		@echo Removed "%%i"))

for /r "%RootFolder%\Game\Assets\GleLogic\Gaff.Core\" %%i in (*.meta) do (
	@if not exist %%~di%%~pi%%~ni (
		@del "%%i"
		@echo Removed "%%i"))

:end