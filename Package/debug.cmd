@echo off
set TargetFramework=%1
set ProjectName=%2

XCOPY "..\Client\bin\Debug\%TargetFramework%\%ProjectName%.Client.Oqtane.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\%TargetFramework%\" /Y
XCOPY "..\Client\bin\Debug\%TargetFramework%\%ProjectName%.Client.Oqtane.pdb" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\%TargetFramework%\" /Y
XCOPY "..\Server\bin\Debug\%TargetFramework%\%ProjectName%.Server.Oqtane.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\%TargetFramework%\" /Y
XCOPY "..\Server\bin\Debug\%TargetFramework%\%ProjectName%.Server.Oqtane.pdb" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\%TargetFramework%\" /Y
XCOPY "..\Shared\bin\Debug\%TargetFramework%\%ProjectName%.Shared.Oqtane.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\%TargetFramework%\" /Y
XCOPY "..\Shared\bin\Debug\%TargetFramework%\%ProjectName%.Shared.Oqtane.pdb" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\%TargetFramework%\" /Y
REM Copy localization satellite assemblies for de-DE culture
XCOPY "..\Client\bin\Debug\%TargetFramework%\de-DE\%ProjectName%.Client.Oqtane.resources.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\%TargetFramework%\de-DE\" /Y /I
XCOPY "..\Server\bin\Debug\%TargetFramework%\de-DE\%ProjectName%.Server.Oqtane.resources.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\%TargetFramework%\de-DE\" /Y /I
XCOPY "..\Shared\bin\Debug\%TargetFramework%\de-DE\%ProjectName%.Shared.Oqtane.resources.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\%TargetFramework%\de-DE\" /Y /I
REM Copy localization satellite assemblies for fr-FR culture
XCOPY "..\Client\bin\Debug\%TargetFramework%\fr-FR\%ProjectName%.Client.Oqtane.resources.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\%TargetFramework%\fr-FR\" /Y /I
XCOPY "..\Server\bin\Debug\%TargetFramework%\fr-FR\%ProjectName%.Server.Oqtane.resources.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\%TargetFramework%\fr-FR\" /Y /I
XCOPY "..\Shared\bin\Debug\%TargetFramework%\fr-FR\%ProjectName%.Shared.Oqtane.resources.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\%TargetFramework%\fr-FR\" /Y /I
REM Copy localization satellite assemblies for es-ES culture
XCOPY "..\Client\bin\Debug\%TargetFramework%\es-ES\%ProjectName%.Client.Oqtane.resources.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\%TargetFramework%\es-ES\" /Y /I
XCOPY "..\Server\bin\Debug\%TargetFramework%\es-ES\%ProjectName%.Server.Oqtane.resources.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\%TargetFramework%\es-ES\" /Y /I
XCOPY "..\Shared\bin\Debug\%TargetFramework%\es-ES\%ProjectName%.Shared.Oqtane.resources.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\%TargetFramework%\es-ES\" /Y /I
REM Copy localization satellite assemblies for it-IT culture
XCOPY "..\Client\bin\Debug\%TargetFramework%\it-IT\%ProjectName%.Client.Oqtane.resources.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\%TargetFramework%\it-IT\" /Y /I
XCOPY "..\Server\bin\Debug\%TargetFramework%\it-IT\%ProjectName%.Server.Oqtane.resources.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\%TargetFramework%\it-IT\" /Y /I
XCOPY "..\Shared\bin\Debug\%TargetFramework%\it-IT\%ProjectName%.Shared.Oqtane.resources.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\%TargetFramework%\it-IT\" /Y /I

XCOPY "..\Server\wwwroot\*" "..\..\oqtane.framework\Oqtane.Server\wwwroot\_content\%ProjectName%\" /Y /S /I