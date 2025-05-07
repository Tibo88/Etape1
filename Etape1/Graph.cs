using Etape1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System.Text.Json;
using Etape1;

namespace Etape1
{
    public class Graph<T>
    {
        public Dictionary<T, Noeud<T>> Noeuds { get; set; }
        public bool EstOriente { get; set; }
        public Dictionary<T, List<T>> ListeAdjacence { get; set; }
        public double[,] MatriceAdjacence { get; set; }
        private Dictionary<string, List<T>> StationsParNom { get; set; }



        public Graph(bool estOriente = false)
        {
            Noeuds = new Dictionary<T, Noeud<T>>();
            EstOriente = estOriente;
            ListeAdjacence = new Dictionary<T, List<T>>();
            StationsParNom = new Dictionary<string, List<T>>();
        }
        /// <summary>
        /// Ajoute un nouveau noeud au graphe.
        /// </summary>
        /// <param name="id">L'ID unique du noeud.</param>
        /// <param name="libelle">Le nom du noeud.</param>
        /// <param name="ligneLibelle">Le libellé de la ligne de métro.</param>
        /// <param name="longitude">La longitude du noeud.</param>
        /// <param name="latitude">La latitude du noeud.</param>
        /// <param name="commune">La commune associée au noeud.</param>
        /// <param name="codeCommune">Le code de la commune.</param>
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


        /// <summary>
        /// Ajoute un lien entre deux stations (noeuds) avec les informations de temps de trajet et de changement.
        /// </summary>
        /// <param name="idStation1">L'ID de la première station (source).</param>
        /// <param name="idStation2">L'ID de la deuxième station (destination).</param>
        /// <param name="tempsTrajet">Le temps de trajet entre les deux stations.</param>
        /// <param name="tempsChangement">Le temps de changement entre les stations.</param>
        /// <param name="distance">La distance entre les stations.</param>
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
            bool lienExistant = false;
            foreach (var l in source.Liens)
            {
                if (l.Destination.Id.Equals(destination.Id))
                {
                    lienExistant = true;
                    break;
                }
            }

            if (lienExistant)
            {
                //Console.WriteLine($"Le lien de {idStation1} vers {idStation2} existe déjà.");
                return;
            }

            var lien = new Lien<T>(source, destination, tempsTrajet, tempsChangement, distance);

            source.Liens.Add(lien);
            if (!EstOriente)
            {
                bool lienInverseExistant = false;
                foreach (var l in destination.Liens)
                {
                    if (l.Destination.Id.Equals(source.Id))
                    {
                        lienInverseExistant = true;
                        break;
                    }
                }

                if (!lienInverseExistant)
                {
                    destination.Liens.Add(lien);
                }
            }

        }
        /// <summary>
        /// Charge les noeuds et les arcs depuis des fichiers CSV spécifiés.
        /// </summary>
        /// <param name="fichierNoeuds">Le chemin du fichier contenant les informations sur les noeuds.</param>
        /// <param name="fichierArcs">Le chemin du fichier contenant les informations sur les arcs.</param>
        public void ChargerDepuisFichier(string fichierNoeuds, string fichierArcs)
        {
            // Charger les noeuds
            //Console.WriteLine("Chargement des noeuds...");
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
                        // Console.WriteLine($"Noeud ajouté: {id}");
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
            //Console.WriteLine("Chargement des arcs...");
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
                            //Console.WriteLine($"Arc ajouté: {noeudActuelId} -> {precedentId}");
                        }

