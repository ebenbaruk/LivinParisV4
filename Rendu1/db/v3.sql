DROP DATABASE IF EXISTS LivInParis;
CREATE DATABASE LivInParis;
USE LivInParis;

-- Table Utilisateur avec support pour entreprises
CREATE TABLE Utilisateur (
   ClientID INT AUTO_INCREMENT,
   TypeClient ENUM('Particulier', 'Entreprise') DEFAULT 'Particulier',
   NomU VARCHAR(100) NOT NULL,
   PrenomU VARCHAR(100),
   NomEntreprise VARCHAR(100),
   NomReferent VARCHAR(100),
   RueU VARCHAR(100) NOT NULL,
   NumeroU INT NOT NULL,
   CodePostalU INT NOT NULL,
   VilleU VARCHAR(50) NOT NULL,
   TelephoneU VARCHAR(20) NOT NULL,
   EmailU VARCHAR(100) NOT NULL,
   StationPlusProcheU VARCHAR(100) NOT NULL,
   MDPU VARCHAR(255) NOT NULL,
   AgeU INT,
   RegimeAlimentaireU VARCHAR(100),
   PreferenceU VARCHAR(100),
   AllergieU TEXT,
   BudgetU DECIMAL(10,2),
   DateInscription DATETIME DEFAULT CURRENT_TIMESTAMP,
   Actif BOOLEAN DEFAULT TRUE,
   PRIMARY KEY(ClientID),
   UNIQUE(EmailU),
   UNIQUE(TelephoneU)
);

-- Table Cuisinier
CREATE TABLE Cuisinier (
   ClientID INT NOT NULL,
   SpecialiteC VARCHAR(255),
   Note DECIMAL(3,2),
   NombreLivraisonsTotal INT DEFAULT 0,
   DateDebutActivite DATETIME DEFAULT CURRENT_TIMESTAMP,
   PRIMARY KEY(ClientID),
   FOREIGN KEY(ClientID) REFERENCES Utilisateur(ClientID) ON DELETE CASCADE
);

-- Table Ingredients améliorée
CREATE TABLE Ingredients (
   IngredientID INT AUTO_INCREMENT,
   NomIngredient VARCHAR(100) NOT NULL,
   Description TEXT NOT NULL,
   UniteQuantite VARCHAR(20) NOT NULL,
   ContientAllergenes BOOLEAN DEFAULT FALSE,
   TypeAllergene VARCHAR(100),
   EstDisponible BOOLEAN DEFAULT TRUE,
   PRIMARY KEY(IngredientID)
);

-- Table Plat avec plus de détails
CREATE TABLE Plat (
   PlatID INT AUTO_INCREMENT,
   CuisinierID INT NOT NULL,
   NomPlat VARCHAR(100) NOT NULL,
   TypePlat ENUM('Entrée', 'PlatPrincipal', 'Dessert') NOT NULL,
   Description TEXT NOT NULL,
   DateCreation DATETIME DEFAULT CURRENT_TIMESTAMP,
   DatePeremption DATETIME NOT NULL,
   PrixParPersonne DECIMAL(10,2) NOT NULL,
   NombrePersonnes INT NOT NULL,
   NationaliteCuisine VARCHAR(50) NOT NULL,
   RegimeAlimentaire VARCHAR(100),
   PhotoURL VARCHAR(255),
   EstDisponible BOOLEAN DEFAULT TRUE,
   PRIMARY KEY(PlatID),
   FOREIGN KEY(CuisinierID) REFERENCES Cuisinier(ClientID) ON DELETE CASCADE
);

-- Table PlatRecette (relation Plat-Ingrédients avec quantités)
CREATE TABLE PlatRecette (
    PlatID INT,
    IngredientID INT,
    Quantite DOUBLE NOT NULL,
    UniteQuantite VARCHAR(20) NOT NULL,
    PRIMARY KEY(PlatID, IngredientID),
    FOREIGN KEY(PlatID) REFERENCES Plat(PlatID) ON DELETE CASCADE,
    FOREIGN KEY(IngredientID) REFERENCES Ingredients(IngredientID) ON DELETE CASCADE
);

-- Table BonDeCommande_Liv améliorée
CREATE TABLE BonDeCommande_Liv (
   CommandeID VARCHAR(50),
   ClientID INT NOT NULL,
   CuisinierID INT,
   PrixPaye DECIMAL(10,2) NOT NULL,
   DateCommande DATETIME DEFAULT CURRENT_TIMESTAMP,
   DateSouhaitee DATETIME NOT NULL,
   AdresseBon VARCHAR(255) NOT NULL,
   Statut ENUM('En attente', 'Acceptée', 'En préparation', 'En livraison', 'Livrée', 'Annulée') DEFAULT 'En attente',
   DateLivraison DATETIME,
   ModePaiement VARCHAR(50),
   Commentaires TEXT,
   PRIMARY KEY(CommandeID),
   FOREIGN KEY(ClientID) REFERENCES Utilisateur(ClientID) ON DELETE CASCADE,
   FOREIGN KEY(CuisinierID) REFERENCES Cuisinier(ClientID) ON DELETE SET NULL
);

