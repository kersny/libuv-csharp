echo off
IF %1.==. GOTO Build
IF %1==clean (
	GOTO Clean
) ELSE (
	GOTO Build
)
:Build
	del src\wrapper\uv_wrap.def
	echo LIBRARY UVWRAP >> src\wrapper\uv_wrap.def
	echo EXPORTS >> src\wrapper\uv_wrap.def
	set count=0
	setlocal ENABLEDELAYEDEXPANSION
	for /f "tokens=1,2,3,4,5 delims=( " %%a in ('findstr "extern" src\Libuv\*.cs src\Libuv.Tests\*.cs') do (
		set /a count=count+1
		echo %%e @!count! >> src\wrapper\uv_wrap.def
	)
	for /f %%b IN ('msbuildpath.exe') do %%bMSBuild.exe deps\libuv\msvs\libuv.sln
	call "%VS100COMNTOOLS%vsvars32.bat"
	FOR /F %%a IN ('msbuildpath.exe') DO %%aMSBuild.exe src\Libuv.sln
	"%VCINSTALLDIR%bin\cl.exe" src\wrapper\uv_wrap.c /o build\uvwrap.dll deps\libuv\build\Win32\Debug\libuv.lib ws2_32.lib advapi32.lib /link /DLL /DEF:src\wrapper\uv_wrap.def
GOTO Done
:Clean
	rmdir /S /Q build
	rmdir /S /Q deps\libuv\build
	del /Q uv_wrap.obj
	del /Q src\wrapper\uv_wrap.def
GOTO Done
:Done
echo on