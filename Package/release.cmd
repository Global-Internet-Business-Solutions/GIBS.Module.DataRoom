@echo off
set TargetFramework=%1
set ProjectName=%2

if not exist "..\..\oqtane.framework\Oqtane.Server\bin\Release\%TargetFramework%\it" mkdir "..\..\oqtane.framework\Oqtane.Server\bin\Release\%TargetFramework%\it"
if not exist "..\..\oqtane.framework\Oqtane.Server\bin\Release\%TargetFramework%\it-IT" mkdir "..\..\oqtane.framework\Oqtane.Server\bin\Release\%TargetFramework%\it-IT"
XCOPY "..\Client\bin\Release\%TargetFramework%\%ProjectName%.Client.Oqtane.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Release\%TargetFramework%\" /Y
XCOPY "..\Client\bin\Release\%TargetFramework%\%ProjectName%.Client.Oqtane.pdb" "..\..\oqtane.framework\Oqtane.Server\bin\Release\%TargetFramework%\" /Y
XCOPY "..\Server\bin\Release\%TargetFramework%\%ProjectName%.Server.Oqtane.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Release\%TargetFramework%\" /Y
XCOPY "..\Server\bin\Release\%TargetFramework%\%ProjectName%.Server.Oqtane.pdb" "..\..\oqtane.framework\Oqtane.Server\bin\Release\%TargetFramework%\" /Y
XCOPY "..\Shared\bin\Release\%TargetFramework%\%ProjectName%.Shared.Oqtane.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Release\%TargetFramework%\" /Y
XCOPY "..\Shared\bin\Release\%TargetFramework%\%ProjectName%.Shared.Oqtane.pdb" "..\..\oqtane.framework\Oqtane.Server\bin\Release\%TargetFramework%\" /Y
REM Copy localization satellite assemblies for it culture
XCOPY "..\Client\bin\Release\%TargetFramework%\it\%ProjectName%.Client.Oqtane.resources.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Release\%TargetFramework%\it\" /Y /I
REM Copy localization satellite assemblies for it-IT culture
XCOPY "..\Client\bin\Release\%TargetFramework%\it-IT\%ProjectName%.Client.Oqtane.resources.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Release\%TargetFramework%\it-IT\" /Y /I
XCOPY "..\Server\wwwroot\*" "..\..\oqtane.framework\Oqtane.Server\wwwroot\_content\%ProjectName%\" /Y /S /I

set PackageOutputDir=%CD%
set PackageFallbackDir=%CD%\nupkg-temp

del "%ProjectName%.*.nupkg" 2>nul
if exist "%ProjectName%.*.nupkg" (
  echo Existing package file is locked. Using fallback output folder: %PackageFallbackDir%
  if not exist "%PackageFallbackDir%" mkdir "%PackageFallbackDir%"
  del "%PackageFallbackDir%\%ProjectName%.*.nupkg" 2>nul
  set PackageOutputDir=%PackageFallbackDir%
)

"..\..\oqtane.framework\oqtane.package\FixProps.exe"
"..\..\oqtane.framework\oqtane.package\nuget.exe" pack %ProjectName%.nuspec -Properties targetframework=%TargetFramework%;projectname=%ProjectName% -OutputDirectory "%PackageOutputDir%"
