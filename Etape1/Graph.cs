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

        public void AjouterLien(T idStation1, T idStation2, double tempsTrajet, double tempsChangement)
        {
            if (!Noeuds.ContainsKey(idStation1) || !Noeuds.ContainsKey(idStation2))
            {
                Console.WriteLine($"Les noeuds {idStation1} et/ou {idStation2} n'existent pas.");
                return;
            }

            var source = Noeuds[idStation1];
            var destination = Noeuds[idStation2];

            var lien = new Lien<T>(source, destination, tempsTrajet, tempsChangement);

            source.Liens.Add(lien);
            if (!EstOriente)
            {
                destination.Liens.Add(lien);
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
                        T station1Id = (T)Convert.ChangeType(parties[0].Trim(), typeof(T));
                        T station2Id = (T)Convert.ChangeType(parties[2].Trim(), typeof(T));
                        double tempsTrajet = Convert.ToDouble(parties[4].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                        double tempsChangement = Convert.ToDouble(parties[5].Trim(), System.Globalization.CultureInfo.InvariantCulture);

                        AjouterLien(station1Id, station2Id, tempsTrajet, tempsChangement);
                        Console.WriteLine($"Arc ajouté: {station1Id} -> {station2Id}");
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



        public void CreerListeAdjacence()
        {
            ListeAdjacence.Clear();

            foreach (var noeud in Noeuds.Values)
            {
                var voisins = new List<T>();

                foreach (var lien in noeud.Liens)
                {
                    voisins.Add(lien.Destination.Id);
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
