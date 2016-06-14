# dotnet-bash

Tab completion for `dotnet`

###
cp dotnet.sh /etc/bash_completion.d/dotnet.sh
source /etc/bash_completion.d/dotnet.sh
dotnet build -r ubuntu.14.04-x64 -c Release
sudo ln -s bin/Release/netcoreapp1.0/ubuntu.14.04-x64/_dotnet-complete /usr/bin/_dotnet-complete
###
