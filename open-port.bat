@echo off
netsh advfirewall firewall add rule name="CompsoVarUnity_Port4210" dir=in action=allow protocol=UDP localport=4210
pause