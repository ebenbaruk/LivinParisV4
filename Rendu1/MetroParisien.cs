using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;

namespace Rendu1
{
    public class MetroParisien
    {
        public Graphe<Station> GrapheMetro { get; private set; }
        public Dictionary<string, Noeud<Station>> StationsParNom { get; private set; }
        
        // Répertoire pour les visualisations
        private readonly string dossierVisualisations = "Visualisations";
        
        public MetroParisien(string cheminFichier)
        {
            GrapheMetro = new Graphe<Station>(estOriente: true, estPondere: true);
            StationsParNom = new Dictionary<string, Noeud<Station>>();
            ChargerDonnees(cheminFichier);
            
            // Créer le répertoire des visualisations s'il n'existe pas
            if (!Directory.Exists(dossierVisualisations))
            {
                Directory.CreateDirectory(dossierVisualisations);
            }
            
            // Générer une visualisation du graphe complet
            VisualiserGraphe();
        }
        
        private void ChargerDonnees(string cheminFichier)
        {
            if (!File.Exists(cheminFichier))
            {
                Console.WriteLine($"Le fichier {cheminFichier} n'existe pas.");
                return;
            }
            
            Dictionary<string, Station> stations = new Dictionary<string, Station>();
            Dictionary<string, List<string>> connexions = new Dictionary<string, List<string>>();
            Dictionary<string, List<int>> changements = new Dictionary<string, List<int>>();
            
            // Première lecture: créer toutes les stations
            string[] lignes = File.ReadAllLines(cheminFichier);
            
            // Ignorer la première ligne (en-têtes)
            for (int i = 1; i < lignes.Length; i++)
            {
                string ligne = lignes[i];
                string[] champs = ligne.Split(';');
                
                if (champs.Length < 6) continue;
                
                // Utiliser le nom de ligne complet
                string nomLigneComplet = champs[0];
                int numeroLigne;
                
                // Extraire le numéro principal de la ligne pour les comparaisons
                if (!int.TryParse(nomLigneComplet, out numeroLigne))
                {
                    // Extraire les chiffres du début de la chaîne
                    string chiffres = new string(nomLigneComplet.TakeWhile(char.IsDigit).ToArray());
                    if (!int.TryParse(chiffres, out numeroLigne))
                    {
                        Console.WriteLine($"Numéro de ligne invalide : {nomLigneComplet}");
                        continue;
                    }
                }
                
                string nomStation = champs[1];
                string coordonnees = champs[2];
                string stationPrecedente = champs[3];
                string stationSuivante = champs[4];
                string changementsStr = champs[5];
                
                // Clé unique pour la station: ligne_nom
                string cleStation = $"{nomLigneComplet}_{nomStation}";
                
                // Créer la station si elle n'existe pas déjà
                if (!stations.ContainsKey(cleStation))
                {
                    Station station = new Station(numeroLigne, nomLigneComplet, nomStation, coordonnees, stationPrecedente, stationSuivante, changementsStr);
                    stations[cleStation] = station;
                    
                    // Ajouter la station au graphe
                    GrapheMetro.AjouterNoeud(station);
                    
                    // Garder une référence au noeud
                    var noeud = GrapheMetro.Noeuds.LastOrDefault();
                    if (noeud != null) 
                    {
                        // Stocker la référence au noeud par son nom de station pour faciliter la recherche
                        if (!StationsParNom.ContainsKey(nomStation))
                        {
                            StationsParNom[nomStation] = noeud;
                        }
                    }
                    
                    // Enregistrer les connexions pour les traiter plus tard
                    connexions[cleStation] = new List<string>();
                    if (!string.IsNullOrEmpty(stationPrecedente))
                        connexions[cleStation].Add($"{nomLigneComplet}_{stationPrecedente}");
                    if (!string.IsNullOrEmpty(stationSuivante))
                        connexions[cleStation].Add($"{nomLigneComplet}_{stationSuivante}");
                    
                    // Enregistrer les changements de ligne
                    List<int> changementsLignes = new List<int>();
                    if (!string.IsNullOrEmpty(changementsStr))
                    {
                        string[] lignesChangement = changementsStr.Split(',');
                        foreach (string ligneChangement in lignesChangement)
                        {
                            if (int.TryParse(ligneChangement.Trim(), out int numLigne))
                                changementsLignes.Add(numLigne);
                        }
                    }
                    changements[cleStation] = changementsLignes;
                }
            }
            
            // Deuxième lecture: créer les liens entre les stations
            foreach (var kvp in connexions)
            {
                string stationCle = kvp.Key;
                List<string> stationsConnectees = kvp.Value;
                
                if (!stations.ContainsKey(stationCle)) continue;
                
                Station stationSource = stations[stationCle];
                int idSource = GrapheMetro.Noeuds.FindIndex(n => 
                    ((Station)n.Donnees).Ligne == stationSource.Ligne && 
                    ((Station)n.Donnees).Nom == stationSource.Nom
                ) + 1; // +1 car les IDs commencent à 1
                
                foreach (string stationConnecteeCle in stationsConnectees)
                {
                    if (!stations.ContainsKey(stationConnecteeCle)) continue;
                    
                    Station stationDestination = stations[stationConnecteeCle];
                    int idDestination = GrapheMetro.Noeuds.FindIndex(n => 
                        ((Station)n.Donnees).Ligne == stationDestination.Ligne && 
                        ((Station)n.Donnees).Nom == stationDestination.Nom
                    ) + 1; // +1 car les IDs commencent à 1
                    
                    // Temps de parcours entre stations (2 minutes par défaut)
                    double tempsParcours = GrapheMetro.TempsEntreStations;
                    
                    // Ajouter le lien (orienté)
                    GrapheMetro.AjouterLien(idSource, idDestination, tempsParcours);
                }
                
                // Ajouter les liens de correspondance
                if (changements.ContainsKey(stationCle))
                {
                    List<int> lignesCorrespondance = changements[stationCle];
                    
                    foreach (int ligneCorrespondance in lignesCorrespondance)
                    {
                        // Trouver toutes les stations de la ligne de correspondance avec le même nom
                        var stationsCorrespondance = stations.Values
                            .Where(s => s.Ligne == ligneCorrespondance && s.Nom == stationSource.Nom)
                            .ToList();
                        
                        foreach (var stationCorrespondance in stationsCorrespondance)
                        {
                            int idCorrespondance = GrapheMetro.Noeuds.FindIndex(n => 
                                ((Station)n.Donnees).Ligne == stationCorrespondance.Ligne && 
                                ((Station)n.Donnees).Nom == stationCorrespondance.Nom
                            ) + 1; // +1 car les IDs commencent à 1
                            
                            if (idSource != idCorrespondance)
                            {
                                // Ajouter un lien bidirectionnel pour les correspondances
                                GrapheMetro.AjouterLien(idSource, idCorrespondance, GrapheMetro.TempsChangementLigne);
                                GrapheMetro.AjouterLien(idCorrespondance, idSource, GrapheMetro.TempsChangementLigne);
                            }
                        }
                    }
                }
            }
            
            Console.WriteLine($"Graphe créé: {GrapheMetro.Noeuds.Count} stations et {GrapheMetro.Liens.Count} liaisons.");
        }
        
