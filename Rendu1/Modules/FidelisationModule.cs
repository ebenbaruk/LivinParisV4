using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;

namespace Rendu1.Modules
{
    /// <summary>
    /// Module pour la gestion de la fidélité des clients, permettant de calculer la réduction, afficher le statut de fidélité et l'historique des commandes
    /// </summary>
    public class FidelisationModule
    {
        private readonly DatabaseManager _db;

        public FidelisationModule(DatabaseManager db)
        {
            _db = db;
        }

        /// <summary>
        /// Calcule la réduction en fonction du nombre de commandes et du cuisinier
        /// </summary>
        public (decimal reduction, string message) CalculerReduction(int clientId, int cuisinierId)
        {
            try
            {
                /// Compter le nombre de commandes livrées pour ce client avec ce cuisinier
                string sql = @"
                    SELECT COUNT(*) as NbCommandes
                    FROM BonDeCommande_Liv
                    WHERE ClientID = @clientId 
                    AND CuisinierID = @cuisinierId
                    AND Statut = 'Livrée'";

                using var cmd = new MySqlCommand(sql, _db.GetConnection());
                cmd.Parameters.AddWithValue("@clientId", clientId);
                cmd.Parameters.AddWithValue("@cuisinierId", cuisinierId);
                int nbCommandes = Convert.ToInt32(cmd.ExecuteScalar());

                /// Calculer la réduction en fonction du nombre de commmandes
                if (nbCommandes >= 10)
                {
                    return (0.20m, "🌟 Client VIP : -20% de réduction !");
                }
                else if (nbCommandes >= 7)
                {
                    return (0.15m, "🌟 Client Or : -15% de réduction !");
                }
                else if (nbCommandes >= 4)
                {
                    return (0.10m, "🌟 Client Fidèle : -10% de réduction !");
                }

                return (0m, "");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du calcul de la réduction : {ex.Message}");
                return (0m, "");
            }
        }

        /// <summary>
        /// Obtient lle statut de fidélité du client
        /// </summary>
        public string ObtenirStatutFidelite(int clientId)
        {
            try
            {
                /// Compter le nombre total de commandes livrées
                string sql = @"
                    SELECT 
                        COUNT(*) as TotalCommandes,
                        COUNT(DISTINCT CuisinierID) as NbCuisiniers,
                        SUM(PrixPaye) as MontantTotal
                    FROM BonDeCommande_Liv
                    WHERE ClientID = @clientId 
                    AND Statut = 'Livrée'";

                using var cmd = new MySqlCommand(sql, _db.GetConnection());
                cmd.Parameters.AddWithValue("@clientId", clientId);
                using var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    int totalCommandes = Convert.ToInt32(reader["TotalCommandes"]);
                    int nbCuisiniers = Convert.ToInt32(reader["NbCuisiniers"]);
                    decimal montantTotal = reader.GetDecimal("MontantTotal");

                    if (totalCommandes >= 20 && nbCuisiniers >= 5)
                        return "🎖️ Client Platine";
                    else if (totalCommandes >= 15)
                        return "🥇 Client Or";
                    else if (totalCommandes >= 10)
                        return "🥈 Client Argent";
                    else if (totalCommandes >= 5)
                        return "🥉 Client Bronze";
                    else
                        return "🌱 Nouveau Client";
                }

                return "🌱 Nouveau Client";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération du statut : {ex.Message}");
                return "🌱 Nouveau Client";
            }
        }

