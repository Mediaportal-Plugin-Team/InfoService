@echo off
cls
Title Creating InfoService Installer

IF "%programfiles(x86)%XXX"=="XXX" GOTO 32BIT
    :: 64-bit
    SET PROGS=%programfiles(x86)%
    GOTO CONT
:32BIT
    SET PROGS=%ProgramFiles%
:CONT

IF NOT EXIST "%PROGS%\Team MediaPortal\MediaPortal\" SET PROGS=C:

:: Get version from DLL
FOR /F "tokens=*" %%i IN ('..\Tools\Tools\sigcheck.exe /accepteula /nobanner /n "..\InfoService\InfoService\bin\Release\InfoService.dll"') DO (SET version=%%i)

:: Temp xmp2 file
COPY ..\Installer\InfoService.xmp2 ..\Installer\InfoServiceTemp.xmp2

:: Build MPE1
CD ..\Installer
"%PROGS%\Team MediaPortal\MediaPortal\MPEMaker.exe" ..\Installer\InfoServiceTemp.xmp2 /B /V=%version% /UpdateXML
CD ..\build

:: Cleanup
del ..\Installer\InfoServiceTemp.xmp2
