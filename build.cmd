@echo off
call "C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\Tools\VsDevCmd.bat"
msbuild .\PSAsync.sln
if NOT [%ERRORLEVEL%]==[0] pause