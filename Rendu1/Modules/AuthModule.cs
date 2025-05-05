using System;
using MySql.Data.MySqlClient;
using System.Windows.Forms;


namespace Rendu1.Modules
{
    /// <summary>
    /// Module d'authentification pour l'application
    /// Gère la connexion, la création de compte et l'authentification des utilisateurs
    /// </summary>
    public class AuthModule
    {
        private readonly DatabaseManager _db;
        public enum UserType { Admin, Client, Cuisinier }
        public class UserSession
        {
            public int UserId { get; set; }

            public string Email { get; set; }
            public string Nom { get; set; }
            public UserType Type { get; set; }




            public UserSession(int userId, string email, string nom, UserType type)
            {
                UserId = userId;
                Email = email;
                Nom = nom;
                Type = type;
            }
        }

        /// <summary>
        /// Constructeureé de la classe AuthModule
        /// </summary>
        public AuthModule(DatabaseManager db)
        {
            _db = db;
        }

        /// <summary>
        /// Affiche le menu de connexion et gère les choix de l'utilisateur,
        /// permet de se connecter, créer un compte client ou cuisinier,
        /// ou de quitter l'application
        /// 
        /// </summary>
        public UserSession? AfficherMenuConnexion()
        {

            while (true)
            {



                Console.Clear();
                Console.WriteLine("=== LIV'IN PARIS - CONNEXION ===\n");
                Console.WriteLine("1. Se connecter");
                Console.WriteLine("2. Créer un compte client");
                Console.WriteLine("3. Créer un compte cuisinier");
                Console.WriteLine("4. Mode administrateur");
                Console.WriteLine("0. Quitter");

                Console.Write("\nVotre choix : ");
                string? choix = Console.ReadLine();

                switch (choix)
                {
                    case "1":
                        var session = Connexion();
                        if (session != null) return session;
                        break;
                    case "2":
                        CreerCompteClient();
                        break;


                    case "3":
                        CreerCompteCuisinier();
                        break;



                    case "4":
                        return new UserSession(0, "admin", "Administrateur", UserType.Admin);
                    case "0":
                        return null;

                    default:
                        Console.WriteLine("Choix invalide. Appuyez sur une touche pour continuer...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        /// <summary>
        /// Gère la connexion de l'utilisateur, vérifie son emaills et son mot de passe dans la base de données
        /// </summary>
        private UserSession? Connexion()
        {
            Console.Clear();
            Console.WriteLine("=== CONNEXION ===\n");

            Console.Write("Email : ");
            string email = Console.ReadLine() ?? "";
            Console.Write("Mot de passe : ");
            string mdp = Console.ReadLine() ?? "";

            /// Requête pour récupérer les informations de l'utilisateur et son type. 
            
            string sql = @"
                SELECT 
                    u.ClientID,
                    u.EmailU,
                    CONCAT(u.NomU, ' ', COALESCE(u.PrenomU, '')) as Nom,
                    CASE 
                        WHEN c.ClientID IS NOT NULL THEN 'Cuisinier'
                        ELSE 'Client'
                    END as Type
                FROM Utilisateur u
                LEFT JOIN Cuisinier c ON u.ClientID = c.ClientID
                WHERE u.EmailU = @email AND u.MDPU = @mdp";

            try
            {
                using var cmd = new MySqlCommand(sql, _db.GetConnection());
                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@mdp", mdp);

                using var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    int userId = Convert.ToInt32(reader["ClientID"]);
                    string nom = reader["Nom"].ToString() ?? "";

                    bool estCuisinier = reader["Type"].ToString() == "Cuisinier";

                    if (estCuisinier)
                    {
                        Console.Clear();
                        Console.WriteLine($"=== BIENVENUE {nom} ===\n");
                        Console.WriteLine("Vous êtes enregistré comme cuisinier.");
                        Console.WriteLine("Comment souhaitez-vous vous connecter ?\n");


                        Console.WriteLine("1. Mode Client (pour commander des plats)");
                        Console.WriteLine("2. Mode Cuisinier (pour gérer vos plats)");
                        Console.Write("\nVotre choix : ");

                        string? choix = Console.ReadLine();
                        return choix switch
                        {
                            "1" => new UserSession(userId, email, nom, UserType.Client),
                            "2" => new UserSession(userId, email, nom, UserType.Cuisinier),
                            _ => null
                        };
                    }


                    else
                    {
                        return new UserSession(userId, email, nom, UserType.Client);
                    }
                }
                else
                {
                    Console.WriteLine("\n Email ou mot de passe incorrect.");

                    Console.ReadKey();


                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n Erreur : {ex.Message}");
                Console.ReadKey();

                return null;
            }
        }

        /// <summary>
        /// Crée un compte client, ajoute les informations dans la base de données,
        /// et affiche un message de succès
        /// </summary>
        private void CreerCompteClient()
        {
            Console.Clear();

            Console.WriteLine("=== CRÉATION DE COMPTE CLIENT ===\n");

            try
            {
                Console.Write("Nom : ");
                string nom = Console.ReadLine() ?? "";
                Console.Write("Prénom : ");

                string prenom = Console.ReadLine() ?? "";

                Console.Write("Email : ");
                string email = Console.ReadLine() ?? "";


                Console.Write("Mot de passe : ");
                string mdp = Console.ReadLine() ?? "";
                Console.Write("Téléphone : ");
                string telephone = Console.ReadLine() ?? "";
                Console.Write("Station de métro la plus proche : ");
                string station = Console.ReadLine() ?? "";

                /// Requête pour insérer un nouvel utilisateur dans la base de données.
                string sql = @"INSERT INTO Utilisateur 
                    (TypeClient, NomU, PrenomU, EmailU, MDPU, TelephoneU, StationPlusProcheU, 
                    RueU, NumeroU, CodePostalU, VilleU)
                    VALUES 
                    ('Particulier', @nom, @prenom, @email, @mdp, @telephone, @station,
                    'À compléter', 0, 75000, 'Paris')";

                using var cmd = new MySqlCommand(sql, _db.GetConnection());
                cmd.Parameters.AddWithValue("@nom", nom);
                cmd.Parameters.AddWithValue("@prenom", prenom);
                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@mdp", mdp);
                cmd.Parameters.AddWithValue("@telephone", telephone);
                cmd.Parameters.AddWithValue("@station", station);

                cmd.ExecuteNonQuery();
                Console.WriteLine("\n Compte créé avec succès ! Vous pouvez maintenant vous connecter.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n Erreur : {ex.Message}");
            }

            Console.ReadKey();
        }

        /// <summary>
        /// Crée un compte cuisinier
        /// </summary>
        private void CreerCompteCuisinier()
        {
            Console.Clear();
            Console.WriteLine("=== CRÉATION DE COMPTE CUISINIER ===\n");

            try
            {
                Console.Write("Nom : ");
                string nom = Console.ReadLine() ?? "";
                Console.Write("Prénom : ");
                string prenom = Console.ReadLine() ?? "";


                Console.Write("Email : ");
                string email = Console.ReadLine() ?? "";
                Console.Write("Mot de passe : ");

                string mdp = Console.ReadLine() ?? "";
                Console.Write("Téléphone : ");
                string telephone = Console.ReadLine() ?? "";
                Console.Write("Station de métro la plus proche : ");
                string station = Console.ReadLine() ?? "";
                Console.Write("Spécialité culinaire : ");

                string specialite = Console.ReadLine() ?? "";

                /// Transaction pour créer l'utilisateur et le cuisinier
                using var transaction = _db.GetConnection().BeginTransaction();
                try
                {
                    /// Création de l'utilisateur
                    string sqlUser = @"INSERT INTO Utilisateur 
                        (TypeClient, NomU, PrenomU, EmailU, MDPU, TelephoneU, StationPlusProcheU, 
                        RueU, NumeroU, CodePostalU, VilleU)
                        VALUES 
                        ('Particulier', @nom, @prenom, @email, @mdp, @telephone, @station,
                        'À compléter', 0, 75000, 'Paris');
                        SELECT LAST_INSERT_ID();";

                    int userId;
                    using (var cmd = new MySqlCommand(sqlUser, _db.GetConnection()))
                    {
                        cmd.Transaction = transaction;
                        cmd.Parameters.AddWithValue("@nom", nom);
                        cmd.Parameters.AddWithValue("@prenom", prenom);
                        cmd.Parameters.AddWithValue("@email", email);
                        cmd.Parameters.AddWithValue("@mdp", mdp);
                        cmd.Parameters.AddWithValue("@telephone", telephone);
                        cmd.Parameters.AddWithValue("@station", station);
                        userId = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    /// Création du cuisinier
                    string sqlCuisinier = @"INSERT INTO Cuisinier 
                        (ClientID, SpecialiteC, Note) 
                        VALUES 
                        (@userId, @specialite, 0)";

                    using (var cmd = new MySqlCommand(sqlCuisinier, _db.GetConnection()))
                    {
                        cmd.Transaction = transaction;
                        cmd.Parameters.AddWithValue("@userId", userId);
                        cmd.Parameters.AddWithValue("@specialite", specialite);
                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    Console.WriteLine("\n✅ Compte créé avec succès ! Vous pouvez maintenant vous connecter.");
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n Erreur : {ex.Message}");
            }

            Console.ReadKey();
        }
        /// <summary>
        /// Authentifie un utilisateur en vérifiant son email et son mot de passe dans la base de données
        /// </summary>
        public UserSession? AuthenticateUser(string email, string password)
        {
            /// Requête pour récupérer les informations de l'utilisateur et son type. 
            string sql = @"
                SELECT 
                    u.ClientID,
                    u.EmailU,
                    CONCAT(u.NomU, ' ', COALESCE(u.PrenomU, '')) as Nom,
                    CASE 
                        WHEN c.ClientID IS NOT NULL THEN 'Cuisinier'
                        ELSE 'Client'
                    END as Type
                FROM Utilisateur u
                LEFT JOIN Cuisinier c ON u.ClientID = c.ClientID
                WHERE u.EmailU = @email AND u.MDPU = @password";

            try
            {
                using var cmd = new MySqlCommand(sql, _db.GetConnection());
                cmd.Parameters.AddWithValue("@email", email);

                cmd.Parameters.AddWithValue("@password", password);
                using var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    int userId = Convert.ToInt32(reader["ClientID"]);
                    string nom = reader["Nom"].ToString() ?? "";

                    bool estCuisinier = reader["Type"].ToString() == "Cuisinier";

                    if (estCuisinier)
                    {
                        var choixForm = new ChoixModeForm(nom);
                        if (choixForm.ShowDialog() == DialogResult.OK && choixForm.SelectedMode.HasValue)
                        {
                            return new UserSession(userId, email, nom, choixForm.SelectedMode.Value);
                        }

                        
                        return null;
                    }
                    else
                    {
                        return new UserSession(userId, email, nom, UserType.Client);
                    }
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
} 