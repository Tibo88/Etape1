using Etape2;
using System;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;


class Program
{
    static string connectionString = "server=localhost;database=AppliV2;user=root;password=&Mot2passe;";
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
        graphe.AfficherCheminPlusCourt(1, 3);
        Console.WriteLine();

        Console.WriteLine("Exécution de Dijkstra");
        List<int> cheminDijkstra = graphe.Dijkstra(1, 115);
        graphe.AfficherChemin(cheminDijkstra);
        Console.WriteLine();

        Console.WriteLine("Exécution de Bellman-Ford");
        List<int> cheminBellmanFord = graphe.BellmanFord(1, 3);
        graphe.AfficherChemin(cheminBellmanFord);

        string nomFichier = "plan_metro_parisien.png";
        GraphForm.GenererPlanDuMetro(graphe, nomFichier);

        // Ouvrir l'image avec le programme par défaut (sans spécifier un exécutable)
        try
        {
            Process.Start(new ProcessStartInfo(nomFichier) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de l'ouverture du fichier : {ex.Message}");
        }

        MenuPrincipal();
    }

    static void MenuPrincipal()
    {
        Console.WriteLine("Bienvenue dans l'application de commande !");
        while (true)
        {
            Console.WriteLine("\n1. Créer un compte");
            Console.WriteLine("2. Se connecter");
            Console.WriteLine("3. Quitter");
            Console.Write("Votre choix : ");
            string choix = Console.ReadLine();

            switch (choix)
            {
                case "1":
                    CreerCompte();
                    break;
                case "2":
                    SeConnecter();
                    break;
                case "3":
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("Choix invalide !");
                    break;
            }
        }
    }

    static void CreerCompte()
    {
        Console.WriteLine("\nCréation de compte");
        Console.Write("Nom d'utilisateur : ");
        string username = Console.ReadLine();
        Console.Write("Mot de passe : ");
        string password = Console.ReadLine();
        Console.Write("Type (client_particulier/client_entreprise/cuisinier) : ");
        string type = Console.ReadLine().ToLower();

        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            conn.Open();

            // Récupérer le dernier 'numero' pour incrémenter
            string query = "SELECT MAX(numero) FROM " + type;
            int numero = 1;  // Si aucun numéro n'est trouvé, on commence à 1.
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                object result = cmd.ExecuteScalar();
                if (result != DBNull.Value)
                {
                    numero = Convert.ToInt32(result) + 1;
                }
            }

