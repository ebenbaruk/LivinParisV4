# Système de Navigation Métro Parisien

## Description
Ce projet implémente un système de navigation pour le métro parisien permettant de trouver le chemin le plus court entre deux stations en utilisant différents algorithmes de graphe.

## Fonctionnalités
- Chargement et analyse des données du réseau de métro parisien
- Recherche du plus court chemin entre deux stations utilisant trois algorithmes différents :
  - Dijkstra
  - Bellman-Ford
  - Floyd-Warshall
- Comparaison des performances des algorithmes
- Interface utilisateur interactive pour rechercher des itinéraires personnalisés
- Visualisation graphique du réseau de métro et des itinéraires calculés

## Visualisation
Le système génère des visualisations pour :
- Le réseau complet du métro parisien
- Les itinéraires calculés par chaque algorithme

Les visualisations sont sauvegardées dans le dossier "Visualisations" et peuvent être consultées directement depuis l'application.

## Algorithmes implémentés

### Dijkstra
- **Complexité** : O((V+E)log V)
- **Avantages** : Efficace pour trouver le chemin entre deux stations spécifiques
- **Limitations** : Ne fonctionne pas avec des poids négatifs

### Bellman-Ford
- **Complexité** : O(V*E)
- **Avantages** : Peut gérer les poids négatifs
- **Limitations** : Plus lent que Dijkstra pour les graphes sans poids négatifs

### Floyd-Warshall
- **Complexité** : O(V³)
- **Avantages** : Calcule tous les chemins les plus courts entre toutes les paires de stations
- **Limitations** : Plus lent pour trouver un seul chemin spécifique

## Structure du projet
- **Program.cs** : Point d'entrée du programme avec l'interface utilisateur
- **Graphe.cs** : Implémentation générique d'un graphe et des algorithmes de plus court chemin
- **Station.cs** : Classe représentant une station de métro
- **MetroParisien.cs** : Classe gérant le chargement et la manipulation des données du métro
- **MetroVisualisation.cs** : Classe pour la visualisation graphique du réseau et des chemins

## Prérequis
- .NET 8.0 ou supérieur
- SkiaSharp pour la visualisation graphique

## Comment utiliser l'application
1. Lancer l'application
2. Le menu principal propose plusieurs options :
   - Exécuter les tests prédéfinis pour voir des exemples d'itinéraires
   - Rechercher un itinéraire personnalisé entre deux stations
   - Afficher la liste des stations disponibles
   - Ouvrir le dossier contenant les visualisations générées
3. Pour rechercher un itinéraire personnalisé :
   - Entrez le nom (ou une partie du nom) de la station de départ
   - Sélectionnez la station dans la liste des correspondances
   - Répétez pour la station d'arrivée
   - Le système affichera les itinéraires calculés par les trois algorithmes

## Format des données
Le système utilise un fichier CSV contenant les informations sur les stations, au format :
```
Ligne;Station;Coordonnées;StationPrécédente;StationSuivante;Correspondances
```
Le fichier doit être placé dans le dossier "DataMetro" sous le nom "metro.csv".

## Améliorations futures
- Mise à jour des données avec les nouvelles lignes et stations
- Prise en compte des horaires réels de passage des trains
- Intégration d'une interface graphique plus élaborée
- Application mobile pour accéder au système en déplacement 