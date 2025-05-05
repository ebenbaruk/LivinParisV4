using System;
using System.IO;
using System.Diagnostics;
using System.Linq;
using Rendu1.Modules;

namespace Rendu1
{
    /// <summary>
    /// Programme principal pour l'app Liv'in Paris
    /// </summary>
    class Program
    {
        // Couleurs pour la console
        private static readonly ConsoleColor CouleurTitre = ConsoleColor.Cyan;
        private static readonly ConsoleColor CouleurSousTitre = ConsoleColor.Yellow;
        private static readonly ConsoleColor CouleurTexte = ConsoleColor.White;
        private static readonly ConsoleColor CouleurErreur = ConsoleColor.Red;
        private static readonly ConsoleColor CouleurSucces = ConsoleColor.Green;
        private static readonly ConsoleColor CouleurInfo = ConsoleColor.Blue;

        /// <summary>
        /// Point d'entrée principal de l'application
        /// </summary>
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            
            AfficherTitre("LIV'IN PARIS - SYSTÈME DE GESTION");

            // Connexion à la base de données MySQL
            var db = new DatabaseManager(password: "2015Franc");
            if (!db.Connect())
            {
                Console.WriteLine("Appuyez sur une touche pour quitter...");
                Console.ReadKey();
                return;
            }

            /// Chargement des données du métro parisien
            string cheminBase = AppDomain.CurrentDomain.BaseDirectory;
            string cheminFichierMetro = Path.Combine(cheminBase, "DataMetro", "metro.csv");
            var metroParisien = new MetroParisien(cheminFichierMetro);

            /// Initialisation des modules
            var moduleAuth = new AuthModule(db);
            var moduleClient = new ClientModule(db);
            var moduleCuisinier = new CuisinierModule(db);
            var moduleFidelisation = new FidelisationModule(db);
            var moduleCommande = new CommandeModule(db, metroParisien, moduleClient, moduleFidelisation);
        
            var moduleStatistiques = new StatistiquesModule(db);
            var moduleGraphe = new ColorationGraphe(db);

            while (true)
            {
                /// Afficher le menu de connexion
                var session = moduleAuth.AfficherMenuConnexion();
                if (session == null)
                    break;

                /// Gérer la session selon le type d'utilisateur
                switch (session.Type)
                {
                    case AuthModule.UserType.Admin:
                        GererSessionAdmin(moduleClient, moduleCuisinier, moduleCommande, moduleStatistiques, metroParisien, moduleGraphe);
                        break;
                    case AuthModule.UserType.Client:
                        GererSessionClient(session, moduleCommande);
                        break;
                    case AuthModule.UserType.Cuisinier:
                        GererSessionCuisinier(session, moduleCommande);
                        break;
                }
            }

            /// Déconnexion de la base de données
            db.Disconnect();
            Console.WriteLine("\nMerci d'avoir utilisé Liv'in Paris !");
        }

