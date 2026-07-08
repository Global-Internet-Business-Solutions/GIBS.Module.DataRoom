@echo off
set TargetFramework=%1
set ProjectName=%2

XCOPY "..\Client\bin\Debug\%TargetFramework%\%ProjectName%.Client.Oqtane.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\%TargetFramework%\" /Y
XCOPY "..\Client\bin\Debug\%TargetFramework%\%ProjectName%.Client.Oqtane.pdb" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\%TargetFramework%\" /Y
XCOPY "..\Server\bin\Debug\%TargetFramework%\%ProjectName%.Server.Oqtane.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\%TargetFramework%\" /Y
XCOPY "..\Server\bin\Debug\%TargetFramework%\%ProjectName%.Server.Oqtane.pdb" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\%TargetFramework%\" /Y
XCOPY "..\Shared\bin\Debug\%TargetFramework%\%ProjectName%.Shared.Oqtane.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\%TargetFramework%\" /Y
XCOPY "..\Shared\bin\Debug\%TargetFramework%\%ProjectName%.Shared.Oqtane.pdb" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\%TargetFramework%\" /Y
REM Copy localization satellite assemblies for it culture
XCOPY "..\Client\bin\Debug\%TargetFramework%\it\%ProjectName%.Client.Oqtane.resources.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\%TargetFramework%\it\" /Y /I
XCOPY "..\Server\bin\Debug\%TargetFramework%\it\%ProjectName%.Server.Oqtane.resources.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\%TargetFramework%\it\" /Y /I
XCOPY "..\Shared\bin\Debug\%TargetFramework%\it\%ProjectName%.Shared.Oqtane.resources.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\%TargetFramework%\it\" /Y /I
REM Copy localization satellite assemblies for it-IT culture
XCOPY "..\Client\bin\Debug\%TargetFramework%\it-IT\%ProjectName%.Client.Oqtane.resources.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\%TargetFramework%\it-IT\" /Y /I
XCOPY "..\Server\bin\Debug\%TargetFramework%\it-IT\%ProjectName%.Server.Oqtane.resources.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\%TargetFramework%\it-IT\" /Y /I
XCOPY "..\Shared\bin\Debug\%TargetFramework%\it-IT\%ProjectName%.Shared.Oqtane.resources.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\%TargetFramework%\it-IT\" /Y /I

XCOPY "..\Server\wwwroot\*" "..\..\oqtane.framework\Oqtane.Server\wwwroot\_content\%ProjectName%\" /Y /S /I