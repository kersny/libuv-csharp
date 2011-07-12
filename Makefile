uname_S := $(shell sh -c 'uname -s 2>/dev/null || echo not')

ifdef MSVC
	uname_S := MINGW
endif

ifneq (,$(findstring MINGW,$(uname_S)))
	include config-mingw.mk
else
	include config-unix.mk
endif

all: build/webserver.exe build/libuvwrap.dylib

build/webserver.exe: webserver.cs webserver.csproj
	$(CSBUILD) webserver.csproj

libuv/uv.a:
	$(UVFLAGS) $(MAKE) -C libuv

build/libuvwrap.dylib: libuv/uv.a uv_wrap.c
	$(CC) -dynamiclib uv_wrap.c -m32 -o build/libuvwrap.dylib libuv/uv.a 
	$(CC) -shared uv_wrap.c -m32 -o build/libuvwrap.so libuv/uv.a 

clean:
	$(RM) -rf build/
	$(MAKE) -C libuv clean
