
using Etape2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using MySql.Data.MySqlClient;
using SkiaSharp;
using System.Text.RegularExpressions;


class Program
{
    static string connectionString = "server=localhost;database=AppliV3;user=root;password=&Mot2passe;";

    static void Main()
    {
        Graph<int> graphe = new Graph<int>();
        graphe.ChargerDepuisFichier("Noeuds2.txt", "Arcs2.txt");
        graphe.AjouterLiensManquants();

        //Console.WriteLine("Liste d'adjacence:");
        graphe.CreerListeAdjacence();
        //graphe.AfficherListe();
        //Console.WriteLine();

        //Console.WriteLine("Exécution de Floyd-Warshall");
        //graphe.FloydWarshall();
        //graphe.AfficherCheminPlusCourt(24, 240);
        //Console.WriteLine();

        //Console.WriteLine("Exécution de Dijkstra");
        //List<int> cheminDijkstra = graphe.Dijkstra(1, 3);
        //graphe.AfficherChemin(cheminDijkstra);
        //Console.WriteLine();

        //Console.WriteLine("Exécution de Bellman-Ford");
        //List<int> cheminBellmanFord = graphe.BellmanFord(113, 225);
        //graphe.AfficherChemin(cheminBellmanFord);
        string nomFichier = "plan_metro_parisien.png";
        GraphForm.GenererPlanDuMetro(graphe, nomFichier);

        try
        {
            Process.Start(new ProcessStartInfo(nomFichier) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de l'ouverture du fichier : {ex.Message}");
        }
        Console.WriteLine("Bienvenue dans l'application Liv'in Paris");

        MenuPrincipal(graphe);


    }

    /// <summary>
    /// Remplit le graphe des relations clients-cuisiniers à partir de la base de données.
    /// </summary>
    /// <param name="grapheRelation">Le graphe des relations clients-cuisiniers.</param>
    static void RemplirGrapheDepuisBDD(Graph<string> graphe)
    {
        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            conn.Open();
            string query = @"
                SELECT c.client_id, c.cuisinier_id
                FROM commande c
                JOIN client_particulier cp ON cp.identifiant = c.client_id
                JOIN cuisinier cu ON cu.identifiant = c.cuisinier_id";

            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    string clientId = reader.GetString(0);
                    string cuisinierId = reader.GetString(1);

                    // Ajouter les nœuds au graphe
                    graphe.AjouterNoeud(clientId, clientId, "", 0, 0, "", "");
                    graphe.AjouterNoeud(cuisinierId, cuisinierId, "", 0, 0, "", "");

                    // Ajouter l'arête représentant la commande
                    graphe.AjouterLien(clientId, cuisinierId, 0, 0, 0);
                }
            }
        }
    }


    /// <summary>
    /// Affiche le menu principal de l'application.
    /// </summary>
    /// <param name="graphe">Le graphe des stations de métro.</param>
    static void MenuPrincipal(Graph<int> graphe)
    {

        while (true)
        {
            Console.WriteLine();

            Console.WriteLine("1. Créer un compte");
            Console.WriteLine("2. Se connecter");
            Console.WriteLine("3. Accéder au module administrateur");
            Console.WriteLine("4. Afficher le graphe des relations"); // Nouvelle option
            Console.WriteLine("5. Quitter");
            Console.Write("Votre choix : ");
            string choix = Console.ReadLine();
            switch (choix)
            {
                case "1":
                    CreerCompte();
                    break;
                case "2":
                    SeConnecter(graphe);
                    break;
                case "3":
                    MenuAdministrateur();
                    break;
                case "4": // Nouvelle option pour afficher le graphe des relations
                    GraphRelation grapheRelation = new GraphRelation();
                    grapheRelation.AfficherListeAdjacence();
                    grapheRelation.GenererGraphe("graphe_relations.png");
                    break;
                case "5":
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("Choix invalide !");
                    break;
            }
        }
    }


    /// <summary>
    /// Affiche le menu administrateur.
    /// </summary>
    static void MenuAdministrateur()
    {
        Console.WriteLine();

        while (true)
        {
            Console.WriteLine("\nMenu Administrateur");
            Console.WriteLine("1. Afficher les clients par ordre alphabétique");
            Console.WriteLine("2. Afficher les clients par nom de rue/avenue");
            Console.WriteLine("7. Retour au menu principal");
            Console.Write("Votre choix : ");
            string choix = Console.ReadLine();
            switch (choix)
            {
                case "1":
                    AfficherClientsParOrdreAlphabetique();
                    break;
                case "2":
                    AfficherClientsParNomRue();
                    break;
                case "7":
                    return;
                default:
                    Console.WriteLine("Choix invalide !");
                    break;
            }
        }
    }

    /// <summary>
    /// Affiche les clients par ordre alphabétique.
    /// </summary>
    static void AfficherClientsParOrdreAlphabetique()
    {
        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            Console.WriteLine();
            conn.Open();
            string query = "SELECT identifiant, nom, prenom FROM client_particulier ORDER BY nom, prenom";
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                Console.WriteLine("Clients par ordre alphabétique :");
                while (reader.Read())
                {
                    string identifiant = reader.IsDBNull(0) ? "N/A" : reader.GetString(0);
                    string nom = reader.IsDBNull(1) ? "N/A" : reader.GetString(1);
                    string prenom = reader.IsDBNull(2) ? "N/A" : reader.GetString(2);
                    Console.WriteLine($"{identifiant} - {nom} {prenom}");
                }
            }
        }
    }

    /// <summary>
    /// Affiche les clients par nom de rue/avenue.
    /// </summary>
    static void AfficherClientsParNomRue()
    {
        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            Console.WriteLine();
            conn.Open();
            string query = "SELECT identifiant, nom, prenom, adresse FROM client_particulier";
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                // Lire les données dans une liste
                List<(string Identifiant, string Nom, string Prenom, string Adresse)> clients = new List<(string, string, string, string)>();
                while (reader.Read())
                {
                    string identifiant = reader.IsDBNull(0) ? "N/A" : reader.GetString(0);
                    string nom = reader.IsDBNull(1) ? "N/A" : reader.GetString(1);
                    string prenom = reader.IsDBNull(2) ? "N/A" : reader.GetString(2);
                    string adresse = reader.IsDBNull(3) ? "N/A" : reader.GetString(3);
                    clients.Add((identifiant, nom, prenom, adresse));
                }

                // Trier les clients par nom de rue/avenue
                clients = clients
                    .OrderBy(c => ExtraireNomRue(c.Adresse))
                    .ThenBy(c => c.Adresse)
                    .ToList();

                // Afficher les résultats
                Console.WriteLine("Clients par nom de rue/avenue :");
                foreach (var client in clients)
                {
                    Console.WriteLine($"{client.Identifiant} - {client.Nom} {client.Prenom}, {client.Adresse}");
                }
            }
        }
    }

    /// <summary>
    /// Extrait le nom de la rue à partir d'une adresse.
    /// </summary>
    /// <param name="adresse">L'adresse complète.</param>
    /// <returns>Le nom de la rue.</returns>
    static string ExtraireNomRue(string adresse)
    {
        // Regex pour extraire le nom de la rue/avenue
        string pattern = @"\d+\s*(rue|avenue|boulevard|impasse|place|chemin|allée|route)\s+(.+)";
        Match match = Regex.Match(adresse, pattern, RegexOptions.IgnoreCase);

        if (match.Success)
        {
            return match.Groups[2].Value.Trim();
        }

        return adresse; // Retourner l'adresse complète si aucun nom de rue n'est trouvé
    }

    /// <summary>
    /// Crée un nouveau compte utilisateur.
    /// </summary>
    static void CreerCompte()
    {
        Console.WriteLine("\nCréation de compte");
        Console.Write("Nom d'utilisateur : ");
        string username = Console.ReadLine();
        Console.Write("Mot de passe : ");
        string password = Console.ReadLine();
        Console.Write("Type (client_particulier/client_entreprise/cuisinier) : ");
        string type = Console.ReadLine().ToLower();

        Console.Write("Nom : ");
        string nom = Console.ReadLine();
        Console.Write("Prénom : ");
        string prenom = Console.ReadLine();
        Console.Write("Adresse : ");
        string adresse = Console.ReadLine();
        Console.Write("Téléphone : ");
        string telephone = Console.ReadLine();
        Console.Write("Email : ");
        string email = Console.ReadLine();
        Console.Write("Numéro de la station la plus proche (entre 1 et 331) : ");
        int metroProche = 0;
        while (metroProche < 1 || metroProche > 331)
        {
            Console.Write("Veuillez entrer un numéro valide : ");
            if (int.TryParse(Console.ReadLine(), out metroProche))
            {
                if (metroProche < 1 || metroProche > 331)
                {
                    Console.WriteLine("Numéro de station invalide. Veuillez entrer un numéro entre 1 et 331.");
                }
            }
            else
            {
                Console.WriteLine("Entrée invalide. Veuillez entrer un numéro entre 1 et 331.");
            }
        }



        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            conn.Open();

            // Récupérer le dernier 'numero' pour incrémenter
            string query = "SELECT MAX(numero) FROM " + type;
            int numero = 1; // Si aucun numéro n'est trouvé, on commence à 1.
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                object result = cmd.ExecuteScalar();
                if (result != DBNull.Value)
                {
                    numero = Convert.ToInt32(result) + 1;
                }
            }

            // Insertion du nouvel utilisateur avec le numéro généré
            query = $"INSERT INTO {type} (numero, identifiant, mot_de_passe, nom, prenom, adresse, telephone, email, metro_proche) VALUES (@numero, @username, @password, @nom, @prenom, @adresse, @telephone, @email, @metroProche)";
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@numero", numero);
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password", password);
                cmd.Parameters.AddWithValue("@nom", nom);
                cmd.Parameters.AddWithValue("@prenom", prenom);
                cmd.Parameters.AddWithValue("@adresse", adresse);
                cmd.Parameters.AddWithValue("@telephone", telephone);
                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@metroProche", metroProche);

                cmd.ExecuteNonQuery();
            }
        }
        Console.WriteLine();

        Console.WriteLine($"Compte {type} créé avec succès !");
    }

    /// <summary>
    /// Connecte un utilisateur existant.
    /// </summary>
    /// <param name="graphe">Le graphe des stations de métro.</param>
    static void SeConnecter(Graph<int> graphe)
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
            MenuClient(username, graphe);
        else if (role == "cuisinier")
            MenuCuisinier(username, graphe);
        else
            Console.WriteLine("Identifiants incorrects !");
    }

    /// <summary>
    /// Affiche le menu client.
    /// </summary>
    /// <param name="username">Le nom d'utilisateur du client.</param>
    /// <param name="graphe">Le graphe des stations de métro.</param>
    static void MenuClient(string username, Graph<int> graphe)
    {
        while (true)
        {
            Console.WriteLine("\n1. Passer une commande");
            Console.WriteLine("2. Supprimer mon compte");
            Console.WriteLine("3. Déconnexion");
            Console.Write("Votre choix : ");
            string choix = Console.ReadLine();

            if (choix == "1") PasserCommande(username);
            else if (choix == "2") SupprimerCompte(username, graphe);

            else if (choix == "3") return;
            else Console.WriteLine("Choix invalide !");
        }
    }

    /// <summary>
    /// Supprime le compte d'un utilisateur.
    /// </summary>
    /// <param name="username">Le nom d'utilisateur de l'utilisateur à supprimer.</param>
    /// <param name="graphe">Le graphe des stations de métro.</param>
    static void SupprimerCompte(string username, Graph<int> graphe)
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
        MenuPrincipal(graphe);
    }

    /// <summary>
    /// Affiche le menu cuisinier.
    /// </summary>
    /// <param name="username">Le nom d'utilisateur du cuisinier.</param>
    /// <param name="graphe">Le graphe des stations de métro.</param>
    static void MenuCuisinier(string username, Graph<int> graphe)
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
                    AjouterPlatDisponible(username);
                    break;
                case "3":
                    SupprimerCompte(username, graphe);
                    return;
                case "4":
                    LivrerCommande(graphe, username);
                    break;
                case "5":
                    return;
                default:
                    Console.WriteLine("Choix invalide !");
                    break;
            }
        }
    }

    /// <summary>
    /// Permet de livrer une commande
    /// </summary>
    /// <param name="graphe">Le graphe des stations de métro</param>
    /// <param name="username">Le nom d'utilisateur du cuisinier</param>
    static void LivrerCommande(Graph<int> graphe, string username)
    {
        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            conn.Open();

            // Afficher les commandes
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

            Console.Write("\nEntrez l'ID de la commande que vous souhaitez livrer : ");
            if (!int.TryParse(Console.ReadLine(), out int idCommande))
            {
                Console.WriteLine("ID invalide !");
                return;
            }

            // Récupérer station du client
            int stationClient = -1;
            using (MySqlCommand cmdClient = new MySqlCommand(@"
            SELECT cp.metro_proche
            FROM sous_commandes sc
            JOIN commande c ON c.numero_commande = sc.numero_commande
            JOIN client_particulier cp ON cp.identifiant = c.client_id
            WHERE sc.id_sous_commandes = @idCommande", conn))
            {
                cmdClient.Parameters.AddWithValue("@idCommande", idCommande);
                object result = cmdClient.ExecuteScalar();
                stationClient = (result != null && result != DBNull.Value) ? Convert.ToInt32(result) : -1;
            }


            int stationCuisinier = ObtenirStationUtilisateur(username, "cuisinier");

            if (stationClient == -1 || stationCuisinier == -1)
            {
                Console.WriteLine("Erreur : stations introuvables.");
                return;
            }

            // Calculer chemin avec Bellman-Ford
            List<int> chemin = graphe.BellmanFord(stationCuisinier, stationClient);

            string nomFichier = "chemin_livraison.png";
            GraphForm.GenererPlanDuMetro(graphe, nomFichier, chemin);

            try
            {
                Process.Start(new ProcessStartInfo(nomFichier) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'ouverture du fichier : {ex.Message}");
            }

            // Supprimer la commande livrée
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
    /// <summary>
    /// Permet de passer une commande
    /// </summary>
    /// <param name="username">Le nom d'utilisateur de l'utilisateur client souhaitant commander.</param>
    static void PasserCommande(string username)
    {
        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            conn.Open();

            // Demander l'adresse de livraison
            Console.Write("\nAdresse de livraison : ");
            string adresse = Console.ReadLine();

            // Récupérer et incrémenter l'ID de la commande
            int idCommande;
            string queryGetMaxCommande = "SELECT IFNULL(MAX(numero_commande), 0) + 1 FROM commande";
            using (MySqlCommand cmd = new MySqlCommand(queryGetMaxCommande, conn))
            {
                idCommande = Convert.ToInt32(cmd.ExecuteScalar());
            }

            // Choix du plat
            Console.WriteLine("\nVoulez-vous :");
            Console.WriteLine("1. Commander un plat déjà préparé");
            Console.WriteLine("2. Commander un plat spécifique");
            Console.Write("Votre choix : ");
            string choix = Console.ReadLine();

            int idPlatChoisi = -1;
            string nomPlat = "";
            decimal prixPlat = 0;
            int quantite = 1;
            string cuisinierId = ""; // Déclaré ici pour être utilisé plus tard

            if (choix == "1") // Choisir un plat déjà préparé
            {
                Console.WriteLine("\nPlats disponibles :");
                // Modification: On récupère aussi le cuisinier_id du plat
                string query = "SELECT p.id_plat, p.nom, p.prix, p.quantite, p.cuisinier_id FROM plat p WHERE p.quantite > 0";
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

                // Modification: On récupère aussi le cuisinier_id
                string queryPlat = "SELECT nom, prix, quantite, cuisinier_id FROM plat WHERE id_plat = @idPlat";
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
                            cuisinierId = reader.GetString(3); // Récupération du cuisinier
                        }
                        else
                        {
                            Console.WriteLine("ID du plat invalide !");
                            return;
                        }
                    }
                }

                // Mise à jour de la quantité du plat après la commande
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

                // Modification: On demande quel cuisinier crée le plat
                Console.WriteLine("\nCuisiniers disponibles :");
                string queryCuisiniers = "SELECT identifiant, nom FROM cuisinier";
                using (MySqlCommand cmd = new MySqlCommand(queryCuisiniers, conn))
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Console.WriteLine($"ID: {reader.GetString(0)}, Nom: {reader.GetString(1)}");
                    }
                }

                Console.Write("Entrez l'ID du cuisinier qui prépare ce plat : ");
                cuisinierId = Console.ReadLine();

                string queryGetMaxPlat = "SELECT IFNULL(MAX(id_plat), 0) + 1 FROM plat";
                using (MySqlCommand cmd = new MySqlCommand(queryGetMaxPlat, conn))
                {
                    idPlatChoisi = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // Modification: On ajoute le cuisinier_id lors de la création du plat
                string queryInsertPlat = "INSERT INTO plat (id_plat, nom, prix, quantite, cuisinier_id) VALUES (@idPlat, @nomPlat, @prixPlat, @quantite, @cuisinierId)";
                using (MySqlCommand cmd = new MySqlCommand(queryInsertPlat, conn))
                {
                    cmd.Parameters.AddWithValue("@idPlat", idPlatChoisi);
                    cmd.Parameters.AddWithValue("@nomPlat", nomPlat);
                    cmd.Parameters.AddWithValue("@prixPlat", prixPlat);
                    cmd.Parameters.AddWithValue("@quantite", quantite);
                    cmd.Parameters.AddWithValue("@cuisinierId", cuisinierId);
                    cmd.ExecuteNonQuery();
                }
            }
            else
            {
                Console.WriteLine("Choix invalide !");
                return;
            }

            // Insérer la commande avec le cuisinier associé au plat
            string queryInsertCommande = @"
            INSERT INTO commande (numero_commande, client_id, cuisinier_id, date_commande)
            VALUES (@numero, @client_id, @cuisinier_id, CURDATE())";
            using (MySqlCommand cmd = new MySqlCommand(queryInsertCommande, conn))
            {
                cmd.Parameters.AddWithValue("@numero", idCommande);
                cmd.Parameters.AddWithValue("@client_id", username);
                cmd.Parameters.AddWithValue("@cuisinier_id", cuisinierId); // Utilisation du cuisinier du plat
                cmd.ExecuteNonQuery();
            }

            // Récupérer et incrémenter l'ID de la sous-commande
            int idSousCommande;
            string queryGetMaxSousCommande = "SELECT IFNULL(MAX(id_sous_commandes), 0) + 1 FROM sous_commandes";
            using (MySqlCommand cmd = new MySqlCommand(queryGetMaxSousCommande, conn))
            {
                idSousCommande = Convert.ToInt32(cmd.ExecuteScalar());
            }

            // Insérer la sous-commande
            string queryInsertSousCommande = "INSERT INTO sous_commandes (id_sous_commandes, date_livraison, adresse_livraison, numero_commande, plat_id) VALUES (@idSousCommande, CURDATE(), @adresse, @idCommande, @idPlat)";
            using (MySqlCommand cmd = new MySqlCommand(queryInsertSousCommande, conn))
            {
                cmd.Parameters.AddWithValue("@idSousCommande", idSousCommande);
                cmd.Parameters.AddWithValue("@adresse", adresse);
                cmd.Parameters.AddWithValue("@idCommande", idCommande);
                cmd.Parameters.AddWithValue("@idPlat", idPlatChoisi);
                cmd.ExecuteNonQuery();
            }

            Console.WriteLine($"Commande enregistrée avec succès ! (Commande ID: {idCommande}, Plat: {nomPlat}, Cuisinier: {cuisinierId})");
        }
    }



    /// <summary>
    /// Affiche les commandes à préparer.
    /// </summary>
    static void VoirCommandes()
    {
        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            Console.WriteLine();
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

    /// <summary>
    /// Ajoute un plat disponible.
    /// </summary>
    static void AjouterPlatDisponible(string id_cuisinier)
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
            query = "INSERT INTO plat (id_plat, nom, prix, quantite,cuisinier_id) VALUES (@idPlat, @nomPlat, @prixPlat, @quantitePlat,@cuisinier_id)";
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@idPlat", nouvelIdPlat);
                cmd.Parameters.AddWithValue("@nomPlat", nomPlat);
                cmd.Parameters.AddWithValue("@prixPlat", prixPlat);
                cmd.Parameters.AddWithValue("@quantitePlat", quantitePlat);
                cmd.Parameters.AddWithValue("@cuisinier_id", id_cuisinier);
                cmd.ExecuteNonQuery();
            }
        }
        Console.WriteLine();
        Console.WriteLine($"Plat ajouté avec succès avec l'ID : {nouvelIdPlat} !");
    }
    static int ObtenirStationUtilisateur(string identifiant, string table)
    {
        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            conn.Open();
            string query = $"SELECT metro_proche FROM {table} WHERE identifiant = @identifiant";
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@identifiant", identifiant);
                object result = cmd.ExecuteScalar();
                return (result != null && result != DBNull.Value) ? Convert.ToInt32(result) : -1;
            }
        }
    }
}
