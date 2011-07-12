uname_S := $(shell sh -c 'uname -s 2>/dev/null || echo not')

ifdef MSVC
	uname_S := MINGW
endif

ifneq (,$(findstring MINGW,$(uname_S)))
	include config-mingw.mk
else
	include config-unix.mk
endif

all: build/webserver.exe build/libuvwrap.$(STATIC_LIBRARY)

build/webserver.exe: webserver.cs webserver.csproj
	$(CSBUILD) webserver.csproj

libuv/uv.a:
	$(UVFLAGS) $(MAKE) -C libuv

build/libuvwrap.%: libuv/uv.a uv_wrap.c
	$(CC) $($(STATIC_LIBRARY)) uv_wrap.c -m32 -o build/libuvwrap.$(STATIC_LIBRARY) libuv/uv.a 

clean:
	$(RM) -r build/
	$(RM) uv_wrap.c.*
	$(MAKE) -C libuv clean
