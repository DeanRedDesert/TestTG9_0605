ECHO off

@FOR %%A IN ("%~dp0.") DO SET RootFolder=%%~dpA

echo Building Game Logic Files
@dotnet build -c Release %RootFolder%Logic\GLE\logic.sln > nul

@mkdir "%RootFolder%\Temp"

REM Run the code gen
"%RootFolder%Logic\GLE\Game\dlls\gen.exe" "%RootFolder%Logic\GLE" "%RootFolder%Temp" /source /registries /documentation /game_info:"%RootFolder%GameInfo.json"

@rmdir /s /q "%RootFolder%\Game\Registries" > nul
@rmdir /s /q "%RootFolder%\Documentation" > nul
@rmdir /s /q "%RootFolder%\Logic\GLE\Logic\Obj" > nul
@rmdir /s /q "%RootFolder%\Logic\GLE\Gaff\Obj" > nul

REM Copy the generated game files
@xcopy "%RootFolder%\Temp\*.*" "%RootFolder%\Game\Assets\GleLogic\Game\" /f /y 
@xcopy "%RootFolder%\Temp\Registries\*.*" "%RootFolder%\Game\Registries\" /f /y 
@xcopy "%RootFolder%\Temp\Documentation\*.*" "%RootFolder%\Documentation\" /f /y 

REM Need to remove old files and empty folders
@rmdir /s /q "%RootFolder%\Temp" > nul
@del /s /q "%RootFolder%\Game\Assets\GleLogic\Logic\*.cs" > nul
@del /s /q "%RootFolder%\Game\Assets\GleLogic\Gaff\*.cs" > nul

@xcopy "%RootFolder%\Logic\GLE\Gaff\*.cs" "%RootFolder%\Game\Assets\GleLogic\Gaff\" /s /f /y
@xcopy "%RootFolder%\Logic\GLE\Logic\*.cs" "%RootFolder%\Game\Assets\GleLogic\Logic\" /s /f /y 

@FOR %%A IN ("%~dp0.") DO SET RootFolder=%%~dpA

@for /r "%RootFolder%\Game\Assets\GleLogic\Logic\" %%i in (*.meta) do (
@if not exist %%~di%%~pi%%~ni (
	@del %%i
	@echo Removed %%i))

@for /r "%RootFolder%\Game\Assets\GleLogic\Gaff\" %%i in (*.meta) do (
@if not exist %%~di%%~pi%%~ni (
	@del %%i
	@echo Removed %%i))


