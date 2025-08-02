FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
# Cache busting argument to force fresh build
ARG CACHEBUST=1
WORKDIR /src
COPY ["DibatechLinkerAPI.csproj", "."]
RUN dotnet restore "DibatechLinkerAPI.csproj" --no-cache
COPY . .
WORKDIR "/src/."
RUN dotnet clean && dotnet build "DibatechLinkerAPI.csproj" -c Release -o /app/build --no-restore

FROM build AS publish
RUN dotnet publish "DibatechLinkerAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DibatechLinkerAPI.dll"]