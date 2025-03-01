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
        Console.WriteLine("\nParcours en profondeur depuis le noeud 1:");
        foreach (var noeud in graphe.DFS(1))
        {
            Console.Write(noeud + ", ");
        }
        Console.WriteLine();

        Console.WriteLine("\nParcours en largeur depuis le noeud 1:");
        foreach (var noeud in graphe.BFS(1))
        {
            Console.Write(noeud + ", ");
        }
        Console.WriteLine();

        //Afficher le graphe
        string imagePath = "graph.png";
        graphe.ImageGraphe(imagePath);
        Console.WriteLine($"Image du graphe générée : {imagePath}");
        Process.Start(new ProcessStartInfo(imagePath) { UseShellExecute = true });

        Console.WriteLine();

        //on vérifie si le graphe est connexe
        if (graphe.TestConnexe())
        {
            Console.WriteLine("Le graphe est connexe");
        }
        else
        {
            Console.WriteLine("Le graphe n'est pas connexe");
        }

        Console.WriteLine();

        //on cherche s'il existe au moins un circuit/cycle dans le graphe
        if (graphe.ContientCycle())
        {
            Console.WriteLine("Le graphe contient au moins un cycle");
        }
        else
        {
            Console.WriteLine("Le graphe ne contient pas de cycle");
        }
    }
}


    }


}
