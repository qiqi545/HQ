@ECHO OFF

REM remove all source shadow copies
FOR /D %%p IN ("src\HQ\*.*") DO rmdir "%%p" /s /q
FOR /D %%p IN ("src\HQ.Azure\*.*") DO rmdir "%%p" /s /q