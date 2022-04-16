# ReverseShell_NC
This repository contains a program that gives remote shell connection on the sockets back to the c2 server. This connection can be listened on Netcat as well. It is just a PoC for getting reverse shells by sending input messages, that are commands (from c2) and getting back output messages that are outputs or errors of those commands.

This program can be used as a remote access trojan which communicates on sockets and provides a reverse shell. The reverse shell listener could be a custom created server or even netcat. In this PoC, i've used netcat as a c2 server for my reverse shell. We can specify any ip address and port number for our c2 server in this program. I've setup a kali machine on my local network and used netcat to listen and pass commands to this reverse shell.

for setting c2 using netcat:
1) install net-cat in your machine
2) type this command: nc -l -p 4444
3) Port can be changed as you want, but must change it in the source code as well.
4) Ip address will be the address of machine that your're using netcat.

DISCLAIMER: Educational purposes only!
