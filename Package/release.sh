TargetFramework=$1
ProjectName=$2

find . -name "*.nupkg" -delete
"..\..\oqtane.framework\oqtane.package\FixProps.exe"
"..\..\oqtane.framework\oqtane.package\nuget.exe" pack %ProjectName%.nuspec -Properties targetframework=%TargetFramework%;projectname=%ProjectName%
cp -f "*.nupkg" "..\..\oqtane.framework\Oqtane.Server\Packages\"