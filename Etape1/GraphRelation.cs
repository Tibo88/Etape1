
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
                        string identifiant = reader.IsDBNull(0) ? "N/A" : reader.GetString(0);
                        string nom = reader.IsDBNull(1) ? "N/A" : reader.GetString(1);
                        string prenom = reader.IsDBNull(2) ? "N/A" : reader.GetString(2);
                        Utilisateurs[identifiant] = new Utilisateur(identifiant, nom, prenom, "client_particulier");
                        Console.WriteLine($"Client particulier chargé : {identifiant} - {nom} {prenom}");
                    }
                }

                // Charger les clients entreprises
                query = "SELECT identifiant, nom_entreprise, nom_referent FROM client_entreprise";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string identifiant = reader.IsDBNull(0) ? "N/A" : reader.GetString(0);
                        string nom = reader.IsDBNull(1) ? "N/A" : reader.GetString(1);
                        string prenom = reader.IsDBNull(2) ? "N/A" : reader.GetString(2);
                        Utilisateurs[identifiant] = new Utilisateur(identifiant, nom, prenom, "client_entreprise");
                        Console.WriteLine($"Client entreprise chargé : {identifiant} - {nom} {prenom}");
                    }
                }

                // Charger les cuisiniers
                query = "SELECT identifiant, nom, prenom FROM cuisinier";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string identifiant = reader.IsDBNull(0) ? "N/A" : reader.GetString(0);
                        string nom = reader.IsDBNull(1) ? "N/A" : reader.GetString(1);
                        string prenom = reader.IsDBNull(2) ? "N/A" : reader.GetString(2);
                        Utilisateurs[identifiant] = new Utilisateur(identifiant, nom, prenom, "cuisinier");
                        Console.WriteLine($"Cuisinier chargé : {identifiant} - {nom} {prenom}");
                    }
                }

                // Charger les commandes
                query = "SELECT client_id, cuisinier_id FROM commande";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string clientId = reader.IsDBNull(0) ? "N/A" : reader.GetString(0);
                        string cuisinierId = reader.IsDBNull(1) ? "N/A" : reader.GetString(1);
                        AjouterLien(clientId, cuisinierId);
                        Console.WriteLine($"Commande chargée : Client {clientId} -> Cuisinier {cuisinierId}");
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

        /// <summary>
        /// Affiche la liste d'adjacence des utilisateurs.
        /// </summary>
        public void AfficherListeAdjacence()
        {
            Console.WriteLine("Liste d'adjacence :");
            foreach (var utilisateur in ListeAdjacence)
            {
                Console.Write($"{utilisateur.Key} -> ");
                Console.WriteLine(string.Join(", ", utilisateur.Value));
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

                // Dessiner les noeuds
                foreach (var utilisateur in Utilisateurs.Values)
                {
                    var pos = positions[utilisateur.Identifiant];
                    canvas.DrawCircle(pos.x, pos.y, 10, new SKPaint { Color = SKColors.Red });
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
