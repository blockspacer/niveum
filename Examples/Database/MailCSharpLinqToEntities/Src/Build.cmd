PATH %SystemRoot%\Microsoft.NET\Framework\v4.0.30319;%PATH%

MSBuild Database.sln /t:Rebuild /p:Configuration=Release
pause