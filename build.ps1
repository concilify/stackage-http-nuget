dotnet restore
dotnet pack --configuration Release -p:Version=0.0.0-local --output . --no-restore
dotnet test --configuration Release -p:Version=0.0.0-local
