#! /bin/sh
set -e
dotnet restore
dotnet pack -c Release -o /src/packages
dotnet publish -c Release -o /src/publish/dotnet-toolbox
