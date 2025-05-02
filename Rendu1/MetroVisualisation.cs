using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Rendu1
{
    public class MetroVisualisation
    {
        private readonly Graphe<Station> graphe;
        private readonly Dictionary<int, SKPoint> positionsStations;
        private readonly Dictionary<string, SKColor> couleursLignes;
        
        // Constantes pour le dessin
        private const int LARGEUR = 1600;
        private const int HAUTEUR = 1200;
        private const int RAYON_STATION = 6;
        private const int RAYON_STATION_ITINERAIRE = 8;
        private const int EPAISSEUR_LIGNE = 2;
        private const int EPAISSEUR_ITINERAIRE = 4;
        private const int MARGE = 50;
        
        public MetroVisualisation(Graphe<Station> graphe)
        {
            this.graphe = graphe ?? throw new ArgumentNullException(nameof(graphe));
            this.positionsStations = new Dictionary<int, SKPoint>();
            this.couleursLignes = new Dictionary<string, SKColor>();
            InitialiserCouleursLignes();
            CalculerPositionsStations();
        }
        
        private void InitialiserCouleursLignes()
        {
            couleursLignes.Clear();
            couleursLignes.Add("1", new SKColor(255, 206, 0));      // Jaune
            couleursLignes.Add("2", new SKColor(0, 0, 255));        // Bleu
            couleursLignes.Add("3", new SKColor(149, 179, 64));     // Vert olive
            couleursLignes.Add("3bis", new SKColor(134, 204, 206)); // Bleu clair
            couleursLignes.Add("4", new SKColor(187, 76, 158));     // Violet
            couleursLignes.Add("5", new SKColor(255, 137, 0));      // Orange
            couleursLignes.Add("6", new SKColor(118, 188, 33));     // Vert lime
            couleursLignes.Add("7", new SKColor(255, 170, 213));    // Rose
            couleursLignes.Add("7bis", new SKColor(122, 211, 193)); // Turquoise
            couleursLignes.Add("8", new SKColor(155, 105, 203));    // Mauve
            couleursLignes.Add("9", new SKColor(168, 219, 168));    // Vert mint
            couleursLignes.Add("10", new SKColor(226, 156, 0));     // Jaune foncé
            couleursLignes.Add("11", new SKColor(128, 63, 33));     // Brun
            couleursLignes.Add("12", new SKColor(0, 154, 73));      // Vert
            couleursLignes.Add("13", new SKColor(137, 207, 227));   // Bleu ciel
            couleursLignes.Add("14", new SKColor(100, 25, 115));    // Violet foncé
        }
        
        private void CalculerPositionsStations()
        {
            positionsStations.Clear();
            
            int stationsAvecCoordsValides = 0;
            int stationsTotales = graphe.Noeuds.Count;
            
            // Vérifier combien de stations ont des coordonnées valides
            foreach (var noeud in graphe.Noeuds)
            {
                var station = (Station)noeud.Donnees;
                if (station.Latitude != 0 && station.Longitude != 0)
                    stationsAvecCoordsValides++;
            }
            
            if (stationsAvecCoordsValides == 0)
            {
                // Générer des positions aléatoires pour éviter les erreurs
                GenererPositionsAleatoires();
                return;
            }
            
            // Calculer les positions des stations en fonction de leurs coordonnées GPS
            double minLat = double.MaxValue;
            double maxLat = double.MinValue;
            double minLong = double.MaxValue;
            double maxLong = double.MinValue;
            
            // Trouver les min/max en ignorant les valeurs nulles
            foreach (var noeud in graphe.Noeuds)
            {
                var station = (Station)noeud.Donnees;
                if (station.Latitude != 0)
                {
                    minLat = Math.Min(minLat, station.Latitude);
                    maxLat = Math.Max(maxLat, station.Latitude);
                }
                
                if (station.Longitude != 0)
                {
                    minLong = Math.Min(minLong, station.Longitude);
                    maxLong = Math.Max(maxLong, station.Longitude);
                }
            }
            
            // Calculer l'échelle
            double echelleLat = (HAUTEUR - 2 * MARGE) / (maxLat - minLat);
            double echelleLong = (LARGEUR - 2 * MARGE) / (maxLong - minLong);
            
            foreach (var noeud in graphe.Noeuds)
            {
                var station = (Station)noeud.Donnees;
                float x, y;
                
                if (station.Latitude != 0 && station.Longitude != 0)
                {
                    // Coordonnées réelles
                    x = (float)((station.Longitude - minLong) * echelleLong + MARGE);
                    // Inverser l'axe y car les coordonnées GPS sont inversées par rapport à l'écran
                    y = (float)(HAUTEUR - ((station.Latitude - minLat) * echelleLat + MARGE));
                }
                else
                {
                    // Coordonnées aléatoires pour les stations sans coordonnées valides
                    Random rand = new Random(noeud.Id);
                    x = (float)(rand.NextDouble() * (LARGEUR - 2 * MARGE) + MARGE);
                    y = (float)(rand.NextDouble() * (HAUTEUR - 2 * MARGE) + MARGE);
                }
                
                positionsStations[noeud.Id] = new SKPoint(x, y);
            }
        }
        
        /// <summary>
        /// Génère des positions aléatoires pour toutes les stations 
        /// (utilisé en cas d'erreur avec les coordonnées réelles)
        /// </summary>
        private void GenererPositionsAleatoires()
        {
            Random rand = new Random(42); // Seed fixe pour la reproductibilité
            
            foreach (var noeud in graphe.Noeuds)
            {
                float x = (float)(rand.NextDouble() * (LARGEUR - 2 * MARGE) + MARGE);
                float y = (float)(rand.NextDouble() * (HAUTEUR - 2 * MARGE) + MARGE);
                positionsStations[noeud.Id] = new SKPoint(x, y);
            }
        }
        
        public void Dessiner(string cheminFichier, List<int>? itineraire = null)
        {
            using (var surface = SKSurface.Create(new SKImageInfo(LARGEUR, HAUTEUR)))
            {
                var canvas = surface.Canvas;
                canvas.Clear(SKColors.White);
                
                // Dessiner toutes les lignes du métro
                DessinerLignesMetro(canvas);
                
                // Dessiner toutes les stations
                DessinerStations(canvas, itineraire);
                
                // Dessiner l'itinéraire s'il est fourni
                if (itineraire != null && itineraire.Count > 0)
                {
                    DessinerItineraire(canvas, itineraire);
                }
                
                // Dessiner la légende
                DessinerLegende(canvas);
                
                // Sauvegarder l'image
                using (var image = surface.Snapshot())
                using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                using (var stream = File.OpenWrite(cheminFichier))
                {
                    data.SaveTo(stream);
                }
            }
        }
        
        private void DessinerLignesMetro(SKCanvas canvas)
        {
            // Regrouper les lignes par numéro de ligne
            var lignesParNumero = graphe.Liens.GroupBy(l => ((Station)l.Source.Donnees).NomLigne);
            
            foreach (var ligneSerie in lignesParNumero)
            {
                string nomLigne = ligneSerie.Key;
                
                // Obtenir la couleur de la ligne (ou une couleur par défaut)
                SKColor couleur = couleursLignes.ContainsKey(nomLigne) 
                    ? couleursLignes[nomLigne] 
                    : new SKColor(100, 100, 100);
                
                using (var paint = new SKPaint
                {
                    Color = couleur,
                    StrokeWidth = EPAISSEUR_LIGNE,
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke
                })
                {
                    foreach (var lien in ligneSerie)
                    {
                        // Ne dessiner que les liens entre stations de la même ligne
                        var stationSource = (Station)lien.Source.Donnees;
                        var stationDest = (Station)lien.Destination.Donnees;
                        
                        if (stationSource.NomLigne == stationDest.NomLigne)
                        {
                            SKPoint debut = positionsStations[lien.Source.Id];
                            SKPoint fin = positionsStations[lien.Destination.Id];
                            canvas.DrawLine(debut, fin, paint);
                        }
                    }
                }
            }
        }
        
        private void DessinerStations(SKCanvas canvas, List<int>? itineraire)
        {
            using (var paintStationNormale = new SKPaint
            {
                Color = SKColors.White,
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            })
            using (var paintStationContour = new SKPaint
            {
                Color = SKColors.Black,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1
            })
            using (var paintStationItineraire = new SKPaint
            {
                Color = SKColors.Yellow,
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            })
            {
                foreach (var noeud in graphe.Noeuds)
                {
                    SKPoint position = positionsStations[noeud.Id];
                    
                    // Vérifier si la station fait partie de l'itinéraire
                    bool estDansItineraire = itineraire != null && itineraire.Contains(noeud.Id);
                    
                    if (estDansItineraire)
                    {
                        canvas.DrawCircle(position, RAYON_STATION_ITINERAIRE, paintStationItineraire);
                        canvas.DrawCircle(position, RAYON_STATION_ITINERAIRE, paintStationContour);
                    }
                    else
                    {
                        canvas.DrawCircle(position, RAYON_STATION, paintStationNormale);
                        canvas.DrawCircle(position, RAYON_STATION, paintStationContour);
                    }
                }
            }
        }
        
        private void DessinerItineraire(SKCanvas canvas, List<int> itineraire)
        {
            if (itineraire.Count < 2) return;
            
            using (var paint = new SKPaint
            {
                Color = SKColors.Red,
                StrokeWidth = EPAISSEUR_ITINERAIRE,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            })
            {
                for (int i = 0; i < itineraire.Count - 1; i++)
                {
                    int idSource = itineraire[i];
                    int idDest = itineraire[i + 1];
                    
                    if (positionsStations.ContainsKey(idSource) && positionsStations.ContainsKey(idDest))
                    {
                        SKPoint debut = positionsStations[idSource];
                        SKPoint fin = positionsStations[idDest];
                        canvas.DrawLine(debut, fin, paint);
                    }
                }
            }
            
            // Dessiner des points pour le départ et l'arrivée
            using (var paintDepart = new SKPaint
            {
                Color = SKColors.Green,
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            })
            using (var paintArrivee = new SKPaint
            {
                Color = SKColors.Red,
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            })
            using (var paintContour = new SKPaint
            {
                Color = SKColors.Black,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1
            })
            {
                // Départ
                SKPoint positionDepart = positionsStations[itineraire.First()];
                canvas.DrawCircle(positionDepart, RAYON_STATION_ITINERAIRE + 2, paintDepart);
                canvas.DrawCircle(positionDepart, RAYON_STATION_ITINERAIRE + 2, paintContour);
                
                // Arrivée
                SKPoint positionArrivee = positionsStations[itineraire.Last()];
                canvas.DrawCircle(positionArrivee, RAYON_STATION_ITINERAIRE + 2, paintArrivee);
                canvas.DrawCircle(positionArrivee, RAYON_STATION_ITINERAIRE + 2, paintContour);
            }
        }
        
        private void DessinerLegende(SKCanvas canvas)
        {
            int posY = 30;
            int posX = 30;
            int largeurCarre = 15;
            int espacementY = 20;
            
            using (var paintTexte = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = 14,
                IsAntialias = true
            })
            {
                // Titre de la légende
                canvas.DrawText("Légende des lignes de métro", posX, posY, paintTexte);
                posY += 25;
                
                // Dessiner chaque ligne avec sa couleur
                foreach (var ligne in couleursLignes.OrderBy(l => l.Key))
                {
                    using (var paintLigne = new SKPaint
                    {
                        Color = ligne.Value,
                        Style = SKPaintStyle.Fill
                    })
                    {
                        canvas.DrawRect(new SKRect(posX, posY - largeurCarre + 3, posX + largeurCarre, posY + 3), paintLigne);
                        canvas.DrawText($"Ligne {ligne.Key}", posX + largeurCarre + 10, posY, paintTexte);
                        posY += espacementY;
                    }
                }
                
                // Légende pour l'itinéraire
                posY += 10;
                using (var paintItineraire = new SKPaint
                {
                    Color = SKColors.Red,
                    Style = SKPaintStyle.Fill
                })
                {
                    canvas.DrawRect(new SKRect(posX, posY - largeurCarre + 3, posX + largeurCarre, posY + 3), paintItineraire);
                    canvas.DrawText("Itinéraire", posX + largeurCarre + 10, posY, paintTexte);
                    posY += espacementY;
                }
                
                // Légende pour les stations de départ et d'arrivée
                using (var paintDepart = new SKPaint
                {
                    Color = SKColors.Green,
                    Style = SKPaintStyle.Fill
                })
                {
                    canvas.DrawCircle(posX + largeurCarre / 2, posY, largeurCarre / 2, paintDepart);
                    canvas.DrawText("Station de départ", posX + largeurCarre + 10, posY + 5, paintTexte);
                    posY += espacementY;
                }
                
                using (var paintArrivee = new SKPaint
                {
                    Color = SKColors.Red,
                    Style = SKPaintStyle.Fill
                })
                {
                    canvas.DrawCircle(posX + largeurCarre / 2, posY, largeurCarre / 2, paintArrivee);
                    canvas.DrawText("Station d'arrivée", posX + largeurCarre + 10, posY + 5, paintTexte);
                }
            }
        }
    }
} 