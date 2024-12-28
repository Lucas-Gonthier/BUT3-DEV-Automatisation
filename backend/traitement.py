import json
import random
from flask import Flask, request, jsonify
from flask_cors import CORS
import requests

app = Flask(__name__)
CORS(app)

@app.route("/")
def accueil():
    return "Bonjour, Monde!"


# Vérifier si un nombre est pair (argument de type int, retourne un booléen)
def est_pair(nombre: int) -> bool:
    return nombre % 2 == 0

# Vérifier si un nombre est premier (argument de type int, retourne un booléen)
def est_premier(nombre: int) -> bool:
    if nombre < 2:
        return False

    for i in range(2, nombre):
        if nombre % i == 0:
            return False
    return True

# Vérifier si un nombre est parfait (argument de type int, retourne un booléen)
def est_parfait(nombre: int) -> bool:
    return nombre == sum([i for i in range(1, nombre) if nombre % i == 0])

# Générer la suite de Syracuse (argument de type int, retourne une liste)
def suite_syracuse(nombre: int) -> list:
    suite = [nombre]
    while nombre != 1:
        if nombre % 2 == 0:
            nombre = nombre // 2
        else:
            nombre = 3 * nombre + 1
        suite.append(nombre)
    return suite

# Récupérer les données d'un nombre (pair, premier, parfait) (argument de type int, retourne un dictionnaire)
def obtenir_donnees_nombre(nombre: int) -> dict:
    return {
        "nombre": int(nombre),
        "pair": est_pair(int(nombre)),
        "premier": est_premier(int(nombre)),
        "parfait": est_parfait(int(nombre))
    }

# Vérifier si un nombre existe déjà dans la base de données (argument de type int)
def verifier_nombre_bdd(nombre: int):
    url = f"http://localhost:8080/api/check?nombre={nombre}"  # Changement ici
    response = requests.get(url)
    if response.status_code == 200:  
        json_response = response.json()
        return json_response.get("exists", False)
    else:
        raise Exception("Erreur lors de la communication avec l'API .NET check")

# Récupérer les données d'un nombre depuis la base de données (argument de type int)
def recuperer_donnees_nombre(nombre: int):
    url = f"http://localhost:8080/api/recuperer?nombre={nombre}"  # Changement ici
    response = requests.get(url)
    if response.status_code == 200:
        return response.json()
    else:
        raise Exception("Erreur lors de la communication avec l'API .NET recup")

# Ajouter les données d'un nombre dans la base de données (argument de type int)
def ajouter_nombre_bdd(nombre: int):
    donnees = obtenir_donnees_nombre(nombre)
    url = "http://localhost:8080/api/ajout"  # Changement ici
    headers = {'Content-Type': 'application/json'}
    response = requests.post(url, headers=headers, data=json.dumps(donnees))

    if response.status_code == 200:
        return recuperer_donnees_nombre(nombre) 
        # Récupération des données du nombre après ajout
    else:
        raise Exception("Erreur lors de la communication avec l'API .NET ajout")

# Envoi de la suite de syracuse à l'API NET (argument de type int)
def envoyer_suite_syracuse(nombre: int):
    suite = suite_syracuse(int(nombre))
    syracuse_data = {
        'Nombre': int(nombre),
        'Suite': suite
    }
    url = "http://localhost:8080/api/envoyer-syracuse"  # Changement ici
    headers = {'Content-Type': 'application/json'}
    response = requests.post(url, headers=headers, data=json.dumps(syracuse_data))

    if response.status_code != 200:
        raise Exception("Erreur lors de la communication avec l'API .NET syracuse")

# Téléchargememt la suite de Syracuse depuis l'API .NET (argument de type int)
def telecharger_syracuse(nombre: int):
    url = f"http://localhost:8080/api/download-syracuse?nombre={nombre}"  # Changement ici
    response = requests.get(url)
    
    if response.status_code == 200:
        return response.json()
    else:
        raise Exception("Erreur lors de la communication avec l'API .NET telecharger-syracuse")

# Routes
@app.route("/afficher", methods=["POST"])
def afficher_nombre():
    # Récupérer les données JSON de la requête
    data = request.get_json()
    nombre = data["number"]

    # Vérifier si le nombre existe dans la BDD via l'API .NET
    existe = verifier_nombre_bdd(nombre)
    if existe:
        resultat = recuperer_donnees_nombre(nombre)  # Récupération des données du nombre
        return jsonify(resultat)
    else:
        #envoyer_suite_syracuse(nombre)  # Envoie de la suite de Syracuse à l'API .NET
        resultat = ajouter_nombre_bdd(nombre)  # Ajouter le nombre à la BDD et récupérer les données après ajout
        #resultat = {**resultat, "syracuse": telecharger_syracuse(nombre)} 
        # Téléchargement de la suite de Syracuse depuis l'API .NET et ajout dans le résultat
        return jsonify(resultat)


@app.route("/all", methods=["POST"])
def afficher_tous_nombres():
    url = "http://localhost:8080/api/all"  # Changement ici
    response = requests.get(url)
    if response.status_code == 200:
        return response.json()
    else:
        return jsonify({"success": False})


if __name__ == "__main__":
    app.run(host="0.0.0.0", port=5000, debug=True)