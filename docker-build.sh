#! /bin/sh
set -e
apt-get update
apt-get -y install zip
dotnet restore
dotnet pack -c Release -o /src/packages
dotnet publish -c Release -o /src/publish/dotnet-toolbox
zip -r /src/publish/dotnet-toolbox /src/publish/dotnet-toolbox 
tar -cvzf /src/publish/dotnet-toolbox.tar.gz /src/publish/dotnet-toolbox
