using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Etape1
{
    public class Noeud<T>
    {
        public T nom;
        public List<Lien<T>> liens;

        public Noeud(T nom)
        {
            this.nom = nom;
        }

        public T Nom
        {
            get { return nom ; }
        }
        public List<Lien<T>> Liens
        { 
            get { return liens ; }
            set { liens = value ; }
        }

    }

}
