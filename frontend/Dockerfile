# Étape 1 : Construire l'application Nuxt.js
FROM node:18 AS build-stage

WORKDIR /app
COPY ./frontend /app
RUN npm install
RUN npm run build

# Étape 2 : Créer l'image pour servir le frontend
FROM nginx:alpine

COPY --from=build-stage /app/.output/public /usr/share/nginx/html

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]