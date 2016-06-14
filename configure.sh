#!/bin/bash
sudo cp dotnet.sh /etc/bash_completion.d/dotnet.sh -f
. /etc/bash_completion.d/dotnet.sh
source /etc/bash_completion.d/dotnet.sh
dotnet build -r ubuntu.14.04-x64 -c Release
sudo cp bin/Release/netcoreapp1.0/ubuntu.14.04-x64/* /usr/local/bin/