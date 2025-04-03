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
        private Dictionary<string, List<T>> StationsParNom { get; set; }

        public Graph(bool estOriente = false)
        {
            Noeuds = new Dictionary<T, Noeud<T>>();
            EstOriente = estOriente;
            ListeAdjacence = new Dictionary<T, List<T>>();
            StationsParNom = new Dictionary<string, List<T>>();
        }

        public void AjouterNoeud(T id, string libelle, string ligneLibelle, double longitude, double latitude, string commune, string codeCommune)
        {
            if (!Noeuds.ContainsKey(id))
            {
                Noeuds[id] = new Noeud<T>(id, libelle, ligneLibelle, longitude, latitude, commune, codeCommune);

                // Ajouter le noeud au groupe de noms
                if (!StationsParNom.ContainsKey(libelle))
                {
                    StationsParNom[libelle] = new List<T>();
                }
                StationsParNom[libelle].Add(id);
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

        public void AjouterLiensManquants()
        {
            foreach (var groupeNom in StationsParNom.Values)
            {
                // Créer des liens entre chaque paire de stations dans le groupe
                for (int i = 0; i < groupeNom.Count; i++)
                {
                    for (int j = i + 1; j < groupeNom.Count; j++)
                    {
                        var idStation1 = groupeNom[i];
                        var idStation2 = groupeNom[j];

                        // Ajouter un lien dans les deux sens si le graphe n'est pas orienté
                        double tempsTrajet = 0; // Vous pouvez définir un temps de trajet par défaut pour les changements de ligne
                        double tempsChangement = 5; // Exemple de temps de changement, à ajuster selon vos besoins
                        double distance = 0; // La distance peut être nulle ou une valeur par défaut pour les changements de ligne

                        AjouterLien(idStation1, idStation2, tempsTrajet, tempsChangement, distance);

                        if (!EstOriente)
                        {
                            AjouterLien(idStation2, idStation1, tempsTrajet, tempsChangement, distance);
                        }
                    }
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

            foreach (var groupeNom in StationsParNom.Values)
            {
                var voisinsCommuns = new HashSet<T>();

                foreach (var noeudId in groupeNom)
                {
                    var noeud = Noeuds[noeudId];
                    foreach (var lien in noeud.Liens)
                    {
                        // Ajouter uniquement les voisins qui ne sont pas le noeud lui-même et n'ont pas le même nom
                        if (!lien.Destination.Id.Equals(noeud.Id) && !noeud.Nom.Equals(lien.Destination.Nom))
                        {
                            voisinsCommuns.Add(lien.Destination.Id);
                        }
                    }
                }

                foreach (var noeudId in groupeNom)
                {
                    // Filtrer les voisins pour exclure ceux ayant le même nom que le noeud actuel
                    var voisinsFiltres = voisinsCommuns.Where(v => !Noeuds[v].Nom.Equals(Noeuds[noeudId].Nom)).ToList();
                    ListeAdjacence[noeudId] = voisinsFiltres;
                }
            }
        }



        

        public void AfficherListe()
        {
            Console.WriteLine("Liste d'adjacence :");

            // Trier les clés de la liste d'adjacence
            var keysSorted = ListeAdjacence.Keys.ToList();
            keysSorted.Sort();

            foreach (var key in keysSorted)
            {
                Console.Write(key + " -> ");

                // Trier les voisins avant de les afficher
                var voisinsSorted = ListeAdjacence[key].ToList();
                voisinsSorted.Sort();

                if (voisinsSorted.Count > 0)
                {
                    Console.WriteLine(string.Join(", ", voisinsSorted));
                }
                else
                {
                    Console.WriteLine("Aucun");
                }
            }
        }


        public List<T> Dijkstra(T start, T end)
        {
            var distances = new Dictionary<T, double>();
            var previousNodes = new Dictionary<T, T>();
            var priorityQueue = new SortedSet<(T Node, double Distance)>();

            foreach (var noeud in Noeuds.Keys)
            {
                distances[noeud] = double.MaxValue;
                previousNodes[noeud] = default(T);
                priorityQueue.Add((noeud, double.MaxValue));
            }

            distances[start] = 0;
            priorityQueue.Remove((start, double.MaxValue));
            priorityQueue.Add((start, 0));

            while (priorityQueue.Count > 0)
            {
                var (currentNode, currentDistance) = priorityQueue.First();
                priorityQueue.Remove((currentNode, currentDistance));

                if (currentNode.Equals(end))
                {
                    break; // Arrêter si le noeud de destination est atteint
                }

                if (currentDistance > distances[currentNode])
                {
                    continue; // Ignorer les anciennes distances
                }

                // Utiliser la liste d'adjacence pour trouver les voisins
                if (ListeAdjacence.ContainsKey(currentNode))
                {
                    foreach (var neighbor in ListeAdjacence[currentNode])
                    {
                        var newDistance = currentDistance + GetTempsTrajet(currentNode, neighbor);

                        if (newDistance < distances[neighbor])
                        {
                            distances[neighbor] = newDistance;
                            previousNodes[neighbor] = currentNode;
                            priorityQueue.Remove((neighbor, distances[neighbor]));
                            priorityQueue.Add((neighbor, newDistance));
                        }
                    }
                }
            }

            return ReconstructPath(previousNodes, start, end);
        }

        private List<T> ReconstructPath(Dictionary<T, T> previousNodes, T start, T end)
        {
            var path = new List<T>();
            var equalityComparer = EqualityComparer<T>.Default;

            for (var step = end; !equalityComparer.Equals(step, start); step = previousNodes.ContainsKey(step) ? previousNodes[step] : default(T))
            {
                if (!equalityComparer.Equals(step, default(T)))
                {
                    path.Add(step);
                }
                else
                {
                    Console.WriteLine($"Erreur : Chemin non trouvé pour le noeud {end}.");
                    return null;
                }
            }

            path.Add(start);
            path.Reverse();
            return path;
        }

        private double GetTempsTrajet(T depart, T arrivee)
        {
            var lien = Noeuds[depart].Liens.FirstOrDefault(l => l.Destination.Id.Equals(arrivee));
            return lien != null ? lien.TempsTrajet : double.MaxValue;
        }

        

        public double CalculerTempsTrajet(List<T> chemin)
        {
            double tempsTotal = 0;

            for (int i = 0; i < chemin.Count - 1; i++)
            {
                T depart = chemin[i];
                T arrivee = chemin[i + 1];

                // Trouver le lien entre les deux stations
                var lien = Noeuds[depart].Liens.FirstOrDefault(l => l.Destination.Id.Equals(arrivee));

                if (lien != null)
                {
                    tempsTotal += lien.TempsTrajet;
                }
                else
                {
                    Console.WriteLine($"Aucun lien trouvé entre {depart} et {arrivee}.");
                    return -1; // Retourner -1 ou une autre valeur d'erreur si un lien est manquant
                }
            }

            return tempsTotal;
        }



        public void AfficherChemin(List<T> chemin)
        {
            if (chemin == null || chemin.Count == 0)
            {
                Console.WriteLine("Aucun chemin trouvé.");
                return;
            }

            Console.WriteLine("Chemin trouvé :");
            foreach (var noeud in chemin)
            {
                Console.Write(noeud + " ");
            }
            Console.WriteLine();
        }

    }
}
