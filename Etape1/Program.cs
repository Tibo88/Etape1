using Etape2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;


class Program
{
    static void Main()
{
    Graph<int> graphe = new Graph<int>();
    graphe.ChargerDepuisFichier("Noeuds2.txt", "Arcs2.txt");
    graphe.AjouterLiensManquants();

    Console.WriteLine("Liste d'adjacence:");
    graphe.CreerListeAdjacence();
    graphe.AfficherListe();
    Console.WriteLine();
    Console.WriteLine("Exécution de Floyd-Warshall");
    graphe.FloydWarshall();
    graphe.AfficherCheminPlusCourt(80, 148);
    Console.WriteLine();

    Console.WriteLine("Exécution de Dijkstra");
    List<int> cheminDijkstra = graphe.Dijkstra(80, 148);
    graphe.AfficherChemin(cheminDijkstra);
    Console.WriteLine();

    Console.WriteLine("Exécution de Bellman-Ford");
    List<int> cheminBellmanFord = graphe.BellmanFord(80, 148);
    graphe.AfficherChemin(cheminBellmanFord);

    // MenuPrincipal();
}
}
