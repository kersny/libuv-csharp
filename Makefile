#####
uname_S := $(shell sh -c 'uname -s 2>/dev/null || echo not')

CSBUILD=xbuild
UVFLAGS=CPPFLAGS="-m32"
CC=$(PREFIX)gcc

STATIC_LIBRARY=so

ifeq (Darwin,$(uname_S))
STATIC_LIBRARY=dylib
endif

so=-shared
dylib=-dynamiclib
#####
TEST_DIRS = src/Libuv.Tests
TEST_DEPS := $(foreach dir, $(TEST_DIRS), $(wildcard $(dir)/*))
LIBUV_DIRS = src/Libuv
LIBUV_DEPS := $(foreach dir, $(LIBUV_DIRS), $(wildcard $(dir)/*))

all: build/Libuv.Tests.exe

build/Libuv.dll: build/libuvwrap.$(STATIC_LIBRARY) $(LIBUV_DEPS)
	$(CSBUILD) src/Libuv.sln /target:Libuv

build/Libuv.Tests.exe: build/Libuv.dll $(TEST_DEPS)
	$(CSBUILD) src/Libuv.sln

deps/libuv/uv.a:
	mkdir build/
	$(UVFLAGS) $(MAKE) -C deps/libuv

build/libuvwrap.%: deps/libuv/uv.a src/wrapper/uv_wrap.c
	$(CC) $($(STATIC_LIBRARY)) src/wrapper/uv_wrap.c -m32 -o build/libuvwrap.$(STATIC_LIBRARY) deps/libuv/uv.a 

clean:
	$(RM) -r build/
	$(MAKE) -C deps/libuv clean
