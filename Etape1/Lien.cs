using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etape1
{
    public class Lien<T>
    {
        Noeud<T> source;
        Noeud<T> destination;

        public Lien(Noeud<T> source, Noeud<T> destination)
        {
            this.source = source;
            this.destination = destination;
        }

        public Noeud<T> Source
        {
            get { return source; }
        }

        public Noeud<T> Destination
        {
            get { return destination;  }
        }
    }
}
