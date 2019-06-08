@ECHO OFF

REM remove all source shadow copies
FOR /D %%p IN ("src\HQ\*.*") DO rmdir "%%p" /s /q

REM need to move into the directory for MSBuild parameters to work
cd src

REM build once for merge source, and again for compile
START /WAIT dotnet build -c Debug
START /WAIT dotnet build -c Debug

cd..