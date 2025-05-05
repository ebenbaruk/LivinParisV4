# Liv'in Paris - Application de Livraison de Repas

## Description
Liv'in Paris est une application de livraison de repas qui met en relation des cuisiniers locaux avec des clients. Elle permet aux cuisiniers de proposer leurs plats et aux clients de commander des repas faits maison.

## Fonctionnalités Principales

### Pour les Clients
- Inscription et connexion sécurisée
- Consultation du catalogue de plats disponibles
- Recherche et filtrage des plats
- Passage de commandes
- Suivi en temps réel des commandes
- Modification du profil utilisateur

### Pour les Cuisiniers
- Double mode de connexion (Client/Cuisinier)
- Gestion de leur catalogue de plats
  - Ajout de nouveaux plats
  - Modification des plats existants
  - Retrait de plats
- Gestion des commandes entrantes
- Suivi des revenus et statistiques

## Architecture du Projet

### Interface Utilisateur (Forms/)
- `Form1.cs` : Formulaire de connexion
- `RegisterForm.cs` : Inscription des nouveaux utilisateurs
- `MainForm.cs` : Interface principale pour les clients
- `CuisinierForm.cs` : Interface dédiée aux cuisiniers
- `CommandeForm.cs` : Gestion des commandes
- `ChoixModeForm.cs` : Sélection du mode (Client/Cuisinier)
- `ModifierPlatForm.cs` : Édition des plats
- `AjouterPlatForm.cs` : Création de nouveaux plats
- `ModifierProfilForm.cs` : Modification du profil utilisateur
- `DetailsCommandeForm.cs` : Détails d'une commande
- `CommandeProgressForm.cs` : Suivi de l'état des commandes

### Logique Métier (Rendu1/Modules/)
- Gestion de l'authentification
- Gestion des utilisateurs
- Gestion des commandes
- Gestion du catalogue de plats
- Export des données (XML/JSON)

### Base de Données
- Utilisation de MySQL pour le stockage des données
- Tables principales :
  - Utilisateurs
  - Cuisiniers
  - Plats
  - Commandes
  - Évaluations

## Technologies Utilisées
- C# .NET
- Windows Forms pour l'interface graphique
- MySQL pour la base de données


## Installation et Prérequis
1. .NET Framework 4.8 ou supérieur
2. MySQL Server
3. Visual Studio 2019 ou supérieur

## Configuration
1. Cloner le repository
2. Restaurer les packages NuGet
3. Configurer la connexion à la base de données dans `DatabaseManager.cs`
4. Compiler et exécuter le projet

## Structure de la Base de Données
La base de données contient les tables suivantes avec leurs relations :
- `Utilisateur` : Informations de base des utilisateurs
- `Cuisinier` : Informations spécifiques aux cuisiniers
- `Plat` : Catalogue des plats disponibles
- `Commande` : Suivi des commandes
- `Evaluation` : Avis des clients

## Fonctionnalités à Venir
- Système de paiement intégré
- Chat en temps réel entre clients et cuisiniers
- Système de notation et d'avis plus élaboré
- Gestion des allergènes et régimes spéciaux

