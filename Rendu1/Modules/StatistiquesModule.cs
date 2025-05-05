using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace Rendu1.Modules
{
    /// <summary>
    /// Module pour la gestion des statistiques, permettant de visualiser les bilans, les commandes par période, les moyennes de prix, les statistiques des comptes clients et les commandes filtrées
    /// </summary>
    public class StatistiquesModule
    {
        private readonly DatabaseManager _db;

        public StatistiquesModule(DatabaseManager db)
        {
            _db = db;
        }

        /// <summary>
        /// Affiche le menu des statistiques
        /// </summary>
        public void AfficherMenuStatistiques()
        {
            bool continuer = true;
            while (continuer)
            {
                Console.Clear();
                Console.WriteLine("=== MENU STATISTIQUES ===");
                Console.WriteLine("1. Bilan des livraisons par cuisinier");
                Console.WriteLine("2. Commandes par période");
                Console.WriteLine("3. Moyenne des prix des commandes");
                Console.WriteLine("4. Statistiques des comptes clients");
                Console.WriteLine("5. Commandes par client (filtres avancés)");
                Console.WriteLine("0. Retour au menu principal");

                Console.Write("\nVotre choix : ");
                string? choix = Console.ReadLine();

                switch (choix)
                {
                    case "1":
                        AfficherBilanLivraisonsParCuisinier();
                        break;
                    case "2":
                        AfficherCommandesParPeriode();
                        break;
                    case "3":
                        AfficherMoyennePrixCommandes();
                        break;
                    case "4":
                        AfficherStatistiquesClients();
                        break;
                    case "5":
                        AfficherCommandesClientFiltrees();
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
        /// Affiche le bilan des livraisons par cuisinier
        /// </summary>
        private void AfficherBilanLivraisonsParCuisinier()
        {
            Console.Clear();
            Console.WriteLine("=== BILAN DES LIVRAISONS PAR CUISINIER ===\n");

            string sql = @"
                SELECT 
                    CONCAT(u.NomU, ' ', u.PrenomU) as Cuisinier,
                    COUNT(b.CommandeID) as TotalCommandes,
                    COUNT(CASE WHEN b.Statut = 'Livrée' THEN 1 END) as CommandesLivrees,
                    AVG(b.PrixPaye) as MoyennePrixCommande,
                    MIN(b.DateCommande) as PremiereCommande,
                    MAX(b.DateCommande) as DerniereCommande
                FROM Utilisateur u
                JOIN Cuisinier c ON u.ClientID = c.ClientID
                LEFT JOIN BonDeCommande_Liv b ON c.ClientID = b.CuisinierID
                GROUP BY u.ClientID
                ORDER BY CommandesLivrees DESC";

            /// Afficher les données

            try
            {
                using var cmd = new MySqlCommand(sql, _db.GetConnection());
                using var reader = cmd.ExecuteReader();

                if (!reader.HasRows)
                {
                    Console.WriteLine("Aucune donnée disponible.");
                    Console.ReadKey();
                    return;
                }
                

                while (reader.Read())
                {
                    Console.WriteLine(new string('=', 80));
                    Console.WriteLine($"Cuisinier : {reader["Cuisinier"]}");
                    Console.WriteLine($"Total des commandes : {reader["TotalCommandes"]}");
                    Console.WriteLine($"Commandes livrées : {reader["CommandesLivrees"]}");
                    Console.WriteLine($"Prix moyen des commandes : {Convert.ToDecimal(reader["MoyennePrixCommande"]):F2}€");
                    
                    if (reader["PremiereCommande"] != DBNull.Value)
                    {
                        Console.WriteLine($"Première commande : {Convert.ToDateTime(reader["PremiereCommande"]):dd/MM/yyyy}");
                        Console.WriteLine($"Dernière commande : {Convert.ToDateTime(reader["DerniereCommande"]):dd/MM/yyyy}");
                    }
                    Console.WriteLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Erreur : {ex.Message}");
            }

            Console.WriteLine("\nAppuyez sur une touche pour continuer...");
            Console.ReadKey();
        }

        /// <summary>
        /// Affiche les commandes par période, entre 2 dattes
        /// </summary>
        private void AfficherCommandesParPeriode()
        {
            Console.Clear();
            Console.WriteLine("=== COMMANDES PAR PÉRIODE ===\n");

            Console.Write("Date de début (YYYY-MM-DD) : ");
            if (!DateTime.TryParse(Console.ReadLine(), out DateTime dateDebut))
            {
                Console.WriteLine("Date invalide.");
                Console.ReadKey();
                return;
            }

            Console.Write("Date de fin (YYYY-MM-DD) : ");
            if (!DateTime.TryParse(Console.ReadLine(), out DateTime dateFin))
            {
                Console.WriteLine("Date invalide.");
                Console.ReadKey();
                return;
            }

            string sql = @"
                SELECT 
                    b.CommandeID,
                    b.DateCommande,
                    b.Statut,
                    CONCAT(u1.NomU, ' ', COALESCE(u1.PrenomU, '')) as Client,
                    CONCAT(u2.NomU, ' ', u2.PrenomU) as Cuisinier,
                    GROUP_CONCAT(CONCAT(p.NomPlat, ' (', c.Quantite, 'x)')) as Plats,
                    b.PrixPaye
                FROM BonDeCommande_Liv b
                JOIN Utilisateur u1 ON b.ClientID = u1.ClientID
                JOIN Utilisateur u2 ON b.CuisinierID = u2.ClientID
                JOIN Correspond c ON b.CommandeID = c.CommandeID
                JOIN Plat p ON c.PlatID = p.PlatID
                WHERE b.DateCommande BETWEEN @dateDebut AND @dateFin
                GROUP BY b.CommandeID
                ORDER BY b.DateCommande DESC";

            try
            {
                using var cmd = new MySqlCommand(sql, _db.GetConnection());
                cmd.Parameters.AddWithValue("@dateDebut", dateDebut);
                cmd.Parameters.AddWithValue("@dateFin", dateFin);
                using var reader = cmd.ExecuteReader();

                if (!reader.HasRows)
                {
                    Console.WriteLine("Aucune commande trouvée pour cette période.");
                    Console.ReadKey();
                    return;
                }

                while (reader.Read())
                {
                    Console.WriteLine(new string('-', 80));
                    Console.WriteLine($"Commande : {reader["CommandeID"]}");
                    Console.WriteLine($"Date : {Convert.ToDateTime(reader["DateCommande"]):dd/MM/yyyy HH:mm}");
                    Console.WriteLine($"Client : {reader["Client"]}");
                    Console.WriteLine($"Cuisinier : {reader["Cuisinier"]}");
                    Console.WriteLine($"Plats : {reader["Plats"]}");
                    Console.WriteLine($"Prix : {reader["PrixPaye"]}€");
                    Console.WriteLine($"Statut : {reader["Statut"]}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Erreur : {ex.Message}");
            }

            Console.WriteLine("\nAppuyez sur une touche pour continuer...");
            Console.ReadKey();
        }

        /// <summary>
        /// Affiche la moyenne des prix des commandes
        /// </summary>
        private void AfficherMoyennePrixCommandes()
        {
            Console.Clear();
            Console.WriteLine("=== MOYENNE DES PRIX DES COMMANDES ===\n");

            string sql = @"
                SELECT 
                    COUNT(*) as NombreCommandes,
                    AVG(PrixPaye) as MoyennePrix,
                    MIN(PrixPaye) as PrixMin,
                    MAX(PrixPaye) as PrixMax,
                    SUM(PrixPaye) as ChiffreAffaires
                FROM BonDeCommande_Liv
                WHERE Statut != 'Annulée'";

            try
            {
                using var cmd = new MySqlCommand(sql, _db.GetConnection());
                using var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    Console.WriteLine($"Nombre total de commandes : {reader["NombreCommandes"]}");
                    Console.WriteLine($"Prix moyen des commandes : {Convert.ToDecimal(reader["MoyennePrix"]):F2}€");
                    Console.WriteLine($"Prix minimum : {reader["PrixMin"]}€");
                    Console.WriteLine($"Prix maximum : {reader["PrixMax"]}€");
                    Console.WriteLine($"Chiffre d'affaires total : {reader["ChiffreAffaires"]}€");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Erreur : {ex.Message}");
            }

            Console.WriteLine("\nAppuyez sur une touche pour continuer...");
            Console.ReadKey();
        }

        /// <summary>
        /// Affiche les statistiques des comptes clients
        /// </summary>
        private void AfficherStatistiquesClients()
        {
            Console.Clear();
            Console.WriteLine("=== STATISTIQUES DES COMPTES CLIENTS ===\n");

            string sql = @"
                SELECT 
                    COUNT(*) as TotalClients,
                    COUNT(CASE WHEN TypeClient = 'Particulier' THEN 1 END) as NombreParticuliers,
                    COUNT(CASE WHEN TypeClient = 'Entreprise' THEN 1 END) as NombreEntreprises,
                    AVG(CASE WHEN TypeClient = 'Particulier' THEN BudgetU END) as MoyenneBudgetParticuliers,
                    AVG(CASE WHEN TypeClient = 'Entreprise' THEN BudgetU END) as MoyenneBudgetEntreprises,
                    COUNT(DISTINCT StationPlusProcheU) as NombreStationsUniques
                FROM Utilisateur
                WHERE ClientID NOT IN (SELECT ClientID FROM Cuisinier)";

            try
            {
                using var cmd = new MySqlCommand(sql, _db.GetConnection());
                using var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    Console.WriteLine($"Nombre total de clients : {reader["TotalClients"]}");
                    Console.WriteLine($"Particuliers : {reader["NombreParticuliers"]}");
                    Console.WriteLine($"Entreprises : {reader["NombreEntreprises"]}");
                    Console.WriteLine($"Budget moyen particuliers : {Convert.ToDecimal(reader["MoyenneBudgetParticuliers"]):F2}€");
                    Console.WriteLine($"Budget moyen entreprises : {Convert.ToDecimal(reader["MoyenneBudgetEntreprises"]):F2}€");
                    Console.WriteLine($"Nombre de stations desservies : {reader["NombreStationsUniques"]}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Erreur : {ex.Message}");
            }

            Console.WriteLine("\nAppuyez sur une touche pour continuer...");
            Console.ReadKey();
        }

        /// <summary>
        /// Affiche les commandes filtrées par client
        /// </summary>
        private void AfficherCommandesClientFiltrees()
        {
            Console.Clear();
            Console.WriteLine("=== RECHERCHE DE COMMANDES FILTRÉES ===\n");

            Console.Write("Email du client : ");
            string email = Console.ReadLine() ?? "";

            Console.Write("Nationalité du plat (laisser vide pour toutes) : ");
            string nationalite = Console.ReadLine() ?? "";

            Console.Write("Date de début (YYYY-MM-DD, laisser vide pour toutes) : ");
            string dateDebutStr = Console.ReadLine() ?? "";
            DateTime? dateDebut = null;
            if (!string.IsNullOrEmpty(dateDebutStr))
            {
                if (!DateTime.TryParse(dateDebutStr, out DateTime date))
                {
                    Console.WriteLine("Format de date invalide.");
                    Console.ReadKey();
                    return;
                }
                dateDebut = date;
            }

            /// Afficher les données

            Console.Write("Date de fin (YYYY-MM-DD, laisser vide pour toutes) : ");
            string dateFinStr = Console.ReadLine() ?? "";
            DateTime? dateFin = null;
            if (!string.IsNullOrEmpty(dateFinStr))
            {
                if (!DateTime.TryParse(dateFinStr, out DateTime date))
                {
                    Console.WriteLine("Format de date invalide.");
                    Console.ReadKey();
                    return;
                }
                dateFin = date;
            }

            string sql = @"
                SELECT 
                    b.CommandeID,
                    b.DateCommande,
                    b.Statut,
                    GROUP_CONCAT(DISTINCT p.NationaliteCuisine) as Nationalites,
                    GROUP_CONCAT(CONCAT(p.NomPlat, ' (', c.Quantite, 'x)')) as Plats,
                    b.PrixPaye
                FROM BonDeCommande_Liv b
                JOIN Utilisateur u ON b.ClientID = u.ClientID
                JOIN Correspond c ON b.CommandeID = c.CommandeID
                JOIN Plat p ON c.PlatID = p.PlatID
                WHERE u.EmailU = @email";

            if (!string.IsNullOrEmpty(nationalite))
                sql += " AND p.NationaliteCuisine = @nationalite";
            if (dateDebut.HasValue)
                sql += " AND b.DateCommande >= @dateDebut";
            if (dateFin.HasValue)
                sql += " AND b.DateCommande <= @dateFin";

            sql += " GROUP BY b.CommandeID ORDER BY b.DateCommande DESC";

            try
            {
                using var cmd = new MySqlCommand(sql, _db.GetConnection());
                cmd.Parameters.AddWithValue("@email", email);
                if (!string.IsNullOrEmpty(nationalite))
                    cmd.Parameters.AddWithValue("@nationalite", nationalite);
                if (dateDebut.HasValue)
                    cmd.Parameters.AddWithValue("@dateDebut", dateDebut.Value);
                if (dateFin.HasValue)
                    cmd.Parameters.AddWithValue("@dateFin", dateFin.Value);

                using var reader = cmd.ExecuteReader();

                if (!reader.HasRows)
                {
                    Console.WriteLine("Aucune commande trouvée avec ces critères.");
                    Console.ReadKey();
                    return;
                }

                while (reader.Read())
                {
                    Console.WriteLine(new string('-', 80));
                    Console.WriteLine($"Commande : {reader["CommandeID"]}");
                    Console.WriteLine($"Date : {Convert.ToDateTime(reader["DateCommande"]):dd/MM/yyyy HH:mm}");
                    Console.WriteLine($"Nationalités : {reader["Nationalites"]}");
                    Console.WriteLine($"Plats : {reader["Plats"]}");
                    Console.WriteLine($"Prix : {reader["PrixPaye"]}€");
                    Console.WriteLine($"Statut : {reader["Statut"]}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Erreur : {ex.Message}");
            }

            Console.WriteLine("\nAppuyez sur une touche pour continuer...");
            Console.ReadKey();
        }
    }
} 