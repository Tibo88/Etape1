using Etape1;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;


class Program
{
    static void Main()
    {
        Graph<int> graphe = new Graph<int>();
        graphe.ChargerDepuisFichier("soc-karate.txt");

        Console.WriteLine("Liste d'adjacence:");
        graphe.CreerListeAdjacence();
        graphe.AfficherListe();

        Console.WriteLine("\nMatrice d'adjacence:");
        graphe.CreerMatriceAdjacence();
        graphe.AfficherMatrice();

        //Tests parcours profondeur et largeur
        Console.WriteLine("\nParcours en profondeur depuis le nœud 1:");
        foreach (var noeud in graphe.DFS(1))
        {
            Console.Write(noeud + ", ");
        }

        Console.WriteLine("\nParcours en largeur depuis le nœud 1:");
        foreach (var noeud in graphe.BFS(1))
        {
            Console.Write(noeud + ", ");
        }

        //Afficher le graphe
        string imagePath = "graph.png";
        graphe.GenererGraphe(imagePath);
        Console.WriteLine($"Image du graphe générée : {imagePath}");
        Process.Start(new ProcessStartInfo(imagePath) { UseShellExecute = true });

        //on vérifie si le graphe est connexe
        if (graphe.TestConnexe())
        {
            Console.WriteLine("Le graphe est connexe");
        }
        else
        {
            Console.WriteLine("Le graphe n'est pas connexe");
        }




    }


}
