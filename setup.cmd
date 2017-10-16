@echo off
if "%1" == "" (
    call build\install\setup.cmd /shortcut
	if %errorlevel% NEQ 0 exit /b %errorlevel%
	
	echo Press enter to close this windows and then open the environment using the desktop shortcut
	set /p =
	exit
)

if "%1" == "/snippets" (
	call build\install\setup.cmd /snippets
	goto :eof
)

echo Unknown parameters