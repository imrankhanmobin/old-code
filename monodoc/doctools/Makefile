# make libmonodoc and monodoc-qt.exe

DIRS = lib gtk-monodoc mkmaster

all:
	for dir in $(DIRS); do $(MAKE) -C $$dir $@; done

clean:
	for dir in $(DIRS); do $(MAKE) -C $$dir $@; done

install:
	for dir in $(DIRS); do $(MAKE) -C $$dir $@; done

uninstall:
	for dir in $(DIRS); do $(MAKE) -C $$dir $@; done

.PHONY: all clean install uninstall
