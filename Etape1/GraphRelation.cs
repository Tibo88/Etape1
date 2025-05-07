
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using MySql.Data.MySqlClient;
using SkiaSharp;

namespace Etape2
{
    public class GraphRelation
    {
        private Dictionary<string, Utilisateur> Utilisateurs { get; set; }
        private Dictionary<string, List<string>> ListeAdjacence { get; set; }
        private Dictionary<string, int> couleurSommets = new Dictionary<string, int>();
        private string connectionString = "server=localhost;database=AppliV3;user=root;password=&Mot2passe;";

        public GraphRelation()
        {
            Utilisateurs = new Dictionary<string, Utilisateur>();
            ListeAdjacence = new Dictionary<string, List<string>>();
            ChargerDonneesDepuisBase();
            CreerListeAdjacence();
        }

        /// <summary>
        /// Charge les données des utilisateurs depuis la base de données.
        /// </summary>
        private void ChargerDonneesDepuisBase()
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                // Charger les clients particuliers
                string query = "SELECT identifiant, nom, prenom FROM client_particulier";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string identifiant;
                        if (reader.IsDBNull(0))
                            identifiant = "";
                        else
                            identifiant = reader.GetString(0);

                        string nom;
                        if (reader.IsDBNull(1))
                            nom = "";
                        else
                            nom = reader.GetString(1);

                        string prenom;
                        if (reader.IsDBNull(2))
                            prenom = "";
                        else
                            prenom = reader.GetString(2);

                        Utilisateurs[identifiant] = new Utilisateur(identifiant, nom, prenom, "client_particulier");

