using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Etape2
{
    public class Lien<T>
    {
        public Noeud<T> Source { get; set; }
        public Noeud<T> Destination { get; set; }
        public double TempsTrajet { get; set; }
        public double TempsChangement { get; set; }
        public double Distance { get; set; }

        public Lien(Noeud<T> source, Noeud<T> destination, double tempsTrajet, double tempsChangement, double distance)
        {
            Source = source;
            Destination = destination;
            TempsTrajet = tempsTrajet;
            TempsChangement = tempsChangement;
            Distance = distance;
        }
    }
}
