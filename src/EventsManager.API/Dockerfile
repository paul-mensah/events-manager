FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["EventsManager.API/EventsManager.API.csproj", "./"]

RUN dotnet restore "EventsManager.API/EventsManager.API.csproj"

COPY . .

RUN dotnet new tool-manifest
RUN dotnet tool install dotnet-ef

WORKDIR "src/EventsManager.API/."
RUN dotnet build "EventsManager.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EventsManager.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
EXPOSE 5467
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EventsManager.API.dll"]
