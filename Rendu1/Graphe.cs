using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Rendu1
{
    public class Graphe<T>
    {
        public List<Noeud<T>> Noeuds { get; private set; }
        public List<Lien<T>> Liens { get; private set; }
        
        /// Matrice d'adjacence du graphe
        public bool[,] MatriceAdjacence;
        
        /// Liste d'adjacence du graphe
        public Dictionary<int, List<int>> ListeAdjacence;

        /// Matrice de poids (pour les algorithmes de plus court chemin)
        public double[,] MatricePoids;

        // Propriétés
        public bool EstOriente { get; private set; }
        public bool EstPondere { get; private set; }

        /// Constructeur du graphe vide
        public Graphe(bool estOriente = false, bool estPondere = false)
        {
            Noeuds = new List<Noeud<T>>();
            Liens = new List<Lien<T>>();
            ListeAdjacence = new Dictionary<int, List<int>>();
            MatriceAdjacence = new bool[0, 0];
            MatricePoids = new double[0, 0];
            EstOriente = estOriente;
            EstPondere = estPondere;
        }

        /// Ajoute un noeud au graphe avec l'identifiant unique et ses données
        public void AjouterNoeud(T donnees)
        {
            if (donnees == null)
                throw new ArgumentNullException(nameof(donnees));

            var noeud = new Noeud<T>(Noeuds.Count + 1, donnees);
            Noeuds.Add(noeud);
        }

        /// Ajoute un lien entre deux noeuds identifiés par leurs ID
        public void AjouterLien(int sourceId, int destinationId, double poids = 1.0)
        {
            var source = Noeuds.FirstOrDefault(n => n.Id == sourceId);
            var destination = Noeuds.FirstOrDefault(n => n.Id == destinationId);

            if (source == null || destination == null)
                throw new ArgumentException("Source ou destination non trouvée");

            var lien = new Lien<T>(source, destination, EstOriente, poids);
            Liens.Add(lien);
        }

        /// Mise à jour des matrices d'adjacence et de poids
        private void MettreAJourMatrices()
        {
            int taille = Noeuds.Count;
            MatriceAdjacence = new bool[taille, taille];
            MatricePoids = new double[taille, taille];
            
            // Initialisation de la matrice de poids avec des valeurs infinies
            for (int i = 0; i < taille; i++)
            {
                for (int j = 0; j < taille; j++)
                {
                    if (i == j)
                        MatricePoids[i, j] = 0; // La distance d'un noeud à lui-même est 0
                    else
                        MatricePoids[i, j] = double.PositiveInfinity; // Distance infinie par défaut
                }
            }
            
            // Mise à jour des liens existants
            foreach (var lien in Liens)
            {
                int sourceIndex = Noeuds.IndexOf(lien.Source);
                int destIndex = Noeuds.IndexOf(lien.Destination);
                
                if (sourceIndex >= 0 && destIndex >= 0 && sourceIndex < taille && destIndex < taille)
                {
                    MatriceAdjacence[sourceIndex, destIndex] = true;
                    MatricePoids[sourceIndex, destIndex] = lien.TempsParcours;
                    
                    if (!EstOriente)
                    {
                        MatriceAdjacence[destIndex, sourceIndex] = true;
                        MatricePoids[destIndex, sourceIndex] = lien.TempsParcours;
                    }
                }
            }
        }
        
        // Temps de correspondance pour un changement de ligne (en minutes)
        public double TempsChangementLigne = 4.0;

        // Temps entre deux stations adjacentes (en minutes)
        public double TempsEntreStations = 2.0;
        
        /// <summary>
        /// Retourne le nombre de noeuds dans le graphe (l'ordre)
        /// </summary>
        public int ObtenirOrdre()
        {
            return Noeuds.Count;
        }
        
        /// <summary>
        /// Retourne le nombre de liens dans le graphe (la taille)
        /// </summary>
        public int ObtenirTaille()
        {
            return Liens.Count;
        }
        
        /// <summary>
        /// Vérifie si le graphe est connexe (tous les noeuds sont atteignables depuis n'importe quel autre noeud)
        /// </summary>
        public bool EstConnexe()
        {
            if (Noeuds.Count == 0)
                return true;
                
            // Utiliser un parcours en largeur depuis le premier noeud
            var visites = new HashSet<int>();
            var fileAttente = new Queue<Noeud<T>>();
            
            var premierNoeud = Noeuds.FirstOrDefault();
            if (premierNoeud == null)
                return true;
                
            fileAttente.Enqueue(premierNoeud);
            visites.Add(premierNoeud.Id);
            
            while (fileAttente.Count > 0)
            {
                var noeud = fileAttente.Dequeue();
                
                foreach (var voisin in noeud.Voisins)
                {
                    if (!visites.Contains(voisin.Id))
                    {
                        visites.Add(voisin.Id);
                        fileAttente.Enqueue(voisin);
                    }
                }
            }
            
            // Le graphe est connexe si tous les noeuds ont été visités
            return visites.Count == Noeuds.Count;
        }
        
        /// <summary>
        /// Vérifie si le graphe contient des cycles
        /// </summary>
        public bool ContientCycles()
        {
            if (Noeuds.Count <= 1)
                return false;
                
            // Pour un graphe non orienté, on utilise une approche différente
            if (!EstOriente)
            {
                return ContientCyclesNonOriente();
            }
            
            // Pour un graphe orienté, on utilise la détection de cycle par DFS
            var visites = new Dictionary<int, bool>();
            var enTraitement = new Dictionary<int, bool>();
            
            foreach (var noeud in Noeuds)
            {
                visites[noeud.Id] = false;
                enTraitement[noeud.Id] = false;
            }
            
            foreach (var noeud in Noeuds)
            {
                if (!visites[noeud.Id])
                {
                    if (DetecterCycleRec(noeud.Id, visites, enTraitement))
                        return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Méthode récursive pour la détection de cycle (algorithme DFS)
        /// </summary>
        private bool DetecterCycleRec(int noeudId, Dictionary<int, bool> visites, Dictionary<int, bool> enTraitement)
        {
            // Marquer le noeud comme visité et en cours de traitement
            visites[noeudId] = true;
            enTraitement[noeudId] = true;
            
            // Trouver le noeud correspondant à l'ID
            var noeud = Noeuds.FirstOrDefault(n => n.Id == noeudId);
            if (noeud == null)
                return false;
                
            // Vérifier tous les voisins
            foreach (var voisin in noeud.Voisins)
            {
                // Si le voisin n'est pas visité, vérifier récursivement
                if (!visites[voisin.Id])
                {
                    if (DetecterCycleRec(voisin.Id, visites, enTraitement))
                        return true;
                }
                // Si le voisin est en cours de traitement, un cycle est détecté
                else if (enTraitement[voisin.Id])
                {
                    return true;
                }
            }
            
            // Marquer le noeud comme plus en cours de traitement
            enTraitement[noeudId] = false;
            
            return false;
        }
        
        /// <summary>
        /// Détecte les cycles dans un graphe non orienté
        /// </summary>
        private bool ContientCyclesNonOriente()
        {
            // Pour un graphe non orienté, on utilise un parcours en profondeur modifié
            var visites = new Dictionary<int, bool>();
            foreach (var noeud in Noeuds)
            {
                visites[noeud.Id] = false;
            }
            
            // Vérifier chaque noeud non visité
            foreach (var noeud in Noeuds)
            {
                if (!visites[noeud.Id])
                {
                    // Utiliser -1 comme parent pour le premier noeud de chaque composante connexe
                    if (DetecterCycleNonOriente(noeud.Id, visites, -1))
                        return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Méthode récursive pour détecter un cycle dans un graphe non orienté
        /// </summary>
        private bool DetecterCycleNonOriente(int noeudId, Dictionary<int, bool> visites, int parent)
        {
            // Marquer le noeud actuel comme visité
            visites[noeudId] = true;
            
            // Trouver le noeud correspondant à l'ID
            var noeud = Noeuds.FirstOrDefault(n => n.Id == noeudId);
            if (noeud == null)
                return false;
                
            // Parcourir tous les voisins
            foreach (var voisin in noeud.Voisins)
            {
                // Si le voisin n'est pas visité, vérifier récursivement
                if (!visites[voisin.Id])
                {
                    if (DetecterCycleNonOriente(voisin.Id, visites, noeudId))
                        return true;
                }
                // Si le voisin est déjà visité et n'est pas le parent direct
                // (ce qui signifierait simplement qu'on revient sur nos pas dans un graphe non orienté),
                // alors un cycle est détecté
                else if (voisin.Id != parent)
                {
                    return true;
                }
            }
            
            return false;
        }
        
        #region Algorithmes de plus court chemin
        
        /// Algorithme de Dijkstra pour trouver le plus court chemin entre deux noeuds
        public (List<int> chemin, double distance) Dijkstra(int depart, int arrivee)
        {
            var distances = new Dictionary<int, double>();
            var precedents = new Dictionary<int, int>();
            var nonVisites = new List<int>();
            
            // Initialisation
            foreach (var noeud in Noeuds)
            {
                distances[noeud.Id] = noeud.Id == depart ? 0 : double.PositiveInfinity;
                precedents[noeud.Id] = -1;
                nonVisites.Add(noeud.Id);
            }
            
            while (nonVisites.Count > 0)
            {
                // Trouver le noeud non visité avec la plus petite distance
                int u = -1;
                double minDistance = double.PositiveInfinity;
                foreach (var noeudId in nonVisites)
                {
                    if (distances[noeudId] < minDistance)
                    {
                        minDistance = distances[noeudId];
                        u = noeudId;
                    }
                }
                
                // Si aucun chemin n'est trouvé ou si on est arrivé
                if (u == -1 || u == arrivee)
                    break;
                    
                nonVisites.Remove(u);
                
                // Parcourir tous les liens pour trouver les voisins
                foreach (var lien in Liens)
                {
                    if (lien.Source.Id == u || (!EstOriente && lien.Destination.Id == u))
                    {
                        int v = lien.Source.Id == u ? lien.Destination.Id : lien.Source.Id;
                        
                        if (nonVisites.Contains(v))
                        {
                            double alt = distances[u] + lien.TempsParcours;
                            
                            // Vérifier le changement de ligne
                            if (precedents[u] != -1)
                            {
                                var noeudU = Noeuds.First(n => n.Id == u);
                                var noeudPrec = Noeuds.First(n => n.Id == precedents[u]);
                                
                                if (noeudU.Donnees is Station stationU && 
                                    noeudPrec.Donnees is Station stationPrec && 
                                    stationU.Ligne != stationPrec.Ligne)
                                {
                                    alt += TempsChangementLigne;
                                }
                            }
                            
                            if (alt < distances[v])
                            {
                                distances[v] = alt;
                                precedents[v] = u;
                            }
                        }
                    }
                }
            }
            
            // Reconstruction du chemin
            var chemin = new List<int>();
            double distance = distances[arrivee];
            
            if (distance != double.PositiveInfinity)
            {
                int courant = arrivee;
                while (courant != -1)
                {
                    chemin.Insert(0, courant);
                    courant = precedents[courant];
                }
            }
            
            return (chemin, distance);
        }
        
        /// Algorithme de Bellman-Ford pour trouver le plus court chemin entre deux noeuds
        public (List<int> chemin, double distance) BellmanFord(int depart, int arrivee)
        {
            var distances = new Dictionary<int, double>();
            var precedents = new Dictionary<int, int>();
            
            // Initialisation
            foreach (var noeud in Noeuds)
            {
                if (noeud.Id == depart)
                    distances[noeud.Id] = 0;
                else
                    distances[noeud.Id] = double.PositiveInfinity;
                
                precedents[noeud.Id] = -1;
            }
            
            // Création d'une structure pour suivre les lignes des stations
            var ligneActuelle = new Dictionary<int, int>();
            var noeudDepart = Noeuds.FirstOrDefault(n => n.Id == depart);
            if (noeudDepart != null && noeudDepart.Donnees is Station stationDepart)
            {
                ligneActuelle[depart] = stationDepart.Ligne;
            }
            
            // Relaxation des arêtes |V|-1 fois
            for (int i = 1; i < Noeuds.Count; i++)
            {
                bool changement = false;
                
                foreach (var lien in Liens)
                {
                    int u = lien.Source.Id;
                    int v = lien.Destination.Id;
                    
                    if (distances[u] != double.PositiveInfinity)
                    {
                        double tempsParcours = lien.TempsParcours;
                        
                        // Vérifier s'il y a un changement de ligne
                        if (lien.Source.Donnees is Station stationSource && 
                            lien.Destination.Donnees is Station stationDestination)
                        {
                            // Si on a déjà visité le noeud source et qu'on change de ligne
                            if (ligneActuelle.ContainsKey(u) && ligneActuelle[u] != stationSource.Ligne)
                            {
                                tempsParcours += TempsChangementLigne;
                            }
                        }
                        
                        double nouvelleDistance = distances[u] + tempsParcours;
                        
                        if (nouvelleDistance < distances[v])
                        {
                            distances[v] = nouvelleDistance;
                            precedents[v] = u;
                            
                            // Mise à jour de la ligne actuelle
                            if (lien.Destination.Donnees is Station stationDest)
                            {
                                ligneActuelle[v] = stationDest.Ligne;
                            }
                            
                            changement = true;
                        }
                    }
                }
                
                // Si aucun changement lors de cette itération, on peut s'arrêter
                if (!changement)
                    break;
            }
            
            // Vérification des cycles négatifs
            bool cycleTrouve = false;
            // Une dernière itération pour détecter les cycles négatifs
            foreach (var lien in Liens)
            {
                int u = lien.Source.Id;
                int v = lien.Destination.Id;
                
                if (distances[u] != double.PositiveInfinity)
                {
                    double tempsParcours = lien.TempsParcours;
                    
                    // Ne pas ajouter le temps de correspondance ici car c'est déjà inclus dans les distances
                    if (distances[u] + tempsParcours < distances[v])
                    {
                        cycleTrouve = true;
                        break;
                    }
                }
            }
            
            if (cycleTrouve)
            {
                // Continuer quand même au lieu de retourner une erreur
            }
            
            // Reconstruction du chemin
            var chemin = new List<int>();
            double distance = distances[arrivee];
            
            if (distance != double.PositiveInfinity)
            {
                int courant = arrivee;
                // Limiter la reconstruction du chemin pour éviter les boucles infinies
                int maxSteps = Noeuds.Count;
                int steps = 0;
                
                while (courant != -1 && courant != depart && steps < maxSteps)
                {
                    chemin.Insert(0, courant);
                    courant = precedents[courant];
                    steps++;
                }
                
                if (courant == depart)
                    chemin.Insert(0, depart);
            }
            
            return (chemin, distance);
        }
        
        /// Algorithme de Floyd-Warshall pour trouver les plus courts chemins entre tous les noeuds
        public (double[,] distances, int[,] successeurs) FloydWarshall()
        {
            int n = Noeuds.Count;
            double[,] dist = new double[n, n];
            int[,] succ = new int[n, n];
            
            // Initialisation
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    dist[i, j] = (i == j) ? 0 : double.PositiveInfinity;
                    succ[i, j] = -1;
                }
            }
            
            // Initialisation avec les liens existants
            foreach (var lien in Liens)
            {
                int i = Noeuds.IndexOf(lien.Source);
                int j = Noeuds.IndexOf(lien.Destination);
                
                if (i >= 0 && j >= 0 && i < n && j < n)
                {
                    dist[i, j] = lien.TempsParcours;
                    succ[i, j] = j;
                }
                
                if (!EstOriente && i >= 0 && j >= 0 && i < n && j < n)
                {
                    dist[j, i] = lien.TempsParcours;
                    succ[j, i] = i;
                }
            }
            
            // Algorithme de Floyd-Warshall
            for (int k = 0; k < n; k++)
            {
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (dist[i, k] != double.PositiveInfinity && dist[k, j] != double.PositiveInfinity)
                        {
                            double nouvelleDist = dist[i, k] + dist[k, j];
                            
                            // Ajouter le temps de correspondance si nécessaire
                            var noeudI = Noeuds[i];
                            var noeudK = Noeuds[k];
                            var noeudJ = Noeuds[j];
                            
                            // Vérifier si les noeuds contiennent des objets Station
                            if (noeudI.Donnees is Station && noeudK.Donnees is Station)
                            {
                                var stationI = noeudI.Donnees as Station;
                                var stationK = noeudK.Donnees as Station;
                                
                                if (stationI != null && stationK != null && stationI.Ligne != stationK.Ligne)
                                    nouvelleDist += TempsChangementLigne;
                            }
                                
                            if (noeudK.Donnees is Station && noeudJ.Donnees is Station)
                            {
                                var stationK = noeudK.Donnees as Station;
                                var stationJ = noeudJ.Donnees as Station;
                                
                                if (stationK != null && stationJ != null && stationK.Ligne != stationJ.Ligne)
                                    nouvelleDist += TempsChangementLigne;
                            }
                            
                            if (nouvelleDist < dist[i, j])
                            {
                                dist[i, j] = nouvelleDist;
                                succ[i, j] = succ[i, k];
                            }
                        }
                    }
                }
            }
            
            return (dist, succ);
        }
        
        /// Méthode utilitaire pour reconstruire le chemin à partir des résultats de Floyd-Warshall
        public (List<int> chemin, double distance) ReconstruireChemin(int depart, int arrivee, double[,] distances, int[,] successeurs)
        {
            int departIndex = Noeuds.FindIndex(n => n.Id == depart);
            int arriveeIndex = Noeuds.FindIndex(n => n.Id == arrivee);
            
            if (departIndex == -1 || arriveeIndex == -1)
                return (new List<int>(), double.PositiveInfinity);
                
            double distance = distances[departIndex, arriveeIndex];
            
            if (distance == double.PositiveInfinity)
                return (new List<int>(), double.PositiveInfinity);
                
            var chemin = new List<int> { depart };
            int courant = departIndex;
            
            while (courant != arriveeIndex)
            {
                courant = successeurs[courant, arriveeIndex];
                if (courant == -1) break;
                chemin.Add(Noeuds[courant].Id);
            }
            
            return (chemin, distance);
        }
        
        #endregion
        
        #region Méthodes de parcours

        public List<int> ParcoursLargeur(int depart)
        {
            List<int> resultat = new List<int>();
            Queue<Noeud<T>> file = new Queue<Noeud<T>>();
            List<int> visites = new List<int>();

            // Vérification du noeud de départ
            Noeud<T> noeudDepart = Noeuds.FirstOrDefault(n => n.Id == depart);
            
            if (noeudDepart == null) return resultat;

            file.Enqueue(noeudDepart);
            visites.Add(depart);

            while (file.Count > 0)
            {
                Noeud<T> noeudCourant = file.Dequeue();
                resultat.Add(noeudCourant.Id);

                foreach (Noeud<T> voisin in noeudCourant.Voisins)
                {
                    if (!visites.Contains(voisin.Id))
                    {
                        file.Enqueue(voisin);
                        visites.Add(voisin.Id);
                    }
                }
            }

            return resultat;
        }

        public List<int> ParcoursProfondeur(int depart)
        {
            List<int> resultat = new List<int>();
            List<int> visites = new List<int>();
            
            // Appel récursive
            DFS(depart, resultat, visites);
            
            return resultat;
        }
        
        /// Version récursive pour le parcours en profondeur
        private void DFS(int noeudId, List<int> resultat, List<int> visites)
        {
            if (!visites.Contains(noeudId))
            {
                visites.Add(noeudId);
                resultat.Add(noeudId);

                // Vérifier si la clé existe dans le dictionnaire avant d'y accéder
                if (ListeAdjacence.ContainsKey(noeudId))
                {
                    foreach (int voisinId in ListeAdjacence[noeudId])
                    {
                        DFS(voisinId, resultat, visites);
                    }
                }
            }
        }

        #endregion
        
        #region Méthodes utilitaires
        
        /// Affiche les informations sur le chemin trouvé
        public void AfficherChemin(List<int> chemin, double temps)
        {
            if (chemin.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Aucun chemin trouvé.");
                Console.ForegroundColor = ConsoleColor.White;
                return;
            }
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Chemin trouvé : temps total = {temps} minutes");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Stations parcourues :");
            
            double distanceTotale = 0;
            for (int i = 0; i < chemin.Count; i++)
            {
                var noeud = Noeuds.FirstOrDefault(n => n.Id == chemin[i]);
                if (noeud != null)
                {
                    var station = noeud.Donnees as Station;
                    if (station != null)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.Write($"{i+1}. ");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine($"{station}");
                        
                        // Calculer la distance avec la station suivante
                        if (i < chemin.Count - 1)
                        {
                            var prochainNoeud = Noeuds.FirstOrDefault(n => n.Id == chemin[i+1]);
                            if (prochainNoeud != null)
                            {
                                var prochainStation = prochainNoeud.Donnees as Station;
                                if (prochainStation != null)
                                {
                                    double distance = Station.CalculerDistance(station, prochainStation);
                                    distanceTotale += distance;
                                    
                                    // Afficher les changements de ligne
                                    if (station.Ligne != prochainStation.Ligne)
                                    {
                                        Console.ForegroundColor = ConsoleColor.Yellow;
                                        Console.WriteLine($"   Changement de ligne : {station.Ligne} -> {prochainStation.Ligne} (+ {TempsChangementLigne} min)");
                                        Console.ForegroundColor = ConsoleColor.White;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"\nDistance totale parcourue : {distanceTotale:F2} km");
            Console.ForegroundColor = ConsoleColor.White;
        }
        
        /// Trouve le noeud d'une station par son nom (prend la première occurrence)
        public Noeud<T>? TrouverStationParNom(string nom)
        {
            return Noeuds.FirstOrDefault(n => 
            {
                if (n.Donnees is Station station)
                {
                    return station.Nom.Equals(nom, StringComparison.OrdinalIgnoreCase);
                }
                return false;
            });
        }
        
        #endregion
    }
} 
