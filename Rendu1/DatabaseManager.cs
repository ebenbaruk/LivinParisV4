using System;
using System.Data;
using MySql.Data.MySqlClient;

namespace Rendu1
{
    /// <summary>
    /// Gestionnaire de base de données MySQL
    /// </summary>
    public class DatabaseManager
    {
        private readonly string _connectionString;
        private MySqlConnection _connection;

        public DatabaseManager(string server = "localhost", string database = "LivInParis", string user = "root", string password = "")
        {
            _connectionString = $"server={server};port=3306;database={database};user={user};password={password};";
        }

        /// <summary>
        /// Connexion à la base de données
        /// </summary>
        public bool Connect()
        {
            try
            {
                _connection = new MySqlConnection(_connectionString);
                _connection.Open();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✅ Connexion à la base de données réussie !");
                Console.ResetColor();
                return true;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(" Erreur de connexion : " + ex.Message);
                Console.ResetColor();
                return false;
            }
        }

        /// <summary>
        /// Déconnexion de la base de données
        /// </summary>
        public void Disconnect()
        {
            if (_connection != null && _connection.State == ConnectionState.Open)
            {
                _connection.Close();
                Console.WriteLine("🔌 Déconnexion de la base réussie.");
            }
        }

        /// <summary>
        /// Récupérer la connexion à la base de données
        /// </summary>
        public MySqlConnection GetConnection()
        {
            if (_connection == null || _connection.State != ConnectionState.Open)
            {
                Connect();
            }
            return _connection;
        }

        /// <summary>
        /// Exécuter une requête non-sélective
        /// </summary>
        public void ExecuteNonQuery(string sql)
        {
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, _connection))
                {
                    cmd.ExecuteNonQuery();
                    Console.WriteLine("✔️ Requête exécutée avec succès.");
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(" Erreur lors de l'exécution de la requête : " + ex.Message);
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Exécuter une requête sélective
        /// </summary>
        public void ExecuteQuery(string sql)
        {
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, _connection))
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    Console.WriteLine("📊 Résultat de la requête :");
                    while (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            Console.Write($"{reader.GetName(i)}: {reader[i]}  ");
                        }
                        Console.WriteLine();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(" Erreur lors de la lecture des résultats : " + ex.Message);
                Console.ResetColor();
            }
        }
    }
}
