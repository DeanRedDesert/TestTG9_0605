ECHO OFF
ECHO Creating git sub module for game.
FOR %%A IN ("%~dp0.") DO SET RootFolder=%%~dpA
SET LogicRepo=%~1
IF "%LogicRepo%"=="" (
ECHO Enter HTTP for Logic Git Repo (EG. https://github.com/igt-all/midas-logic-XXXXXXXXXXXXXX)
set /p LogicRepo=
)

git -C %RootFolder% submodule add %LogicRepo% Logic
git -C %RootFolder% submodule update --remote