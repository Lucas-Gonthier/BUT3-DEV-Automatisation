# Étape 1 : Utiliser l'image Python
FROM python:3.9-slim

# Définir le répertoire de travail
WORKDIR /app

# Copier le code du backend
COPY ./backend /app

# Installer les dépendances
RUN pip install -r requirements.txt

EXPOSE 5000
CMD ["python", "app.py"]
