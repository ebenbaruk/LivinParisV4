DROP DATABASE IF EXISTS LivInParis;
CREATE DATABASE LivInParis;
USE LivInParis;

-- Table Utilisateur
CREATE TABLE Utilisateur (
   ClientID INT AUTO_INCREMENT,
   NomU VARCHAR(50) NOT NULL,
   PrenomU VARCHAR(50) NOT NULL,
   RueU VARCHAR(50) NOT NULL,
   NumeroU INT NOT NULL,
   CodePostalU INT NOT NULL,
   VilleU VARCHAR(50) NOT NULL,
   TelephoneU VARCHAR(50) NOT NULL,
   EmailU VARCHAR(50) NOT NULL,
   StationPlusProcheU VARCHAR(50) NOT NULL,
   MDPU VARCHAR(50) NOT NULL,
   AgeU INT,
   RegimeAlimentaireU VARCHAR(50),
   PreferenceU VARCHAR(50),
   AllergieU VARCHAR(50),
   BudgetU DOUBLE,
   PRIMARY KEY(ClientID),
   UNIQUE(EmailU),
   UNIQUE(TelephoneU)
);

INSERT INTO Utilisateur 
(NomU, PrenomU, RueU, NumeroU, CodePostalU, VilleU, TelephoneU, EmailU, StationPlusProcheU, MDPU, AgeU, RegimeAlimentaireU, PreferenceU, AllergieU, BudgetU)
VALUES
("Durand", "Medhy", "Rue Cardinet", 15, 75017, "Paris", 1234567890, "Mdurand@gmail.com", "Cardinet", "Mdurand123", 30, "Vegan", "Thailandais", "Noix", 200),
("Leonelli W", "Charles", "Rue Ducis", 9, 78000, "Paris", 641435341, "charles.leonelli_wendling@edu.devinci.fr", "Versailles Rive Droite", "BGdu78", 19, "Aucun", "Japonais", "Aucune", 10000),
("Benbaruk", "Eli", "Avenue Hubert Germain", 30, 75116, "Paris", 616493533, "eli.benbaruk@edu.devinci.fr", "Porte Dauphine", "EliLePlusRiche", 20, "Vegan", "Japonais", "Aucune", 100000),
("Benjelloun", "Amine", "Rue des Etudiants", 4, 92400, "Paris", 668865211, "amine.bejelloun@edu.devinci.fr", "La Defense", "Benjell731", 19, "Aucun", "Kebab", "Aucune", 15);

-- Table Plat
CREATE TABLE Plat (
   PlatID VARCHAR(50),
   NomPlat VARCHAR(50),
   TypePlat VARCHAR(50),
   DateCreation DATE,
   DatePeremption DATE,
   PrixParPersonne VARCHAR(50),
   QuantitePlat INT NOT NULL,
   CategorieAlimentaire VARCHAR(50),
   PhotoURL VARCHAR(50),
   PRIMARY KEY(PlatID)
);

INSERT INTO Plat 
(PlatID, NomPlat, TypePlat, DateCreation, DatePeremption, PrixParPersonne, QuantitePlat, CategorieAlimentaire, PhotoURL)
VALUES
(1, "Raclette", "Plat", "2025-01-10", "2025-01-15", 10, 6, "Française", "Aucune"),
(2, "Salade de fruit", "Dessert", "2025-01-10", "2025-01-15", 5, 6, "Végétarien", "Aucune");

-- Table Cuisinier
CREATE TABLE Cuisinier(
   ClientID INT NOT NULL,
   SpecialiteC VARCHAR(255),
   PRIMARY KEY(ClientID),
   FOREIGN KEY(ClientID) REFERENCES Utilisateur(ClientID)
);

-- Cuisinier existant (ex: Eli)
INSERT INTO Cuisinier (ClientID, SpecialiteC) VALUES (3, "Italien");

-- Table Ingredients
CREATE TABLE Ingredients(
   IngredientID INT,
   NomIngredient VARCHAR(50) NOT NULL,
   PRIMARY KEY(IngredientID)
);

-- Ingrédients de base pour test
INSERT INTO Ingredients 
(IngredientID, NomIngredient)
VALUES
(1, "raclette fromage"),
(2, "pommes_de_terre"),
(3, "jambon"),
(4, "cornichon"),
(5, "fraise"),
(6, "kiwi"),
(9, "sucre");

-- Table BonDeCommande_Liv
CREATE TABLE BonDeCommande_Liv(
   CommandeID VARCHAR(50),
   PrixPaye DOUBLE,
   DateSouhaitee DATETIME,
   AdresseBon VARCHAR(50),
   Statut VARCHAR(50),
   DateLivraison DATETIME,
   TrajetID VARCHAR(50),
   ClientID INT,
   PRIMARY KEY(CommandeID),
   FOREIGN KEY(ClientID) REFERENCES Utilisateur(ClientID)
);

INSERT INTO BonDeCommande_Liv 
(CommandeID, PrixPaye, DateSouhaitee, AdresseBon, Statut, DateLivraison, TrajetID, ClientID)
VALUES
(1, 60, "2025-01-11", NULL, "En préparation", NULL, 1, 1),
(2, 30, "2025-01-12", NULL, "Livré", "2025-01-12", 2, 1);

-- Mise à jour automatique de l'adresse de livraison avec les infos client
UPDATE BonDeCommande_Liv B
JOIN Utilisateur U ON B.ClientID = U.ClientID
SET B.AdresseBon = CONCAT(U.NumeroU, ' ', U.RueU, ', ', U.CodePostalU, ' ', U.VilleU)
WHERE B.CommandeID IS NOT NULL;


-- Table Correspond
CREATE TABLE Correspond(
   PlatID VARCHAR(50),
   CommandeID VARCHAR(50),
   PRIMARY KEY(PlatID, CommandeID),
   FOREIGN KEY(PlatID) REFERENCES Plat(PlatID),
   FOREIGN KEY(CommandeID) REFERENCES BonDeCommande_Liv(CommandeID)
);

INSERT INTO Correspond 
(PlatID, CommandeID)
VALUES
(1, 1),
(2, 1);

-- Table Composé
CREATE TABLE Composé(
   PlatID VARCHAR(50),
   IngredientID INT,
   QuantiteUtile VARCHAR(50),
   PRIMARY KEY(PlatID, IngredientID),
   FOREIGN KEY(PlatID) REFERENCES Plat(PlatID),
   FOREIGN KEY(IngredientID) REFERENCES Ingredients(IngredientID)
);

INSERT INTO Composé 
(PlatID, IngredientID, QuantiteUtile)
VALUES
(1, 1, "250g"),
(1, 2, "200g"),
(1, 3, "200g"),
(1, 4, "3p"),
(2, 5, "100g"),
(2, 6, "100g"),
(2, 9, "10g");


