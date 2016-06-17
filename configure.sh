#!/bin/bash
sudo cp dotnet.sh /etc/bash_completion.d/dotnet.sh -f
. /etc/bash_completion.d/dotnet.sh
source /etc/bash_completion.d/dotnet.sh
dotnet restore --ignore-failed-sources
dotnet build -c Release -r ubuntu.14.04-x64
sudo cp bin/Release/netcoreapp1.0/ubuntu.14.04-x64/* /usr/local/bin/
