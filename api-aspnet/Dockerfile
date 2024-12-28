# Utiliser l'image officielle de .NET
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 8080

# Utiliser l'image de build pour ASP.NET
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["api-aspnet/api-aspnet.csproj", "api-aspnet/"]
RUN dotnet restore "api-aspnet/api-aspnet.csproj"
COPY . .
WORKDIR "/src/api-aspnet"
RUN dotnet build "api-aspnet.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "api-aspnet.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "api-aspnet.dll"]
