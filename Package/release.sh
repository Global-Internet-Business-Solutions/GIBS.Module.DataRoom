TargetFramework=$1
ProjectName=$2

mkdir -p "../../oqtane.framework/Oqtane.Server/bin/Release/$TargetFramework/it"
mkdir -p "../../oqtane.framework/Oqtane.Server/bin/Release/$TargetFramework/it-IT"
cp -f "../Client/bin/Release/$TargetFramework/$ProjectName$.Client.Oqtane.dll" "../../oqtane.framework/Oqtane.Server/bin/Release/$TargetFramework/"
cp -f "../Client/bin/Release/$TargetFramework/$ProjectName$.Client.Oqtane.pdb" "../../oqtane.framework/Oqtane.Server/bin/Release/$TargetFramework/"
cp -f "../Server/bin/Release/$TargetFramework/$ProjectName$.Server.Oqtane.dll" "../../oqtane.framework/Oqtane.Server/bin/Release/$TargetFramework/"
cp -f "../Server/bin/Release/$TargetFramework/$ProjectName$.Server.Oqtane.pdb" "../../oqtane.framework/Oqtane.Server/bin/Release/$TargetFramework/"
cp -f "../Shared/bin/Release/$TargetFramework/$ProjectName$.Shared.Oqtane.dll" "../../oqtane.framework/Oqtane.Server/bin/Release/$TargetFramework/"
cp -f "../Shared/bin/Release/$TargetFramework/$ProjectName$.Shared.Oqtane.pdb" "../../oqtane.framework/Oqtane.Server/bin/Release/$TargetFramework/"
cp -f "../Client/bin/Release/$TargetFramework/it/$ProjectName$.Client.Oqtane.resources.dll" "../../oqtane.framework/Oqtane.Server/bin/Release/$TargetFramework/it/"
cp -f "../Client/bin/Release/$TargetFramework/it-IT/$ProjectName$.Client.Oqtane.resources.dll" "../../oqtane.framework/Oqtane.Server/bin/Release/$TargetFramework/it-IT/"
cp -f "../Server/wwwroot/*" "../../oqtane.framework/Oqtane.Server/wwwroot/_content/$ProjectName$/"

find . -name "*.nupkg" -delete
"..\..\oqtane.framework\oqtane.package\FixProps.exe"
"..\..\oqtane.framework\oqtane.package\nuget.exe" pack %ProjectName%.nuspec -Properties targetframework=%TargetFramework%;projectname=%ProjectName%
