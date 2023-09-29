# Network Test
## Description
This program is meant to assist with testing how stable an internet connection is. The basis is simple, provide an IP Address to the console (ex. google is 8.8.8.8) and then run the program. It will also have you name your report for ease later
It will first run a tracert command to show what all the ping command will be hitting to get to the IP address you have put in. Then it will run the continuous ping of whatever IP Address you have entered. In the event that pings begin to time out, it will run additional tracert commands in the background in an attempt to show where the pings are failing and to provide additional information for troubleshooting.
Once your command ends, whether it hit its time limit or you end it manually, the report will save to your desktop and open immediately for your review.

## Known Bugs
When entering a timeout period for ping (the -w argument), it is kicking the user input to the next line.
No clue what that is about.

## To-Do
	Look into additional data that may be useful in the report
	Research the different Ping Responses in order to take those into account
	