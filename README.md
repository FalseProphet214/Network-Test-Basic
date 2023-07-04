# Network Test
## Description
This program is meant to assist with testing how stable an internet connection is. The basis is simple, provide an IP Address to the console (ex. google is 8.8.8.8) and then run the program.
It will run a continuous ping of that IP Address for as long as you leave the program to be running. Once you end the program, it will provide a report containing every instance of pings timing out, and they times those time outs started.

## Known Bugs
Currently, the line for "Longest Timeout Period:" is wonky. Even if you do not have any pings time out, it will still show a time. In addition, the times are always wrong.	For example, if I had a ping time out at 14:29:56, and then a ping is sucessfull immediately after that at 14:30:06, then we had a time out duration of roughly half a second. However, the report would show a difference of several hours rather than less than a second.
	> -Researching Fix-
	I believe I have the fix for this inplemented, however I discovered that I am not accounting for other events in which pings are not returning, such as "Destination Host Unreachable"

## To-Do
-Fix the bug with the Longest Timeout Period-
Rework how the dropped pings show in the report
	- Research maybe only showing the longest ping time out duration times?
Look into adding a timer set by user to let the program be automated
Look into additional data that may be useful in the report
Add code for user to name the report