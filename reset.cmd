@ECHO OFF

REM remove all source shadow copies
FOR /D %%p IN ("src\HQ\*.*") DO rmdir "%%p" /s /q
FOR /D %%p IN ("src\HQ.DocumentDb\*.*") DO rmdir "%%p" /s /q
FOR /D %%p IN ("src\HQ.MySql\*.*") DO rmdir "%%p" /s /q
FOR /D %%p IN ("src\HQ.Sqlite\*.*") DO rmdir "%%p" /s /q
FOR /D %%p IN ("src\HQ.SqlServer\*.*") DO rmdir "%%p" /s /q