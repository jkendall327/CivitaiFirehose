FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["CivitaiFirehose/CivitaiFirehose.csproj", "CivitaiFirehose/"]
COPY ["CivitaiFirehose.Core/CivitaiFirehose.Core.csproj", "CivitaiFirehose.Core/"]
RUN dotnet restore "CivitaiFirehose/CivitaiFirehose.csproj"
COPY . .
WORKDIR "/src/CivitaiFirehose"
RUN dotnet build "CivitaiFirehose.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CivitaiFirehose.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CivitaiFirehose.dll"]
