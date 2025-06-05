ECHO OFF
ECHO Updated the Logic repo to the last version.

FOR %%A IN ("%~dp0.") DO SET RootFolder=%%~dpA

git -C %RootFolder% submodule update --remote