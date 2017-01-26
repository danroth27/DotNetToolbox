#! /bin/sh
set -e
dotnet restore
dotnet pack -c Release -o /src/packages
dotnet publish -c Release -o /src/publish/dotnet-toolbox
apt-get update
apt-get -y install zip
zip -r /src/publish/dotnet-toolbox /src/publish/dotnet-toolbox 
tar -cvzf /src/publish/dotnet-toolbox.tar.gz /src/publish/dotnet-toolbox
apt-get -y install jq
release=$(curl https://api.github.com/repos/danroth27/dotnettoolbox/releases/latest)
asset_ids=$(echo $release | jq '.assets | .[] | .id')
for id in asset_ids
do
  curl -X DELETE https://api.github.com/repos/danroth27/dotnettoolbox/releases/assets/$id -u $GitHubToken:
done
cd /src/publish
release_id=$(echo release | jq .id)
curl -X POST -H 'Content-Type:application/zip' --data-binary @dotnet-toolbox.zip https://uploads.github.com/repos/danroth27/DotNetToolbox/releases/$release_id/assets?name=dotnet-toolbox.zip -u $GitHubToken:
curl -X POST -H 'Content-Type:application/zip' --data-binary @dotnet-toolbox.tar.gz https://uploads.github.com/repos/danroth27/DotNetToolbox/releases/$release_id/assets?name=dotnet-toolbox.tar.gz -u $GitHubToken:
