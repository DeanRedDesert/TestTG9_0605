@if not defined _echo echo off
rem reference https://github.com/microsoft/vswhere/wiki/Start-Developer-Command-Prompt
for /f "usebackq delims=" %%i in (`"%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe" -prerelease -latest -requires Microsoft.Component.MSBuild -products * -property installationPath`) do (
  if exist "%%i\Common7\Tools\vsdevcmd.bat" (
    call "%%i\Common7\Tools\vsdevcmd.bat"
    goto Build
  )
)

rem Instance or command prompt not found
echo No msbuild instance is found on this computer. >&2
exit /b 2

:Build
call msbuild %* /p:OutputPath=..\.Exe\ /p:IntermediateOutputPath=..\.Exe\Obj\
rmdir ..\.Exe\Obj\ /s /q

pause

exit /b %ERRORLEVEL%