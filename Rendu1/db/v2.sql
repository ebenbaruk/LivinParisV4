ALTER TABLE BonDeCommande_Liv 
ADD COLUMN CuisinierID INT,
ADD FOREIGN KEY (CuisinierID) REFERENCES Cuisinier(ClientID);


UPDATE BonDeCommande_Liv SET CuisinierID = 3 WHERE CuisinierID IS NULL;