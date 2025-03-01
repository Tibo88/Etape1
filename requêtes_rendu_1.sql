SELECT * FROM client_particulier;

SELECT COUNT(*) AS total_commandes FROM commande;

SELECT id_plat, nom, prix, date_fabrication, date_peremption FROM plat;

SELECT numero, nom, prenom, adresse, telephone, email, metro_proche FROM cuisinier;

INSERT INTO commande (numero_commande) VALUES (7);

INSERT INTO sous_commandes (id_sous_commandes, date_livraison, adresse_livraison, numero_commande) 
VALUES (10, '2025-03-10', '123 rue de Paris', 7);

SELECT ingredient, volume FROM ingredients;

DELETE FROM sous_commandes WHERE id_sous_commandes = 10;

