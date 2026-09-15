@echo off
set TargetFramework=%1
set ProjectName=%2

if not exist "..\..\oqtane.framework\Oqtane.Server\bin\Release\%TargetFramework%\de-DE" mkdir "..\..\oqtane.framework\Oqtane.Server\bin\Release\%TargetFramework%\de-DE"
if not exist "..\..\oqtane.framework\Oqtane.Server\bin\Release\%TargetFramework%\fr-FR" mkdir "..\..\oqtane.framework\Oqtane.Server\bin\Release\%TargetFramework%\fr-FR"
if not exist "..\..\oqtane.framework\Oqtane.Server\bin\Release\%TargetFramework%\es-ES" mkdir "..\..\oqtane.framework\Oqtane.Server\bin\Release\%TargetFramework%\es-ES"
if not exist "..\..\oqtane.framework\Oqtane.Server\bin\Release\%TargetFramework%\it-IT" mkdir "..\..\oqtane.framework\Oqtane.Server\bin\Release\%TargetFramework%\it-IT"
XCOPY "..\Client\bin\Release\%TargetFramework%\%ProjectName%.Client.Oqtane.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Release\%TargetFramework%\" /Y
XCOPY "..\Client\bin\Release\%TargetFramework%\%ProjectName%.Client.Oqtane.pdb" "..\..\oqtane.framework\Oqtane.Server\bin\Release\%TargetFramework%\" /Y
XCOPY "..\Server\bin\Release\%TargetFramework%\%ProjectName%.Server.Oqtane.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Release\%TargetFramework%\" /Y
XCOPY "..\Server\bin\Release\%TargetFramework%\%ProjectName%.Server.Oqtane.pdb" "..\..\oqtane.framework\Oqtane.Server\bin\Release\%TargetFramework%\" /Y
XCOPY "..\Shared\bin\Release\%TargetFramework%\%ProjectName%.Shared.Oqtane.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Release\%TargetFramework%\" /Y
XCOPY "..\Shared\bin\Release\%TargetFramework%\%ProjectName%.Shared.Oqtane.pdb" "..\..\oqtane.framework\Oqtane.Server\bin\Release\%TargetFramework%\" /Y
REM Copy localization satellite assemblies for de-DE culture
XCOPY "..\Client\bin\Release\%TargetFramework%\de-DE\%ProjectName%.Client.Oqtane.resources.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Release\%TargetFramework%\de-DE\" /Y /I
XCOPY "..\Server\bin\Release\%TargetFramework%\de-DE\%ProjectName%.Server.Oqtane.resources.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Release\%TargetFramework%\de-DE\" /Y /I
XCOPY "..\Shared\bin\Release\%TargetFramework%\de-DE\%ProjectName%.Shared.Oqtane.resources.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Release\%TargetFramework%\de-DE\" /Y /I
REM Copy localization satellite assemblies for fr-FR culture
XCOPY "..\Client\bin\Release\%TargetFramework%\fr-FR\%ProjectName%.Client.Oqtane.resources.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Release\%TargetFramework%\fr-FR\" /Y /I
XCOPY "..\Server\bin\Release\%TargetFramework%\fr-FR\%ProjectName%.Server.Oqtane.resources.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Release\%TargetFramework%\fr-FR\" /Y /I
XCOPY "..\Shared\bin\Release\%TargetFramework%\fr-FR\%ProjectName%.Shared.Oqtane.resources.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Release\%TargetFramework%\fr-FR\" /Y /I
REM Copy localization satellite assemblies for es-ES culture
XCOPY "..\Client\bin\Release\%TargetFramework%\es-ES\%ProjectName%.Client.Oqtane.resources.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Release\%TargetFramework%\es-ES\" /Y /I
XCOPY "..\Server\bin\Release\%TargetFramework%\es-ES\%ProjectName%.Server.Oqtane.resources.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Release\%TargetFramework%\es-ES\" /Y /I
XCOPY "..\Shared\bin\Release\%TargetFramework%\es-ES\%ProjectName%.Shared.Oqtane.resources.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Release\%TargetFramework%\es-ES\" /Y /I
REM Copy localization satellite assemblies for it-IT culture
XCOPY "..\Client\bin\Release\%TargetFramework%\it-IT\%ProjectName%.Client.Oqtane.resources.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Release\%TargetFramework%\it-IT\" /Y /I
XCOPY "..\Server\bin\Release\%TargetFramework%\it-IT\%ProjectName%.Server.Oqtane.resources.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Release\%TargetFramework%\it-IT\" /Y /I
XCOPY "..\Shared\bin\Release\%TargetFramework%\it-IT\%ProjectName%.Shared.Oqtane.resources.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Release\%TargetFramework%\it-IT\" /Y /I
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
