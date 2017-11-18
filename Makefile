CSC=xbuild

all: build run
    
build:
	$(CSC) /p:Configuration=Release p2p_irc.sln
	
run:
	cd chat_ui/bin/Release && mono chat_ui.exe

run_console:
	cd p2p_irc/bin/Release && mono p2p_irc.exe

clean:
	rm -fr bin obj chat_ui/bin chat_ui/obj p2p_irc/bin p2p_irc/obj
