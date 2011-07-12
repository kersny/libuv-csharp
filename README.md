libuv-sharp
==========

What
----

C# bindings to [libuv][0]... that's about it. Ultimate goal is to provide really good cross-platform evented sockets for [manos][1].

Status
-----

It runs on my machine :)
Right now the server is one file and simply writes what it recieves over tcp to stdout.
Currently the makefile works on OS X and Ubuntu. Windows support is in progress.

Requirements
------------

* [Mono][2] (Tested under 2.10.2)
* gcc

Build and Run
-------------

	make
	mono build/webserver.exe

Somewhere else:

	telnet localhost 8080


TODO
----

* Build on all platforms
* Implement all of the api
* Get rid of memory leaks

[0]: https://www.github.com/joyent/libuv
[1]: https://www.github.com/jacksonh/manos
[2]: http://www.go-mono.com/mono-downloads/download.html
