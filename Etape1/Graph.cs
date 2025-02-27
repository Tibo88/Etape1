using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etape1
{
    public class Graph<T>
    {
        public Dictionary<T, Noeud<T>> noeuds { get; }

        public bool EstOriente { get; }
        private Dictionary<T, List<T>> listeAdjacence;
        private int[,] matriceAdjacence;

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
                throw new InvalidOperationException("Erreur : Un des noeuds est null après ajout.");
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

        public void ChargerDepuisFichier(string cheminFichier)
        {
            foreach (var ligne in File.ReadLines(cheminFichier))
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

        public void CreerMatriceAdjacence()
        {
            int taille = 34; 
            matriceAdjacence = new int[taille, taille];


            foreach (var noeud in noeuds.Values)
            {
                int i = Convert.ToInt32(noeud.Nom) - 1; // Ajustement pour que 1 → 0, 34 → 33

                foreach (var lien in noeud.Liens)
                {
                    int j = Convert.ToInt32(lien.Destination.Nom) - 1; 

                    if (i != j) 
                    {
                        matriceAdjacence[i, j] = 1;

                        // Si le graphe est non orienté, ajouter aussi la connexion inverse
                        if (!EstOriente)
                        {
                            matriceAdjacence[j, i] = 1;
                        }
                    }
                }
            }


        // Afficher la matrice d'adjacence
        AfficherMatriceAdjacence();
        }



        public void AfficherListeAdjacence()
        {
            foreach (var pair in listeAdjacence)
            {
                Console.Write(pair.Key + " -> ");
                Console.WriteLine(string.Join(", ", pair.Value));
            }
        }

        public void AfficherMatriceAdjacence()
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

        public void DFS(T depart)
        {
            if (!noeuds.ContainsKey(depart))
            {
                Console.WriteLine("Le nœud de départ n'existe pas.");
                return;
            }

            List<T> visite = new List<T>(); // Liste contenant les nœuds déjà visités
            Stack<T> pile = new Stack<T>(); // Pile utilisée pour le parcours en profondeur

            pile.Push(depart);

            while (pile.Count > 0)
            {
                T courant = pile.Pop();

                if (!visite.Contains(courant)) // Vérifier si le nœud a déjà été visité
                {
                    Console.Write(courant + " "); // Affichage du nœud visité
                    visite.Add(courant); // Marquer le nœud comme visité

                    if (listeAdjacence.ContainsKey(courant))
                    {
                        foreach (T voisin in listeAdjacence[courant])
                        {
                            if (!visite.Contains(voisin)) // Ajouter seulement les nœuds non visités
                            {
                                pile.Push(voisin);
                            }
                        }
                    }
                }
            }
        }

        public void BFS(T depart)
        {
            if (!noeuds.ContainsKey(depart))
            {
                Console.WriteLine("Le nœud de départ n'existe pas.");
                return;
            }

            List<T> visite = new List<T>(); // Liste pour suivre les nœuds visités
            Queue<T> file = new Queue<T>(); // File d'attente pour le BFS

            file.Enqueue(depart);
            visite.Add(depart);

            while (file.Count > 0)
            {
                T courant = file.Dequeue();
                Console.Write(courant + " "); // Affichage du nœud visité

                if (listeAdjacence.ContainsKey(courant))
                {
                    foreach (T voisin in listeAdjacence[courant])
                    {
                        if (!visite.Contains(voisin)) // Vérifie si le voisin n'a pas encore été visité
                        {
                            file.Enqueue(voisin);
                            visite.Add(voisin); // Marquer le nœud comme visité
                        }
                    }
                }
            }
        }






    }
}
