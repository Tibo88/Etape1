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
        graphe.ChargerDepuisFichier("Noeuds2.txt", "Arcs2.txt");
        graphe.AjouterLiensManquants();

        Console.WriteLine("Liste d'adjacence:");
        graphe.CreerListeAdjacence();
        graphe.AfficherListe();
        List<int> chemin = graphe.Dijkstra(1, 115);
        graphe.AfficherChemin(chemin);
        
        
    }
}
