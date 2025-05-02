using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using MySql.Data.MySqlClient;

namespace Rendu1.Modules
{
    public class CuisinierModule
    {
        private readonly DatabaseManager _db;

        public CuisinierModule(DatabaseManager db)
        {
            _db = db;
        }

        public void AfficherMenuCuisinier()
        {
            bool continuer = true;
            while (continuer)
            {
                Console.Clear();
                Console.WriteLine("=== MENU CUISINIER ===");
                Console.WriteLine("1. Ajouter un nouveau cuisinier");
                Console.WriteLine("2. Modifier un cuisinier");
                Console.WriteLine("3. Supprimer un cuisinier");
                Console.WriteLine("4. Afficher les clients servis");
                Console.WriteLine("5. Afficher les plats par fréquence");
                Console.WriteLine("6. Gérer le plat du jour");
                Console.WriteLine("7. Importer des cuisiniers depuis un fichier");
                Console.WriteLine("8. Afficher tous les cuisiniers");
                Console.WriteLine("0. Retour au menu principal");

                Console.Write("\nVotre choix : ");
                string? choix = Console.ReadLine();

                switch (choix)
                {
                    case "1":
                        AjouterCuisinier();
                        break;
                    case "2":
                        ModifierCuisinier();
                        break;
                    case "3":
                        SupprimerCuisinier();
                        break;
                    case "4":
                        AfficherClientsServis();
                        break;
                    case "5":
                        AfficherPlatsParFrequence();
                        break;
                    case "6":
                        GererPlatDuJour();
                        break;
                    case "7":
                        ImporterCuisiniersDepuisFichier();
                        break;
                    case "8":
                        AfficherTousLesCuisiniers();
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

        private void AjouterCuisinier()
        {
            Console.Clear();
            Console.WriteLine("=== AJOUT D'UN NOUVEAU CUISINIER ===");

            // D'abord, créer un utilisateur
            Console.WriteLine("\n--- Informations personnelles ---");
            Console.Write("Nom : ");
            string nomU = Console.ReadLine() ?? "";
            Console.Write("Prénom : ");
            string prenomU = Console.ReadLine() ?? "";
            Console.Write("Rue : ");
            string rueU = Console.ReadLine() ?? "";
            Console.Write("Numéro : ");
            int numeroU = int.Parse(Console.ReadLine() ?? "0");
            Console.Write("Code postal : ");
            int codePostalU = int.Parse(Console.ReadLine() ?? "0");
            Console.Write("Ville : ");
            string villeU = Console.ReadLine() ?? "";
            Console.Write("Téléphone : ");
            string telephoneU = Console.ReadLine() ?? "";
            Console.Write("Email : ");
            string emailU = Console.ReadLine() ?? "";
            Console.Write("Station de métro la plus proche : ");
            string stationPlusProcheU = Console.ReadLine() ?? "";
            Console.Write("Mot de passe : ");
            string mdpU = Console.ReadLine() ?? "";

            Console.WriteLine("\n--- Informations cuisinier ---");
            Console.Write("Spécialité : ");
            string specialiteC = Console.ReadLine() ?? "";

            try
            {
                // Insérer l'utilisateur
                string sqlUser = @"INSERT INTO Utilisateur 
                    (TypeClient, NomU, PrenomU, RueU, NumeroU, CodePostalU, VilleU, 
                    TelephoneU, EmailU, StationPlusProcheU, MDPU)
                    VALUES 
                    ('Particulier', @nom, @prenom, @rue, @numero, @codePostal, @ville,
                    @telephone, @email, @station, @mdp);
                    SELECT LAST_INSERT_ID();";

                int clientId;
                using (var cmd = new MySqlCommand(sqlUser, _db.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@nom", nomU);
                    cmd.Parameters.AddWithValue("@prenom", prenomU);
                    cmd.Parameters.AddWithValue("@rue", rueU);
                    cmd.Parameters.AddWithValue("@numero", numeroU);
                    cmd.Parameters.AddWithValue("@codePostal", codePostalU);
                    cmd.Parameters.AddWithValue("@ville", villeU);
                    cmd.Parameters.AddWithValue("@telephone", telephoneU);
                    cmd.Parameters.AddWithValue("@email", emailU);
                    cmd.Parameters.AddWithValue("@station", stationPlusProcheU);
                    cmd.Parameters.AddWithValue("@mdp", mdpU);

                    clientId = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // Insérer le cuisinier
                string sqlCuisinier = @"INSERT INTO Cuisinier 
                    (ClientID, SpecialiteC, Note)
                    VALUES 
                    (@clientId, @specialite, 0)";

                using (var cmd = new MySqlCommand(sqlCuisinier, _db.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@clientId", clientId);
                    cmd.Parameters.AddWithValue("@specialite", specialiteC);
                    cmd.ExecuteNonQuery();
                }

                Console.WriteLine("\n✅ Cuisinier ajouté avec succès !");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Erreur lors de l'ajout du cuisinier : {ex.Message}");
            }

            Console.WriteLine("\nAppuyez sur une touche pour continuer...");
            Console.ReadKey();
        }

        private void ModifierCuisinier()
        {
            Console.Clear();
            Console.WriteLine("=== MODIFICATION D'UN CUISINIER ===");

            Console.Write("Email du cuisinier à modifier : ");
            string email = Console.ReadLine() ?? "";

            try
            {
                // Vérifier si le cuisinier existe
                string checkSql = @"SELECT c.ClientID, c.SpecialiteC, u.* 
                    FROM Cuisinier c
                    JOIN Utilisateur u ON c.ClientID = u.ClientID
                    WHERE u.EmailU = @email";

                using (var cmd = new MySqlCommand(checkSql, _db.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@email", email);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            Console.WriteLine("❌ Cuisinier non trouvé.");
                            Console.ReadKey();
                            return;
                        }
                    }
                }

                Console.WriteLine("\nNouvelles informations (laisser vide pour ne pas modifier) :");
                
                Console.Write("Spécialité : ");
                string specialite = Console.ReadLine() ?? "";
                
                Console.Write("Téléphone : ");
                string telephone = Console.ReadLine() ?? "";
                
                Console.Write("Rue : ");
                string rue = Console.ReadLine() ?? "";
                
                Console.Write("Numéro : ");
                string numeroStr = Console.ReadLine() ?? "";
                
                Console.Write("Code postal : ");
                string codePostalStr = Console.ReadLine() ?? "";
                
                Console.Write("Ville : ");
                string ville = Console.ReadLine() ?? "";

                // Mettre à jour l'utilisateur
                if (!string.IsNullOrWhiteSpace(telephone) || !string.IsNullOrWhiteSpace(rue) || 
                    !string.IsNullOrWhiteSpace(numeroStr) || !string.IsNullOrWhiteSpace(codePostalStr) || 
                    !string.IsNullOrWhiteSpace(ville))
                {
                    string updateUserSql = "UPDATE Utilisateur SET ";
                    List<string> updates = new List<string>();
                    var parameters = new MySqlCommand();

                    if (!string.IsNullOrWhiteSpace(telephone))
                    {
                        updates.Add("TelephoneU = @telephone");
                        parameters.Parameters.AddWithValue("@telephone", telephone);
                    }
                    if (!string.IsNullOrWhiteSpace(rue))
                    {
                        updates.Add("RueU = @rue");
                        parameters.Parameters.AddWithValue("@rue", rue);
                    }
                    if (!string.IsNullOrWhiteSpace(numeroStr) && int.TryParse(numeroStr, out int numero))
                    {
                        updates.Add("NumeroU = @numero");
                        parameters.Parameters.AddWithValue("@numero", numero);
                    }
                    if (!string.IsNullOrWhiteSpace(codePostalStr) && int.TryParse(codePostalStr, out int codePostal))
                    {
                        updates.Add("CodePostalU = @codePostal");
                        parameters.Parameters.AddWithValue("@codePostal", codePostal);
                    }
                    if (!string.IsNullOrWhiteSpace(ville))
                    {
                        updates.Add("VilleU = @ville");
                        parameters.Parameters.AddWithValue("@ville", ville);
                    }

                    if (updates.Count > 0)
                    {
                        updateUserSql += string.Join(", ", updates);
                        updateUserSql += " WHERE EmailU = @email";
                        parameters.Parameters.AddWithValue("@email", email);

                        using (var cmd = new MySqlCommand(updateUserSql, _db.GetConnection()))
                        {
                            foreach (MySqlParameter param in parameters.Parameters)
                            {
                                cmd.Parameters.Add(param);
                            }
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                // Mettre à jour le cuisinier
                if (!string.IsNullOrWhiteSpace(specialite))
                {
                    string updateCuisinierSql = @"UPDATE Cuisinier c
                        JOIN Utilisateur u ON c.ClientID = u.ClientID
                        SET c.SpecialiteC = @specialite
                        WHERE u.EmailU = @email";

                    using (var cmd = new MySqlCommand(updateCuisinierSql, _db.GetConnection()))
                    {
                        cmd.Parameters.AddWithValue("@specialite", specialite);
                        cmd.Parameters.AddWithValue("@email", email);
                        cmd.ExecuteNonQuery();
                    }
                }

                Console.WriteLine("\n✅ Cuisinier modifié avec succès !");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Erreur lors de la modification : {ex.Message}");
            }

            Console.WriteLine("\nAppuyez sur une touche pour continuer...");
            Console.ReadKey();
        }

        private void SupprimerCuisinier()
        {
            Console.Clear();
            Console.WriteLine("=== SUPPRESSION D'UN CUISINIER ===");

            Console.Write("Email du cuisinier à supprimer : ");
            string email = Console.ReadLine() ?? "";

            try
            {
                string sql = @"UPDATE Utilisateur u
                    JOIN Cuisinier c ON u.ClientID = c.ClientID
                    SET u.Actif = FALSE 
                    WHERE u.EmailU = @email";

                using (var cmd = new MySqlCommand(sql, _db.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@email", email);
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                        Console.WriteLine("\n✅ Cuisinier désactivé avec succès !");
                    else
                        Console.WriteLine("\n❌ Cuisinier non trouvé.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Erreur lors de la suppression : {ex.Message}");
            }

            Console.WriteLine("\nAppuyez sur une touche pour continuer...");
            Console.ReadKey();
        }

        private void AfficherClientsServis()
        {
            Console.Clear();
            Console.WriteLine("=== CLIENTS SERVIS PAR UN CUISINIER ===\n");

            Console.Write("Email du cuisinier : ");
            string email = Console.ReadLine() ?? "";

            Console.WriteLine("\nPériode de recherche :");
            Console.WriteLine("1. Depuis l'inscription");
            Console.WriteLine("2. Période personnalisée");
            Console.Write("Votre choix (1 ou 2) : ");
            string choixPeriode = Console.ReadLine() ?? "1";

            string dateCondition = "";
            var parameters = new MySqlCommand();
            parameters.Parameters.AddWithValue("@email", email);

            if (choixPeriode == "2")
            {
                Console.Write("\nDate de début (YYYY-MM-DD) : ");
                string dateDebut = Console.ReadLine() ?? DateTime.Now.AddYears(-1).ToString("yyyy-MM-dd");
                Console.Write("Date de fin (YYYY-MM-DD) : ");
                string dateFin = Console.ReadLine() ?? DateTime.Now.ToString("yyyy-MM-dd");

                dateCondition = "AND b.DateCommande BETWEEN @dateDebut AND @dateFin";
                parameters.Parameters.AddWithValue("@dateDebut", dateDebut);
                parameters.Parameters.AddWithValue("@dateFin", dateFin);
            }

            string sql = $@"SELECT 
                CASE 
                    WHEN u.TypeClient = 'Particulier' THEN CONCAT(u.NomU, ' ', u.PrenomU)
                    ELSE u.NomEntreprise
                END as Client,
                b.CommandeID,
                b.DateCommande,
                b.PrixPaye,
                GROUP_CONCAT(p.NomPlat SEPARATOR ', ') as Plats
                FROM Utilisateur u2
                JOIN Cuisinier c ON u2.ClientID = c.ClientID
                JOIN BonDeCommande_Liv b ON c.ClientID = b.CuisinierID
                JOIN Utilisateur u ON b.ClientID = u.ClientID
                JOIN Correspond co ON b.CommandeID = co.CommandeID
                JOIN Plat p ON co.PlatID = p.PlatID
                WHERE u2.EmailU = @email {dateCondition}
                GROUP BY b.CommandeID
                ORDER BY b.DateCommande DESC";

            try
            {
                using (var cmd = new MySqlCommand(sql, _db.GetConnection()))
                {
                    foreach (MySqlParameter param in parameters.Parameters)
                    {
                        cmd.Parameters.Add(param);
                    }

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                        {
                            Console.WriteLine("\nAucun client servi pour cette période.");
                            Console.ReadKey();
                            return;
                        }

                        // Afficher les en-têtes
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            Console.Write($"{reader.GetName(i),-25} ");
                        }
                        Console.WriteLine("\n" + new string('-', reader.FieldCount * 26));

                        // Afficher les données
                        while (reader.Read())
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                Console.Write($"{reader[i],-25} ");
                            }
                            Console.WriteLine();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Erreur lors de l'affichage : {ex.Message}");
            }

            Console.WriteLine("\nAppuyez sur une touche pour continuer...");
            Console.ReadKey();
        }

        private void AfficherPlatsParFrequence()
        {
            Console.Clear();
            Console.WriteLine("=== PLATS PAR FRÉQUENCE ===\n");

            Console.Write("Email du cuisinier : ");
            string email = Console.ReadLine() ?? "";

            string sql = @"SELECT 
                p.NomPlat,
                COUNT(co.PlatID) as NombreDeCommandes,
                SUM(co.Quantite) as QuantiteTotale,
                MIN(b.DateCommande) as PremiereCommande,
                MAX(b.DateCommande) as DerniereCommande
                FROM Utilisateur u
                JOIN Cuisinier c ON u.ClientID = c.ClientID
                JOIN Plat p ON c.ClientID = p.CuisinierID
                LEFT JOIN Correspond co ON p.PlatID = co.PlatID
                LEFT JOIN BonDeCommande_Liv b ON co.CommandeID = b.CommandeID
                WHERE u.EmailU = @email
                GROUP BY p.PlatID
                ORDER BY NombreDeCommandes DESC";

            try
            {
                using (var cmd = new MySqlCommand(sql, _db.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@email", email);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                        {
                            Console.WriteLine("\nAucun plat trouvé pour ce cuisinier.");
                            Console.ReadKey();
                            return;
                        }

                        // Afficher les en-têtes
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            Console.Write($"{reader.GetName(i),-25} ");
                        }
                        Console.WriteLine("\n" + new string('-', reader.FieldCount * 26));

                        // Afficher les données
                        while (reader.Read())
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                Console.Write($"{reader[i],-25} ");
                            }
                            Console.WriteLine();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Erreur lors de l'affichage : {ex.Message}");
            }

            Console.WriteLine("\nAppuyez sur une touche pour continuer...");
            Console.ReadKey();
        }

        private void GererPlatDuJour()
        {
            Console.Clear();
            Console.WriteLine("=== GESTION DU PLAT DU JOUR ===\n");

            Console.Write("Email du cuisinier : ");
            string email = Console.ReadLine() ?? "";

            // Afficher les plats actuels du cuisinier
            string sqlPlats = @"SELECT p.PlatID, p.NomPlat, p.PrixParPersonne, p.EstDisponible
                FROM Utilisateur u
                JOIN Cuisinier c ON u.ClientID = c.ClientID
                JOIN Plat p ON c.ClientID = p.CuisinierID
                WHERE u.EmailU = @email
                ORDER BY p.DateCreation DESC";

            try
            {
                Console.WriteLine("\nVos plats :");
                using (var cmd = new MySqlCommand(sqlPlats, _db.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@email", email);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                        {
                            Console.WriteLine("Aucun plat trouvé. Veuillez d'abord créer des plats.");
                            Console.ReadKey();
                            return;
                        }

                        while (reader.Read())
                        {
                            Console.WriteLine($"{reader["PlatID"]}. {reader["NomPlat"]} - {reader["PrixParPersonne"]}€ " +
                                           $"({(Convert.ToBoolean(reader["EstDisponible"]) ? "Disponible" : "Non disponible")})");
                        }
                    }
                }

                Console.Write("\nID du plat à définir comme plat du jour (0 pour annuler) : ");
                if (int.TryParse(Console.ReadLine(), out int platId) && platId > 0)
                {
                    // Désactiver tous les plats
                    string sqlUpdate1 = @"UPDATE Plat p
                        JOIN Cuisinier c ON p.CuisinierID = c.ClientID
                        JOIN Utilisateur u ON c.ClientID = u.ClientID
                        SET p.EstDisponible = FALSE
                        WHERE u.EmailU = @email";

                    // Activer le plat sélectionné
                    string sqlUpdate2 = "UPDATE Plat SET EstDisponible = TRUE WHERE PlatID = @platId";

                    using (var cmd1 = new MySqlCommand(sqlUpdate1, _db.GetConnection()))
                    using (var cmd2 = new MySqlCommand(sqlUpdate2, _db.GetConnection()))
                    {
                        cmd1.Parameters.AddWithValue("@email", email);
                        cmd1.ExecuteNonQuery();

                        cmd2.Parameters.AddWithValue("@platId", platId);
                        cmd2.ExecuteNonQuery();

                        Console.WriteLine("\n✅ Plat du jour mis à jour avec succès !");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Erreur lors de la gestion du plat du jour : {ex.Message}");
            }

            Console.WriteLine("\nAppuyez sur une touche pour continuer...");
            Console.ReadKey();
        }

        private void ImporterCuisiniersDepuisFichier()
        {
            Console.Clear();
            Console.WriteLine("=== IMPORT DE CUISINIERS DEPUIS UN FICHIER ===");
            Console.WriteLine("Le fichier doit être au format CSV avec les colonnes suivantes :");
            Console.WriteLine("Nom;Prenom;Rue;Numero;CodePostal;Ville;Telephone;Email;Station;MotDePasse;Specialite\n");

            Console.Write("Chemin du fichier : ");
            string chemin = Console.ReadLine() ?? "";

            if (!File.Exists(chemin))
            {
                Console.WriteLine("\n❌ Fichier non trouvé.");
                Console.ReadKey();
                return;
            }

            try
            {
                int importes = 0;
                int erreurs = 0;

                foreach (string ligne in File.ReadLines(chemin).Skip(1)) // Skip header
                {
                    string[] colonnes = ligne.Split(';');
                    if (colonnes.Length != 11) continue;

                    try
                    {
                        // Insérer l'utilisateur
                        string sqlUser = @"INSERT INTO Utilisateur 
                            (TypeClient, NomU, PrenomU, RueU, NumeroU, CodePostalU, VilleU, 
                            TelephoneU, EmailU, StationPlusProcheU, MDPU)
                            VALUES 
                            ('Particulier', @nom, @prenom, @rue, @numero, @codePostal, @ville,
                            @telephone, @email, @station, @mdp);
                            SELECT LAST_INSERT_ID();";

                        int clientId;
                        using (var cmd = new MySqlCommand(sqlUser, _db.GetConnection()))
                        {
                            cmd.Parameters.AddWithValue("@nom", colonnes[0]);
                            cmd.Parameters.AddWithValue("@prenom", colonnes[1]);
                            cmd.Parameters.AddWithValue("@rue", colonnes[2]);
                            cmd.Parameters.AddWithValue("@numero", int.Parse(colonnes[3]));
                            cmd.Parameters.AddWithValue("@codePostal", int.Parse(colonnes[4]));
                            cmd.Parameters.AddWithValue("@ville", colonnes[5]);
                            cmd.Parameters.AddWithValue("@telephone", colonnes[6]);
                            cmd.Parameters.AddWithValue("@email", colonnes[7]);
                            cmd.Parameters.AddWithValue("@station", colonnes[8]);
                            cmd.Parameters.AddWithValue("@mdp", colonnes[9]);

                            clientId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        // Insérer le cuisinier
                        string sqlCuisinier = @"INSERT INTO Cuisinier 
                            (ClientID, SpecialiteC, Note)
                            VALUES 
                            (@clientId, @specialite, 0)";

                        using (var cmd = new MySqlCommand(sqlCuisinier, _db.GetConnection()))
                        {
                            cmd.Parameters.AddWithValue("@clientId", clientId);
                            cmd.Parameters.AddWithValue("@specialite", colonnes[10]);
                            cmd.ExecuteNonQuery();
                        }

                        importes++;
                    }
                    catch
                    {
                        erreurs++;
                    }
                }

                Console.WriteLine($"\n✅ Import terminé : {importes} cuisiniers importés, {erreurs} erreurs.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Erreur lors de l'import : {ex.Message}");
            }

            Console.WriteLine("\nAppuyez sur une touche pour continuer...");
            Console.ReadKey();
        }

        private void AfficherTousLesCuisiniers()
        {
            Console.Clear();
            Console.WriteLine("=== LISTE DES CUISINIERS ===\n");

            string sql = @"SELECT 
                u.ClientID,
                u.NomU,
                u.PrenomU,
                u.EmailU,
                u.TelephoneU,
                u.VilleU,
                c.SpecialiteC,
                c.Note,
                c.NombreLivraisonsTotal,
                c.DateDebutActivite,
                (SELECT COUNT(DISTINCT p.PlatID) 
                 FROM Plat p 
                 WHERE p.CuisinierID = u.ClientID) as NombrePlats,
                (SELECT COUNT(DISTINCT b.CommandeID) 
                 FROM BonDeCommande_Liv b 
                 WHERE b.CuisinierID = u.ClientID) as NombreCommandes,
                (SELECT AVG(a.Note) 
                 FROM Avis a 
                 JOIN BonDeCommande_Liv b ON a.CommandeID = b.CommandeID 
                 WHERE b.CuisinierID = u.ClientID) as NoteMoyenne
                FROM Utilisateur u
                JOIN Cuisinier c ON u.ClientID = c.ClientID
                WHERE u.Actif = TRUE
                ORDER BY u.NomU, u.PrenomU";

            try
            {
                using (var cmd = new MySqlCommand(sql, _db.GetConnection()))
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        Console.WriteLine("Aucun cuisinier trouvé dans la base de données.");
                        Console.ReadKey();
                        return;
                    }

                    while (reader.Read())
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($"\n=== {reader["NomU"]} {reader["PrenomU"]} ===");
                        Console.ForegroundColor = ConsoleColor.White;
                        
                        Console.WriteLine($"ID: {reader["ClientID"]}");
                        Console.WriteLine($"Email: {reader["EmailU"]}");
                        Console.WriteLine($"Téléphone: {reader["TelephoneU"]}");
                        Console.WriteLine($"Ville: {reader["VilleU"]}");
                        Console.WriteLine($"Spécialité: {reader["SpecialiteC"]}");
                        
                        var noteMoyenne = reader["NoteMoyenne"] == DBNull.Value ? "N/A" : $"{Convert.ToDouble(reader["NoteMoyenne"]):F1}/5";
                        Console.WriteLine($"Note moyenne: {noteMoyenne}");
                        
                        Console.WriteLine($"Nombre de livraisons: {reader["NombreLivraisonsTotal"]}");
                        Console.WriteLine($"Nombre de plats proposés: {reader["NombrePlats"]}");
                        Console.WriteLine($"Nombre de commandes: {reader["NombreCommandes"]}");
                        
                        var dateDebut = Convert.ToDateTime(reader["DateDebutActivite"]);
                        var experience = (DateTime.Now - dateDebut).Days;
                        Console.WriteLine($"En activité depuis: {experience} jours ({dateDebut:dd/MM/yyyy})");
                        
                        Console.WriteLine(new string('-', 50));
                    }
                }

                Console.WriteLine("\nAppuyez sur une touche pour continuer...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Erreur lors de l'affichage des cuisiniers : {ex.Message}");
                Console.ReadKey();
            }
        }
    }
} 