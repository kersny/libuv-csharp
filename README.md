libuv-sharp
==========

What
----

C# bindings to [libuv][0]... that's about it. Ultimate goal is to provide really good cross-platform evented sockets for [manos][1].

Status
-----

Builds and runs on Windows, Mac and Linux.

Requirements
------------

### *nixes

* [Mono][2] (Tested under 2.10.2)
* gcc

### Windows

* MSVC 2010 (Not tested with the express edition, but it should work)
* MSVC# 2010

Build and Run
-------------

### Get it

	git pull git://github.com/kersny/libuv-csharp.git
	git submodule update --init

### *nixes

	make
	mono build/Libuv.Tests.exe
	(to clean) make clean

### Windows

	winbuild.bat
	cd build
	Libuv.Tests.exe
	(to clean) winbuild.bat clean



Somewhere else:

	telnet localhost 8080


TODO
----

* Test for Memory Leaks
* Fix Child Processes
* Proper test suite to make bumping UV Version easier
* Auto-generated struct sizes for all platforms

[0]: https://www.github.com/joyent/libuv
[1]: https://www.github.com/jacksonh/manos
[2]: http://www.go-mono.com/mono-downloads/download.html