                        //Console.WriteLine($"Client particulier chargé : {identifiant} - {nom} {prenom}");
                    }
                }

                // Charger les clients entreprises
                query = "SELECT identifiant, nom_entreprise, nom_referent FROM client_entreprise";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string identifiant;
                        if (reader.IsDBNull(0))
                            identifiant = "";
                        else
                            identifiant = reader.GetString(0);

                        string nom;
                        if (reader.IsDBNull(1))
                            nom = "";
                        else
                            nom = reader.GetString(1);

                        string prenom;
                        if (reader.IsDBNull(2))
                            prenom = "";
                        else
                            prenom = reader.GetString(2);

                        Utilisateurs[identifiant] = new Utilisateur(identifiant, nom, prenom, "client_entreprise");

                        // Console.WriteLine($"Client entreprise chargé : {identifiant} - {nom} {prenom}");
                    }
                }

                // Charger les cuisiniers
                query = "SELECT identifiant, nom, prenom FROM cuisinier";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string identifiant;
                        if (reader.IsDBNull(0))
                            identifiant = "";
                        else
                            identifiant = reader.GetString(0);

                        string nom;
                        if (reader.IsDBNull(1))
                            nom = "";
                        else
                            nom = reader.GetString(1);

                        string prenom;
                        if (reader.IsDBNull(2))
                            prenom = "";
                        else
                            prenom = reader.GetString(2);

                        Utilisateurs[identifiant] = new Utilisateur(identifiant, nom, prenom, "cuisinier");

                        // Console.WriteLine($"Cuisinier chargé : {identifiant} - {nom} {prenom}");
                    }
                }

                // Charger les commandes
                query = "SELECT client_id, cuisinier_id FROM commande";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string clientId;
                        if (reader.IsDBNull(0))
                            clientId = "";
                        else
                            clientId = reader.GetString(0);

                        string cuisinierId;
                        if (reader.IsDBNull(1))
                            cuisinierId = "";
                        else
                            cuisinierId = reader.GetString(1);

                        AjouterLien(clientId, cuisinierId);

                        //Console.WriteLine($"Commande chargée : Client {clientId} -> Cuisinier {cuisinierId}");
                    }
                }
            }
        }

        /// <summary>
        /// Crée la liste d'adjacence à partir des liens entre les utilisateurs.
        /// </summary>
        private void CreerListeAdjacence()
        {
            ListeAdjacence.Clear();
            foreach (var utilisateur in Utilisateurs.Values)
            {
                ListeAdjacence[utilisateur.Identifiant] = new List<string>();
            }

            foreach (var utilisateur in Utilisateurs.Values)
            {
                foreach (var lien in utilisateur.Liens)
                {
                    ListeAdjacence[utilisateur.Identifiant].Add(lien);
                }
            }
        }

        /// <summary>
        /// Ajoute un lien entre un client et un cuisinier.
        /// </summary>
        /// <param name="clientId">L'ID du client.</param>
        /// <param name="cuisinierId">L'ID du cuisinier.</param>
        public void AjouterLien(string clientId, string cuisinierId)
        {
            if (Utilisateurs.ContainsKey(clientId) && Utilisateurs.ContainsKey(cuisinierId))
            {
                Utilisateurs[clientId].Liens.Add(cuisinierId);
                Utilisateurs[cuisinierId].Liens.Add(clientId);
                CreerListeAdjacence();
            }
        }

        public void ColorierGrapheWelshPowell()
        {
            // Trier les sommets par ordre décroissant de degré
            var sommetsTries = Utilisateurs.Values
                .OrderByDescending(u => ListeAdjacence[u.Identifiant].Count)
                .ToList();

            // Initialiser toutes les couleurs à -1 (non colorié)
            foreach (var sommet in sommetsTries)
            {
                couleurSommets[sommet.Identifiant] = -1;
            }

            // Tableau pour suivre les couleurs utilisées
            bool[] couleursUtilisees = new bool[sommetsTries.Count];

            // Colorier les sommets
            foreach (var sommet in sommetsTries)
            {
                // Réinitialiser les couleurs utilisées pour chaque sommet
                Array.Clear(couleursUtilisees, 0, couleursUtilisees.Length);

                // Vérifier les couleurs des voisins
                foreach (var voisin in ListeAdjacence[sommet.Identifiant])
                {
                    if (couleurSommets[voisin] != -1)
                    {
                        couleursUtilisees[couleurSommets[voisin]] = true;
                    }
                }

                // Trouver la première couleur non utilisée
                int couleur = 0;
                while (couleur < couleursUtilisees.Length && couleursUtilisees[couleur])
                {
                    couleur++;
                }

                // Attribuer la couleur au sommet
                couleurSommets[sommet.Identifiant] = couleur;
            }

            // Afficher les couleurs attribuées
            foreach (var sommet in couleurSommets)
            {
                Console.WriteLine($"Sommet {sommet.Key} a la couleur {sommet.Value}");
            }
        }

        /// <summary>
        /// Génère et affiche le graphe des relations entre les utilisateurs.
        /// </summary>
        /// <param name="nomFichier">Le nom du fichier où sauvegarder le graphe.</param>
        public void GenererGraphe(string nomFichier)
        {
            int largeurImage = 1000;
            int hauteurImage = 800;

            using (var surface = SKSurface.Create(new SKImageInfo(largeurImage, hauteurImage)))
            {
                var canvas = surface.Canvas;
                canvas.Clear(SKColors.White);

                // Calculer les positions des noeuds (pour simplifier, on les place en cercle)
                double angle = 0;
                double angleIncrement = 2 * Math.PI / Utilisateurs.Count;
                Dictionary<string, (int x, int y)> positions = new Dictionary<string, (int x, int y)>();

                foreach (var utilisateur in Utilisateurs.Values)
                {
                    int x = (int)(largeurImage / 2 + 300 * Math.Cos(angle));
                    int y = (int)(hauteurImage / 2 + 300 * Math.Sin(angle));
                    positions[utilisateur.Identifiant] = (x, y);
                    angle += angleIncrement;
                }

                // Dessiner les liens
                foreach (var utilisateur in Utilisateurs.Values)
                {
                    foreach (var lien in utilisateur.Liens)
                    {
                        var pos1 = positions[utilisateur.Identifiant];
                        var pos2 = positions[lien];
                        canvas.DrawLine(pos1.x, pos1.y, pos2.x, pos2.y, new SKPaint { Color = SKColors.Black, StrokeWidth = 2 });
                    }
                }

                // Dessiner les noeuds avec les couleurs attribuées
                foreach (var utilisateur in Utilisateurs.Values)
                {
                    var pos = positions[utilisateur.Identifiant];
                    int couleur = couleurSommets.ContainsKey(utilisateur.Identifiant) ? couleurSommets[utilisateur.Identifiant] : 0;
                    SKColor[] couleurs = { SKColors.Red, SKColors.Blue, SKColors.Green, SKColors.Yellow, SKColors.Purple, SKColors.Orange };
                    SKColor couleurNoeud = couleurs[couleur % couleurs.Length];

                    canvas.DrawCircle(pos.x, pos.y, 10, new SKPaint { Color = couleurNoeud });
                    canvas.DrawText(utilisateur.Identifiant, pos.x + 15, pos.y - 5, new SKPaint { Color = SKColors.Black, TextSize = 12 });
                }

                // Sauvegarder l'image
                using (var image = surface.Snapshot())
                using (var data = image.Encode())
                using (var stream = File.OpenWrite(nomFichier))
                {
                    data.SaveTo(stream);
                    Console.WriteLine("Le graphe a été sauvegardé dans : " + nomFichier);
                }

                try
                {
                    Process.Start(new ProcessStartInfo(nomFichier) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur lors de l'ouverture du fichier : {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Affiche la liste d'adjacence des utilisateurs.
        /// </summary>
        public void AfficherListeAdjacence()
        {
            //Console.WriteLine("Liste d'adjacence :");
            foreach (var utilisateur in ListeAdjacence)
            {
                // Console.Write($"{utilisateur.Key} -> ");
                // Console.WriteLine(string.Join(", ", utilisateur.Value));
            }
        }

        /// <summary>
        /// Génère et affiche le graphe des relations entre les utilisateurs.
        /// </summary>
        /// <param name="nomFichier">Le nom du fichier où sauvegarder le graphe.</param>
        


    }

    public class Utilisateur
    {
        public string Identifiant { get; }
        public string Nom { get; }
        public string Prenom { get; }
        public string Type { get; }
        public List<string> Liens { get; }

        public Utilisateur(string identifiant, string nom, string prenom, string type)
        {
            Identifiant = identifiant;
            Nom = nom;
            Prenom = prenom;
            Type = type;
            Liens = new List<string>();
        }
    }
}
