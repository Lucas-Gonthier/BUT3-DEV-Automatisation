# Utiliser l'image NGINX comme base
FROM nginx:alpine

# Définir le répertoire par défaut dans le conteneur
WORKDIR /usr/share/nginx/html

# Copier tous les fichiers du répertoire IHM dans le répertoire de NGINX
COPY . .

# Exposer le port 80 (port par défaut de NGINX)
EXPOSE 80

# Démarrer le serveur NGINX
CMD ["nginx", "-g", "daemon off;"]