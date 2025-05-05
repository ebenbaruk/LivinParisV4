using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;

namespace Rendu1.Modules
{
    /// <summary>
    /// Module pour la coloration du graphe,
    /// permet de charger le grapphe, appliquer la coloration de Welsh-Powell et afficher les résultatts comme demandé dans le sujet
    /// </summary>
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
            _graphe = new Graphe<string>(false, false); 
            _nomsUtilisateurs = new Dictionary<int, string>();
            _estCuisinier = new Dictionary<int, bool>();
            _couleurs = new Dictionary<int, int>();
            _nombreCouleurs = 0;
        }

        /// <summary>
        /// Charge le graphe,
        /// </summary>
        public void ChargerGraphe()
        {
            try
            {
                _graphe = new Graphe<string>(false, false);
                _nomsUtilisateurs.Clear();

                _estCuisinier.Clear();

                Console.WriteLine("\n=== CHARGEMENT DU GRAPHE ===\n");

                /// 1. Charger tous les utilisateurs
                string sqlUtilisateurs = @"
                    SELECT 
                        u.ClientID,
                        CONCAT(COALESCE(u.NomU, ''), ' ', COALESCE(u.PrenomU, '')) as Nom,
                        CASE WHEN c.ClientID IS NOT NULL THEN 1 ELSE 0 END as EstCuisinier
                    FROM Utilisateur u
                    LEFT JOIN Cuisinier c ON u.ClientID = c.ClientID";

                Console.WriteLine("1. Chargement des utilisateurs :");
                Console.WriteLine("--------------------------------");
                using (var cmd = new MySqlCommand(sqlUtilisateurs, _db.GetConnection()))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int userId = reader.GetInt32("ClientID");
                        string nom = reader.GetString("Nom").Trim();


                        bool estCuisinier = reader.GetInt32("EstCuisinier") == 1;

                        if (!string.IsNullOrWhiteSpace(nom))
                        {
                            _nomsUtilisateurs[userId] = nom;
                            _estCuisinier[userId] = estCuisinier;


                            _graphe.AjouterNoeud(nom);
                            Console.WriteLine($"+ {nom} (ID: {userId}) - {(estCuisinier ? "Cuisinier" : "Client")}");
                        }
                    }
                }

                /// 2. Charger toutes les commandes
                string sqlCommandes = @"
                    SELECT 
                        b.CommandeID,
                        b.ClientID,
                        b.CuisinierID,
                        b.Statut
                    FROM BonDeCommande_Liv b
                    WHERE b.Statut != 'Annulée'
                    AND b.CuisinierID IS NOT NULL";

                Console.WriteLine("\n2. Chargement des commandes :");
                Console.WriteLine("-----------------------------");
                int nbCommandesTotal = 0;

                int nbCommandesValides = 0;

                using (var cmd = new MySqlCommand(sqlCommandes, _db.GetConnection()))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        nbCommandesTotal++;
                        string commandeId = reader.GetString("CommandeID");
                        int clientId = reader.GetInt32("ClientID");


                        int cuisinierId = reader.GetInt32("CuisinierID");
                        string statut = reader.GetString("Statut");

                        if (_nomsUtilisateurs.ContainsKey(clientId) && _nomsUtilisateurs.ContainsKey(cuisinierId))
                        {
                            string nomClient = _nomsUtilisateurs[clientId];

                            string nomCuisinier = _nomsUtilisateurs[cuisinierId];

                            var noeudClient = _graphe.Noeuds.FirstOrDefault(n => n.Donnees == nomClient);
                            var noeudCuisinier = _graphe.Noeuds.FirstOrDefault(n => n.Donnees == nomCuisinier);

                            if (noeudClient != null && noeudCuisinier != null)
                            {
                                /// Ajouter le lien et mettre à jour les voisins manuellement
                                _graphe.AjouterLien(noeudClient.Id, noeudCuisinier.Id);
                                
                                /// Mettre à jour les voisins manuellement
                                if (!noeudClient.Voisins.Contains(noeudCuisinier))
                                {
                                    noeudClient.Voisins.Add(noeudCuisinier);
                                }


                                if (!noeudCuisinier.Voisins.Contains(noeudClient))
                                {
                                    noeudCuisinier.Voisins.Add(noeudClient);
                                }
                                
                                nbCommandesValides++;
                                Console.WriteLine($"+ Commande {commandeId}: {nomClient} -> {nomCuisinier} ({statut})");
                            }
                        }
                    }
                }

                /// 3. Afficher l'état du graphe
                Console.WriteLine("\n=== ÉTAT DU GRAPHE ===\n");
                Console.WriteLine($"Nombre total d'utilisateurs : {_nomsUtilisateurs.Count}");
                Console.WriteLine($"- Clients : {_nomsUtilisateurs.Count(x => !_estCuisinier[x.Key])}");


                Console.WriteLine($"- Cuisiniers : {_nomsUtilisateurs.Count(x => _estCuisinier[x.Key])}");
                Console.WriteLine($"\nNombre de commandes traitées : {nbCommandesTotal}");

                Console.WriteLine($"Nombre de liens créés : {nbCommandesValides}");

                Console.WriteLine("\n=== VISUALISATION DU GRAPHE ===\n");
                foreach (var noeud in _graphe.Noeuds)
                {
                    bool estCuisinier = _estCuisinier[int.Parse(_nomsUtilisateurs.First(x => x.Value == noeud.Donnees).Key.ToString())];
                    Console.WriteLine($"\n{(estCuisinier ? "👨‍🍳" : "👤")} {noeud.Donnees} (ID: {noeud.Id})");

                    Console.WriteLine($"Degré: {noeud.Voisins.Count}");
                    if (noeud.Voisins.Any())
                    {
                        Console.WriteLine("Relations :");
                        foreach (var voisin in noeud.Voisins)
                        {
                            bool voisinEstCuisinier = _estCuisinier[int.Parse(_nomsUtilisateurs.First(x => x.Value == voisin.Donnees).Key.ToString())];
                            Console.WriteLine($"  └─{(voisinEstCuisinier ? "👨‍🍳" : "👤")} {voisin.Donnees}");
                        }
                    }
                }

                Console.WriteLine("\nAppuyez sur une touche pour continuer...");


                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n ERREUR : {ex.Message}");


                Console.WriteLine($"Stack trace : {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Applique la coloration de Welsh-Powell,
        /// </summary>
        public void AppliquerWelshPowell()
        {
            /// 1. Trier les noeuds par degré décroissant
            var noeudsTriesParDegre = _graphe.Noeuds
                .OrderByDescending(n => n.Voisins.Count)
                .ToList();

            _couleurs.Clear();
            _nombreCouleurs = 0;

            /// 2. Colorer chaque noeud
            foreach (var noeud in noeudsTriesParDegre)
            {
                /// Trouver la première couleur disponible
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

        /// <summary>
        /// Afffiche les résultats,
        /// </summary>
        public void AfficherResultats()
        {
            Console.WriteLine("\n=== ANALYSE DU GRAPHE DE RELATIONS ===\n");

            /// 1. Statistiques de base
            Console.WriteLine("1. STATISTIQUES DE BASE");
            Console.WriteLine("----------------------");

            Console.WriteLine($"Nombre de sommets (ordre) : {_graphe.ObtenirOrdre()}");
            Console.WriteLine($"- Clients : {_nomsUtilisateurs.Count(x => !_estCuisinier[x.Key])}");
            Console.WriteLine($"- Cuisiniers : {_nomsUtilisateurs.Count(x => _estCuisinier[x.Key])}");


            Console.WriteLine($"Nombre de relations (taille) : {_graphe.ObtenirTaille()}");

            Console.WriteLine($"Nombre minimal de couleurs : {_nombreCouleurs}");

            /// 2. Propriétés du graphe
            Console.WriteLine("\n2. PROPRIÉTÉS DU GRAPHE");
            Console.WriteLine("---------------------");
            
            /// Vérification biparti
            bool estBiparti = VerifierGrapheBiparti();
            Console.WriteLine($"\nBipartition :");
            Console.WriteLine($"Le graphe {(estBiparti ? "est" : "n'est pas")} biparti");
            if (estBiparti)
            {
                Console.WriteLine("Justification : Le graphe peut être divisé en deux ensembles distincts");

                Console.WriteLine("(clients et cuisiniers) sans connexions à l'intérieur de chaque ensemble.");
            }
            else
            {
                Console.WriteLine("Justification : Il existe des relations qui empêchent la séparation");
                Console.WriteLine("en deux groupes distincts sans connexions internes.");
            }

            /// Vérification planarité
            bool estPlanaire = VerifierGraphePlanaire();
            Console.WriteLine($"\nPlanarité :");
            Console.WriteLine($"Le graphe {(estPlanaire ? "est" : "n'est pas")} planaire");


            Console.WriteLine($"Justification : Selon le théorème d'Euler pour un graphe planaire :");
            Console.WriteLine($"- Nombre d'arêtes (e) = {_graphe.ObtenirTaille()}");


            Console.WriteLine($"- Nombre de sommets (v) = {_graphe.ObtenirOrdre()}");
            Console.WriteLine($"- Pour être planaire : e ≤ 3v - 6 (pour v ≥ 3)");




            Console.WriteLine($"- Dans notre cas : {_graphe.ObtenirTaille()} ≤ {3 * _graphe.ObtenirOrdre() - 6}");

            /// 3. Groupes indépendants
            Console.WriteLine("\n3. GROUPES INDÉPENDANTS (par couleur)");
            Console.WriteLine("-----------------------------------");
            for (int couleur = 1; couleur <= _nombreCouleurs; couleur++)
            {
                var noeudsMemeCouleur = _graphe.Noeuds.Where(n => _couleurs[n.Id] == couleur).ToList();
                Console.WriteLine($"\nGroupe {couleur} ({noeudsMemeCouleur.Count} membres) :");

                foreach (var noeud in noeudsMemeCouleur)
                {
                    string userId = _nomsUtilisateurs.First(x => x.Value == noeud.Donnees).Key.ToString();
                    bool estCuisinier = _estCuisinier[int.Parse(userId)];
                    Console.WriteLine($"- {noeud.Donnees} ({(estCuisinier ? "👨‍🍳 Cuisinier" : "👤 Client")})");
                    Console.WriteLine($"  Degré : {noeud.Voisins.Count} relation(s)");
                }
            }

            /// 4. Analyse détaillée
            Console.WriteLine("\n4. ANALYSE DÉTAILLÉE");


            Console.WriteLine("-----------------");
            
            /// Densité et degrés
            int v = _graphe.ObtenirOrdre();
            int e = _graphe.ObtenirTaille();
            double densite = v <= 1 ? 0 : (2.0 * e) / (v * (v - 1));
            var degres = _graphe.Noeuds.Select(n => n.Voisins.Count).ToList();
            double degreMoyen = degres.Any() ? degres.Average() : 0;


            int degreMax = degres.Any() ? degres.Max() : 0;


            int degreMin = degres.Any() ? degres.Min() : 0;

            Console.WriteLine($"\nMesures de connectivité :");
            Console.WriteLine($"- Densité du graphe : {densite:F2}");
            Console.WriteLine($"- Degré moyen : {degreMoyen:F2} relations par utilisateur");
            Console.WriteLine($"- Degré maximum : {degreMax} relations");


            Console.WriteLine($"- Degré minimum : {degreMin} relations");

            /// Top cuisiniers
            Console.WriteLine("\nTop cuisiniers par nombre de clients :");
            var cuisiniers = _graphe.Noeuds
                .Where(n => _estCuisinier[int.Parse(_nomsUtilisateurs.First(x => x.Value == n.Donnees).Key.ToString())])
                .OrderByDescending(n => n.Voisins.Count)
                .ToList();

            foreach (var cuisinier in cuisiniers)
            {
                Console.WriteLine($"- 👨‍🍳 {cuisinier.Donnees} : {cuisinier.Voisins.Count} client(s)");
            }

            /// Top clients
            Console.WriteLine("\nTop 5 clients par nombre de cuisiniers différents :");
            var clients = _graphe.Noeuds
                .Where(n => !_estCuisinier[int.Parse(_nomsUtilisateurs.First(x => x.Value == n.Donnees).Key.ToString())])
                .OrderByDescending(n => n.Voisins.Count)
                .Take(5)
                .ToList();

            foreach (var client in clients)
            {
                Console.WriteLine($"- 👤 {client.Donnees} : {client.Voisins.Count} cuisinier(s)");
            }

            Console.WriteLine("\nAppuyez sur une touche pour continuer...");
            Console.ReadKey();
        }

        /// <summary>
        /// Vérifie si le graphe est biparti, 
        /// </summary>
        private bool VerifierGrapheBiparti()
        {
            if (_graphe.ObtenirOrdre() == 0) return true;

            var couleurs = new Dictionary<int, int>();
            foreach (var noeud in _graphe.Noeuds)
            {
                couleurs[noeud.Id] = -1;
            }

            /// BFS pour la coloration bipartie
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

            /// Utilisation du théorème d'Euler pour les graphes planaires
            /// Pour un graphe planaire : e ≤ 3v - 6 (pour v ≥ 3)
            if (v < 3) return true;
            return e <= 3 * v - 6;
        }
    }
} 