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
        public T Id { get; }
        public string Nom { get; }
        public string LigneLibelle { get; }
        public double Longitude { get; }
        public double Latitude { get; }
        public string Commune { get; }
        public string CodeInsee { get; }
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
