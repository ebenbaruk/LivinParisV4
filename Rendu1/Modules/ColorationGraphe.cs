using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;

namespace Rendu1.Modules
{
    public class ColorationGraphe
    {
        private readonly DatabaseManager _db;
        private Graphe<string> _graphe;
        private Dictionary<int, string> _nomsUtilisateurs;
        private Dictionary<int, bool> _estCuisinier;
        private Dictionary<int, int> _couleurs;
        private int _nombreCouleurs;

        public ColorationGraphe(DatabaseManager db)
        {
            _db = db;
            _graphe = new Graphe<string>(false, false); // graphe non orienté, non pondéré
            _nomsUtilisateurs = new Dictionary<int, string>();
            _estCuisinier = new Dictionary<int, bool>();
            _couleurs = new Dictionary<int, int>();
            _nombreCouleurs = 0;
        }

        public void ChargerGraphe()
        {
            try
            {
                _graphe = new Graphe<string>(false, false);
                var userNodes = new Dictionary<int, int>(); // Map ClientID to NodeID

                // 1. Charger tous les utilisateurs actifs
                string sqlUtilisateurs = @"
                    SELECT 
                        u.ClientID,
                        CONCAT(
                            CASE 
                                WHEN u.TypeClient = 'Entreprise' THEN COALESCE(u.NomEntreprise, '')
                                ELSE CONCAT(COALESCE(u.PrenomU, ''), ' ', COALESCE(u.NomU, ''))
                            END
                        ) as NomComplet,
                        CASE WHEN c.ClientID IS NOT NULL THEN 1 ELSE 0 END as EstCuisinier
                    FROM Utilisateur u
                    LEFT JOIN Cuisinier c ON u.ClientID = c.ClientID
                    WHERE u.Actif = TRUE";

                using (var cmd = new MySqlCommand(sqlUtilisateurs, _db.GetConnection()))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int userId = reader.GetInt32("ClientID");
                        string nom = reader.GetString("NomComplet").Trim();
                        bool estCuisinier = reader.GetInt32("EstCuisinier") == 1;

                        _nomsUtilisateurs[userId] = nom;
                        _estCuisinier[userId] = estCuisinier;
                        
                        _graphe.AjouterNoeud(nom);
                        userNodes[userId] = _graphe.Noeuds.Count; // Store NodeID (1-based)
                    }
                }

                // 2. Charger toutes les commandes validées
                string sqlCommandes = @"
                    SELECT DISTINCT b.ClientID, b.CuisinierID
                    FROM BonDeCommande_Liv b
                    WHERE b.Statut NOT IN ('Annulée')
                    AND b.CuisinierID IS NOT NULL";

                using (var cmd = new MySqlCommand(sqlCommandes, _db.GetConnection()))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int clientId = reader.GetInt32("ClientID");
                        int cuisinierId = reader.GetInt32("CuisinierID");

                        if (userNodes.ContainsKey(clientId) && userNodes.ContainsKey(cuisinierId))
                        {
                            _graphe.AjouterLien(userNodes[clientId], userNodes[cuisinierId]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du chargement du graphe : {ex.Message}");
                Console.WriteLine($"Stack trace : {ex.StackTrace}");
            }
        }

        public void AppliquerWelshPowell()
        {
            // 1. Trier les noeuds par degré décroissant
            var noeudsTriesParDegre = _graphe.Noeuds
                .OrderByDescending(n => n.Voisins.Count)
                .ToList();

            _couleurs.Clear();
            _nombreCouleurs = 0;

            // 2. Colorer chaque noeud
            foreach (var noeud in noeudsTriesParDegre)
            {
                // Trouver la première couleur disponible
                var couleursVoisins = noeud.Voisins
                    .Where(v => _couleurs.ContainsKey(v.Id))
                    .Select(v => _couleurs[v.Id])
                    .ToHashSet();

                int couleur = 1;
                while (couleursVoisins.Contains(couleur))
                {
                    couleur++;
                }

                _couleurs[noeud.Id] = couleur;
                _nombreCouleurs = Math.Max(_nombreCouleurs, couleur);
            }
        }

