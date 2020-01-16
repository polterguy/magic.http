cd %~dp0
dotnet build magic.http.contracts/magic.http.contracts/magic.http.contracts.csproj --configuration Release
dotnet build magic.http.services/magic.http.services/magic.http.services.csproj --configuration Release
