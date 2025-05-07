using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Etape2
{
    public class GraphForm
    {
        /// <summary>
        /// Génère un plan du métro à partir des données de graphes et l'enregistre dans un fichier image.
        /// </summary>
        /// <param name="graphe">Le graphe contenant les stations du métro et leurs liens.</param>
        /// <param name="nomFichier">Le nom du fichier dans lequel l'image sera sauvegardée.</param>
        /// <param name="chemin">La liste des stations représentant le chemin trouvé.</param>
        public static void GenererPlanDuMetro(Graph<int> graphe, string nomFichier, List<int> chemin = null)
        {
            // Taille de l'image
            int largeurImage = 1400;
            int hauteurImage = 1200;

            // Créer une image vide
            using (var surface = SKSurface.Create(new SKImageInfo(largeurImage, hauteurImage)))
            {
                var canvas = surface.Canvas;
                canvas.Clear(SKColors.White); // Fond blanc

                // Calculer l'échelle pour convertir les coordonnées de la station en pixels
                double minLongitude = double.MaxValue;
                double maxLongitude = double.MinValue;
                double minLatitude = double.MaxValue;
                double maxLatitude = double.MinValue;

                foreach (var noeud in graphe.Noeuds.Values)
                {
                    if (noeud.Longitude < minLongitude)
                        minLongitude = noeud.Longitude;
                    if (noeud.Longitude > maxLongitude)
                        maxLongitude = noeud.Longitude;
                    if (noeud.Latitude < minLatitude)
                        minLatitude = noeud.Latitude;
                    if (noeud.Latitude > maxLatitude)
                        maxLatitude = noeud.Latitude;
                }


                double echelleLongitude = largeurImage / (maxLongitude - minLongitude);
                double echelleLatitude = hauteurImage / (maxLatitude - minLatitude);

                // Dessiner les liens entre les stations
                foreach (var noeud in graphe.Noeuds.Values)
                {
                    // Inverser la latitude pour que les stations plus au nord apparaissent plus bas
                    int x1 = (int)((noeud.Longitude - minLongitude) * echelleLongitude);
                    int y1 = (int)((maxLatitude - noeud.Latitude) * echelleLatitude);  // Inversion ici

                    // Dessiner les voisins
                    foreach (var lien in noeud.Liens)
                    {
                        Noeud<int> voisin = lien.Destination;
                        int x2 = (int)((voisin.Longitude - minLongitude) * echelleLongitude);
                        int y2 = (int)((maxLatitude - voisin.Latitude) * echelleLatitude);  // Inversion ici

                        // Dessiner une ligne entre les stations
                        var paint = new SKPaint
                        {
                            Color = SKColors.Black,
                            StrokeWidth = 2,
                            IsAntialias = true
                        };

                        canvas.DrawLine(x1, y1, x2, y2, paint);
                    }
                }

                // Dessiner les stations
                foreach (var noeud in graphe.Noeuds.Values)
                {
                    // Inverser la latitude pour que les stations plus au nord apparaissent plus bas
                    int x = (int)((noeud.Longitude - minLongitude) * echelleLongitude);
                    int y = (int)((maxLatitude - noeud.Latitude) * echelleLatitude);  // Inversion ici

                    // Dessiner un cercle représentant la station
                    var paint = new SKPaint
                    {
                        Color = SKColors.Red,
                        IsAntialias = true
                    };
                    canvas.DrawCircle(x, y, 5, paint);

                    // Dessiner le nom de la station à côté
                    var textPaint = new SKPaint
                    {
                        Color = SKColors.Black,
                        TextSize = 14,
                        IsAntialias = true
                    };

                    canvas.DrawText(noeud.Nom, x + 5, y - 5, textPaint);
                }

                // Dessiner le chemin trouvé
                if (chemin != null && chemin.Count > 1)
                {
                    var cheminPaint = new SKPaint
                    {
                        Color = SKColors.Blue,
                        StrokeWidth = 4,
                        IsAntialias = true
                    };

                    for (int i = 0; i < chemin.Count - 1; i++)
                    {
                        var noeud1 = graphe.Noeuds[chemin[i]];
                        var noeud2 = graphe.Noeuds[chemin[i + 1]];

                        int x1 = (int)((noeud1.Longitude - minLongitude) * echelleLongitude);
                        int y1 = (int)((maxLatitude - noeud1.Latitude) * echelleLatitude);
                        int x2 = (int)((noeud2.Longitude - minLongitude) * echelleLongitude);
                        int y2 = (int)((maxLatitude - noeud2.Latitude) * echelleLatitude);

                        canvas.DrawLine(x1, y1, x2, y2, cheminPaint);
                    }
                }

                // Sauvegarder l'image dans un fichier
                using (var image = surface.Snapshot())
                using (var data = image.Encode())
                using (var stream = File.OpenWrite(nomFichier))
                {
                    data.SaveTo(stream);
                    //Console.WriteLine("Le plan du métro a été sauvegardé dans : " + nomFichier);
                }
            }
        }
    }
}