        /// <summary>
        /// Trouve le plus court chemin entre deux stations et compare les résultats des trois algorithmes
        /// </summary>
        public void TrouverPlusCourtChemin(string stationDepart, string stationArrivee)
        {
            var noeudDepart = GrapheMetro.TrouverStationParNom(stationDepart);
            var noeudArrivee = GrapheMetro.TrouverStationParNom(stationArrivee);

            if (noeudDepart == null || noeudArrivee == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Une ou plusieurs stations n'ont pas été trouvées.");
                Console.ForegroundColor = ConsoleColor.White;
                return;
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\nComparaison des algorithmes de recherche de plus court chemin :");
            Console.WriteLine("===========================================================");
            Console.ForegroundColor = ConsoleColor.White;

            // Dijkstra
            var sw = Stopwatch.StartNew();
            var (cheminDijkstra, tempsDijkstra) = GrapheMetro.Dijkstra(noeudDepart.Id, noeudArrivee.Id);
            sw.Stop();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n1. Algorithme de Dijkstra :");
            Console.WriteLine($"   Temps d'exécution : {sw.ElapsedMilliseconds} ms");
            Console.WriteLine($"   Temps de trajet : {tempsDijkstra} minutes");
            Console.ForegroundColor = ConsoleColor.White;
            GrapheMetro.AfficherChemin(cheminDijkstra, tempsDijkstra);

            // Bellman-Ford
            sw.Restart();
            var (cheminBellman, tempsBellman) = GrapheMetro.BellmanFord(noeudDepart.Id, noeudArrivee.Id);
            sw.Stop();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n2. Algorithme de Bellman-Ford :");
            Console.WriteLine($"   Temps d'exécution : {sw.ElapsedMilliseconds} ms");
            Console.WriteLine($"   Temps de trajet : {tempsBellman} minutes");
            Console.ForegroundColor = ConsoleColor.White;
            GrapheMetro.AfficherChemin(cheminBellman, tempsBellman);

            // Floyd-Warshall
            sw.Restart();
            var (distances, successeurs) = GrapheMetro.FloydWarshall();
            var (cheminFloyd, tempsFloyd) = GrapheMetro.ReconstruireChemin(noeudDepart.Id, noeudArrivee.Id, distances, successeurs);
            sw.Stop();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n3. Algorithme de Floyd-Warshall :");
            Console.WriteLine($"   Temps d'exécution : {sw.ElapsedMilliseconds} ms");
            Console.WriteLine($"   Temps de trajet : {tempsFloyd} minutes");
            Console.ForegroundColor = ConsoleColor.White;
            GrapheMetro.AfficherChemin(cheminFloyd, tempsFloyd);

            // Comparaison des résultats
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\nComparaison des résultats :");
            Console.WriteLine("==========================");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"Dijkstra : {tempsDijkstra} minutes");
            Console.WriteLine($"Bellman-Ford : {tempsBellman} minutes");
            Console.WriteLine($"Floyd-Warshall : {tempsFloyd} minutes");

            // Génération des visualisations
            GenererVisualisations(cheminDijkstra, cheminBellman, cheminFloyd, stationDepart, stationArrivee);
        }
        
