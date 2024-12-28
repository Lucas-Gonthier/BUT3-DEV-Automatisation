<template>
  <div>
    <h1>Évaluations Numériques</h1>
    <input v-model="inputData" placeholder="Entrez un nombre" />
    <button @click="sendData">Envoyer</button>

    <div v-if="processedData">
      <h2>Résultats des évaluations :</h2>
      <p>Pair : {{ processedData.pair }}</p>
      <p>Premier : {{ processedData.premier }}</p>
      <p>Parfait : {{ processedData.parfait }}</p>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue';

const inputData = ref('');
const processedData = ref(null);

const sendData = async () => {
    try {
      const response = await $fetch('http://localhost:5000/afficher', {
        method: 'POST',
        body: { number: inputData.value }, // Utilisation de "number" comme clé pour l'API
        headers: { 'Content-Type': 'application/json' }, // Spécification du type de contenu
      });
      processedData.value = response; // Récupérer les résultats de l'évaluation
    } catch (error) {
      console.error("Erreur lors de l'envoi des données", error);
    }
};
</script>
