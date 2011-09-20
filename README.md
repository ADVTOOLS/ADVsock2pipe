# What is ADVsock2pipe?

ADVsock2pipe is a small utility to connect a TCP socket to a Windows named pipe. It can be used, for example, to capture network data with tcpdump on Linux or iPhone/iPad and to see the capture in (almost) real-time in Wireshark on Windows.

# How to use ADVsock2pipe?

For example, to capture data on a device (10.0.0.1) and send it to Wireshark on Windows (10.0.0.99) on port 7777:

On the Windows workstation (10.0.0.99):

- `ADVsock2pipe -pipe=wireshark -p 7777`
- Start Wireshark
- Capture | Options, Interface: Local, \\\\.\pipe\wireshark
- Start

On the device:

- `tcpdump -nn -w - -U -s 0 "not port 7777" | nc 10.0.0.99 7777`

# Why do I get errors in Wireshark when a stop a capture and start a new one?

This is because Wireshark is either expecting a header and does not receive one or is receiving a header when it does not expect one: Wireshark expect data in pcap format, a pcap header follows by packet data. If you stop and start tcpdump, Wireshark receives a pcap header in the middle of a capture and thus generates an error.

So to avoid any problem:

- Always start Wireshark before starting tcpdump.
- Each time you stop tcpdump, stop Wireshark. 
- And each time you stop Wireshark, stop tcpdump.

# What are the command line options?

--pipe=<name> where <name> is the name of the Windows pipe.
--port=<port> where <port> is the TCP port number.
--log=<level> where <level> is Debug, Info, Warning, Error or Critical. Determine the level of details given by this tool.
--help or -h to get some help about this tool.

# How to build ADVsock2pipe?

In order to build ADVsock2pipe, you need to have Visual Studio 2010 or 2011 Developer Preview. Open the solution (ADVsock2pipe.sln) and build it.

# References

[Wireshark Wiki - CaptureSetup/Pipes](http://wiki.wireshark.org/CaptureSetup/Pipes)

# Copyright and license

Copyright (c) 2011 - [ADVTOOLS SARL](http://www.advtools.com)
 
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.
