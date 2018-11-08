@ECHO OFF

REM update all submodules to use tips of master branches
git submodule foreach git pull origin master

REM recursively update the tips of master branches
git submodule update --recursive --remote

REM Remove all source shadow copies
FOR /D %%p IN ("D:\Src\HQ\HQ\src\HQ\*.*") DO rmdir "%%p" /s /q