        /// <summary>
        /// Affiche les avantages de la fidélité
        /// </summary>
        public void AfficherAvantagesFidelite(int clientId)
        {
            string statut = ObtenirStatutFidelite(clientId);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n╔══════════════════════════════╗");
            Console.WriteLine("║     STATUT DE FIDÉLITÉ       ║");
            Console.WriteLine("╚══════════════════════════════╝");
            Console.ResetColor();

            Console.WriteLine($"\n📊 Votre niveau : {statut}");

            ///    Afficher uniquement les prochains paliers pertinents
            if (statut == "🌱 Nouveau Client")
            {
                Console.WriteLine("\n🎯 Prochain objectif :");
                Console.WriteLine("   4 commandes chez un cuisinier → -10% sur ses plats");
                Console.WriteLine("   5 commandes au total → Statut Bronze");
            }
            else if (statut == "🥉 Client Bronze")
            {
                Console.WriteLine("\n🎯 Prochain objectif :");
                Console.WriteLine("   7 commandes chez un cuisinier → -15% sur ses plats");
                Console.WriteLine("   10 commandes au total → Statut Argent");
            }
            else if (statut == "🥈 Client Argent")
            {
                Console.WriteLine("\n🎯 Prochain objectif :");
                Console.WriteLine("   10 commandes chez un cuisinier → -20% sur ses plats");
                Console.WriteLine("   15 commandes au total → Statut Or");
            }
            else if (statut == "🥇 Client Or")
            {
                Console.WriteLine("\n🎯 Prochain objectif :");
                Console.WriteLine("   20 commandes et 5 cuisiniers → Statut Platine");
            }
        }

        /// <summary>
        /// Affiche l'hiistorique des commandes du client
        /// </summary>
        public void AfficherHistoriqueFidelite(int clientId)
        {
            try
            {
                string sql = @"
                    SELECT 
                        c.ClientID as CuisinierID,
                        CONCAT(u.PrenomU, ' ', u.NomU) as NomCuisinier,
                        COUNT(*) as NbCommandes,
                        SUM(b.PrixPaye) as MontantTotal
                    FROM BonDeCommande_Liv b
                    JOIN Utilisateur u ON b.CuisinierID = u.ClientID
                    JOIN Cuisinier c ON u.ClientID = c.ClientID
                    WHERE b.ClientID = @clientId 
                    AND b.Statut = 'Livrée'
                    GROUP BY c.ClientID, u.PrenomU, u.NomU
                    ORDER BY NbCommandes DESC";

                using var cmd = new MySqlCommand(sql, _db.GetConnection());
                cmd.Parameters.AddWithValue("@clientId", clientId);
                using var reader = cmd.ExecuteReader();

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n╔══════════════════════════════╗");
                Console.WriteLine("║    CUISINIERS PRÉFÉRÉS       ║");
                Console.WriteLine("╚══════════════════════════════╝");
                Console.ResetColor();

                /// Afficher les données

                bool hasData = false;
                while (reader.Read())
                {
                    hasData = true;
                    int nbCommandes = Convert.ToInt32(reader["NbCommandes"]);
                    string reduction = nbCommandes >= 10 ? "20%" : 
                                     nbCommandes >= 7 ? "15%" : 
                                     nbCommandes >= 4 ? "10%" : "0%";

                    string nextReduction = nbCommandes >= 10 ? "Max" : 
                                         nbCommandes >= 7 ? "20%" : 
                                         nbCommandes >= 4 ? "15%" : "10%";

                    int commandesRestantes = nbCommandes >= 10 ? 0 :
                                           nbCommandes >= 7 ? 10 - nbCommandes :
                                           nbCommandes >= 4 ? 7 - nbCommandes :
                                           4 - nbCommandes;

                    Console.WriteLine($"\n👨‍🍳 {reader["NomCuisinier"]}");
                    Console.WriteLine($"   📊 {nbCommandes} commandes | 💰 {reader.GetDecimal("MontantTotal"):C2}");
                    
                    if (reduction != "0%")
                    {
                        Console.WriteLine($"   🎁 Réduction actuelle : -{reduction}");
                    }
                    
                    if (commandesRestantes > 0)
                    {
                        Console.WriteLine($"   ⭐ Plus que {commandesRestantes} commandes pour -{nextReduction}");
                    }
                }

                /// Afficher un message si aucune commande n'a été effectuée
                if (!hasData)
                {
                    Console.WriteLine("\n📝 Aucune commande effectuée pour le moment");
                    Console.WriteLine("   Commandez chez un cuisinier 4 fois pour obtenir -10% !");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Erreur : {ex.Message}");
            }
        }
    }
} 