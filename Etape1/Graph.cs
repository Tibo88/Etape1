using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace Etape1
{
    public class Graph<T>
    {
        public Dictionary<T, Noeud<T>> noeuds { get; }

        public bool EstOriente { get; }
        public Dictionary<T, List<T>> listeAdjacence;
        public int[,] matriceAdjacence;

        public Graph(bool estOriente = false)
        {
            noeuds = new Dictionary<T, Noeud<T>>();
            EstOriente = estOriente;
            listeAdjacence = new Dictionary<T, List<T>>();
        }

        public Dictionary<T, List<T>> ListeAdjacence
        {
            get { return listeAdjacence; }
        }

        /// <summary>
        /// ajoute un lien entre deux noeuds du graphe
        /// </summary>
        /// <param name="valeur1">Le premier nœud.</param>
        /// <param name="valeur2">Le second nœud.</param>
        public void AjouterLien(T valeur1, T valeur2)
        {
            if (!noeuds.ContainsKey(valeur1))
                noeuds[valeur1] = new Noeud<T>(valeur1);
            if (!noeuds.ContainsKey(valeur2))
                noeuds[valeur2] = new Noeud<T>(valeur2);

            var noeud1 = noeuds[valeur1];
            var noeud2 = noeuds[valeur2];

            if (noeud1 == null || noeud2 == null)
            {
                throw new InvalidOperationException("Erreur : ajout d'un noeud null");
            }

            var lien = new Lien<T>(noeud1, noeud2);

            if (noeud1.Liens == null)
                noeud1.Liens = new List<Lien<T>>();

            noeud1.Liens.Add(lien);

            if (!EstOriente)
            {
                if (noeud2.Liens == null)
                    noeud2.Liens = new List<Lien<T>>();

                noeud2.Liens.Add(lien);
            }
        }
        /// <summary>
        /// charge un graphe à partir d'un fichier texte contenant les arêtes
        /// </summary>
        /// <param name="nomFichier">Le chemin du fichier contenant les arêtes.</param>
        public void ChargerDepuisFichier(string nomFichier)
        {
            foreach (var ligne in File.ReadLines(nomFichier))
            {

                // Lit les valeurs
                var parties = ligne.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parties.Length == 2)
                {
                    try
                    {
                        T v1 = (T)Convert.ChangeType(parties[0], typeof(T));
                        T v2 = (T)Convert.ChangeType(parties[1], typeof(T));

                        AjouterLien(v1, v2);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Erreur de conversion pour la ligne : {ligne} ({e.Message})");
                    }
                }
            }
        }


        /// <summary>
        /// Crée la liste d'adjacence du graphe en fonction des liens entre les noeuds
        /// </summary>
        public void CreerListeAdjacence()
        {
            listeAdjacence.Clear();
            foreach (var noeud in noeuds.Values)
            {
                var voisins = new List<T>();
                foreach (var lien in noeud.Liens)
                {
                    T voisinValeur = lien.Source.Equals(noeud) ? lien.Destination.Nom : lien.Source.Nom;
                    voisins.Add(voisinValeur);
                }
                listeAdjacence[noeud.Nom] = voisins;
            }
        }

        /// <summary>
        /// créer la matrice d'adjacence du graphe à partir de la liste des liens
        /// </summary>
        public void CreerMatriceAdjacence()
        {
            int taille = 34;
            matriceAdjacence = new int[taille, taille];


            foreach (var noeud in noeuds.Values)
            {
                int i = Convert.ToInt32(noeud.Nom) - 1;

                foreach (var lien in noeud.Liens)
                {
                    int j = Convert.ToInt32(lien.Destination.Nom) - 1;

                    if (i != j)
                    {
                        matriceAdjacence[i, j] = 1;
                        if (!EstOriente)
                        {
                            matriceAdjacence[j, i] = 1;
                        }
                    }
                }
            }
            AfficherMatrice();
        }


        /// <summary>
        /// affiche la liste d'adjacence du graphe
        /// </summary>
        public void AfficherListe()
        {
            foreach (var pair in listeAdjacence)
            {
                Console.Write(pair.Key + " -> ");
                Console.WriteLine(string.Join(", ", pair.Value));
            }
        }
        /// <summary>
        /// Affiche la matrice d'adjacence du graphe
        /// </summary>
        public void AfficherMatrice()
        {
            for (int i = 0; i < matriceAdjacence.GetLength(0); i++)
            {
                for (int j = 0; j < matriceAdjacence.GetLength(1); j++)
                {
                    Console.Write(matriceAdjacence[i, j] + " ");
                }
                Console.WriteLine();
            }
        }
        /// <summary>
        /// effectue un parcours en profondeur/DFS du graphe
        /// </summary>
        /// <param name="start">le noeud de départ</param>
        /// <returns>une liste des noeuds visités</returns>
        public List<T> DFS(T start)
        {
            List<T> visited = new List<T>();
            Stack<T> pile = new Stack<T>();
            pile.Push(start);

            while (pile.Count > 0)
            {
                T courant = pile.Pop();
                if (!visited.Contains(courant))
                {
                    visited.Add(courant);
                    if (listeAdjacence.ContainsKey(courant))
                    {
                        foreach (T voisin in listeAdjacence[courant])
                        {
                            if (!visited.Contains(voisin))
                            {
                                pile.Push(voisin);
                            }
                        }
                    }
                }
            }
            return visited;
        }

        /// <summary>
        /// effectue un parcours en largeur/BFS du graphe
        /// </summary>
        /// <param name="depart">le noeud de départ</param>
        /// <returns>une liste des noeuds visités dans l'ordre du parcours</returns>
        public List<T> BFS(T depart)
        {
            List<T> visited = new List<T>();
            Queue<T> file = new Queue<T>();

            file.Enqueue(depart);
            visited.Add(depart);

            while (file.Count > 0)
            {
                T courant = file.Dequeue();
                if (listeAdjacence.ContainsKey(courant))
                {
                    foreach (T voisin in listeAdjacence[courant])
                    {
                        if (!visited.Contains(voisin))
                        {
                            file.Enqueue(voisin);
                            visited.Add(voisin);
                        }
                    }
                }
            }
            return visited;
        }

        /// <summary>
        /// vérifie si le graphe est connexe
        /// </summary>
        /// <returns>Retourne true si tous les noeuds sont connectés et sinon false</returns>
        public bool TestConnexe()
        {
            if (noeuds.Count == 0)
            {
                return false;
            }

            List<T> visited = new List<T>();
            List<T> aExplorer = new List<T>();

            T premierNoeud = noeuds.Keys.First();
            aExplorer.Add(premierNoeud);
            visited.Add(premierNoeud);

            while (aExplorer.Count > 0)
            {
                T courant = aExplorer[0];
                aExplorer.RemoveAt(0);

                if (listeAdjacence.ContainsKey(courant))
                {
                    foreach (T voisin in listeAdjacence[courant])
                    {
                        if (!visited.Contains(voisin))
                        {
                            visited.Add(voisin);
                            aExplorer.Add(voisin);
                        }
                    }
                }
            }

            return visited.Count == noeuds.Count;
        }

        /// <summary>
        /// vérifie si le graphe contient un cycle
        /// </summary>
        /// <returns>True si un cycle est détecté et sinon false</returns>
        public bool ContientCycle()
        {
            Dictionary<T, bool> visite = new Dictionary<T, bool>();
            Dictionary<T, T> parent = new Dictionary<T, T>();

            foreach (var noeud in noeuds.Keys)
            {
                visite[noeud] = false;
                parent[noeud] = default(T);
            }

            foreach (var noeud in noeuds.Keys)
            {
                if (!visite[noeud])
                {
                    if (DetecterCycle(noeud, visite, parent))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// Utilise le DFS pour détecter un cycle dans le graphe
        /// </summary>
        /// <param name="start">Le noeud de départ</param>
        /// <param name="visited">Dictionnaire des noeuds visités</param>
        /// <param name="parent">Dictionnaire des parents des noeuds</param>
        /// <returns>True si un cycle est détecté et sinon False</returns>
        private bool DetecterCycle(T start, Dictionary<T, bool> visited, Dictionary<T, T> parent)
        {
            List<T> parcours = DFS(start);

            foreach (T noeud in parcours)
            {
                visited[noeud] = true;
                if (!listeAdjacence.ContainsKey(noeud))
                    continue;

                foreach (T voisin in listeAdjacence[noeud])
                {
                    if (!visited[voisin])
                    {
                        parent[voisin] = noeud;
                    }
                    else if (!voisin.Equals(parent[noeud]))
                    {
                        return true;
                    }
                }
            }
            return false;
        }



        /// <summary>
        /// Génère une image du graphe et la sauvegarde dans un fichier
        /// </summary>
        /// <param name="nomFichier">Le chemin du fichier où enregistrer l'image.</param>
        public void ImageGraphe(string nomFichier)
        {
            int largeur = 800;
            int hauteur = 600;

            using (var bitmap = new SKBitmap(largeur, hauteur))
            using (var canvas = new SKCanvas(bitmap))
            {
                canvas.Clear(SKColors.White);
                var paintLien = new SKPaint { Color = SKColors.Black, StrokeWidth = 2, IsAntialias = true };
                var paintNoeud = new SKPaint { Color = SKColors.Red, IsAntialias = true };
                var paintTexte = new SKPaint { Color = SKColors.Black, TextSize = 20, IsAntialias = true };

                Dictionary<T, int> degres = noeuds.ToDictionary(n => n.Key, n => n.Value.Liens.Count);
                var noeudsTries = noeuds.Keys.OrderByDescending(n => degres[n]).ToList();
                Dictionary<T, SKPoint> positions = new Dictionary<T, SKPoint>();
                
                int centreX = largeur / 2;
                int centreY = hauteur / 2;
                int rayonMax = Math.Min(largeur, hauteur) / 3;
                int espaceMin = 80;

                for (int i = 0; i < noeudsTries.Count; i++)
                {
                    double angle = (2 * Math.PI * i) / noeudsTries.Count;
                    int rayon = rayonMax - (degres[noeudsTries[i]] * 10);
                    bool collision;
                    SKPoint tentativePosition;
                    int essais = 0;

                    do
                    {
                        tentativePosition = new SKPoint(
                            centreX + (float)(rayon * Math.Cos(angle)),
                            centreY + (float)(rayon * Math.Sin(angle))
                        );
                        collision = positions.Values.Any(p => SKPoint.Distance(p, tentativePosition) < espaceMin);
                        essais++;
                        rayon += 10;
                    } while (collision && essais < 10);

                    positions[noeudsTries[i]] = tentativePosition;
                }

                foreach (var noeud in noeuds.Values)
                {
                    foreach (var lien in noeud.Liens)
                    {
                        if (positions.ContainsKey(noeud.Nom) && positions.ContainsKey(lien.Destination.Nom))
                        {
                            canvas.DrawLine(positions[noeud.Nom], positions[lien.Destination.Nom], paintLien);
                        }
                    }
                }

                foreach (var noeud in noeuds.Values)
                {
                    if (positions.ContainsKey(noeud.Nom))
                    {
                        var position = positions[noeud.Nom];
                        canvas.DrawCircle(position, 20, paintNoeud);
                        canvas.DrawText(noeud.Nom.ToString(), position.X - 10, position.Y + 5, paintTexte);
                    }
                }

                using (var image = SKImage.FromBitmap(bitmap))
                using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                using (var stream = File.OpenWrite(nomFichier))
                {
                    data.SaveTo(stream);
                }
            }
        }
    }
}
