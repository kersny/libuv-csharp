echo off
IF %1.==. GOTO Build
IF %1==clean (
	GOTO Clean
) ELSE (
	GOTO Build
)
:Build
	for /f %%b IN ('msbuildpath.exe') do %%bMSBuild.exe libuv\msvs\libuv.sln
	call "%VS100COMNTOOLS%vsvars32.bat"
	FOR /F %%a IN ('msbuildpath.exe') DO %%aMSBuild.exe webserver.csproj
	"%VCINSTALLDIR%bin\cl.exe" uv_wrap.c /o build/uvwrap.dll libuv/build/Win32/Debug/libuv.lib ws2_32.lib advapi32.lib /link /DLL /EXPORT:uv_init /EXPORT:manos_uv_tcp_t_size /EXPORT:uv_tcp_init /EXPORT:uv_tcp_listen /EXPORT:uv_accept /EXPORT:manos_uv_read_start /EXPORT:uv_run /EXPORT:manos_uv_tcp_bind
GOTO :EOF
:Clean
	rmdir /S /Q build
	rmdir /S /Q libuv\build
	del /Q uv_wrap.obj
GOTO :EOF