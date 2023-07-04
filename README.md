# Network Test
## Description
This program is meant to assist with testing how stable an internet connection is. The basis is simple, provide an IP Address to the console (ex. google is 8.8.8.8) and then run the program.
It will run a continuous ping of that IP Address for as long as you leave the program to be running. Once you end the program, it will provide a report containing every instance of pings timing out, and they times those time outs started.

## Known Bugs
None at the moment.

## To-Do
	Rework how the dropped pings show in the report
		- Research maybe only showing the longest ping time out duration times?
	Look into adding a timer set by user to let the program be automated
	Look into additional data that may be useful in the report
	Research the different Ping Responses in order to take those into account