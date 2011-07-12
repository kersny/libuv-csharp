CSBUILD=xbuild
UVFLAGS=CPPFLAGS="-m32"
CC=$(PREFIX)gcc

STATIC_LIBRARY=so

ifeq (Darwin,$(uname_S))
STATIC_LIBRARY=dylib
endif

so=-shared
dylib=-dynamiclib