            // Insertion du nouvel utilisateur avec le numéro généré
            query = $"INSERT INTO {type} (numero, identifiant, mot_de_passe) VALUES (@numero, @username, @password)";
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@numero", numero);
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password", password);
                cmd.ExecuteNonQuery();
            }
        }

        Console.WriteLine($"Compte {type} créé avec succès !");
    }


    static void SeConnecter()
    {
        Console.Write("\nNom d'utilisateur : ");
        string username = Console.ReadLine();
        Console.Write("Mot de passe : ");
        string password = Console.ReadLine();

        string[] tables = { "client_particulier", "client_entreprise", "cuisinier" };
        string role = null;

        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            conn.Open();
            foreach (string table in tables)
            {
                string query = $"SELECT '{table}' FROM {table} WHERE identifiant = @username AND mot_de_passe = @password";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", password);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            role = table;
                            break;
                        }
                    }
                }
            }
        }

        if (role == "client_particulier" || role == "client_entreprise")
            MenuClient(username);
        else if (role == "cuisinier")
            MenuCuisinier(username);
        else
            Console.WriteLine("Identifiants incorrects !");
    }

    static void MenuClient(string username)
    {
        while (true)
        {
            Console.WriteLine("\n1. Passer une commande");
            Console.WriteLine("2. Supprimer mon compte");
            Console.WriteLine("3. Déconnexion");
            Console.Write("Votre choix : ");
            string choix = Console.ReadLine();

            if (choix == "1") PasserCommande(username);
            else if (choix == "2") SupprimerCompte(username);
            else if (choix == "3") return;
            else Console.WriteLine("Choix invalide !");
        }
    }

    static void SupprimerCompte(string username)
    {
        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            conn.Open();

            string query = "DELETE FROM client_particulier WHERE identifiant = @username";
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@username", username);
                int rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected > 0)
                    Console.WriteLine("Compte supprimé avec succès ! Redirection vers le menu principal...");
                else
                    Console.WriteLine("Erreur : Compte non trouvé !");
            }
        }
        MenuPrincipal();
    }

    static void MenuCuisinier(string username)
    {
        while (true)
        {
            Console.WriteLine("\n1. Voir commandes à préparer");
            Console.WriteLine("2. Ajouter un plat préparé");
            Console.WriteLine("3. Supprimer mon compte");
            Console.WriteLine("4. Livrer une commande");
            Console.WriteLine("5. Déconnexion");
            Console.Write("Votre choix : ");
            string choix = Console.ReadLine();

            switch (choix)
            {
                case "1":
                    VoirCommandes();
                    break;
                case "2":
                    AjouterPlatDisponible();
                    break;
                case "3":
                    SupprimerCompte(username);
                    return;
                case "4":
                    LivrerCommande();
                    break;
                case "5":
                    return;
                default:
                    Console.WriteLine("Choix invalide !");
                    break;
            }
        }
    }
    static void LivrerCommande()
    {
        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            conn.Open();

            // Afficher les commandes en attente de livraison
            string query = "SELECT id_sous_commandes, adresse_livraison FROM sous_commandes";
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                if (!reader.HasRows)
                {
                    Console.WriteLine("Aucune commande à livrer.");
                    return;
                }

                Console.WriteLine("\nCommandes disponibles :");
                while (reader.Read())
                {
                    Console.WriteLine($"ID: {reader.GetInt32(0)}, Adresse: {reader.GetString(1)}");
                }
            }

            // Demander à l'utilisateur quelle commande livrer
            Console.Write("\nEntrez l'ID de la commande que vous souhaitez livrer : ");
            if (!int.TryParse(Console.ReadLine(), out int idCommande))
            {
                Console.WriteLine("ID invalide !");
                return;
            }

            // Suppression de la commande livrée (au lieu de mettre à jour une colonne qui n'existe pas)
            string deleteQuery = "DELETE FROM sous_commandes WHERE id_sous_commandes = @idCommande";
            using (MySqlCommand deleteCmd = new MySqlCommand(deleteQuery, conn))
            {
                deleteCmd.Parameters.AddWithValue("@idCommande", idCommande);
                int rowsAffected = deleteCmd.ExecuteNonQuery();

                if (rowsAffected > 0)
                    Console.WriteLine($"Commande {idCommande} livrée et supprimée avec succès !");
                else
                    Console.WriteLine("Commande introuvable !");
            }
        }
    }







    static void PasserCommande(string username)
    {
        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            conn.Open();

            // Demander l'adresse de livraison
            Console.Write("\nAdresse de livraison : ");
            string adresse = Console.ReadLine();

            // 🔹 Récupérer et incrémenter l'ID de la commande
            int idCommande;
            string queryGetMaxCommande = "SELECT IFNULL(MAX(numero_commande), 0) + 1 FROM commande";
            using (MySqlCommand cmd = new MySqlCommand(queryGetMaxCommande, conn))
            {
                idCommande = Convert.ToInt32(cmd.ExecuteScalar());
            }

            // 🔹 Insérer une nouvelle commande
            string queryInsertCommande = "INSERT INTO commande (numero_commande) VALUES (@idCommande)";
            using (MySqlCommand cmd = new MySqlCommand(queryInsertCommande, conn))
            {
                cmd.Parameters.AddWithValue("@idCommande", idCommande);
                cmd.ExecuteNonQuery();
            }

            // 🔹 Récupérer et incrémenter l'ID de la sous-commande
            int idSousCommande;
            string queryGetMaxSousCommande = "SELECT IFNULL(MAX(id_sous_commandes), 0) + 1 FROM sous_commandes";
            using (MySqlCommand cmd = new MySqlCommand(queryGetMaxSousCommande, conn))
            {
                idSousCommande = Convert.ToInt32(cmd.ExecuteScalar());
            }

            // 🔹 Choix du plat
            Console.WriteLine("\nVoulez-vous :");
            Console.WriteLine("1. Commander un plat déjà préparé");
            Console.WriteLine("2. Commander un plat spécifique");
            Console.Write("Votre choix : ");
            string choix = Console.ReadLine();

            int idPlatChoisi = -1;
            string nomPlat = "";
            decimal prixPlat = 0;
            int quantite = 1;

            if (choix == "1") // Choisir un plat déjà préparé
            {
                Console.WriteLine("\nPlats disponibles :");
                string query = "SELECT id_plat, nom, prix, quantite FROM plat WHERE quantite > 0";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Console.WriteLine($"ID: {reader.GetInt32(0)}, Nom: {reader.GetString(1)}, Prix: {reader.GetDecimal(2)}, Quantité: {reader.GetInt32(3)}");
                    }
                }

                Console.Write("\nEntrez l'ID du plat que vous souhaitez commander : ");
                if (!int.TryParse(Console.ReadLine(), out idPlatChoisi))
                {
                    Console.WriteLine("ID invalide !");
                    return;
                }

                string queryPlat = "SELECT nom, prix, quantite FROM plat WHERE id_plat = @idPlat";
                using (MySqlCommand cmd = new MySqlCommand(queryPlat, conn))
                {
                    cmd.Parameters.AddWithValue("@idPlat", idPlatChoisi);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            nomPlat = reader.GetString(0);
                            prixPlat = reader.GetDecimal(1);
                            quantite = reader.GetInt32(2);
                        }
                        else
                        {
                            Console.WriteLine("ID du plat invalide !");
                            return;
                        }
                    }
                }

                // 🔹 Mise à jour de la quantité du plat après la commande
                string queryUpdateQuantite = "UPDATE plat SET quantite = quantite - 1 WHERE id_plat = @idPlat AND quantite > 0";
                using (MySqlCommand cmd = new MySqlCommand(queryUpdateQuantite, conn))
                {
                    cmd.Parameters.AddWithValue("@idPlat", idPlatChoisi);
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        Console.WriteLine("Erreur : Quantité insuffisante !");
                        return;
                    }
                }
            }
            else if (choix == "2") // Commander un plat personnalisé
            {
                Console.Write("\nEntrez le nom du plat que vous souhaitez commander : ");
                nomPlat = Console.ReadLine();
                Console.Write("Entrez le prix estimé du plat : ");
                prixPlat = decimal.Parse(Console.ReadLine());
                Console.Write("Entrez la quantité souhaitée : ");
                quantite = int.Parse(Console.ReadLine());

                string queryGetMaxPlat = "SELECT IFNULL(MAX(id_plat), 0) + 1 FROM plat";
                using (MySqlCommand cmd = new MySqlCommand(queryGetMaxPlat, conn))
                {
                    idPlatChoisi = Convert.ToInt32(cmd.ExecuteScalar());
                }

                string queryInsertPlat = "INSERT INTO plat (id_plat, nom, prix, quantite) VALUES (@idPlat, @nomPlat, @prixPlat, @quantite)";
                using (MySqlCommand cmd = new MySqlCommand(queryInsertPlat, conn))
                {
                    cmd.Parameters.AddWithValue("@idPlat", idPlatChoisi);
                    cmd.Parameters.AddWithValue("@nomPlat", nomPlat);
                    cmd.Parameters.AddWithValue("@prixPlat", prixPlat);
                    cmd.Parameters.AddWithValue("@quantite", quantite);
                    cmd.ExecuteNonQuery();
                }
            }
            else
            {
                Console.WriteLine("Choix invalide !");
                return;
            }

            // 🔹 Insérer la sous-commande
            string queryInsertSousCommande = "INSERT INTO sous_commandes (id_sous_commandes, date_livraison, adresse_livraison, numero_commande) VALUES (@idSousCommande, CURDATE(), @adresse, @idCommande)";
            using (MySqlCommand cmd = new MySqlCommand(queryInsertSousCommande, conn))
            {
                cmd.Parameters.AddWithValue("@idSousCommande", idSousCommande);
                cmd.Parameters.AddWithValue("@adresse", adresse);
                cmd.Parameters.AddWithValue("@idCommande", idCommande);
                cmd.ExecuteNonQuery();
            }

            Console.WriteLine($"Commande enregistrée avec succès ! (Commande ID: {idCommande}, Plat: {nomPlat})");
        }
    }





    static void VoirCommandes()
    {
        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            conn.Open();
            string query = "SELECT id_sous_commandes, adresse_livraison FROM sous_commandes";
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    Console.WriteLine($"Commande {reader.GetInt32(0)} à livrer à {reader.GetString(1)}");
                }
            }
        }
    }


    static void AjouterPlatDisponible()
    {
        Console.Write("\nEntrez le nom du plat préparé : ");
        string nomPlat = Console.ReadLine();
        Console.Write("Entrez le prix du plat : ");
        decimal prixPlat = decimal.Parse(Console.ReadLine());
        Console.Write("Entrez la quantité du plat préparé : ");
        int quantitePlat = int.Parse(Console.ReadLine());

        int nouvelIdPlat = 1; // Valeur par défaut si la table est vide

        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            conn.Open();

            // Récupérer le dernier ID utilisé et l'incrémenter
            string query = "SELECT IFNULL(MAX(id_plat), 0) FROM plat";
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                nouvelIdPlat = Convert.ToInt32(cmd.ExecuteScalar()) + 1;
            }

            // Insérer le plat dans la table plat
            query = "INSERT INTO plat (id_plat, nom, prix, quantite) VALUES (@idPlat, @nomPlat, @prixPlat, @quantitePlat)";
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@idPlat", nouvelIdPlat);
                cmd.Parameters.AddWithValue("@nomPlat", nomPlat);
                cmd.Parameters.AddWithValue("@prixPlat", prixPlat);
                cmd.Parameters.AddWithValue("@quantitePlat", quantitePlat);
                cmd.ExecuteNonQuery();
            }
        }

        Console.WriteLine($"Plat ajouté avec succès avec l'ID : {nouvelIdPlat} !");
    }

}

