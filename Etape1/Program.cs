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
        graphe.AfficherListeAdjacence();

        Console.WriteLine("\nMatrice d'adjacence:");
        graphe.CreerMatriceAdjacence();
        graphe.AfficherMatriceAdjacence();

        // Parcours en profondeur et largeur
        Console.WriteLine("\nParcours en profondeur depuis le nœud 1:");
        graphe.DFS(1);

        Console.WriteLine("\nParcours en largeur depuis le nœud 1:");
        graphe.BFS(1);

        // Générer une image du graphe
        Console.WriteLine("\nGénération de l'image du graphe...");
    }

    
}