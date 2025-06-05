ECHO OFF
ECHO Reset to master branch for the Logic submodule

FOR %%A IN ("%~dp0.") DO SET RootFolder=%%~dpA

git -C %RootFolder% submodule set-branch -d  Logic
git -C %RootFolder% submodule sync
git -C %RootFolder% submodule update --init --recursive --remote