@echo off

:: restart IRC Bot service
:: (clear text file before to except duplicate message)

SET scriptpath=%~dp0

break>%scriptpath:~0,-1%\message.txt
net stop IRCCD
net start IRCCD