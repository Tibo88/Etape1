using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Etape2
{
    public class Noeud<T>
    {
        public T Id { get; set; }
        public string Nom { get; set; }
        public string LigneLibelle { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public string Commune { get; set; }
        public string CodeInsee { get; set; }
        public List<Lien<T>> Liens { get; set; }



        public Noeud(T id, string nom, string ligneLibelle, double longitude, double latitude, string commune, string codeInsee)
        {
            Id = id;
            Nom = nom;
            LigneLibelle = ligneLibelle;
            Longitude = longitude;
            Latitude = latitude;
            Commune = commune;
            CodeInsee = codeInsee;
            Liens = new List<Lien<T>>();
        }
    }

}
