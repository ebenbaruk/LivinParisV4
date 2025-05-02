using System;
using System.Collections.Generic;

namespace Rendu1
{
    public class Station
    {
        // Numéro principal de la ligne (1, 2, 3, etc.)
        public int Ligne { get; private set; }
        
        // Nom complet de la ligne (par exemple "3bis")
        public string NomLigne { get; private set; }
        
        public string Nom { get; private set; }
        public string Coordonnees { get; private set; }
        public string ArretPrecedent { get; private set; }
        public string ArretSuivant { get; private set; }
        public string ChangementsPossibles { get; private set; }
        
        // Identifiant unique pour chaque station: ligne_nom
        public string Id { get; private set; }
        
        // Coordonnées géographiques parsées
        public double Latitude { get; private set; }
        public double Longitude { get; private set; }
        
        public string StationPrecedente { get; private set; }
        public string StationSuivante { get; private set; }
        public string Changements { get; private set; }
        
        public Station(int ligne, string nomLigne, string nom, string coordonnees, string stationPrecedente, string stationSuivante, string changements)
        {
            Ligne = ligne;
            NomLigne = nomLigne;
            Nom = nom;
            StationPrecedente = stationPrecedente;
            StationSuivante = stationSuivante;
            Changements = changements;
            
            // Parse des coordonnées
            string[] coords = coordonnees.Split(new[] { ", " }, StringSplitOptions.None);
            if (coords.Length == 2)
            {
                // Remplacer la virgule par un point pour les nombres décimaux
                string latStr = coords[0].Replace(',', '.');
                string lngStr = coords[1].Replace(',', '.');
                
                if (double.TryParse(latStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double lat))
                    Latitude = lat;
                else
                    throw new ArgumentException($"Impossible de parser la latitude pour la station {nom}: {coords[0]}");
                
                if (double.TryParse(lngStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double lng))
                    Longitude = lng;
                else
                    throw new ArgumentException($"Impossible de parser la longitude pour la station {nom}: {coords[1]}");
            }
            else
            {
                throw new ArgumentException($"Format de coordonnées invalide pour la station {nom}: {coordonnees}");
            }
            
            Id = $"{nomLigne}_{nom}";
        }
        
        private List<int> ParseChangements(string changements)
        {
            List<int> resultat = new List<int>();
            
            if (!string.IsNullOrEmpty(changements))
            {
                string[] lignes = changements.Split(',');
                foreach (string ligne in lignes)
                {
                    if (int.TryParse(ligne.Trim(), out int numeroLigne))
                    {
                        resultat.Add(numeroLigne);
                    }
                }
            }
            
            return resultat;
        }
        
        private void ParseCoordonnees(string coordonnees)
        {
            // Format attendu: "48.8826, 2.3260" ou "48,8826, 2,2358"
            if (!string.IsNullOrEmpty(coordonnees))
            {
                try
                {
                    // Séparer les coordonnées (le séparateur entre lat et long est toujours une virgule suivie d'un espace)
                    string[] coords = coordonnees.Split(new[] { ", " }, StringSplitOptions.None);
                    
                    if (coords.Length == 2)
                    {
                        // Remplacer la virgule par un point pour les nombres décimaux
                        string latStr = coords[0].Replace(',', '.');
                        string lngStr = coords[1].Replace(',', '.');
                        
                        if (double.TryParse(latStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double lat))
                            Latitude = lat;
                        else
                            Console.WriteLine($"Impossible de parser la latitude: {coords[0]}");
                        
                        if (double.TryParse(lngStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double lng))
                            Longitude = lng;
                        else
                            Console.WriteLine($"Impossible de parser la longitude: {coords[1]}");
                    }
                    else
                    {
                        Console.WriteLine($"Format de coordonnées invalide: {coordonnees}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur lors du parsing des coordonnées: {ex.Message}");
                }
            }
        }
        
        /// <summary>
        /// Calcule la distance en kilomètres entre deux stations en utilisant la formule de Haversine
        /// </summary>
        public static double CalculerDistance(Station station1, Station station2)
        {
            if (station1 == null || station2 == null)
                throw new ArgumentNullException(nameof(station1), "Les stations ne peuvent pas être null");

            const double RAYON_TERRE = 6371; // Rayon de la Terre en kilomètres

            // Conversion des degrés en radians
            double lat1 = station1.Latitude * Math.PI / 180;
            double lon1 = station1.Longitude * Math.PI / 180;
            double lat2 = station2.Latitude * Math.PI / 180;
            double lon2 = station2.Longitude * Math.PI / 180;

            // Différences de latitude et longitude
            double dLat = lat2 - lat1;
            double dLon = lon2 - lon1;

            // Formule de Haversine
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                      Math.Cos(lat1) * Math.Cos(lat2) *
                      Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            // Distance en kilomètres
            return RAYON_TERRE * c;
        }
        
        public override string ToString()
        {
            return $"{Nom} (Ligne {NomLigne})";
        }
    }
} 