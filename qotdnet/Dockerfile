FROM mcr.microsoft.com/dotnet/core/runtime:2.1-bionic

COPY bin/Release/netcoreapp2.1/publish/ app/
COPY quotes.json /

ENTRYPOINT ["dotnet", "app/qotdnet.dll"]
