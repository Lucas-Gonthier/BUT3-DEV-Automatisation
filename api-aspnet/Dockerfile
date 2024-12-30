# Étape 1 : Construire l'application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copier les fichiers du projet
COPY ./api-aspnet.csproj ./
RUN dotnet restore

# Copier tout le reste des fichiers et construire l'application
COPY . ./
RUN dotnet build -c Release -o /app/build
RUN dotnet publish -c Release -o /app/publish

# Étape 2 : Préparer l'image de production avec uniquement le runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copier les fichiers publiés dans l'image de production
COPY --from=build-env /app/publish .

# Exposer le port pour les requêtes HTTP
EXPOSE 80

# Commande de démarrage
ENTRYPOINT ["dotnet", "api-aspnet.dll"]