using System;
using System.Collections.Generic;
using System.IO;

namespace Etape1
{
    public class Graph<T>
    {
        public Dictionary<T, Noeud<T>> Noeuds { get; }
        public bool EstOriente { get; }
        public Dictionary<T, List<T>> ListeAdjacence { get; private set; }
        public double[,] MatriceAdjacence { get; private set; }

        public Graph(bool estOriente = false)
        {
            Noeuds = new Dictionary<T, Noeud<T>>();
            EstOriente = estOriente;
            ListeAdjacence = new Dictionary<T, List<T>>();
        }

        public void AjouterNoeud(T id, string libelle, string ligneLibelle, double longitude, double latitude, string commune, string codeCommune)
        {
            if (!Noeuds.ContainsKey(id))
            {
                Noeuds[id] = new Noeud<T>(id, libelle, ligneLibelle, longitude, latitude, commune, codeCommune);
            }
        }

        public void AjouterLien(T idStation1, T idStation2, double tempsTrajet, double tempsChangement, double distance)
        {
            // Vérifiez que les deux noeuds existent et qu'ils ne sont pas les mêmes
            if (!Noeuds.ContainsKey(idStation1) || !Noeuds.ContainsKey(idStation2) || idStation1.Equals(idStation2))
            {
                Console.WriteLine($"Les noeuds {idStation1} et/ou {idStation2} n'existent pas ou sont identiques.");
                return;
            }

            var source = Noeuds[idStation1];
            var destination = Noeuds[idStation2];

            // Vérifiez que le lien n'existe pas déjà
            if (source.Liens.Any(l => l.Destination.Id.Equals(destination.Id)))
            {
                Console.WriteLine($"Le lien de {idStation1} vers {idStation2} existe déjà.");
                return;
            }

            var lien = new Lien<T>(source, destination, tempsTrajet, tempsChangement, distance);

            source.Liens.Add(lien);
            if (!EstOriente)
            {
                // Vérifiez également que le lien inverse n'existe pas déjà
                if (!destination.Liens.Any(l => l.Destination.Id.Equals(source.Id)))
                {
                    destination.Liens.Add(lien);
                }
            }
        }
        public void ChargerDepuisFichier(string fichierNoeuds, string fichierArcs)
        {
            // Charger les noeuds
            Console.WriteLine("Chargement des noeuds...");
            foreach (var ligne in File.ReadLines(fichierNoeuds).Skip(1))
            {
                var parties = ligne.Split(',');
                if (parties.Length >= 7)
                {
                    try
                    {
                        T id = (T)Convert.ChangeType(parties[0].Trim(), typeof(T));
                        string nom = parties[2].Trim();
                        string ligneMetro = parties[1].Trim();
                        double longitude = Convert.ToDouble(parties[3].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                        double latitude = Convert.ToDouble(parties[4].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                        string commune = parties[5].Trim();
                        string codeInsee = parties[6].Trim();

                        AjouterNoeud(id, nom, ligneMetro, longitude, latitude, commune, codeInsee);
                        Console.WriteLine($"Noeud ajouté: {id}");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Erreur lors de la lecture du fichier de noeuds: {e.Message}, Ligne: {ligne}");
                    }
                }
                else
                {
                    Console.WriteLine($"Format incorrect pour la ligne: {ligne}");
                }
            }

            // Charger les arcs avec les poids (temps de trajet et de changement)
            Console.WriteLine("Chargement des arcs...");
            foreach (var ligne in File.ReadLines(fichierArcs).Skip(1))
            {
                var parties = ligne.Split(',');
                if (parties.Length >= 6)
                {
                    try
                    {
                        T noeudActuelId = (T)Convert.ChangeType(parties[0].Trim(), typeof(T));
                        T precedentId = (T)Convert.ChangeType(parties[2].Trim(), typeof(T));
                        T suivantId = (T)Convert.ChangeType(parties[3].Trim(), typeof(T));
                        double tempsTrajet = Convert.ToDouble(parties[4].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                        double tempsChangement = Convert.ToDouble(parties[5].Trim(), System.Globalization.CultureInfo.InvariantCulture);

                        // Ajouter un lien vers le précédent si ce n'est pas le même noeud et n'est pas 0
                        if (!precedentId.Equals(0) && !precedentId.Equals(noeudActuelId))
                        {
                            double distance = CalculerDistance(noeudActuelId, precedentId);
                            AjouterLien(noeudActuelId, precedentId, tempsTrajet, tempsChangement, distance);
                            Console.WriteLine($"Arc ajouté: {noeudActuelId} -> {precedentId}");
                        }

                        // Ajouter un lien vers le suivant si ce n'est pas le même noeud et n'est pas 0
                        if (!suivantId.Equals(0) && !suivantId.Equals(noeudActuelId))
                        {
                            double distance = CalculerDistance(noeudActuelId, suivantId);
                            AjouterLien(noeudActuelId, suivantId, tempsTrajet, tempsChangement, distance);
                            Console.WriteLine($"Arc ajouté: {noeudActuelId} -> {suivantId}");
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Erreur lors de la lecture du fichier des arcs: {e.Message}, Ligne: {ligne}");
                    }
                }
                else
                {
                    Console.WriteLine($"Format incorrect pour la ligne: {ligne}");
                }
            }
        }


        private double CalculerDistance(T id1, T id2)
        {
            var noeud1 = Noeuds[id1];
            var noeud2 = Noeuds[id2];

            double phi1 = noeud1.Latitude * Math.PI / 180;
            double phi2 = noeud2.Latitude * Math.PI / 180;
            double deltaPhi = (noeud2.Latitude - noeud1.Latitude) * Math.PI / 180;
            double deltaLambda = (noeud2.Longitude - noeud1.Longitude) * Math.PI / 180;

            double a = Math.Sin(deltaPhi / 2) * Math.Sin(deltaPhi / 2) +
                       Math.Cos(phi1) * Math.Cos(phi2) * Math.Sin(deltaLambda / 2) * Math.Sin(deltaLambda / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            const double R = 6371; // Rayon moyen de la Terre en km
            return R * c;
        }

        public void CreerListeAdjacence()
        {
            ListeAdjacence.Clear();

            foreach (var noeud in Noeuds.Values)
            {
                var voisins = new List<T>();

                foreach (var lien in noeud.Liens)
                {
                    // Ajouter uniquement les voisins qui ne sont pas le noeud lui-même
                    if (!lien.Destination.Id.Equals(noeud.Id))
                    {
                        voisins.Add(lien.Destination.Id);
                    }
                }

                ListeAdjacence[noeud.Id] = voisins;
            }
        }

        public void CreerMatriceAdjacence()
        {
            int taille = Noeuds.Count;
            MatriceAdjacence = new double[taille, taille];

            var idToIndex = new Dictionary<T, int>();
            int index = 0;
            foreach (var noeud in Noeuds.Values)
            {
                idToIndex[noeud.Id] = index++;
            }

            for (int i = 0; i < taille; i++)
            {
                for (int j = 0; j < taille; j++)
                {
                    MatriceAdjacence[i, j] = double.MaxValue;
                }
            }

            foreach (var noeud in Noeuds.Values)
            {
                int i = idToIndex[noeud.Id];

                foreach (var lien in noeud.Liens)
                {
                    int j = idToIndex[lien.Destination.Id];

                    MatriceAdjacence[i, j] = lien.TempsTrajet;
                    if (!EstOriente)
                    {
                        MatriceAdjacence[j, i] = lien.TempsTrajet;
                    }
                }
            }
        }

        public void AfficherListe()
        {
            foreach (var pair in ListeAdjacence)
            {
                Console.Write(pair.Key + " -> ");
                Console.WriteLine(string.Join(", ", pair.Value));
            }
        }

        public void AfficherMatrice()
        {
            for (int i = 0; i < MatriceAdjacence.GetLength(0); i++)
            {
                for (int j = 0; j < MatriceAdjacence.GetLength(1); j++)
                {
                    Console.Write(MatriceAdjacence[i, j] + " ");
                }
                Console.WriteLine();
            }
        }

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
                    if (ListeAdjacence.ContainsKey(courant))
                    {
                        foreach (T voisin in ListeAdjacence[courant])
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

        public List<T> BFS(T depart)
        {
            List<T> visited = new List<T>();
            Queue<T> file = new Queue<T>();

            file.Enqueue(depart);
            visited.Add(depart);

            while (file.Count > 0)
            {
                T courant = file.Dequeue();
                if (ListeAdjacence.ContainsKey(courant))
                {
                    foreach (T voisin in ListeAdjacence[courant])
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

        public bool TestConnexe()
        {
            if (Noeuds.Count == 0)
            {
                return false;
            }

            List<T> visited = new List<T>();
            List<T> aExplorer = new List<T>();

            T premierNoeud = Noeuds.Keys.First();
            aExplorer.Add(premierNoeud);
            visited.Add(premierNoeud);

            while (aExplorer.Count > 0)
            {
                T courant = aExplorer[0];
                aExplorer.RemoveAt(0);

                if (ListeAdjacence.ContainsKey(courant))
                {
                    foreach (T voisin in ListeAdjacence[courant])
                    {
                        if (!visited.Contains(voisin))
                        {
                            visited.Add(voisin);
                            aExplorer.Add(voisin);
                        }
                    }
                }
            }

            return visited.Count == Noeuds.Count;
        }

        public bool ContientCycle()
        {
            Dictionary<T, bool> visite = new Dictionary<T, bool>();
            Dictionary<T, T> parent = new Dictionary<T, T>();

            foreach (var noeud in Noeuds.Keys)
            {
                visite[noeud] = false;
                parent[noeud] = default(T);
            }

            foreach (var noeud in Noeuds.Keys)
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

        private bool DetecterCycle(T start, Dictionary<T, bool> visited, Dictionary<T, T> parent)
        {
            Stack<T> pile = new Stack<T>();
            pile.Push(start);

            while (pile.Count > 0)
            {
                T courant = pile.Pop();
                if (!visited[courant])
                {
                    visited[courant] = true;
                    if (ListeAdjacence.ContainsKey(courant))
                    {
                        foreach (T voisin in ListeAdjacence[courant])
                        {
                            if (!visited[voisin])
                            {
                                parent[voisin] = courant;
                                pile.Push(voisin);
                            }
                            else if (!voisin.Equals(parent[courant]))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
    }
}