-- Table Correspond (liaison Plat-Commande)
CREATE TABLE Correspond (
   PlatID INT,
   CommandeID VARCHAR(50),
   Quantite INT DEFAULT 1,
   PrixUnitaire DECIMAL(10,2) NOT NULL,
   PRIMARY KEY(PlatID, CommandeID),
   FOREIGN KEY(PlatID) REFERENCES Plat(PlatID) ON DELETE CASCADE,
   FOREIGN KEY(CommandeID) REFERENCES BonDeCommande_Liv(CommandeID) ON DELETE CASCADE
);

-- Table Avis
CREATE TABLE Avis (
    AvisID INT AUTO_INCREMENT,
    CommandeID VARCHAR(50),
    ClientID INT,
    Note INT CHECK (Note BETWEEN 1 AND 5),
    Commentaire TEXT,
    DateAvis DATETIME DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY(AvisID),
    FOREIGN KEY(CommandeID) REFERENCES BonDeCommande_Liv(CommandeID) ON DELETE CASCADE,
    FOREIGN KEY(ClientID) REFERENCES Utilisateur(ClientID) ON DELETE CASCADE
);

-- Données de test
INSERT INTO Utilisateur 
(TypeClient, NomU, PrenomU, RueU, NumeroU, CodePostalU, VilleU, TelephoneU, EmailU, StationPlusProcheU, MDPU, AgeU, RegimeAlimentaireU, PreferenceU, AllergieU, BudgetU)
VALUES
('Particulier', 'Durand', 'Medhy', 'Rue Cardinet', 15, 75017, 'Paris', '0123456789', 'mdurand@gmail.com', 'Cardinet', 'Mdurand123', 30, 'Vegan', 'Thailandais', 'Noix', 200.00),
('Particulier', 'Leonelli W', 'Charles', 'Rue Ducis', 9, 78000, 'Paris', '0641435341', 'charles.leonelli@edu.devinci.fr', 'Versailles Rive Droite', 'BGdu78', 19, NULL, 'Japonais', NULL, 10000.00),
('Particulier', 'Benbaruk', 'Eli', 'Avenue Hubert Germain', 30, 75116, 'Paris', '0616493533', 'eli.benbaruk@edu.devinci.fr', 'Porte Dauphine', 'EliLePlusRiche', 20, 'Vegan', 'Japonais', NULL, 100000.00),
('Entreprise', 'Tech Food', NULL, 'Rue de la Défense', 4, 92400, 'Paris', '0668865211', 'contact@techfood.fr', 'La Defense', 'TechFood2024', NULL, NULL, NULL, NULL, 5000.00);

-- Insertion Cuisinier
INSERT INTO Cuisinier (ClientID, SpecialiteC, Note) VALUES 
(3, 'Italien', 4.5);

-- Insertion Ingredients
INSERT INTO Ingredients (NomIngredient, Description, UniteQuantite, ContientAllergenes, TypeAllergene) VALUES
('Fromage à raclette', 'Fromage à raclette traditionnel', 'grammes', TRUE, 'Lactose'),
('Pommes de terre', 'Pommes de terre fraîches', 'grammes', FALSE, NULL),
('Jambon', 'Jambon blanc', 'grammes', FALSE, NULL),
('Cornichons', 'Cornichons au vinaigre', 'pièces', FALSE, NULL);

-- Insertion Plat
INSERT INTO Plat 
(CuisinierID, NomPlat, TypePlat, Description, DatePeremption, PrixParPersonne, NombrePersonnes, NationaliteCuisine, RegimeAlimentaire)
VALUES
(3, 'Raclette Traditionnelle', 'PlatPrincipal', 'Raclette traditionnelle avec fromage, pommes de terre et charcuterie', 
DATE_ADD(CURRENT_TIMESTAMP, INTERVAL 2 DAY), 15.00, 4, 'Française', NULL);

-- Insertion PlatRecette
INSERT INTO PlatRecette (PlatID, IngredientID, Quantite, UniteQuantite) VALUES
(1, 1, 200, 'grammes'),
(1, 2, 400, 'grammes'),
(1, 3, 150, 'grammes'),
(1, 4, 10, 'pièces');

-- Création d'une commande test
INSERT INTO BonDeCommande_Liv 
(CommandeID, ClientID, CuisinierID, PrixPaye, DateSouhaitee, AdresseBon, Statut, ModePaiement)
VALUES
('CMD001', 1, 3, 60.00, DATE_ADD(CURRENT_TIMESTAMP, INTERVAL 1 DAY), '15 Rue Cardinet, 75017 Paris', 'En attente', 'Carte Bancaire');

-- Insertion Correspond
INSERT INTO Correspond (PlatID, CommandeID, Quantite, PrixUnitaire) VALUES
(1, 'CMD001', 4, 15.00);

-- Indexes pour optimiser les recherches
CREATE INDEX idx_plat_cuisine ON Plat(NationaliteCuisine);
CREATE INDEX idx_commande_date ON BonDeCommande_Liv(DateSouhaitee);
CREATE INDEX idx_utilisateur_type ON Utilisateur(TypeClient);