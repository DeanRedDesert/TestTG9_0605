ECHO OFF
ECHO Swap branch for Logic repo.
SET Branch=%~1
IF "%Branch%"=="" (
ECHO Enter branch name
set /p Branch=
)

FOR %%A IN ("%~dp0.") DO SET RootFolder=%%~dpA

cd..
git -C %RootFolder% submodule set-branch -b %Branch% Logic
git -C %RootFolder% submodule sync
git -C %RootFolder% submodule update --init --recursive --remote