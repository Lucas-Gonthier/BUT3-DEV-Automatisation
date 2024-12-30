# Étape 1 : Construire l'application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /app

# Copier le fichier .sln
COPY apiaspnet.sln . 

# Copier les fichiers de projet dans le conteneur
COPY apiaspnet.csproj . 

# Restaurer les dépendances à partir du fichier .sln
RUN dotnet restore apiaspnet.sln

# Copier tout le reste du code source dans le conteneur
COPY . .

# Copier tout le reste des fichiers et construire l'application
RUN dotnet build -c Release -o /app/build
RUN dotnet publish -c Release -o /app/publish

# Étape 2 : Préparer l'image de production avec uniquement le runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Copier les fichiers publiés dans l'image de production
COPY --from=build-env /app/publish .

# Exposer le port pour les requêtes HTTP
EXPOSE 80

# Commande de démarrage
ENTRYPOINT ["dotnet", "apiaspnet.dll"]