        /// <summary>
        /// Gestion de la session pour l'administrateur
        /// </summary>
        static void GererSessionAdmin(ClientModule moduleClient, CuisinierModule moduleCuisinier, 
            CommandeModule moduleCommande, StatistiquesModule moduleStatistiques, MetroParisien metroParisien, ColorationGraphe moduleGraphe)
        {
            bool continuer = true;
            while (continuer)
            {
                Console.Clear();
                AfficherTitre("MENU ADMINISTRATEUR");
                Console.WriteLine("1. Gestion des clients");
                Console.WriteLine("2. Gestion des cuisiniers");
                Console.WriteLine("3. Gestion des commandes");
                Console.WriteLine("4. Calcul d'itinéraires");
                Console.WriteLine("5. Statistiques");
                Console.WriteLine("6. Analyse du graphe de relations");
                Console.WriteLine("0. Déconnexion");

                Console.Write("\nVotre choix : ");
                string? choix = Console.ReadLine();

                switch (choix)
                {
                    case "1":
                        moduleClient.AfficherMenuClient();
                        break;
                    case "2":
                        moduleCuisinier.AfficherMenuCuisinier();
                        break;
                    case "3":
                        moduleCommande.AfficherMenuCommande();
                        break;
                    case "4":
                        GestionItineraires(metroParisien);
                        break;
                    case "5":
                        moduleStatistiques.AfficherMenuStatistiques();
                        break;
                    case "6":
                        AfficherAnalyseGraphe(moduleGraphe);
                        break;
                    case "0":
                        continuer = false;
                        break;
                    default:
                        Console.WriteLine("Choix invalide. Appuyez sur une touche pour continuer...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        /// <summary>
        /// Gestion de la session pour le client
        /// </summary>
        static void GererSessionClient(AuthModule.UserSession session, CommandeModule moduleCommande)
        {
            bool continuer = true;
            while (continuer)
            {
                Console.Clear();
                AfficherTitre($"MENU CLIENT - {session.Nom}");
                Console.WriteLine("1. Passer une nouvelle commande");
                Console.WriteLine("2. Voir mes commandes");
                Console.WriteLine("0. Déconnexion");

                Console.Write("\nVotre choix : ");
                string? choix = Console.ReadLine();

                switch (choix)
                {
                    case "1":
                        moduleCommande.PasserCommandeClient(session);
                        break;
                    case "2":
                        moduleCommande.VoirCommandesClient(session);
                        break;
                    case "0":
                        continuer = false;
                        break;
                    default:
                        Console.WriteLine("Choix invalide. Appuyez sur une touche pour continuer...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        /// <summary>
        /// Gestion de la session pour le cuisinier
        /// </summary>
        static void GererSessionCuisinier(AuthModule.UserSession session, CommandeModule moduleCommande)
        {
            bool continuer = true;
            while (continuer)
            {
                Console.Clear();
                AfficherTitre($"MENU CUISINIER - {session.Nom}");
                Console.WriteLine("1. Voir les commandes en attente");
                Console.WriteLine("2. Gérer mes plats");
                Console.WriteLine("3. Voir mon historique de livraisons");
                Console.WriteLine("0. Déconnexion");

                Console.Write("\nVotre choix : ");
                string? choix = Console.ReadLine();

                switch (choix)
                {
                    case "1":
                        moduleCommande.VoirCommandesEnAttenteCuisinier(session);
                        break;
                    case "2":
                        moduleCommande.GererPlats(session);
                        break;
                    case "3":
                        moduleCommande.VoirHistoriqueLivraisonsCuisinier(session);
                        break;
                    case "0":
                        continuer = false;
                        break;
                    default:
                        Console.WriteLine("Choix invalide. Appuyez sur une touche pour continuer...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        /// <summary>
        /// Gestion des itinéraires
        /// </summary>
        static void GestionItineraires(MetroParisien metroParisien)
        {
            /// Vérifier si le fichier de données existe
            string cheminBase = AppDomain.CurrentDomain.BaseDirectory;
            string cheminFichierMetro = Path.Combine(cheminBase, "DataMetro", "metro.csv");
            
            if (!File.Exists(cheminFichierMetro))
            {
                AfficherErreur($"Le fichier {cheminFichierMetro} n'existe pas.");
                AfficherTexte("Veuillez vérifier que le dossier DataMetro contient le fichier metro.csv.");
                AfficherTexte("\nAppuyez sur une touche pour continuer...");
                Console.ReadKey();
                return;
            }

            bool continuer = true;
            while (continuer)
            {
                /// Recherche d'itinéraire
                if (RechercherItinerairePersonnalisé(metroParisien) == false)
                {
                    continuer = false;
                }
                
                if (continuer)
                {
                    AfficherSeparateur();
                    AfficherTexte("Appuyez sur Entrée pour rechercher un nouvel itinéraire");
                    AfficherTexte("ou sur 'q' pour quitter");
                    AfficherSeparateur();
                    
                    string choix = Console.ReadLine() ?? "";
                    if (choix.ToLower() == "q")
                    {
                        continuer = false;
                    }
                }
            }
        }
        
        /// <summary>
        /// Permet à l'utilisateur de rechercher un itinéraire personnalisé
        /// </summary>
        static bool RechercherItinerairePersonnalisé(MetroParisien metroParisien)
        {
            AfficherSousTitre("RECHERCHE D'ITINÉRAIRE PERSONNALISÉ");
            
            /// Sélection de la station de départ
            string stationDepart = SélectionnerStation(metroParisien, "départ");
            if (string.IsNullOrEmpty(stationDepart))
                return false;
            
            /// Sélection de la station d'arrivée
            string stationArrivee = SélectionnerStation(metroParisien, "arrivée");
            if (string.IsNullOrEmpty(stationArrivee))
                return false;
            
            /// Éviter de rechercher un trajet vers la même station
            if (stationDepart == stationArrivee)
            {
                AfficherErreur("\nLes stations de départ et d'arrivée sont identiques.");
                return true;
            }
            
            AfficherSeparateur();
            AfficherTexte($"Recherche d'itinéraire de {stationDepart} à {stationArrivee}");
            AfficherSeparateur();
            
            // Recherche du plus court chemin
            metroParisien.TrouverPlusCourtChemin(stationDepart, stationArrivee);
            
            return true;
        }
        
        /// <summary>
        /// Permet à l'utilisateur de sélectionner une station existante
        /// </summary>
        
        static string SélectionnerStation(MetroParisien metroParisien, string type)
        {
            while (true)
            {
                AfficherTexte($"\nEntrez le nom (ou une partie) de la station de {type} (ou 'q' pour quitter): ");
                string? recherche = Console.ReadLine();
                
                if (recherche?.ToLower() == "q")
                    return string.Empty;
                
                if (string.IsNullOrWhiteSpace(recherche) || recherche.Length < 2)
                {
                    AfficherErreur("Veuillez entrer au moins 2 caractères pour la recherche.");
                    continue;
                }

                /// Rechercher toutes les stations qui contiennent la chaîne de recherche (insensible à la casse)
                var correspondances = metroParisien.StationsParNom.Keys
                    .Where(nom => nom.IndexOf(recherche, StringComparison.OrdinalIgnoreCase) >= 0)
                    .OrderBy(nom => nom)
                    .ToList();
                
                if (correspondances.Count == 0)
                {
                    AfficherErreur("\nAucune station ne correspond à votre recherche. Essayez avec un autre terme.");
                    continue;
                }
                
                // Si une seule correspondance exacte est trouvée, la sélectionner automatiquement
                if (correspondances.Count == 1 || correspondances.Contains(recherche, StringComparer.OrdinalIgnoreCase))
                {
                    string stationExacte = correspondances.FirstOrDefault(s => s.Equals(recherche, StringComparison.OrdinalIgnoreCase)) ?? correspondances.First();
                    AfficherSucces($"\nStation sélectionnée: {stationExacte}");
                    return stationExacte;
                }
                
                /// Afficher les résultats de la recherche
                AfficherTexte($"\n{correspondances.Count} stations trouvées pour '{recherche}':");
                
                /// Limiter l'affichage à 15 résultats maximum
                int maxResultats = Math.Min(15, correspondances.Count);
                for (int i = 0; i < maxResultats; i++)
                {
                    AfficherTexte($"{i + 1}. {correspondances[i]}");
                }
                
                if (correspondances.Count > maxResultats)
                {
                    AfficherTexte($"... et {correspondances.Count - maxResultats} autres stations.");
                    AfficherTexte("Veuillez affiner votre recherche pour voir plus de résultats.");
                }
                
                AfficherTexte("\nEntrez le numéro de la station désirée (ou 0 pour refaire la recherche): ");
                string? choixStr = Console.ReadLine();
                if (string.IsNullOrEmpty(choixStr) || !int.TryParse(choixStr, out int choix) || choix < 0 || choix > maxResultats)
                {
                    AfficherErreur("Choix invalide. Veuillez réessayer.");
                    continue;
                }
                
                if (choix == 0)
                    continue;
                
                return correspondances[choix - 1];
            }
        }

        private static void AfficherAnalyseGraphe(ColorationGraphe moduleGraphe)
        {
            Console.Clear();
            Console.WriteLine("=== ANALYSE DU GRAPHE DE RELATIONS ===\n");
            Console.WriteLine("Chargement des données...");
            
            moduleGraphe.ChargerGraphe();
            Console.WriteLine("Application de l'algorithme de Welsh-Powell...");
            moduleGraphe.AppliquerWelshPowell();
            
            moduleGraphe.AfficherResultats();
            
            Console.WriteLine("\nAppuyez sur une touche pour continuer...");
            Console.ReadKey();
        }

        #region Méthodes d'affichage

        private static void AfficherTitre(string titre)
        {
            Console.ForegroundColor = CouleurTitre;
            Console.WriteLine("\n" + new string('=', titre.Length + 4));
            Console.WriteLine($"= {titre} =");
            Console.WriteLine(new string('=', titre.Length + 4) + "\n");
            Console.ForegroundColor = CouleurTexte;
        }

        /// <summary>
        /// Affiche un sous titre
        /// </summary>
        private static void AfficherSousTitre(string sousTitre)
        {
            Console.ForegroundColor = CouleurSousTitre;
            Console.WriteLine("\n" + new string('-', sousTitre.Length + 4));
            Console.WriteLine($"- {sousTitre} -");
            Console.WriteLine(new string('-', sousTitre.Length + 4) + "\n");
            Console.ForegroundColor = CouleurTexte;
        }

        /// <summary>
        /// Affiche un séparateur
        /// </summary>
        private static void AfficherSeparateur()
        {
            Console.ForegroundColor = CouleurInfo;
            Console.WriteLine(new string('=', 50));
            Console.ForegroundColor = CouleurTexte;
        }

        /// <summary>
        /// Affiche un texte
        /// </summary>
        private static void AfficherTexte(string texte)
        {
            Console.ForegroundColor = CouleurTexte;
            Console.WriteLine(texte);
        }

        /// <summary>
        /// Affiche un message d'erreur
        /// </summary>
        private static void AfficherErreur(string message)
        {
            Console.ForegroundColor = CouleurErreur;
            Console.WriteLine(message);
            Console.ForegroundColor = CouleurTexte;
        }

        /// <summary>
        /// Affiche un message de succès
        /// </summary>
        private static void AfficherSucces(string message)
        {
            Console.ForegroundColor = CouleurSucces;
            Console.WriteLine(message);
            Console.ForegroundColor = CouleurTexte;
        }

        private static void AfficherInfo(string message)
        {
            Console.ForegroundColor = CouleurInfo;
            Console.WriteLine(message);
            Console.ForegroundColor = CouleurTexte;
        }

        #endregion
    }
}
