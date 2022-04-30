FROM mcr.microsoft.com/dotnet/sdk:6.0.202-alpine3.15-amd64 AS build
WORKDIR /app

# copy csproj and restore as distinct layers
COPY *.sln .
COPY Proxy/*.csproj ./Proxy/
RUN dotnet restore

# copy everything else and build app
COPY Proxy/. ./Proxy/
WORKDIR /app/Proxy
RUN dotnet publish -c Release -o out


FROM mcr.microsoft.com/dotnet/aspnet:6.0.4-alpine3.15-amd64 AS runtime
WORKDIR /app
COPY --from=build /app/Proxy/out ./

ENV ASPNETCORE_URLS http://*:$PORT
ENV UdpOptions__PortToListen $PORT
ENTRYPOINT ["dotnet", "Proxy.dll"]
