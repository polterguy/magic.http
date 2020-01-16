
set version=%1
set key=%2

cd %~dp0
dotnet build magic.http.contracts/magic.http.contracts.csproj --configuration Release --source https://api.nuget.org/v3/index.json
dotnet nuget push magic.http.contracts/bin/Release/magic.http.contracts.%version%.nupkg -k %key% -s https://api.nuget.org/v3/index.json

cd %~dp0
dotnet build magic.http.services/magic.http.services.csproj --configuration Release --source https://api.nuget.org/v3/index.json
dotnet nuget push magic.http.services/bin/Release/magic.http.%version%.nupkg -k %key% -s https://api.nuget.org/v3/index.json
