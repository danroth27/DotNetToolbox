#!/usr/bin/env bash
releases=$(curl https://github.com/danroth27/dotnettoolbox/releases.atom)
uri="https://github.com/danroth27/dotnettoolbox/releases/download/$(echo $releases | grep -oPm1 "(?<=<title>)[^<]+" | sed -n 2p)/dotnet-toolbox.tar.gz"
if command -v tempfile >/dev/null 2>&1; then
    outFile=`tempfile`
else
    outFile=`mktemp`
fi
curl -o $outFile -L $uri
outDir="$outFile-extracted"
echo $outDir
mkdir $outDir
tar -xvzf $outFile -C $outDir
echo $outDir
dotnet "$outDir/dotnet-toolbox.dll" install dotnet-toolbox -v 1.0.0-*
if [ $? -ne 0 ]; then
    echo "Could not install."
    exit 1
fi
if [[ $PATH != *"$HOME/.toolbox"* ]]; then
    echo "PATH=$HOME/.toolbox:$PATH" >> $HOME/.bashrc
    echo "Your .bashrc was updated, either start a new bash instance from scratch or \`source ~/.bashrc\`."
fi