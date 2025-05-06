CREATE DATABASE AppliV3;
USE AppliV3;

CREATE TABLE client_particulier (
    numero INT PRIMARY KEY,
    nom VARCHAR(50),
    prenom VARCHAR(50),
    adresse TEXT,
    telephone VARCHAR(20),
    email VARCHAR(100),
    metro_proche INT,
    identifiant VARCHAR(50) UNIQUE,
    mot_de_passe VARCHAR(255)
);

CREATE TABLE client_entreprise (
    numero INT PRIMARY KEY,
    nom_entreprise VARCHAR(100),
    adresse TEXT,
    telephone VARCHAR(20),
    nom_referent VARCHAR(50),
    metro_proche INT,
    email VARCHAR(100),
    identifiant VARCHAR(50) UNIQUE,
    mot_de_passe VARCHAR(255)
);

CREATE TABLE cuisinier (
    numero INT PRIMARY KEY,
    nom VARCHAR(50),
    prenom VARCHAR(50),
    adresse TEXT,
    telephone VARCHAR(20),
    email VARCHAR(100),
    metro_proche INT,
    identifiant VARCHAR(50) UNIQUE,
    mot_de_passe VARCHAR(255)
);

CREATE TABLE commande (
    numero_commande INT PRIMARY KEY,
    client_id VARCHAR(50),
    cuisinier_id VARCHAR(50),
    date_commande DATE,
    FOREIGN KEY (client_id) REFERENCES client_particulier(identifiant),
    FOREIGN KEY (cuisinier_id) REFERENCES cuisinier(identifiant)
);

CREATE TABLE sous_commandes (
    id_sous_commandes INT PRIMARY KEY,
    date_livraison DATE,
    adresse_livraison TEXT,
    numero_commande INT,
    plat_id INT,
    FOREIGN KEY (numero_commande) REFERENCES commande(numero_commande),
    FOREIGN KEY (plat_id) REFERENCES plat(id_plat)
);

CREATE TABLE plat (
    id_plat INT PRIMARY KEY,
    nom VARCHAR(100),
    prix DECIMAL(10,2),
    quantite INT,
    type VARCHAR(50),
    date_fabrication DATE,
    date_peremption DATE,
    regime VARCHAR(50),
    nature VARCHAR(50),
    cuisinier_id VARCHAR(50)
);

CREATE TABLE ingredients (
    id_ingredient INT PRIMARY KEY,
    ingredient VARCHAR(100),
    volume DECIMAL(10,2)
);
DESCRIBE commande;

SELECT c.client_id, cp.metro_proche
FROM commande c
JOIN client_particulier cp ON cp.identifiant = c.client_id
WHERE c.numero_commande = 1;
SELECT identifiant FROM client_particulier;
UPDATE commande
SET client_id = 'Eliott'
WHERE numero_commande = 1;

SELECT metro_proche FROM client_particulier WHERE identifiant = 'Eliott';

SELECT metro_proche FROM cuisinier WHERE identifiant = 'Tibo';

SELECT numero_commande, client_id FROM commande WHERE numero_commande = 1;
SELECT * FROM commande;
SELECT * FROM sous_commandes;

SELECT c.numero_commande, c.client_id, cp.metro_proche
FROM commande c
JOIN client_particulier cp ON cp.identifiant = c.client_id
WHERE c.numero_commande IN (1004, 1005);

select * from plat;
