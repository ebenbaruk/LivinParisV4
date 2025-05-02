using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using MySql.Data.MySqlClient;

namespace Rendu1.Modules
{
    public class ClientModule
    {
        private readonly DatabaseManager _db;

        public ClientModule(DatabaseManager db)
        {
            _db = db;
        }

        public void AfficherMenuClient()
        {
            bool continuer = true;
            while (continuer)
            {
                Console.Clear();
                Console.WriteLine("=== MENU CLIENT ===");
                Console.WriteLine("1. Ajouter un nouveau client");
                Console.WriteLine("2. Modifier un client");
                Console.WriteLine("3. Supprimer un client");
                Console.WriteLine("4. Afficher les clients par ordre alphabétique");
                Console.WriteLine("5. Afficher les clients par rue");
                Console.WriteLine("6. Afficher les clients par montant d'achats");
                Console.WriteLine("7. Importer des clients depuis un fichier");
                Console.WriteLine("0. Retour au menu principal");

                Console.Write("\nVotre choix : ");
                string? choix = Console.ReadLine();

                switch (choix)
                {
                    case "1":
                        AjouterClient();
                        break;
                    case "2":
                        ModifierClient();
                        break;
                    case "3":
                        SupprimerClient();
                        break;
                    case "4":
                        AfficherClientsParOrdreAlphabetique();
                        break;
                    case "5":
                        AfficherClientsParRue();
                        break;
                    case "6":
                        AfficherClientsParMontantAchats();
                        break;
                    case "7":
                        ImporterClientsDepuisFichier();
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

        private void AjouterClient()
        {
            Console.Clear();
            Console.WriteLine("=== AJOUT D'UN NOUVEAU CLIENT ===");

            Console.Write("Type de client (1: Particulier, 2: Entreprise) : ");
            string? typeClient = Console.ReadLine() == "1" ? "Particulier" : "Entreprise";

            string nomU, prenomU = "", nomEntreprise = "", nomReferent = "";
            
            if (typeClient == "Particulier")
            {
                Console.Write("Nom : ");
                nomU = Console.ReadLine() ?? "";
                Console.Write("Prénom : ");
                prenomU = Console.ReadLine() ?? "";
            }
            else
            {
                Console.Write("Nom de l'entreprise : ");
                nomEntreprise = Console.ReadLine() ?? "";
                Console.Write("Nom du référent : ");
                nomReferent = Console.ReadLine() ?? "";
                nomU = nomEntreprise;
            }

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

            string sql = @"INSERT INTO Utilisateur 
                (TypeClient, NomU, PrenomU, NomEntreprise, NomReferent, RueU, NumeroU, 
                CodePostalU, VilleU, TelephoneU, EmailU, StationPlusProcheU, MDPU)
                VALUES 
                (@type, @nom, @prenom, @nomEntreprise, @nomReferent, @rue, @numero,
                @codePostal, @ville, @telephone, @email, @station, @mdp)";

            try
            {
                using (var cmd = new MySqlCommand(sql, _db.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@type", typeClient);
                    cmd.Parameters.AddWithValue("@nom", nomU);
                    cmd.Parameters.AddWithValue("@prenom", prenomU);
                    cmd.Parameters.AddWithValue("@nomEntreprise", nomEntreprise);
                    cmd.Parameters.AddWithValue("@nomReferent", nomReferent);
                    cmd.Parameters.AddWithValue("@rue", rueU);
                    cmd.Parameters.AddWithValue("@numero", numeroU);
                    cmd.Parameters.AddWithValue("@codePostal", codePostalU);
                    cmd.Parameters.AddWithValue("@ville", villeU);
                    cmd.Parameters.AddWithValue("@telephone", telephoneU);
                    cmd.Parameters.AddWithValue("@email", emailU);
                    cmd.Parameters.AddWithValue("@station", stationPlusProcheU);
                    cmd.Parameters.AddWithValue("@mdp", mdpU);

                    cmd.ExecuteNonQuery();
                    Console.WriteLine("\n✅ Client ajouté avec succès !");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Erreur lors de l'ajout du client : {ex.Message}");
            }

            Console.WriteLine("\nAppuyez sur une touche pour continuer...");
            Console.ReadKey();
        }

        private void ModifierClient()
        {
            Console.Clear();
            Console.WriteLine("=== MODIFICATION D'UN CLIENT ===");

            Console.Write("Email du client à modifier : ");
            string email = Console.ReadLine() ?? "";

            // Vérifier si le client existe
            string checkSql = "SELECT * FROM Utilisateur WHERE EmailU = @email";
            try
            {
                using (var cmd = new MySqlCommand(checkSql, _db.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@email", email);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            Console.WriteLine("❌ Client non trouvé.");
                            Console.ReadKey();
                            return;
                        }
                    }
                }

                Console.WriteLine("\nNouvelles informations (laisser vide pour ne pas modifier) :");
                
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

                Console.Write("Station de métro la plus proche : ");
                string station = Console.ReadLine() ?? "";

                Console.Write("Nouveau mot de passe (laisser vide pour ne pas modifier) : ");
                string mdp = Console.ReadLine() ?? "";

                string updateSql = "UPDATE Utilisateur SET ";
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
                if (!string.IsNullOrWhiteSpace(station))
                {
                    updates.Add("StationPlusProcheU = @station");
                    parameters.Parameters.AddWithValue("@station", station);
                }
                if (!string.IsNullOrWhiteSpace(mdp))
                {
                    updates.Add("MDPU = @mdp");
                    parameters.Parameters.AddWithValue("@mdp", mdp);
                }

                if (updates.Count > 0)
                {
                    updateSql += string.Join(", ", updates);
                    updateSql += " WHERE EmailU = @email";
                    parameters.Parameters.AddWithValue("@email", email);

                    using (var cmd = new MySqlCommand(updateSql, _db.GetConnection()))
                    {
                        foreach (MySqlParameter param in parameters.Parameters)
                        {
                            cmd.Parameters.Add(param);
                        }
                        cmd.ExecuteNonQuery();
                        Console.WriteLine("\n✅ Client modifié avec succès !");
                    }
                }
                else
                {
                    Console.WriteLine("\nAucune modification effectuée.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Erreur lors de la modification : {ex.Message}");
            }

            Console.WriteLine("\nAppuyez sur une touche pour continuer...");
            Console.ReadKey();
        }

        private void SupprimerClient()
        {
            Console.Clear();
            Console.WriteLine("=== SUPPRESSION D'UN CLIENT ===");

            Console.Write("Email du client à supprimer : ");
            string email = Console.ReadLine() ?? "";

            try
            {
                // Suppression réelle du client
                string sql = "DELETE FROM Utilisateur WHERE EmailU = @email";
                using (var cmd = new MySqlCommand(sql, _db.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@email", email);
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                        Console.WriteLine("\n✅ Client supprimé avec succès !");
                    else
                        Console.WriteLine("\n❌ Client non trouvé.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Erreur lors de la suppression : {ex.Message}");
            }

            Console.WriteLine("\nAppuyez sur une touche pour continuer...");
            Console.ReadKey();
        }

        private void AfficherClientsParOrdreAlphabetique()
        {
            Console.Clear();
            Console.WriteLine("=== CLIENTS PAR ORDRE ALPHABÉTIQUE ===\n");

            string sql = @"SELECT 
                TypeClient,
                CASE 
                    WHEN TypeClient = 'Particulier' THEN CONCAT(NomU, ' ', PrenomU)
                    ELSE NomEntreprise
                END as Nom,
                EmailU,
                TelephoneU,
                CONCAT(NumeroU, ' ', RueU, ', ', CodePostalU, ' ', VilleU) as Adresse
                FROM Utilisateur
                WHERE Actif = TRUE
                ORDER BY Nom";

            AfficherResultatsClient(sql);
        }

        private void AfficherClientsParRue()
        {
            Console.Clear();
            Console.WriteLine("=== CLIENTS PAR RUE ===\n");

            string sql = @"SELECT 
                TypeClient,
                CASE 
                    WHEN TypeClient = 'Particulier' THEN CONCAT(NomU, ' ', PrenomU)
                    ELSE NomEntreprise
                END as Nom,
                CONCAT(NumeroU, ' ', RueU) as Adresse,
                CodePostalU,
                VilleU,
                EmailU,
                TelephoneU
                FROM Utilisateur
                WHERE Actif = TRUE
                ORDER BY RueU, NumeroU";

            AfficherResultatsClient(sql);
        }

        private void AfficherClientsParMontantAchats()
        {
            Console.Clear();
            Console.WriteLine("=== CLIENTS PAR MONTANT D'ACHATS ===\n");

            string sql = @"SELECT 
                u.TypeClient,
                CASE 
                    WHEN u.TypeClient = 'Particulier' THEN CONCAT(u.NomU, ' ', u.PrenomU)
                    ELSE u.NomEntreprise
                END as Nom,
                u.EmailU,
                COALESCE(SUM(b.PrixPaye), 0) as MontantTotal,
                COUNT(b.CommandeID) as NombreCommandes
                FROM Utilisateur u
                LEFT JOIN BonDeCommande_Liv b ON u.ClientID = b.ClientID
                WHERE u.Actif = TRUE
                GROUP BY u.ClientID
                ORDER BY MontantTotal DESC";

            AfficherResultatsClient(sql);
        }

        private void ImporterClientsDepuisFichier()
        {
            Console.Clear();
            Console.WriteLine("=== IMPORT DE CLIENTS DEPUIS UN FICHIER ===");
            Console.WriteLine("Le fichier doit être au format CSV avec les colonnes suivantes :");
            Console.WriteLine("TypeClient;Nom;Prenom;NomEntreprise;NomReferent;Rue;Numero;CodePostal;Ville;Telephone;Email;Station;MotDePasse\n");

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
                    if (colonnes.Length != 13) continue;

                    string sql = @"INSERT INTO Utilisateur 
                        (TypeClient, NomU, PrenomU, NomEntreprise, NomReferent, RueU, NumeroU, 
                        CodePostalU, VilleU, TelephoneU, EmailU, StationPlusProcheU, MDPU)
                        VALUES 
                        (@type, @nom, @prenom, @nomEntreprise, @nomReferent, @rue, @numero,
                        @codePostal, @ville, @telephone, @email, @station, @mdp)";

                    try
                    {
                        using (var cmd = new MySqlCommand(sql, _db.GetConnection()))
                        {
                            cmd.Parameters.AddWithValue("@type", colonnes[0]);
                            cmd.Parameters.AddWithValue("@nom", colonnes[1]);
                            cmd.Parameters.AddWithValue("@prenom", colonnes[2]);
                            cmd.Parameters.AddWithValue("@nomEntreprise", colonnes[3]);
                            cmd.Parameters.AddWithValue("@nomReferent", colonnes[4]);
                            cmd.Parameters.AddWithValue("@rue", colonnes[5]);
                            cmd.Parameters.AddWithValue("@numero", int.Parse(colonnes[6]));
                            cmd.Parameters.AddWithValue("@codePostal", int.Parse(colonnes[7]));
                            cmd.Parameters.AddWithValue("@ville", colonnes[8]);
                            cmd.Parameters.AddWithValue("@telephone", colonnes[9]);
                            cmd.Parameters.AddWithValue("@email", colonnes[10]);
                            cmd.Parameters.AddWithValue("@station", colonnes[11]);
                            cmd.Parameters.AddWithValue("@mdp", colonnes[12]);

                            cmd.ExecuteNonQuery();
                            importes++;
                        }
                    }
                    catch
                    {
                        erreurs++;
                    }
                }

                Console.WriteLine($"\n✅ Import terminé : {importes} clients importés, {erreurs} erreurs.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Erreur lors de l'import : {ex.Message}");
            }

            Console.WriteLine("\nAppuyez sur une touche pour continuer...");
            Console.ReadKey();
        }

        private void AfficherResultatsClient(string sql)
        {
            try
            {
                using (var cmd = new MySqlCommand(sql, _db.GetConnection()))
                using (var reader = cmd.ExecuteReader())
                {
                    // Afficher les en-têtes
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        Console.Write($"{reader.GetName(i),-20} ");
                    }
                    Console.WriteLine("\n" + new string('-', reader.FieldCount * 21));

                    // Afficher les données
                    while (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            Console.Write($"{reader[i],-20} ");
                        }
                        Console.WriteLine();
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

        public int CreerNouveauClient()
        {
            Console.Clear();
            Console.WriteLine("=== CRÉATION D'UN NOUVEAU CLIENT ===\n");

            try
            {
                // Type de client
                Console.WriteLine("Type de client :");
                Console.WriteLine("1. Particulier");
                Console.WriteLine("2. Entreprise");
                Console.Write("Votre choix (1 ou 2) : ");
                string typeChoix = Console.ReadLine() ?? "1";
                string typeClient = typeChoix == "2" ? "Entreprise" : "Particulier";

                string nomU = "", prenomU = "", nomEntreprise = "", nomReferent = "";
                if (typeClient == "Particulier")
                {
                    Console.Write("Nom : ");
                    nomU = Console.ReadLine() ?? "";
                    Console.Write("Prénom : ");
                    prenomU = Console.ReadLine() ?? "";
                }
                else
                {
                    Console.Write("Nom de l'entreprise : ");
                    nomEntreprise = Console.ReadLine() ?? "";
                    Console.Write("Nom du référent : ");
                    nomReferent = Console.ReadLine() ?? "";
                }

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

                string sql = @"INSERT INTO Utilisateur 
                    (TypeClient, NomU, PrenomU, NomEntreprise, NomReferent, RueU, NumeroU, 
                    CodePostalU, VilleU, TelephoneU, EmailU, StationPlusProcheU, MDPU)
                    VALUES 
                    (@type, @nom, @prenom, @entreprise, @referent, @rue, @numero, 
                    @codePostal, @ville, @telephone, @email, @station, @mdp);
                    SELECT LAST_INSERT_ID();";

                using (var cmd = new MySqlCommand(sql, _db.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@type", typeClient);
                    cmd.Parameters.AddWithValue("@nom", nomU);
                    cmd.Parameters.AddWithValue("@prenom", prenomU);
                    cmd.Parameters.AddWithValue("@entreprise", nomEntreprise);
                    cmd.Parameters.AddWithValue("@referent", nomReferent);
                    cmd.Parameters.AddWithValue("@rue", rueU);
                    cmd.Parameters.AddWithValue("@numero", numeroU);
                    cmd.Parameters.AddWithValue("@codePostal", codePostalU);
                    cmd.Parameters.AddWithValue("@ville", villeU);
                    cmd.Parameters.AddWithValue("@telephone", telephoneU);
                    cmd.Parameters.AddWithValue("@email", emailU);
                    cmd.Parameters.AddWithValue("@station", stationPlusProcheU);
                    cmd.Parameters.AddWithValue("@mdp", mdpU);

                    int clientId = Convert.ToInt32(cmd.ExecuteScalar());
                    Console.WriteLine("\n✅ Client créé avec succès !");
                    return clientId;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Erreur lors de la création du client : {ex.Message}");
                return -1;
            }
        }
    }
} 