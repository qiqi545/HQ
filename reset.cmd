@ECHO OFF

REM resets all shadow copied files after a source deployment

REM update all submodules to use tips of master branches
git submodule foreach git pull origin master

REM recursively update the tips of master branches
git submodule update --recursive --remote

REM remove all source shadow copies
FOR /D %%p IN ("src\HQ\*.*") DO rmdir "%%p" /s /q