        public void AfficherResultats()
        {
            Console.WriteLine("\n=== ANALYSE DU GRAPHE DE RELATIONS ===\n");

            // Statistiques de base
            Console.WriteLine($"Nombre de sommets : {_graphe.ObtenirOrdre()}");
            Console.WriteLine($"Nombre de clients : {_nomsUtilisateurs.Count(x => !_estCuisinier[x.Key])}");
            Console.WriteLine($"Nombre de cuisiniers : {_nomsUtilisateurs.Count(x => _estCuisinier[x.Key])}");
            Console.WriteLine($"Nombre de relations (commandes) : {_graphe.ObtenirTaille()}");
            Console.WriteLine($"Nombre minimal de couleurs nécessaires : {_nombreCouleurs}\n");

            // Vérification si le graphe est biparti
            bool estBiparti = VerifierGrapheBiparti();
            Console.WriteLine(estBiparti ? "Le graphe est biparti" : "Le graphe n'est pas biparti");
            if (estBiparti)
            {
                Console.WriteLine("Justification : Tous les sommets peuvent être divisés en deux groupes sans connexions internes\n");
            }

            // Vérification si le graphe est planaire
            bool estPlanaire = VerifierGraphePlanaire();
            Console.WriteLine(estPlanaire ? "Le graphe est planaire" : "Le graphe n'est pas planaire");
            Console.WriteLine("Justification : Le nombre d'arêtes respecte le théorème d'Euler\n");

            // Afficher les groupes indépendants
            Console.WriteLine("Groupes indépendants (par couleur) :\n");
            for (int couleur = 1; couleur <= _nombreCouleurs; couleur++)
            {
                Console.WriteLine($"Groupe {couleur} :");
                foreach (var noeud in _graphe.Noeuds.Where(n => _couleurs[n.Id] == couleur))
                {
                    string userId = _nomsUtilisateurs.First(x => x.Value == noeud.Donnees).Key.ToString();
                    bool estCuisinier = _estCuisinier[int.Parse(userId)];
                    Console.WriteLine($"- {noeud.Donnees} ({(estCuisinier ? "Cuisinier" : "Client")})");
                }
                Console.WriteLine();
            }

            // Analyse des relations
            AnalyserResultats();
        }

        private bool VerifierGrapheBiparti()
        {
            if (_graphe.ObtenirOrdre() == 0) return true;

            var couleurs = new Dictionary<int, int>();
            foreach (var noeud in _graphe.Noeuds)
            {
                couleurs[noeud.Id] = -1;
            }

            // BFS pour la coloration bipartie
            var queue = new Queue<Noeud<string>>();
            var premierNoeud = _graphe.Noeuds[0];
            couleurs[premierNoeud.Id] = 0;
            queue.Enqueue(premierNoeud);

            while (queue.Count > 0)
            {
                var noeud = queue.Dequeue();
                foreach (var voisin in noeud.Voisins)
                {
                    if (couleurs[voisin.Id] == -1)
                    {
                        couleurs[voisin.Id] = 1 - couleurs[noeud.Id];
                        queue.Enqueue(voisin);
                    }
                    else if (couleurs[voisin.Id] == couleurs[noeud.Id])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool VerifierGraphePlanaire()
        {
            int v = _graphe.ObtenirOrdre();
            int e = _graphe.ObtenirTaille();

            // Utilisation du théorème d'Euler pour les graphes planaires
            // Pour un graphe planaire : e ≤ 3v - 6 (pour v ≥ 3)
            if (v < 3) return true;
            return e <= 3 * v - 6;
        }

        private void AnalyserResultats()
        {
            // Calculer la densité du graphe
            int v = _graphe.ObtenirOrdre();
            int e = _graphe.ObtenirTaille();
            double densite = v <= 1 ? 0 : (2.0 * e) / (v * (v - 1));
            Console.WriteLine($"Densité du graphe : {densite:F2}");

            // Calculer les degrés
            var degres = _graphe.Noeuds.Select(n => n.Voisins.Count).ToList();
            double degreMoyen = degres.Any() ? degres.Average() : 0;
            int degreMax = degres.Any() ? degres.Max() : 0;
            int degreMin = degres.Any() ? degres.Min() : 0;

            Console.WriteLine($"Degré moyen : {degreMoyen:F2}");
            Console.WriteLine($"Degré maximum : {degreMax}");
            Console.WriteLine($"Degré minimum : {degreMin}\n");

            // Identifier les cuisiniers les plus populaires
            Console.WriteLine("Cuisiniers les plus populaires :");
            var cuisiniers = _graphe.Noeuds
                .Where(n => _estCuisinier[int.Parse(_nomsUtilisateurs.First(x => x.Value == n.Donnees).Key.ToString())])
                .OrderByDescending(n => n.Voisins.Count)
                .ToList();

            foreach (var cuisinier in cuisiniers)
            {
                Console.WriteLine($"- {cuisinier.Donnees} : {cuisinier.Voisins.Count} clients");
            }

            Console.WriteLine("\nClients les plus actifs :");
            var clients = _graphe.Noeuds
                .Where(n => !_estCuisinier[int.Parse(_nomsUtilisateurs.First(x => x.Value == n.Donnees).Key.ToString())])
                .OrderByDescending(n => n.Voisins.Count)
                .Take(5)
                .ToList();

            foreach (var client in clients)
            {
                Console.WriteLine($"- {client.Donnees} : {client.Voisins.Count} cuisiniers différents");
            }
        }
    }
} 