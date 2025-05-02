using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using System.Threading;

namespace Rendu1.Modules
{
    public class CommandeModule
    {
        private readonly DatabaseManager _db;
        private readonly MetroParisien _metroParisien;
        private readonly ClientModule _clientModule;

        public CommandeModule(DatabaseManager db, MetroParisien metroParisien, ClientModule clientModule)
        {
            _db = db;
            _metroParisien = metroParisien;
            _clientModule = clientModule;
        }

        public void AfficherMenuCommande()
        {
            bool continuer = true;
            while (continuer)
            {
                Console.Clear();
                Console.WriteLine("=== MENU COMMANDES ===");
                Console.WriteLine("1. Créer une nouvelle commande");
                Console.WriteLine("2. Modifier le statut d'une commande");
                Console.WriteLine("3. Afficher les détails d'une commande");
                Console.WriteLine("4. Calculer le prix d'une commande");
                Console.WriteLine("5. Afficher l'itinéraire de livraison");
                Console.WriteLine("6. Liste des commandes en cours");
                Console.WriteLine("7. Historique des commandes");
                Console.WriteLine("0. Retour au menu principal");

                Console.Write("\nVotre choix : ");
                string? choix = Console.ReadLine();

                switch (choix)
                {
                    case "1":
                        CreerNouvelleCommande();
                        break;
                    case "2":
                        ModifierStatutCommande();
                        break;
                    case "3":
                        AfficherDetailsCommande();
                        break;
                    case "4":
                        CalculerPrixCommande();
                        break;
                    case "5":
                        AfficherItineraireLivraison();
                        break;
                    case "6":
                        AfficherCommandesEnCours();
                        break;
                    case "7":
                        AfficherHistoriqueCommandes();
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

        private void CreerNouvelleCommande()
        {
            Console.Clear();
            Console.WriteLine("=== CRÉATION D'UNE NOUVELLE COMMANDE ===\n");

            // Vérification/Création du client
            Console.Write("Email du client : ");
            string emailClient = Console.ReadLine() ?? "";
            int clientId = VerifierOuCreerClient(emailClient);
            if (clientId == -1) return;

            // Sélection du cuisinier et de son plat
            Console.WriteLine("\n=== Sélection du cuisinier et du plat ===");
            var (cuisinierId, platId, prixUnitaire) = SelectionnerCuisinierEtPlat();
            if (cuisinierId == -1 || platId == -1) return;

            // Quantité souhaitée
            Console.Write("\nQuantité souhaitée : ");
            if (!int.TryParse(Console.ReadLine(), out int quantite) || quantite <= 0)
            {
                Console.WriteLine("Quantité invalide.");
                Console.ReadKey();
                return;
            }

            // Date souhaitée
            Console.Write("\nDate souhaitée (YYYY-MM-DD HH:mm) : ");
            if (!DateTime.TryParse(Console.ReadLine(), out DateTime dateSouhaitee))
            {
                Console.WriteLine("Date invalide.");
                Console.ReadKey();
                return;
            }

            // Calcul de l'itinéraire
            Console.WriteLine("\n=== Calcul de l'itinéraire ===");
            string stationCuisinier = ObtenirStationUtilisateur(cuisinierId);
            string stationClient = ObtenirStationUtilisateur(clientId);

            Console.WriteLine($"\nItinéraire de livraison :");
            Console.WriteLine($"De : {stationCuisinier}");
            Console.WriteLine($"À : {stationClient}");

            _metroParisien.TrouverPlusCourtChemin(stationCuisinier, stationClient);

            // Création de la commande
            try
            {
                string commandeId = $"CMD{DateTime.Now:yyyyMMddHHmmss}";
                decimal prixTotal = prixUnitaire * quantite;

                string sqlCommande = @"INSERT INTO BonDeCommande_Liv 
                    (CommandeID, ClientID, CuisinierID, PrixPaye, DateSouhaitee, AdresseBon, Statut)
                    VALUES 
                    (@commandeId, @clientId, @cuisinierId, @prixTotal, @dateSouhaitee, @adresse, 'En attente')";

                using (var cmd = new MySqlCommand(sqlCommande, _db.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@commandeId", commandeId);
                    cmd.Parameters.AddWithValue("@clientId", clientId);
                    cmd.Parameters.AddWithValue("@cuisinierId", cuisinierId);
                    cmd.Parameters.AddWithValue("@prixTotal", prixTotal);
                    cmd.Parameters.AddWithValue("@dateSouhaitee", dateSouhaitee);
                    cmd.Parameters.AddWithValue("@adresse", stationClient); // Utilisation de la station comme adresse
                    cmd.ExecuteNonQuery();
                }

                // Ajout des plats commandés
                string sqlCorrespond = @"INSERT INTO Correspond 
                    (PlatID, CommandeID, Quantite, PrixUnitaire)
                    VALUES 
                    (@platId, @commandeId, @quantite, @prixUnitaire)";

                using (var cmd = new MySqlCommand(sqlCorrespond, _db.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@platId", platId);
                    cmd.Parameters.AddWithValue("@commandeId", commandeId);
                    cmd.Parameters.AddWithValue("@quantite", quantite);
                    cmd.Parameters.AddWithValue("@prixUnitaire", prixUnitaire);
                    cmd.ExecuteNonQuery();
                }

                Console.WriteLine($"\n✅ Commande {commandeId} créée avec succès !");
                
                // Simulation interactive du processus de commande
                SimulerProcessusCommande(commandeId, stationCuisinier, stationClient);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Erreur lors de la création de la commande : {ex.Message}");
            }

            Console.WriteLine("\nAppuyez sur une touche pour continuer...");
            Console.ReadKey();
        }

        private void SimulerProcessusCommande(string commandeId, string stationCuisinier, string stationClient)
        {
            Console.Clear();
            Console.WriteLine($"=== SUIVI DE LA COMMANDE {commandeId} ===\n");

            // Étape 1 : En attente
            Console.WriteLine("1. Commande en attente de confirmation par le cuisinier");
            Console.Write("Appuyez sur Entrée pour continuer...");
            Console.ReadLine();

            // Étape 2 : Acceptée
            ModifierStatutCommandeSansPrompt(commandeId, "Acceptée");
            Console.WriteLine("\n2. Commande acceptée par le cuisinier");
            Console.Write("Appuyez sur Entrée pour continuer...");
            Console.ReadLine();

            // Étape 3 : En préparation
            ModifierStatutCommandeSansPrompt(commandeId, "En préparation");
            Console.WriteLine("\n3. Préparation des plats en cours");
            Console.Write("Appuyez sur Entrée pour continuer...");
            Console.ReadLine();

            // Étape 4 : En livraison
            Console.Clear();
            Console.WriteLine($"=== SIMULATION DE LIVRAISON - Commande {commandeId} ===\n");
            Console.WriteLine("🚴 Livraison en cours...\n");
            ModifierStatutCommandeSansPrompt(commandeId, "En livraison");

            Console.WriteLine($"Point de départ : {stationCuisinier}");
            Console.WriteLine($"Destination : {stationClient}\n");
            Console.WriteLine("Calcul de l'itinéraire le plus rapide...");
            Thread.Sleep(2000);

            Console.WriteLine("\nItinéraire :");
            Console.WriteLine("=============");
            
            // Afficher l'itinéraire le plus rapide
            _metroParisien.TrouverPlusCourtChemin(stationCuisinier, stationClient);
            Thread.Sleep(2000);

            Thread.Sleep(2000);
            Console.WriteLine("\n✅ Arrivé à destination !");
            ModifierStatutCommandeSansPrompt(commandeId, "Livrée");
        }

        private void ModifierStatutCommandeSansPrompt(string commandeId, string nouveauStatut)
        {
            try
            {
                string sql = "UPDATE BonDeCommande_Liv SET Statut = @statut WHERE CommandeID = @commandeId";
                using (var cmd = new MySqlCommand(sql, _db.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@statut", nouveauStatut);
                    cmd.Parameters.AddWithValue("@commandeId", commandeId);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Erreur lors de la modification du statut : {ex.Message}");
            }
        }

        private void ModifierStatutCommande()
        {
            Console.Clear();
            Console.WriteLine("=== MODIFICATION DU STATUT D'UNE COMMANDE ===\n");

            Console.Write("Numéro de commande : ");
            string commandeId = Console.ReadLine() ?? "";

            Console.WriteLine("\nStatuts disponibles :");
            Console.WriteLine("1. En attente");
            Console.WriteLine("2. Acceptée");
            Console.WriteLine("3. En préparation");
            Console.WriteLine("4. En livraison");
            Console.WriteLine("5. Livrée");
            Console.WriteLine("6. Annulée");

            Console.Write("\nNouveau statut (1-6) : ");
            string? choix = Console.ReadLine();

            string? nouveauStatut = choix switch
            {
                "1" => "En attente",
                "2" => "Acceptée",
                "3" => "En préparation",
                "4" => "En livraison",
                "5" => "Livrée",
                "6" => "Annulée",
                _ => null
            };

            if (nouveauStatut == null)
            {
                Console.WriteLine("Statut invalide.");
                Console.ReadKey();
                return;
            }

            try
            {
                string sql = "UPDATE BonDeCommande_Liv SET Statut = @statut WHERE CommandeID = @commandeId";
                using (var cmd = new MySqlCommand(sql, _db.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@statut", nouveauStatut);
                    cmd.Parameters.AddWithValue("@commandeId", commandeId);
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                        Console.WriteLine($"\n✅ Statut de la commande mis à jour : {nouveauStatut}");
                    else
                        Console.WriteLine("\n❌ Commande non trouvée.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Erreur lors de la modification : {ex.Message}");
            }

            Console.WriteLine("\nAppuyez sur une touche pour continuer...");
            Console.ReadKey();
        }

        private void AfficherDetailsCommande()
        {
            Console.Clear();
            Console.WriteLine("=== DÉTAILS D'UNE COMMANDE ===\n");

            Console.Write("Numéro de commande : ");
            string commandeId = Console.ReadLine() ?? "";

            string sql = @"SELECT 
                b.CommandeID,
                b.DateCommande,
                b.DateSouhaitee,
                b.PrixPaye,
                b.Statut,
                b.AdresseBon,
                CONCAT(u1.NomU, ' ', u1.PrenomU) as NomClient,
                u1.EmailU as EmailClient,
                CONCAT(u2.NomU, ' ', u2.PrenomU) as NomCuisinier,
                u2.EmailU as EmailCuisinier,
                GROUP_CONCAT(CONCAT(p.NomPlat, ' (', co.Quantite, 'x)') SEPARATOR ', ') as Plats
                FROM BonDeCommande_Liv b
                JOIN Utilisateur u1 ON b.ClientID = u1.ClientID
                JOIN Utilisateur u2 ON b.CuisinierID = u2.ClientID
                JOIN Correspond co ON b.CommandeID = co.CommandeID
                JOIN Plat p ON co.PlatID = p.PlatID
                WHERE b.CommandeID = @commandeId
                GROUP BY b.CommandeID";

            try
            {
                using (var cmd = new MySqlCommand(sql, _db.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@commandeId", commandeId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Console.WriteLine($"\nCommande n° {reader["CommandeID"]}");
                            Console.WriteLine(new string('-', 50));
                            Console.WriteLine($"Date de commande : {Convert.ToDateTime(reader["DateCommande"]):dd/MM/yyyy HH:mm}");
                            Console.WriteLine($"Date souhaitée : {Convert.ToDateTime(reader["DateSouhaitee"]):dd/MM/yyyy HH:mm}");
                            Console.WriteLine($"Statut : {reader["Statut"]}");
                            Console.WriteLine($"Prix total : {reader["PrixPaye"]}€");
                            Console.WriteLine($"Adresse de livraison : {reader["AdresseBon"]}");
                            Console.WriteLine("\nClient :");
                            Console.WriteLine($"- {reader["NomClient"]} ({reader["EmailClient"]})");
                            Console.WriteLine("\nCuisinier :");
                            Console.WriteLine($"- {reader["NomCuisinier"]} ({reader["EmailCuisinier"]})");
                            Console.WriteLine("\nPlats commandés :");
                            Console.WriteLine($"- {reader["Plats"]}");
                        }
                        else
                        {
                            Console.WriteLine("❌ Commande non trouvée.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Erreur lors de la récupération des détails : {ex.Message}");
            }

            Console.WriteLine("\nAppuyez sur une touche pour continuer...");
            Console.ReadKey();
        }

        private void CalculerPrixCommande()
        {
            Console.Clear();
            Console.WriteLine("=== CALCUL DU PRIX D'UNE COMMANDE ===\n");

            Console.Write("Numéro de commande : ");
            string commandeId = Console.ReadLine() ?? "";

            string sql = @"SELECT 
                co.Quantite,
                co.PrixUnitaire,
                p.NomPlat,
                (co.Quantite * co.PrixUnitaire) as SousTotal
                FROM Correspond co
                JOIN Plat p ON co.PlatID = p.PlatID
                WHERE co.CommandeID = @commandeId";

            try
            {
                decimal total = 0;
                using (var cmd = new MySqlCommand(sql, _db.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@commandeId", commandeId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                        {
                            Console.WriteLine("❌ Commande non trouvée.");
                            Console.ReadKey();
                            return;
                        }

                        Console.WriteLine("\nDétail des prix :");
                        Console.WriteLine(new string('-', 50));
                        while (reader.Read())
                        {
                            decimal sousTotal = Convert.ToDecimal(reader["SousTotal"]);
                            Console.WriteLine($"{reader["NomPlat"]} : {reader["Quantite"]}x {reader["PrixUnitaire"]}€ = {sousTotal}€");
                            total += sousTotal;
                        }
                    }
                }

                Console.WriteLine(new string('-', 50));
                Console.WriteLine($"Total de la commande : {total}€");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Erreur lors du calcul : {ex.Message}");
            }

            Console.WriteLine("\nAppuyez sur une touche pour continuer...");
            Console.ReadKey();
        }

        private void AfficherItineraireLivraison()
        {
            Console.Clear();
            Console.WriteLine("=== ITINÉRAIRE DE LIVRAISON ===\n");

            Console.Write("Numéro de commande : ");
            string commandeId = Console.ReadLine() ?? "";

            try
            {
                string sql = @"SELECT 
                    u1.StationPlusProcheU as StationCuisinier,
                    u2.StationPlusProcheU as StationClient
                    FROM BonDeCommande_Liv b
                    JOIN Utilisateur u1 ON b.CuisinierID = u1.ClientID
                    JOIN Utilisateur u2 ON b.ClientID = u2.ClientID
                    WHERE b.CommandeID = @commandeId";

                string? stationDepart = null;
                string? stationArrivee = null;

                using (var cmd = new MySqlCommand(sql, _db.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@commandeId", commandeId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            stationDepart = reader["StationCuisinier"].ToString();
                            stationArrivee = reader["StationClient"].ToString();
                        }
                    }
                }

                if (stationDepart == null || stationArrivee == null)
                {
                    Console.WriteLine("❌ Commande non trouvée.");
                    Console.ReadKey();
                    return;
                }

                Console.WriteLine($"\nItinéraire de livraison :");
                Console.WriteLine($"De : {stationDepart}");
                Console.WriteLine($"À : {stationArrivee}");
                Console.WriteLine(new string('-', 50));

                _metroParisien.TrouverPlusCourtChemin(stationDepart, stationArrivee);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Erreur lors de la récupération de l'itinéraire : {ex.Message}");
            }

            Console.WriteLine("\nAppuyez sur une touche pour continuer...");
            Console.ReadKey();
        }

        private void AfficherCommandesEnCours()
        {
            Console.Clear();
            Console.WriteLine("=== COMMANDES EN COURS ===\n");

            string sql = @"SELECT 
                b.CommandeID,
                b.DateCommande,
                b.DateSouhaitee,
                b.Statut,
                CONCAT(u1.NomU, ' ', u1.PrenomU) as Client,
                CONCAT(u2.NomU, ' ', u2.PrenomU) as Cuisinier,
                b.PrixPaye
                FROM BonDeCommande_Liv b
                JOIN Utilisateur u1 ON b.ClientID = u1.ClientID
                JOIN Utilisateur u2 ON b.CuisinierID = u2.ClientID
                WHERE b.Statut NOT IN ('Livrée', 'Annulée')
                ORDER BY b.DateSouhaitee ASC";

            try
            {
                using (var cmd = new MySqlCommand(sql, _db.GetConnection()))
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        Console.WriteLine("Aucune commande en cours.");
                        Console.ReadKey();
                        return;
                    }

                    // Afficher les en-têtes
                    Console.WriteLine($"{"N° Commande",-15} {"Date souhaitée",-20} {"Statut",-15} {"Client",-25} {"Cuisinier",-25} {"Prix",-10}");
                    Console.WriteLine(new string('-', 110));

                    while (reader.Read())
                    {
                        Console.WriteLine($"{reader["CommandeID"],-15} " +
                                        $"{Convert.ToDateTime(reader["DateSouhaitee"]):dd/MM/yyyy HH:mm}  " +
                                        $"{reader["Statut"],-15} " +
                                        $"{reader["Client"],-25} " +
                                        $"{reader["Cuisinier"],-25} " +
                                        $"{reader["PrixPaye"]}€");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Erreur lors de l'affichage des commandes : {ex.Message}");
            }

            Console.WriteLine("\nAppuyez sur une touche pour continuer...");
            Console.ReadKey();
        }

        private void AfficherHistoriqueCommandes()
        {
            Console.Clear();
            Console.WriteLine("=== HISTORIQUE DES COMMANDES ===\n");

            string sql = @"
                SELECT 
                    b.CommandeID,
                    b.DateCommande,
                    b.DateSouhaitee,
                    b.DateLivraison,
                    b.Statut,
                    CONCAT(u1.NomU, ' ', COALESCE(u1.PrenomU, ''), 
                        CASE WHEN u1.TypeClient = 'Entreprise' THEN CONCAT(' (', u1.NomEntreprise, ')') ELSE '' END) as Client,
                    CONCAT(u2.PrenomU, ' ', u2.NomU) as Cuisinier,
                    b.PrixPaye,
                    GROUP_CONCAT(CONCAT(p.NomPlat, ' (', c.Quantite, 'x)') SEPARATOR ', ') as Plats,
                    b.AdresseBon
                FROM BonDeCommande_Liv b
                JOIN Utilisateur u1 ON b.ClientID = u1.ClientID
                LEFT JOIN Cuisinier cui ON b.CuisinierID = cui.ClientID
                LEFT JOIN Utilisateur u2 ON cui.ClientID = u2.ClientID
                LEFT JOIN Correspond c ON b.CommandeID = c.CommandeID
                LEFT JOIN Plat p ON c.PlatID = p.PlatID
                GROUP BY b.CommandeID, b.DateCommande, b.DateSouhaitee, b.DateLivraison, 
                         b.Statut, Client, Cuisinier, b.PrixPaye, b.AdresseBon
                ORDER BY b.DateCommande DESC";

            try
            {
                using (var cmd = new MySqlCommand(sql, _db.GetConnection()))
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        Console.WriteLine("Aucune commande trouvée.");
                        Console.ReadKey();
                        return;
                    }

                    while (reader.Read())
                    {
                        Console.WriteLine(new string('=', 100));
                        Console.WriteLine($"Commande N° : {reader["CommandeID"]}");
                        Console.WriteLine($"Date de commande : {Convert.ToDateTime(reader["DateCommande"]):dd/MM/yyyy HH:mm}");
                        Console.WriteLine($"Date souhaitée : {Convert.ToDateTime(reader["DateSouhaitee"]):dd/MM/yyyy HH:mm}");
                        
                        if (reader["DateLivraison"] != DBNull.Value)
                            Console.WriteLine($"Date de livraison : {Convert.ToDateTime(reader["DateLivraison"]):dd/MM/yyyy HH:mm}");
                        
                        Console.WriteLine($"Statut : {reader["Statut"]}");
                        Console.WriteLine($"Client : {reader["Client"]}");
                        
                        if (reader["Cuisinier"] != DBNull.Value)
                            Console.WriteLine($"Cuisinier : {reader["Cuisinier"]}");
                        else
                            Console.WriteLine("Cuisinier : Non assigné");

                        Console.WriteLine($"Prix total : {reader["PrixPaye"]:C2}");
                        
                        if (reader["Plats"] != DBNull.Value)
                            Console.WriteLine($"Plats commandés : {reader["Plats"]}");
                        else
                            Console.WriteLine("Plats commandés : Information non disponible");

                        Console.WriteLine($"Adresse de livraison : {reader["AdresseBon"]}");
                        Console.WriteLine();
                    }
                }

                Console.WriteLine("\nAppuyez sur une touche pour continuer...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Erreur lors de la récupération de l'historique : {ex.Message}");
                Console.ReadKey();
            }
        }

        private int VerifierOuCreerClient(string email)
        {
            string sqlCheck = "SELECT ClientID FROM Utilisateur WHERE EmailU = @email";
            using (var cmd = new MySqlCommand(sqlCheck, _db.GetConnection()))
            {
                cmd.Parameters.AddWithValue("@email", email);
                var result = cmd.ExecuteScalar();

                if (result != null)
                    return Convert.ToInt32(result);

                Console.WriteLine("\nClient non trouvé. Création d'un nouveau client...");
                return _clientModule.CreerNouveauClient();
            }
        }

        private (int cuisinierId, int platId, decimal prixUnitaire) SelectionnerCuisinierEtPlat()
        {
            string sql = @"SELECT 
                u.ClientID,
                CONCAT(u.NomU, ' ', u.PrenomU) as NomComplet,
                c.SpecialiteC,
                p.PlatID,
                p.NomPlat,
                p.PrixParPersonne
                FROM Utilisateur u
                JOIN Cuisinier c ON u.ClientID = c.ClientID
                JOIN Plat p ON c.ClientID = p.CuisinierID
                WHERE u.Actif = TRUE AND p.EstDisponible = TRUE
                ORDER BY u.NomU, u.PrenomU";

            try
            {
                using (var cmd = new MySqlCommand(sql, _db.GetConnection()))
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        Console.WriteLine("Aucun cuisinier ou plat disponible.");
                        return (-1, -1, 0);
                    }

                    var cuisiniers = new Dictionary<int, string>();
                    var plats = new Dictionary<int, (string nom, decimal prix)>();
                    int currentCuisinier = -1;

                    while (reader.Read())
                    {
                        int cuisinierId = Convert.ToInt32(reader["ClientID"]);
                        if (!cuisiniers.ContainsKey(cuisinierId))
                        {
                            cuisiniers[cuisinierId] = $"{reader["NomComplet"]} - {reader["SpecialiteC"]}";
                        }
                        plats[Convert.ToInt32(reader["PlatID"])] = (
                            reader["NomPlat"].ToString() ?? "",
                            Convert.ToDecimal(reader["PrixParPersonne"])
                        );
                    }

                    // Afficher les cuisiniers
                    Console.WriteLine("\nCuisiniers disponibles :");
                    foreach (var cuisinier in cuisiniers)
                    {
                        Console.WriteLine($"{cuisinier.Key}. {cuisinier.Value}");
                    }

                    Console.Write("\nChoisissez un cuisinier (ID) : ");
                    if (!int.TryParse(Console.ReadLine(), out int cuisinierChoisi) || !cuisiniers.ContainsKey(cuisinierChoisi))
                    {
                        Console.WriteLine("Choix invalide.");
                        return (-1, -1, 0);
                    }

                    // Afficher les plats du cuisinier
                    Console.WriteLine("\nPlats disponibles :");
                    foreach (var plat in plats)
                    {
                        Console.WriteLine($"{plat.Key}. {plat.Value.nom} - {plat.Value.prix}€");
                    }

                    Console.Write("\nChoisissez un plat (ID) : ");
                    if (!int.TryParse(Console.ReadLine(), out int platChoisi) || !plats.ContainsKey(platChoisi))
                    {
                        Console.WriteLine("Choix invalide.");
                        return (-1, -1, 0);
                    }

                    return (cuisinierChoisi, platChoisi, plats[platChoisi].prix);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur : {ex.Message}");
                return (-1, -1, 0);
            }
        }

        private string ObtenirStationUtilisateur(int clientId)
        {
            string sql = "SELECT StationPlusProcheU FROM Utilisateur WHERE ClientID = @clientId";
            using (var cmd = new MySqlCommand(sql, _db.GetConnection()))
            {
                cmd.Parameters.AddWithValue("@clientId", clientId);
                return cmd.ExecuteScalar()?.ToString() ?? "";
            }
        }

        private void SimulerLivraisonInteractive(string commandeId, string stationClient)
        {
            Console.Clear();
            Console.WriteLine($"=== SIMULATION DE LIVRAISON - Commande {commandeId} ===\n");

            // Récupérer la station du cuisinier
            string sql = @"
                SELECT 
                    u.StationPlusProcheU,
                    CONCAT(u.PrenomU, ' ', u.NomU) as NomCuisinier
                FROM BonDeCommande_Liv b
                JOIN Plat p ON p.PlatID = (SELECT PlatID FROM Correspond WHERE CommandeID = b.CommandeID LIMIT 1)
                JOIN Cuisinier c ON p.CuisinierID = c.ClientID
                JOIN Utilisateur u ON c.ClientID = u.ClientID
                WHERE b.CommandeID = @commandeId";

            string stationCuisinier;
            string nomCuisinier;
            using (var cmd = new MySqlCommand(sql, _db.GetConnection()))
            {
                cmd.Parameters.AddWithValue("@commandeId", commandeId);
                using var reader = cmd.ExecuteReader();
                reader.Read();
                stationCuisinier = reader["StationPlusProcheU"].ToString() ?? "";
                nomCuisinier = reader["NomCuisinier"].ToString() ?? "";
            }

            // Étape 1 : Commande reçue
            Console.WriteLine("📝 Commande reçue et en attente de confirmation...");
            ModifierStatutCommandeSansPrompt(commandeId, "En attente");
            Thread.Sleep(2000);
            Console.Write("\nAppuyez sur Entrée pour continuer...");
            Console.ReadLine();

            // Étape 2 : Acceptation par le cuisinier
            Console.Clear();
            Console.WriteLine($"=== SIMULATION DE LIVRAISON - Commande {commandeId} ===\n");
            Console.WriteLine($"👨‍🍳 Le cuisinier {nomCuisinier} a accepté votre commande !");
            ModifierStatutCommandeSansPrompt(commandeId, "Acceptée");
            Thread.Sleep(2000);
            Console.Write("\nAppuyez sur Entrée pour continuer...");
            Console.ReadLine();

            // Étape 3 : Préparation
            Console.Clear();
            Console.WriteLine($"=== SIMULATION DE LIVRAISON - Commande {commandeId} ===\n");
            Console.WriteLine("👨‍🍳 Préparation de votre commande en cours...");
            ModifierStatutCommandeSansPrompt(commandeId, "En préparation");
            
            // Animation de préparation
            for (int i = 0; i < 3; i++)
            {
                Thread.Sleep(1000);
                Console.Write("🔪 ");
                Thread.Sleep(1000);
                Console.Write("🥘 ");
                Thread.Sleep(1000);
                Console.Write("📦 ");
            }
            Console.WriteLine("\n✅ Préparation terminée !");
            Thread.Sleep(1000);
            Console.Write("\nAppuyez sur Entrée pour commencer la livraison...");
            Console.ReadLine();

            // Étape 4 : Livraison
            Console.Clear();
            Console.WriteLine($"=== SIMULATION DE LIVRAISON - Commande {commandeId} ===\n");
            Console.WriteLine("🚴 Livraison en cours...\n");
            ModifierStatutCommandeSansPrompt(commandeId, "En livraison");

            Console.WriteLine($"Point de départ : {stationCuisinier}");
            Console.WriteLine($"Destination : {stationClient}\n");
            Console.WriteLine("Calcul de l'itinéraire le plus rapide...");
            Thread.Sleep(2000);

            Console.WriteLine("\nItinéraire :");
            Console.WriteLine("=============");
            
            // Afficher l'itinéraire le plus rapide
            _metroParisien.TrouverPlusCourtChemin(stationCuisinier, stationClient);
            Thread.Sleep(2000);

            Thread.Sleep(2000);
            Console.WriteLine("\n✅ Arrivé à destination !");
            ModifierStatutCommandeSansPrompt(commandeId, "Livrée");

            // Mise à jour de la date de livraison
            string sqlUpdateLivraison = @"
                UPDATE BonDeCommande_Liv 
                SET DateLivraison = NOW()
                WHERE CommandeID = @commandeId";

            using (var cmd = new MySqlCommand(sqlUpdateLivraison, _db.GetConnection()))
            {
                cmd.Parameters.AddWithValue("@commandeId", commandeId);
                cmd.ExecuteNonQuery();
            }

            Console.WriteLine("\n🎉 Votre commande a été livrée avec succès !");
            Console.WriteLine("\nAppuyez sur une touche pour continuer...");
            Console.ReadKey();
        }

        private List<(int id, string nom, string description, decimal prix, string cuisinier, int cuisinierId)> AfficherPlatsDisponibles(int? clientId = null, string? recherche = null)
        {
            var plats = new List<(int id, string nom, string description, decimal prix, string cuisinier, int cuisinierId)>();

            string sql = @"
                SELECT DISTINCT
                    p.PlatID,
                    p.NomPlat,
                    p.Description,
                    p.PrixParPersonne,
                    CONCAT(u.PrenomU, ' ', u.NomU) as NomCuisinier,
                    p.DatePeremption,
                    c.ClientID as CuisinierID,
                    p.PlatDuJour,
                    p.NationaliteCuisine,
                    GROUP_CONCAT(DISTINCT 
                        CASE 
                            WHEN i.ContientAllergenes = 1 
                            THEN CONCAT(i.NomIngredient, ' (', i.TypeAllergene, ')')
                        END
                        SEPARATOR ', '
                    ) as Allergenes
                FROM Plat p
                JOIN Cuisinier c ON p.CuisinierID = c.ClientID
                JOIN Utilisateur u ON c.ClientID = u.ClientID
                LEFT JOIN PlatRecette pr ON p.PlatID = pr.PlatID
                LEFT JOIN Ingredients i ON pr.IngredientID = i.IngredientID
                WHERE p.EstDisponible = TRUE
                AND (@clientId IS NULL OR p.CuisinierID != @clientId)
                AND (@recherche IS NULL 
                    OR p.NomPlat LIKE @rechercheLike
                    OR p.Description LIKE @rechercheLike
                    OR p.NationaliteCuisine LIKE @rechercheLike)
                GROUP BY p.PlatID
                ORDER BY p.PlatDuJour DESC, p.DateCreation DESC";

            try
            {
                using var cmd = new MySqlCommand(sql, _db.GetConnection());
                cmd.Parameters.AddWithValue("@clientId", clientId ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@recherche", recherche ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@rechercheLike", $"%{recherche}%");
                using var reader = cmd.ExecuteReader();

                int numero = 1;
                while (reader.Read())
                {
                    plats.Add((
                        reader.GetInt32("PlatID"),
                        reader.GetString("NomPlat"),
                        reader.GetString("Description"),
                        reader.GetDecimal("PrixParPersonne"),
                        reader.GetString("NomCuisinier"),
                        reader.GetInt32("CuisinierID")
                    ));

                    string platDuJourMention = reader.GetBoolean("PlatDuJour") ? "⭐ PLAT DU JOUR ⭐\n" : "";
                    Console.WriteLine($"\n{platDuJourMention}{numero}. {reader["NomPlat"]}");
                    Console.WriteLine($"   Description : {reader["Description"]}");
                    Console.WriteLine($"   Cuisine : {reader["NationaliteCuisine"]}");
                    Console.WriteLine($"   Prix : {reader.GetDecimal("PrixParPersonne"):C2}");
                    Console.WriteLine($"   Cuisinier : {reader["NomCuisinier"]}");
                    Console.WriteLine($"   Date de péremption : {((DateTime)reader["DatePeremption"]).ToString("dd/MM/yyyy")}");
                    
                    if (!reader.IsDBNull(reader.GetOrdinal("Allergenes")) && !string.IsNullOrEmpty(reader["Allergenes"].ToString()))
                    {
                        Console.WriteLine($"   ⚠️ Allergènes : {reader["Allergenes"]}");
                    }
                    
                    numero++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Erreur : {ex.Message}");
            }

            return plats;
        }

        private string ObtenirStationClient(int clientId)
        {
            string sql = "SELECT StationPlusProcheU FROM Utilisateur WHERE ClientID = @clientId";
            using var cmd = new MySqlCommand(sql, _db.GetConnection());
            cmd.Parameters.AddWithValue("@clientId", clientId);
            return cmd.ExecuteScalar()?.ToString() ?? "";
        }

        public void PasserCommandeClient(AuthModule.UserSession session)
        {
            Console.Clear();
            Console.WriteLine("=== PASSER UNE NOUVELLE COMMANDE ===\n");

            try
            {
                string? recherche = null;
                Console.WriteLine("Rechercher un plat (appuyez sur Entrée pour voir tous les plats) :");
                Console.Write("Mot-clé (nom, description ou type de cuisine) : ");
                string? input = Console.ReadLine()?.Trim();
                if (!string.IsNullOrEmpty(input))
                {
                    recherche = input;
                }

                // Afficher les plats disponibles avec la recherche
                var platsDisponibles = AfficherPlatsDisponibles(session.UserId, recherche);
                if (platsDisponibles.Count == 0)
                {
                    if (recherche != null)
                        Console.WriteLine($"\n❌ Aucun plat ne correspond à votre recherche : '{recherche}'");
                    else
                        Console.WriteLine("\n❌ Aucun plat n'est disponible pour le moment.");
                    Console.ReadKey();
                    return;
                }

                // 2. Sélectionner les plats et quantités
                var platSelectionnes = new Dictionary<int, (int platId, int quantite, decimal prix)>();
                decimal totalCommande = 0;
                int cuisinierId = -1; // Pour stocker l'ID du cuisinier

                while (true)
                {
                    Console.Write("\nEntrez le numéro du plat (0 pour terminer) : ");
                    if (!int.TryParse(Console.ReadLine(), out int choixPlat) || choixPlat < 0 || choixPlat > platsDisponibles.Count)
                    {
                        Console.WriteLine("❌ Choix invalide.");
                        continue;
                    }

                    if (choixPlat == 0)
                        break;

                    var platChoisi = platsDisponibles[choixPlat - 1];
                    
                    // Si c'est le premier plat, on enregistre le cuisinier
                    if (cuisinierId == -1)
                    {
                        cuisinierId = platChoisi.cuisinierId;
                    }
                    // Sinon, on vérifie que c'est le même cuisinier
                    else if (cuisinierId != platChoisi.cuisinierId)
                    {
                        Console.WriteLine("❌ Tous les plats doivent être du même cuisinier.");
                        continue;
                    }

                    Console.Write("Quantité désirée : ");
                    if (!int.TryParse(Console.ReadLine(), out int quantite) || quantite <= 0)
                    {
                        Console.WriteLine("❌ Quantité invalide.");
                        continue;
                    }

                    decimal prixTotal = platChoisi.prix * quantite;
                    totalCommande += prixTotal;

                    platSelectionnes[choixPlat] = (platChoisi.id, quantite, platChoisi.prix);
                    Console.WriteLine($"✅ Ajouté : {platChoisi.nom} x{quantite} = {prixTotal:C2}");
                }

                if (platSelectionnes.Count == 0)
                {
                    Console.WriteLine("\n❌ Aucun plat sélectionné.");
                    Console.ReadKey();
                    return;
                }

                // 3. Choisir la date de livraison
                Console.Write("\nDate de livraison souhaitée (format: dd/MM/yyyy HH:mm) : ");
                if (!DateTime.TryParseExact(Console.ReadLine(), "dd/MM/yyyy HH:mm", null, System.Globalization.DateTimeStyles.None, out DateTime dateLivraison))
                {
                    Console.WriteLine("❌ Format de date invalide.");
                    Console.ReadKey();
                    return;
                }

                // 4. Récupérer la station du client
                string stationClient = ObtenirStationClient(session.UserId);

                // 5. Créer la commande
                string commandeId = $"CMD{DateTime.Now:yyyyMMddHHmmss}";
                string sqlCommande = @"
                    INSERT INTO BonDeCommande_Liv 
                    (CommandeID, ClientID, CuisinierID, PrixPaye, DateSouhaitee, AdresseBon, Statut, ModePaiement)
                    VALUES 
                    (@commandeId, @clientId, @cuisinierId, @prixTotal, @dateLivraison, @adresse, 'En attente', 'Carte Bancaire')";

                using (var cmd = new MySqlCommand(sqlCommande, _db.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@commandeId", commandeId);
                    cmd.Parameters.AddWithValue("@clientId", session.UserId);
                    cmd.Parameters.AddWithValue("@cuisinierId", cuisinierId);
                    cmd.Parameters.AddWithValue("@prixTotal", totalCommande);
                    cmd.Parameters.AddWithValue("@dateLivraison", dateLivraison);
                    cmd.Parameters.AddWithValue("@adresse", stationClient);
                    cmd.ExecuteNonQuery();
                }

                // 6. Ajouter les plats à la commande
                foreach (var plat in platSelectionnes.Values)
                {
                    string sqlCorrespond = @"
                        INSERT INTO Correspond (PlatID, CommandeID, Quantite, PrixUnitaire)
                        VALUES (@platId, @commandeId, @quantite, @prixUnitaire)";

                    using (var cmd = new MySqlCommand(sqlCorrespond, _db.GetConnection()))
                    {
                        cmd.Parameters.AddWithValue("@platId", plat.platId);
                        cmd.Parameters.AddWithValue("@commandeId", commandeId);
                        cmd.Parameters.AddWithValue("@quantite", plat.quantite);
                        cmd.Parameters.AddWithValue("@prixUnitaire", plat.prix);
                        cmd.ExecuteNonQuery();
                    }
                }

                Console.WriteLine($"\n✅ Commande {commandeId} créée avec succès ! Total : {totalCommande:C2}");
                Thread.Sleep(2000);

                // 7. Lancer la simulation de livraison
                SimulerLivraisonInteractive(commandeId, stationClient);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Erreur : {ex.Message}");
                Console.ReadKey();
            }
        }

        public void VoirCommandesClient(AuthModule.UserSession session)
        {
            Console.Clear();
            Console.WriteLine("=== MES COMMANDES ===\n");

            string sql = @"
                SELECT 
                    c.CommandeID,
                    c.DateCommande,
                    c.DateSouhaitee,
                    c.DateLivraison,
                    c.PrixPaye,
                    c.Statut,
                    c.AdresseBon,
                    GROUP_CONCAT(
                        CONCAT(p.NomPlat, ' x', co.Quantite)
                        SEPARATOR ', '
                    ) as Plats,
                    cu.NomU as NomCuisinier,
                    cu.PrenomU as PrenomCuisinier
                FROM BonDeCommande_Liv c
                LEFT JOIN Correspond co ON c.CommandeID = co.CommandeID
                LEFT JOIN Plat p ON co.PlatID = p.PlatID
                LEFT JOIN Cuisinier cui ON c.CuisinierID = cui.ClientID
                LEFT JOIN Utilisateur cu ON cui.ClientID = cu.ClientID
                WHERE c.ClientID = @clientId
                GROUP BY c.CommandeID
                ORDER BY c.DateCommande DESC";

            try
            {
                using var cmd = new MySqlCommand(sql, _db.GetConnection());
                cmd.Parameters.AddWithValue("@clientId", session.UserId);
                using var reader = cmd.ExecuteReader();

                if (!reader.HasRows)
                {
                    Console.WriteLine("Vous n'avez pas encore passé de commande.");
                }
                else
                {
                    while (reader.Read())
                    {
                        Console.WriteLine($"\n=== Commande {reader["CommandeID"]} ===");
                        Console.WriteLine($"Date de commande : {((DateTime)reader["DateCommande"]).ToString("dd/MM/yyyy HH:mm")}");
                        Console.WriteLine($"Date souhaitée : {((DateTime)reader["DateSouhaitee"]).ToString("dd/MM/yyyy HH:mm")}");
                        if (reader["DateLivraison"] != DBNull.Value)
                            Console.WriteLine($"Date de livraison : {((DateTime)reader["DateLivraison"]).ToString("dd/MM/yyyy HH:mm")}");
                        Console.WriteLine($"Statut : {reader["Statut"]}");
                        Console.WriteLine($"Prix total : {((decimal)reader["PrixPaye"]):C2}");
                        Console.WriteLine($"Adresse de livraison : {reader["AdresseBon"]}");
                        Console.WriteLine($"Plats commandés : {reader["Plats"]}");
                        
                        if (reader["NomCuisinier"] != DBNull.Value)
                            Console.WriteLine($"Cuisinier : {reader["PrenomCuisinier"]} {reader["NomCuisinier"]}");
                        
                        Console.WriteLine(new string('-', 50));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Erreur : {ex.Message}");
            }

            Console.WriteLine("\nAppuyez sur une touche pour continuer...");
            Console.ReadKey();
        }

        public void VoirHistoriqueLivraisonsCuisinier(AuthModule.UserSession session)
        {
            Console.Clear();
            Console.WriteLine("=== MON HISTORIQUE DE LIVRAISONS ===\n");

            string sql = @"
                SELECT 
                    b.CommandeID,
                    b.DateCommande,
                    b.DateSouhaitee,
                    b.DateLivraison,
                    b.PrixPaye,
                    b.Statut,
                    CONCAT(u.PrenomU, ' ', u.NomU) as NomClient,
                    u.StationPlusProcheU as StationClient,
                    GROUP_CONCAT(
                        CONCAT(
                            COALESCE(p.NomPlat, 'Plat non disponible'),
                            ' x',
                            c.Quantite
                        ) SEPARATOR ', '
                    ) as Plats
                FROM BonDeCommande_Liv b
                LEFT JOIN Utilisateur u ON b.ClientID = u.ClientID
                LEFT JOIN Correspond c ON b.CommandeID = c.CommandeID
                LEFT JOIN Plat p ON c.PlatID = p.PlatID
                WHERE b.CuisinierID = @cuisinierId 
                AND b.Statut IN ('Livrée', 'Annulée')
                GROUP BY b.CommandeID, b.DateCommande, b.DateSouhaitee, b.DateLivraison,
                         b.PrixPaye, b.Statut, NomClient, StationClient
                ORDER BY b.DateLivraison DESC, b.DateCommande DESC";

            try
            {
                using var cmd = new MySqlCommand(sql, _db.GetConnection());
                cmd.Parameters.AddWithValue("@cuisinierId", session.UserId);
                using var reader = cmd.ExecuteReader();

                if (!reader.HasRows)
                {
                    Console.WriteLine("Aucune livraison dans l'historique.");
                }
                else
                {
                    while (reader.Read())
                    {
                        Console.WriteLine($"\n=== Commande {reader["CommandeID"]} ===");
                        Console.WriteLine($"Date de commande : {((DateTime)reader["DateCommande"]):dd/MM/yyyy HH:mm}");
                        Console.WriteLine($"Date souhaitée : {((DateTime)reader["DateSouhaitee"]):dd/MM/yyyy HH:mm}");
                        if (reader["DateLivraison"] != DBNull.Value)
                            Console.WriteLine($"Date de livraison : {((DateTime)reader["DateLivraison"]):dd/MM/yyyy HH:mm}");
                        Console.WriteLine($"Statut : {reader["Statut"]}");
                        
                        if (reader["NomClient"] != DBNull.Value)
                            Console.WriteLine($"Client : {reader["NomClient"]}");
                        else
                            Console.WriteLine("Client : Information non disponible");

                        if (reader["StationClient"] != DBNull.Value)
                            Console.WriteLine($"Station de livraison : {reader["StationClient"]}");
                        else
                            Console.WriteLine("Station de livraison : Non spécifiée");

                        Console.WriteLine($"Prix total : {((decimal)reader["PrixPaye"]):C2}");
                        
                        if (reader["Plats"] != DBNull.Value)
                            Console.WriteLine($"Plats : {reader["Plats"]}");
                        else
                            Console.WriteLine("Plats : Information non disponible");

                        Console.WriteLine(new string('-', 50));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Erreur : {ex.Message}");
            }

            Console.WriteLine("\nAppuyez sur une touche pour continuer...");
            Console.ReadKey();
        }

        public void GererPlats(AuthModule.UserSession session)
        {
            bool continuer = true;
            while (continuer)
            {
                Console.Clear();
                Console.WriteLine("=== GESTION DES PLATS ===\n");
                Console.WriteLine("1. Voir mes plats");
                Console.WriteLine("2. Ajouter un plat");
                Console.WriteLine("3. Modifier un plat");
                Console.WriteLine("4. Supprimer un plat");
                Console.WriteLine("5. Définir le plat du jour");
                Console.WriteLine("0. Retour");

                Console.Write("\nVotre choix : ");
                string? choix = Console.ReadLine();

                switch (choix)
                {
                    case "1":
                        AfficherPlatsParCuisinier(session.UserId);
                        break;
                    case "2":
                        AjouterPlat(session.UserId);
                        break;
                    case "3":
                        ModifierPlat(session.UserId);
                        break;
                    case "4":
                        SupprimerPlat(session.UserId);
                        break;
                    case "5":
                        DefinirPlatDuJour(session.UserId);
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

        private void AfficherPlatsParCuisinier(int cuisinierId)
        {
            Console.Clear();
            Console.WriteLine("=== MES PLATS ===\n");

            string sql = @"
                SELECT PlatID, NomPlat, TypePlat, Description, PrixParPersonne, 
                       NombrePersonnes, NationaliteCuisine, DatePeremption, EstDisponible,
                       PlatDuJour
                FROM Plat
                WHERE CuisinierID = @cuisinierId
                ORDER BY DateCreation DESC";

            try
            {
                using var cmd = new MySqlCommand(sql, _db.GetConnection());
                cmd.Parameters.AddWithValue("@cuisinierId", cuisinierId);
                using var reader = cmd.ExecuteReader();

                if (!reader.HasRows)
                {
                    Console.WriteLine("Vous n'avez pas encore de plats.");
                }
                else
                {
                    while (reader.Read())
                    {
                        string platDuJourMention = reader.GetBoolean("PlatDuJour") ? "⭐ PLAT DU JOUR ⭐\n" : "";
                        Console.WriteLine($"\n{platDuJourMention}=== {reader["NomPlat"]} ===");
                        Console.WriteLine($"ID: {reader["PlatID"]}");
                        Console.WriteLine($"Type: {reader["TypePlat"]}");
                        Console.WriteLine($"Description: {reader["Description"]}");
                        Console.WriteLine($"Prix: {reader["PrixParPersonne"]:C2}");
                        Console.WriteLine($"Nombre de personnes: {reader["NombrePersonnes"]}");
                        Console.WriteLine($"Cuisine: {reader["NationaliteCuisine"]}");
                        Console.WriteLine($"Date de péremption: {((DateTime)reader["DatePeremption"]).ToString("dd/MM/yyyy")}");
                        Console.WriteLine($"Disponible: {(bool)reader["EstDisponible"]}");
                        Console.WriteLine(new string('-', 50));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Erreur : {ex.Message}");
            }

            Console.WriteLine("\nAppuyez sur une touche pour continuer...");
            Console.ReadKey();
        }

        private void AjouterPlat(int cuisinierId)
        {
            Console.Clear();
            Console.WriteLine("=== AJOUTER UN PLAT ===\n");

            try
            {
                Console.Write("Nom du plat : ");
                string nom = Console.ReadLine() ?? "";

                Console.WriteLine("\nType de plat :");
                Console.WriteLine("1. Entrée");
                Console.WriteLine("2. Plat Principal");
                Console.WriteLine("3. Dessert");
                Console.Write("Votre choix : ");
                string? typeChoix = Console.ReadLine();
                string type = typeChoix switch
                {
                    "1" => "Entrée",
                    "2" => "PlatPrincipal",
                    "3" => "Dessert",
                    _ => throw new Exception("Type de plat invalide")
                };

                Console.Write("\nDescription : ");
                string description = Console.ReadLine() ?? "";

                Console.Write("Prix par personne : ");
                if (!decimal.TryParse(Console.ReadLine(), out decimal prix))
                    throw new Exception("Prix invalide");

                Console.Write("Nombre de personnes : ");
                if (!int.TryParse(Console.ReadLine(), out int nombrePersonnes))
                    throw new Exception("Nombre de personnes invalide");

                Console.Write("Nationalité de la cuisine : ");
                string nationalite = Console.ReadLine() ?? "";

                Console.Write("Date de péremption (dd/MM/yyyy) : ");
                if (!DateTime.TryParseExact(Console.ReadLine(), "dd/MM/yyyy", null, 
                    System.Globalization.DateTimeStyles.None, out DateTime datePeremption))
                    throw new Exception("Format de date invalide");

                string sql = @"
                    INSERT INTO Plat (CuisinierID, NomPlat, TypePlat, Description, 
                                    DatePeremption, PrixParPersonne, NombrePersonnes, 
                                    NationaliteCuisine, EstDisponible)
                    VALUES (@cuisinierId, @nom, @type, @description, @datePeremption, 
                            @prix, @nombrePersonnes, @nationalite, TRUE)";

                using var cmd = new MySqlCommand(sql, _db.GetConnection());
                cmd.Parameters.AddWithValue("@cuisinierId", cuisinierId);
                cmd.Parameters.AddWithValue("@nom", nom);
                cmd.Parameters.AddWithValue("@type", type);
                cmd.Parameters.AddWithValue("@description", description);
                cmd.Parameters.AddWithValue("@datePeremption", datePeremption);
                cmd.Parameters.AddWithValue("@prix", prix);
                cmd.Parameters.AddWithValue("@nombrePersonnes", nombrePersonnes);
                cmd.Parameters.AddWithValue("@nationalite", nationalite);

                cmd.ExecuteNonQuery();
                Console.WriteLine("\n✅ Plat ajouté avec succès !");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Erreur : {ex.Message}");
            }

            Console.WriteLine("\nAppuyez sur une touche pour continuer...");
            Console.ReadKey();
        }

        private void ModifierPlat(int cuisinierId)
        {
            Console.Clear();
            Console.WriteLine("=== MODIFIER UN PLAT ===\n");

            try
            {
                AfficherPlatsParCuisinier(cuisinierId);

                Console.Write("\nID du plat à modifier : ");
                if (!int.TryParse(Console.ReadLine(), out int platId))
                    throw new Exception("ID invalide");

                // Vérifier que le plat appartient au cuisinier
                string sqlVerif = @"
                    SELECT COUNT(*) FROM Plat 
                    WHERE PlatID = @platId AND CuisinierID = @cuisinierId";
                using (var cmdVerif = new MySqlCommand(sqlVerif, _db.GetConnection()))
                {
                    cmdVerif.Parameters.AddWithValue("@platId", platId);
                    cmdVerif.Parameters.AddWithValue("@cuisinierId", cuisinierId);
                    if (Convert.ToInt32(cmdVerif.ExecuteScalar()) == 0)
                        throw new Exception("Ce plat ne vous appartient pas");
                }

                Console.Write("Nouveau prix (laisser vide pour ne pas modifier) : ");
                string? prixStr = Console.ReadLine();
                if (!string.IsNullOrEmpty(prixStr))
                {
                    if (!decimal.TryParse(prixStr, out decimal prix))
                        throw new Exception("Prix invalide");

                    string sqlPrix = "UPDATE Plat SET PrixParPersonne = @prix WHERE PlatID = @platId";
                    using var cmdPrix = new MySqlCommand(sqlPrix, _db.GetConnection());
                    cmdPrix.Parameters.AddWithValue("@prix", prix);
                    cmdPrix.Parameters.AddWithValue("@platId", platId);
                    cmdPrix.ExecuteNonQuery();
                }

                Console.Write("Nouvelle date de péremption (dd/MM/yyyy, laisser vide pour ne pas modifier) : ");
                string? dateStr = Console.ReadLine();
                if (!string.IsNullOrEmpty(dateStr))
                {
                    if (!DateTime.TryParseExact(dateStr, "dd/MM/yyyy", null, 
                        System.Globalization.DateTimeStyles.None, out DateTime datePeremption))
                        throw new Exception("Format de date invalide");

                    string sqlDate = "UPDATE Plat SET DatePeremption = @date WHERE PlatID = @platId";
                    using var cmdDate = new MySqlCommand(sqlDate, _db.GetConnection());
                    cmdDate.Parameters.AddWithValue("@date", datePeremption);
                    cmdDate.Parameters.AddWithValue("@platId", platId);
                    cmdDate.ExecuteNonQuery();
                }

                Console.Write("Disponibilité (o/n, laisser vide pour ne pas modifier) : ");
                string? dispStr = Console.ReadLine()?.ToLower();
                if (!string.IsNullOrEmpty(dispStr))
                {
                    bool disponible = dispStr == "o";
                    string sqlDisp = "UPDATE Plat SET EstDisponible = @disp WHERE PlatID = @platId";
                    using var cmdDisp = new MySqlCommand(sqlDisp, _db.GetConnection());
                    cmdDisp.Parameters.AddWithValue("@disp", disponible);
                    cmdDisp.Parameters.AddWithValue("@platId", platId);
                    cmdDisp.ExecuteNonQuery();
                }

                Console.WriteLine("\n✅ Plat modifié avec succès !");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Erreur : {ex.Message}");
            }

            Console.WriteLine("\nAppuyez sur une touche pour continuer...");
            Console.ReadKey();
        }

        private void SupprimerPlat(int cuisinierId)
        {
            Console.Clear();
            Console.WriteLine("=== SUPPRIMER UN PLAT ===\n");

            try
            {
                AfficherPlatsParCuisinier(cuisinierId);

                Console.Write("\nID du plat à supprimer : ");
                if (!int.TryParse(Console.ReadLine(), out int platId))
                    throw new Exception("ID invalide");

                // Vérifier que le plat appartient au cuisinier
                string sqlVerif = @"
                    SELECT COUNT(*) FROM Plat 
                    WHERE PlatID = @platId AND CuisinierID = @cuisinierId";
                using (var cmdVerif = new MySqlCommand(sqlVerif, _db.GetConnection()))
                {
                    cmdVerif.Parameters.AddWithValue("@platId", platId);
                    cmdVerif.Parameters.AddWithValue("@cuisinierId", cuisinierId);
                    if (Convert.ToInt32(cmdVerif.ExecuteScalar()) == 0)
                        throw new Exception("Ce plat ne vous appartient pas");
                }

                Console.Write("\nÊtes-vous sûr de vouloir supprimer ce plat ? (o/n) : ");
                if (Console.ReadLine()?.ToLower() != "o")
                {
                    Console.WriteLine("Suppression annulée.");
                    Console.ReadKey();
                    return;
                }

                string sql = "DELETE FROM Plat WHERE PlatID = @platId";
                using var cmd = new MySqlCommand(sql, _db.GetConnection());
                cmd.Parameters.AddWithValue("@platId", platId);
                cmd.ExecuteNonQuery();

                Console.WriteLine("\n✅ Plat supprimé avec succès !");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Erreur : {ex.Message}");
            }

            Console.WriteLine("\nAppuyez sur une touche pour continuer...");
            Console.ReadKey();
        }

        private void DefinirPlatDuJour(int cuisinierId)
        {
            Console.Clear();
            Console.WriteLine("=== DÉFINIR LE PLAT DU JOUR ===\n");

            try
            {
                // Réinitialiser tous les plats du jour du cuisinier
                string sqlReset = @"
                    UPDATE Plat 
                    SET PlatDuJour = FALSE 
                    WHERE CuisinierID = @cuisinierId";
                
                using (var cmdReset = new MySqlCommand(sqlReset, _db.GetConnection()))
                {
                    cmdReset.Parameters.AddWithValue("@cuisinierId", cuisinierId);
                    cmdReset.ExecuteNonQuery();
                }

                // Afficher les plats disponibles
                AfficherPlatsParCuisinier(cuisinierId);

                Console.Write("\nID du plat à définir comme plat du jour (0 pour annuler) : ");
                if (!int.TryParse(Console.ReadLine(), out int platId) || platId < 0)
                {
                    Console.WriteLine("ID invalide.");
                    Console.ReadKey();
                    return;
                }

                if (platId == 0)
                {
                    Console.WriteLine("Opération annulée.");
                    Console.ReadKey();
                    return;
                }

                string sqlUpdate = @"
                    UPDATE Plat 
                    SET PlatDuJour = TRUE 
                    WHERE PlatID = @platId AND CuisinierID = @cuisinierId";

                using (var cmd = new MySqlCommand(sqlUpdate, _db.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@platId", platId);
                    cmd.Parameters.AddWithValue("@cuisinierId", cuisinierId);
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                        Console.WriteLine("\n✅ Plat du jour défini avec succès !");
                    else
                        Console.WriteLine("\n❌ Plat non trouvé ou non autorisé.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Erreur : {ex.Message}");
            }

            Console.WriteLine("\nAppuyez sur une touche pour continuer...");
            Console.ReadKey();
        }

        public void VoirCommandesEnAttenteCuisinier(AuthModule.UserSession session)
        {
            Console.Clear();
            Console.WriteLine("=== COMMANDES EN ATTENTE ===\n");

            string sql = @"
                SELECT 
                    b.CommandeID,
                    b.DateCommande,
                    b.DateSouhaitee,
                    b.PrixPaye,
                    b.Statut,
                    CONCAT(u.PrenomU, ' ', u.NomU) as NomClient,
                    u.StationPlusProcheU as StationClient,
                    GROUP_CONCAT(CONCAT(p.NomPlat, ' x', c.Quantite) SEPARATOR ', ') as Plats
                FROM BonDeCommande_Liv b
                JOIN Utilisateur u ON b.ClientID = u.ClientID
                LEFT JOIN Correspond c ON b.CommandeID = c.CommandeID
                LEFT JOIN Plat p ON c.PlatID = p.PlatID
                WHERE b.CuisinierID = @cuisinierId 
                AND b.Statut IN ('En attente', 'Acceptée', 'En préparation')
                GROUP BY b.CommandeID, b.DateCommande, b.DateSouhaitee, b.PrixPaye, 
                         b.Statut, NomClient, StationClient
                ORDER BY b.DateSouhaitee ASC";

            try
            {
                using var cmd = new MySqlCommand(sql, _db.GetConnection());
                cmd.Parameters.AddWithValue("@cuisinierId", session.UserId);
                using var reader = cmd.ExecuteReader();

                if (!reader.HasRows)
                {
                    Console.WriteLine("Aucune commande en attente.");
                }
                else
                {
                    while (reader.Read())
                    {
                        Console.WriteLine($"\n=== Commande {reader["CommandeID"]} ===");
                        Console.WriteLine($"Date de commande : {((DateTime)reader["DateCommande"]):dd/MM/yyyy HH:mm}");
                        Console.WriteLine($"Date souhaitée : {((DateTime)reader["DateSouhaitee"]):dd/MM/yyyy HH:mm}");
                        Console.WriteLine($"Statut : {reader["Statut"]}");
                        Console.WriteLine($"Client : {reader["NomClient"]}");
                        Console.WriteLine($"Station de livraison : {reader["StationClient"]}");
                        Console.WriteLine($"Prix total : {((decimal)reader["PrixPaye"]):C2}");
                        Console.WriteLine($"Plats : {reader["Plats"]}");
                        Console.WriteLine(new string('-', 50));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Erreur : {ex.Message}");
            }

            Console.WriteLine("\nAppuyez sur une touche pour continuer...");
            Console.ReadKey();
        }
    }
} 