        /// <summary>
        /// Visualise le graphe complet du métro parisien
        /// </summary>
        public void VisualiserGraphe()
        {
            try
            {
                var visualisation = new MetroVisualisation(GrapheMetro);
                string cheminFichier = Path.Combine(dossierVisualisations, "metro_graphe.png");
                visualisation.Dessiner(cheminFichier);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la visualisation du graphe: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Visualise un itinéraire particulier sur le graphe du métro
        /// </summary>
        /// <param name="chemin">Liste des IDs des stations constituant l'itinéraire</param>
        /// <param name="stationDepart">Nom de la station de départ</param>
        /// <param name="stationArrivee">Nom de la station d'arrivée</param>
        /// <param name="algorithme">Nom de l'algorithme utilisé pour générer l'itinéraire</param>
        public void VisualiserItineraire(List<int> chemin, string stationDepart, string stationArrivee, string algorithme)
        {
            try
            {
                var visualisation = new MetroVisualisation(GrapheMetro);
                
                // Normaliser les noms de stations pour éviter les problèmes avec les caractères spéciaux dans les noms de fichiers
                string nomFichierSanitise = $"itineraire_{algorithme}_{SanitiserNomFichier(stationDepart)}_{SanitiserNomFichier(stationArrivee)}.png";
                string cheminFichier = Path.Combine(dossierVisualisations, nomFichierSanitise);
                
                visualisation.Dessiner(cheminFichier, chemin);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la visualisation de l'itinéraire: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Sanitise un nom de station pour l'utilisation dans un nom de fichier
        /// </summary>
        private string SanitiserNomFichier(string nom)
        {
            // Remplacer les caractères non valides pour un nom de fichier
            char[] caracteresInterdits = Path.GetInvalidFileNameChars();
            string nomSanitise = string.Join("_", nom.Split(caracteresInterdits, StringSplitOptions.RemoveEmptyEntries))
                .Replace(" ", "_")
                .Replace("(", "")
                .Replace(")", "")
                .Replace("-", "_");
            
            return nomSanitise;
        }

        /// <summary>
        /// Génère les visualisations pour les différents itinéraires trouvés
        /// </summary>
        private void GenererVisualisations(List<int> cheminDijkstra, List<int> cheminBellman, List<int> cheminFloyd, string stationDepart, string stationArrivee)
        {
            // Créer le dossier Visualisations s'il n'existe pas
            string dossierVisualisations = "Visualisations";
            if (!Directory.Exists(dossierVisualisations))
            {
                Directory.CreateDirectory(dossierVisualisations);
            }

            // Générer les visualisations pour chaque algorithme
            VisualiserItineraire(cheminDijkstra, stationDepart, stationArrivee, "Dijkstra");
            VisualiserItineraire(cheminBellman, stationDepart, stationArrivee, "BellmanFord");
            VisualiserItineraire(cheminFloyd, stationDepart, stationArrivee, "FloydWarshall");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nLes visualisations des itinéraires ont été générées dans le dossier 'Visualisations'.");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
} 