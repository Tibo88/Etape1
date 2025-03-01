CREATE DATABASE AppliV2;
USE AppliV2;

CREATE TABLE client_particulier (
    numero INT PRIMARY KEY,
    nom VARCHAR(50),
    prenom VARCHAR(50),
    adresse TEXT,
    telephone VARCHAR(20),
    email VARCHAR(100),
    metro_proche VARCHAR(50),
    identifiant VARCHAR(50) UNIQUE,
    mot_de_passe VARCHAR(255)
);

CREATE TABLE client_entreprise (
    numero INT PRIMARY KEY,
    nom_entreprise VARCHAR(100),
    adresse TEXT,
    telephone VARCHAR(20),
    nom_referent VARCHAR(50),
    metro_proche VARCHAR(50),
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
    metro_proche VARCHAR(50),
    identifiant VARCHAR(50) UNIQUE,
    mot_de_passe VARCHAR(255)
);

CREATE TABLE commande (
    numero_commande INT PRIMARY KEY
);

CREATE TABLE sous_commandes (
    id_sous_commandes INT PRIMARY KEY,
    date_livraison DATE,
    adresse_livraison TEXT,
    numero_commande INT,
    FOREIGN KEY (numero_commande) REFERENCES commande(numero_commande)
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
    nature VARCHAR(50)
);

CREATE TABLE ingredients (
    id_ingredient INT PRIMARY KEY,
    ingredient VARCHAR(100),
    volume DECIMAL(10,2)
);