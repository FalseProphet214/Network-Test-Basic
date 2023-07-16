# Network Test
## Description
This program is meant to assist with testing how stable an internet connection is. The basis is simple, provide an IP Address to the console (ex. google is 8.8.8.8) and then run the program.
It will first run a tracert command to show what all the ping command will be hitting to get to the IP address you have put in. Then it will run the continuous ping of whatever IP Address you have entered. In the event that pings begin to time out, it will run additional tracert commands in the background in an attempt to show where the pings are failing and to provide additional information for troubleshooting.

## Known Bugs
When entering a timeout period for ping (the -w argument), it is kicking the user input to the next line.
No clue what that is about.

## To-Do
	Look into adding a timer set by user to let the program be automated - Complete
	Look into additional data that may be useful in the report
	Research the different Ping Responses in order to take those into account
	Allow user set timeout for pings - Complete
	Add code to open report as soon as test is done - Complete
	Add code to run a TraceRT as soon as pings start timing out - Complete