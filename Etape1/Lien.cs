using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Etape1
{
    public class Lien<T>
    {
        public Noeud<T> Source { get; set; }
        public Noeud<T> Destination { get; set; }
        public double TempsTrajet { get; set; }
        public double TempsChangement { get; set; }

        public Lien(Noeud<T> source, Noeud<T> destination, double tempsTrajet, double tempsChangement)
        {
            Source = source;
            Destination = destination;
            TempsTrajet = tempsTrajet;
            TempsChangement = tempsChangement;
        }
    }
}