                        // Ajouter un lien vers le suivant si ce n'est pas le même noeud et n'est pas 0
                        if (!suivantId.Equals(0) && !suivantId.Equals(noeudActuelId))
                        {
                            double distance = CalculerDistance(noeudActuelId, suivantId);
                            AjouterLien(noeudActuelId, suivantId, tempsTrajet, tempsChangement, distance);
                            //Console.WriteLine($"Arc ajouté: {noeudActuelId} -> {suivantId}");
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
        /// <summary>
        /// Ajoute des liens manquants entre les stations qui partagent le même nom (ex. stations situées sur la même ligne de métro).
        /// Cette méthode crée des liens entre chaque paire de stations ayant le même nom (même ligne).
        /// </summary>
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


        /// <summary>
        /// Calcule la distance entre deux stations en utilisant la formule de Haversine.
        /// La formule de Haversine permet de calculer la distance entre deux points sur une sphère en fonction de leurs latitudes et longitudes.
        /// </summary>
        /// <param name="id1">L'ID du premier noeud (station).</param>
        /// <param name="id2">L'ID du deuxième noeud (station).</param>
        /// <returns>La distance en kilomètres entre les deux stations.</returns>

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
        /// <summary>
        /// Crée la liste d'adjacence à partir des liens entre les stations.
        /// La liste d'adjacence permet de représenter les voisins de chaque station dans le graphe.
        /// </summary>
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
                    List<T> voisinsFiltres = new List<T>();

                    foreach (var v in voisinsCommuns)
                    {
                        if (!Noeuds[v].Nom.Equals(Noeuds[noeudId].Nom))
                        {
                            voisinsFiltres.Add(v);
                        }
                    }

                    ListeAdjacence[noeudId] = voisinsFiltres;
                }

            }
        }



        /// <summary>
        /// Affiche la liste d'adjacence du graphe, qui montre les voisins de chaque station.
        /// </summary>

        public void AfficherListe()
        {
            //Console.WriteLine("Liste d'adjacence :");

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
        /// <summary>
        /// Applique l'algorithme de Dijkstra pour trouver le chemin le plus court entre deux stations.
        /// Cet algorithme trouve le plus court chemin dans un graphe pondéré à partir d'un noeud de départ vers un noeud de destination.
        /// </summary>
        /// <param name="start">L'ID de la station de départ.</param>
        /// <param name="end">L'ID de la station de destination.</param>
        /// <returns>La liste des stations représentant le chemin le plus court entre les deux stations.</returns>

        public List<T> Dijkstra(T start, T end)
        {
            var distances = new Dictionary<T, double>();
            var previousNodes = new Dictionary<T, T>();
            var priorityQueue = new SortedDictionary<double, List<T>>();

            // Initialisation
            foreach (var noeud in Noeuds.Keys)
            {
                distances[noeud] = double.MaxValue;  // Distance infinie par défaut
                previousNodes[noeud] = default(T);  // Pas de prédécesseur
            }

            distances[start] = 0;
            priorityQueue[0] = new List<T> { start };

            while (priorityQueue.Count > 0)
            {
                // Récupère le nœud avec la plus petite distance
                var currentDistance = priorityQueue.Keys.First();
                var currentNode = priorityQueue[currentDistance].First();
                priorityQueue[currentDistance].RemoveAt(0);

                if (priorityQueue[currentDistance].Count == 0)
                {
                    priorityQueue.Remove(currentDistance);
                }

                // Si on atteint la destination, on arrête
                if (currentNode.Equals(end))
                {
                    break;
                }

                // Exploration des voisins
                if (ListeAdjacence.ContainsKey(currentNode))
                {
                    foreach (var neighbor in ListeAdjacence[currentNode])
                    {
                        double tempsTrajet = GetTempsTrajet(currentNode, neighbor);
                        double newDistance = currentDistance + tempsTrajet;

                        // Vérifie si on a trouvé un chemin plus court
                        if (newDistance < distances[neighbor])
                        {
                            distances[neighbor] = newDistance;
                            previousNodes[neighbor] = currentNode;

                            if (!priorityQueue.ContainsKey(newDistance))
                            {
                                priorityQueue[newDistance] = new List<T>();
                            }
                            priorityQueue[newDistance].Add(neighbor);
                        }
                    }
                }
            }

            // Reconstruct le chemin
            return ReconstructPath(previousNodes, start, end);
        }



        /// <summary>
        /// Reconstruit le chemin le plus court entre deux stations à partir des prédécesseurs calculés par l'algorithme de Dijkstra.
        /// </summary>
        /// <param name="predecessors">Un dictionnaire associant chaque station à son prédécesseur dans le chemin le plus court.</param>
        /// <param name="start">La station de départ.</param>
        /// <param name="end">La station de destination.</param>
        /// <returns>La liste des stations représentant le chemin le plus court.</returns>

        private List<T> ReconstructPath(Dictionary<T, T> predecessors, T start, T end)
        {
            var path = new List<T>();
            var equalityComparer = EqualityComparer<T>.Default;

            for (var step = end; !equalityComparer.Equals(step, start); step = predecessors.ContainsKey(step) ? predecessors[step] : default(T))
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
        /// <summary>
        /// Récupère le temps de trajet entre deux stations à partir des liens existants dans le graphe.
        /// </summary>
        /// <param name="depart">L'ID de la station de départ.</param>
        /// <param name="arrivee">L'ID de la station d'arrivée.</param>
        /// <returns>Le temps de trajet entre les deux stations.</returns>
        private double GetTempsTrajet(T depart, T arrivee)
        {
            var lien = Noeuds[depart].Liens.FirstOrDefault(l => l.Destination.Id.Equals(arrivee));
            return lien != null ? lien.TempsTrajet : double.MaxValue;
        }


        /// <summary>
        /// Calcule le temps total du trajet en fonction des stations présentes dans le chemin.
        /// </summary>
        /// <param name="chemin">Le chemin sous forme de liste de stations (noeuds).</param>
        /// <returns>Le temps total du trajet.</returns>
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


        /// <summary>
        /// Affiche le chemin trouvé entre deux stations et son temps total de trajet.
        /// </summary>
        /// <param name="chemin">La liste des stations représentant le chemin.</param>
        public void AfficherChemin(List<T> chemin)
        {
            if (chemin == null || chemin.Count == 0)
            {
                Console.WriteLine("Aucun chemin trouvé.");
                return;
            }

            Console.WriteLine("Chemin trouvé :");
            Console.WriteLine(string.Join(" -> ", chemin));

            // Calculer et afficher le temps total du trajet
            double tempsTotal = CalculerTempsTrajet(chemin);
            if (tempsTotal != -1)
            {
                Console.WriteLine($"Temps total du trajet : {tempsTotal:F1}");
            }
            else
            {
                Console.WriteLine("Erreur dans le calcul du temps de trajet.");
            }
        }

        /// <summary>
        /// Applique l'algorithme de Floyd-Warshall pour calculer le plus court chemin entre toutes les paires de stations.
        /// </summary>

        public void FloydWarshall()
        {
            int n = Noeuds.Count;
            double[,] dist = new double[n, n];
            int[,] pred = new int[n, n];
            var idToIndex = new Dictionary<T, int>();
            var indexToId = new Dictionary<int, T>();
            int index = 0;

            // Associer chaque ID de sommet à un index numérique
            foreach (var noeud in Noeuds.Values)
            {
                idToIndex[noeud.Id] = index;
                indexToId[index] = noeud.Id;
                index++;
            }

            // Initialiser les matrices
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (i == j)
                    {
                        dist[i, j] = 0;
                        pred[i, j] = i; // Un sommet mène à lui-même
                    }
                    else
                    {
                        dist[i, j] = double.MaxValue;
                        pred[i, j] = -1; // Aucun prédécesseur initialement
                    }
                }
            }

            // Remplir les distances directes et les prédécesseurs
            foreach (var noeud in Noeuds.Values)
            {
                int i = idToIndex[noeud.Id];

                foreach (var lien in noeud.Liens)
                {
                    int j = idToIndex[lien.Destination.Id];
                    dist[i, j] = lien.TempsTrajet;
                    pred[i, j] = i; // Le prédécesseur de j est i
                }
            }

            // Algorithme de Floyd-Warshall
            for (int k = 0; k < n; k++)
            {
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (dist[i, k] != double.MaxValue && dist[k, j] != double.MaxValue &&
                            dist[i, k] + dist[k, j] < dist[i, j])
                        {
                            dist[i, j] = dist[i, k] + dist[k, j];
                            pred[i, j] = pred[k, j]; // Mettre à jour le prédécesseur
                        }
                    }
                }
            }

            // Sauvegarde des résultats pour une utilisation ultérieure
            this.Distances = dist;
            this.Predecesseurs = pred;
            this.IndexToId = indexToId;
            this.IdToIndex = idToIndex;
        }

        // Stockage des résultats après l'exécution de Floyd-Warshall
        private double[,] Distances;
        private int[,] Predecesseurs;
        private Dictionary<int, T> IndexToId;
        private Dictionary<T, int> IdToIndex;


        /// <summary>
        /// Affiche le chemin le plus court entre deux stations en utilisant les résultats de l'algorithme de Floyd-Warshall.
        /// Cette méthode récupère le chemin à partir des prédécesseurs calculés et affiche le trajet ainsi que le temps total.
        /// </summary>
        /// <param name="depart">L'ID de la station de départ.</param>
        /// <param name="arrivee">L'ID de la station d'arrivée.</param>
        public void AfficherCheminPlusCourt(T depart, T arrivee)
        {
            if (!IdToIndex.ContainsKey(depart) || !IdToIndex.ContainsKey(arrivee))
            {
                Console.WriteLine("Les sommets spécifiés n'existent pas.");
                return;
            }

            int start = IdToIndex[depart];
            int end = IdToIndex[arrivee];

            if (Distances[start, end] == double.MaxValue)
            {
                Console.WriteLine($"Il n'y a pas de chemin entre {depart} et {arrivee}.");
                return;
            }

            // Récupérer le chemin en remontant les prédécesseurs
            List<T> chemin = new List<T>();
            for (int at = end; at != start; at = Predecesseurs[start, at])
            {
                if (at == -1)
                {
                    Console.WriteLine("Chemin impossible.");
                    return;
                }
                chemin.Add(IndexToId[at]);
            }
            chemin.Add(depart);
            chemin.Reverse();

            // Affichage du résultat
            Console.WriteLine($"Le plus court chemin de {depart} à {arrivee} est :");
            Console.WriteLine(string.Join(" -> ", chemin));

            // Calculer et afficher le temps total du trajet
            double tempsTotal = CalculerTempsTrajet(chemin);
            if (tempsTotal != -1)
            {
                Console.WriteLine($"Temps total du trajet : {tempsTotal:F1}");
            }
            else
            {
                Console.WriteLine("Erreur dans le calcul du temps de trajet.");
            }
        }




        /// <summary>
        /// Implémente l'algorithme de Bellman-Ford pour trouver le plus court chemin dans un graphe avec des poids d'arêtes.
        /// L'algorithme calcule les distances minimales depuis une station de départ vers toutes les autres stations.
        /// </summary>
        /// <param name="start">L'ID de la station de départ.</param>
        /// <param name="end">L'ID de la station d'arrivée.</param>
        /// <returns>Une liste des stations représentant le chemin le plus court entre les deux stations, ou null si aucun chemin n'existe.</returns>
        public List<T> BellmanFord(T start, T end)
        {
            var distances = new Dictionary<T, double>();
            var predecessors = new Dictionary<T, T>();

            // Initialisation
            foreach (var noeud in Noeuds.Keys)
            {
                distances[noeud] = double.MaxValue;  // Distance infinie par défaut
                predecessors[noeud] = default(T);    // Pas de prédécesseur
            }

            distances[start] = 0;

            // Relaxation des arêtes
            for (int i = 1; i < Noeuds.Count; i++)
            {
                foreach (var noeud in Noeuds.Values)
                {
                    foreach (var lien in noeud.Liens)
                    {
                        double newDistance = distances[noeud.Id] + lien.TempsTrajet;
                        if (newDistance < distances[lien.Destination.Id])
                        {
                            distances[lien.Destination.Id] = newDistance;
                            predecessors[lien.Destination.Id] = noeud.Id;
                        }
                    }
                }
            }

            // Vérification des cycles de poids négatif
            foreach (var noeud in Noeuds.Values)
            {
                foreach (var lien in noeud.Liens)
                {
                    if (distances[noeud.Id] + lien.TempsTrajet < distances[lien.Destination.Id])
                    {
                        Console.WriteLine("Le graphe contient un cycle de poids négatif.");
                        return null;
                    }
                }
            }

            // Reconstruction du chemin
            return ReconstructPath(predecessors, start, end);
        }
    }
}
