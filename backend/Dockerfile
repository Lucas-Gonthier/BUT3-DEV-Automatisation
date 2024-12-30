# Étape 1 : Utiliser l'image Python officielle légère
FROM python:3.9-slim

# Définir le répertoire de travail
WORKDIR /app

# Copier uniquement le fichier Python nécessaire
COPY . ./

# Si aucune dépendance n'est nécessaire, sauter l'étape d'installation
# Si des dépendances sont nécessaires, créez un fichier requirements.txt et ajoutez les lignes suivantes :
# COPY requirements.txt /app
RUN pip install flask flask-cors requests

# Exposer le port sur lequel votre application écoutera
EXPOSE 5000

# Commande par défaut pour exécuter le fichier Python
CMD ["python", "traitement.py"]
