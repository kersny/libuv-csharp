all: build/webserver.exe build/libuvwrap.dylib

build/webserver.exe: webserver.cs webserver.csproj
	xbuild

libuv/uv.a:
	CPPFLAGS=-m32 $(MAKE) -C libuv

build/libuvwrap.dylib: libuv/uv.a uv_wrap.c
	#ar -x libuv/uv.a
	$(CC) -dynamiclib uv_wrap.c -m32 -o build/libuvwrap.dylib libuv/uv.a 
	#$(RM) *.o __.SYMDEF\ SORTED


clean:
	$(RM) -rf build/
	$(MAKE) -C libuv clean
