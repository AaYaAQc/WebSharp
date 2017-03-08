@echo off
set SELF=%~dp0

set FLAVOR=%1

if "%FLAVOR%" equ "" set FLAVOR=Release

mkdir "%SELF%\build\nuget\content\websharp" > nul 2>&1
mkdir "%SELF%\build\nuget\tools" > nul 2>&1

if not exist "%SELF%\build\download.exe" (
	csc /out:"%SELF%\build\download.exe" "%SELF%\download.cs"
)

if not exist "%SELF%\build\nuget.exe" (
	"%SELF%\build\download.exe" http://nuget.org/nuget.exe "%SELF%\build\nuget.exe"
	"%SELF%\build\nuget.exe" update -self
)

rem csc /out:"%SELF%\..\src\websharpjs\WebSharp.js\bin\Release\net451\WebSharpJs.dll" /target:library "%SELF%\..\src\websharpjs\WebSharp.js\dotnet\*.cs" "%SELF%\..\src\websharpjs\WebSharp.js\dotnet\WebSharpJs.Browser\*.cs"
rem if %ERRORLEVEL% neq 0 exit /b -1

cd "%SELF%\..\src\websharpjs\WebSharp.js"
dotnet restore WebSharp.js.sln /p:Configuration=%FLAVOR% /p:Platform="Any CPU"
if %ERRORLEVEL% neq 0 exit /b -1
dotnet build WebSharp.js.sln /p:Configuration=%FLAVOR% /p:Platform="Any CPU"
if %ERRORLEVEL% neq 0 exit /b -1
dotnet pack WebSharp.js.sln /p:Configuration=%FLAVOR% /p:Platform="Any CPU" /p:IncludeSymbols=true

if %ERRORLEVEL% neq 0 (
	echo Failure building Nuget package
	cd "%SELF%"
	exit /b -1
)

cd "%SELF%"
copy /y "%SELF%\..\src\websharpjs\WebSharp.js\bin\%FLAVOR%\*.nupkg" "%SELF%\build\nuget"
rem Make it available to the electron-dotnet module
copy /y "%SELF%\..\src\websharpjs\WebSharp.js\bin\%FLAVOR%\net451\*.dll" "%SELF%\..\lib\bin"
echo SUCCESS. Nuget package at %SELF%\build\nuget

exit /b 0
