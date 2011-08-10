echo off
IF %1.==. GOTO Build
IF %1==clean (
	GOTO Clean
) ELSE (
	GOTO Build
)
:Build
	del uv.def
	echo LIBRARY UVWRAP >> uv.def
	echo EXPORTS >> uv.def
	set count=0
	setlocal ENABLEDELAYEDEXPANSION
	for /f "tokens=1,2,3,4,5 delims=( " %%a in ('findstr "extern" src\Libuv\*.cs src\Libuv.Tests\*.cs') do (
		set /a count=count+1
		echo %%e @!count! >> uv.def
	)
	call "deps\libuv\create-msvs-files.bat"
	cd ../..
	for /f %%b IN ('msbuildpath.exe') do %%bMSBuild.exe deps\libuv\build\all.sln
	call "%VS100COMNTOOLS%vsvars32.bat"
	FOR /F %%a IN ('msbuildpath.exe') DO %%aMSBuild.exe src\Libuv.sln
	"%VCINSTALLDIR%bin\cl.exe" /o build\uv.dll deps\libuv\build\Debug\obj\uv\*.obj ws2_32.lib advapi32.lib /link /DLL /DEF:uv.def
GOTO Done
:Clean
	rmdir /S /Q deps\libuv\build\Debug
	rmdir /S /Q build
	del /Q uv_wrap.obj
	del /Q uv_wrap.def
GOTO Done
:Done
echo on
