FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["FipeNotifier/FipeNotifier.csproj", "FipeNotifier/"]
RUN dotnet restore "FipeNotifier/FipeNotifier.csproj"
COPY . .
WORKDIR "/src/FipeNotifier"
RUN dotnet build "FipeNotifier.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FipeNotifier.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FipeNotifier.dll"]