using Rendu1;

namespace LivinParisTest
{
    [TestClass]
    public class GraphTests
    {
        [TestMethod]
        public void TestCreationGraphe()
        {
            // Arrange & Act
            Graphe<int> graphe = new Graphe<int>();
            
            // Assert
            Assert.AreEqual(0, graphe.ObtenirOrdre(), "Un nouveau graphe doit avoir 0 noeud");
            Assert.AreEqual(0, graphe.ObtenirTaille(), "Un nouveau graphe doit avoir 0 lien");
            Assert.IsFalse(graphe.EstOriente, "Par défaut, le graphe ne doit pas être orienté");
            Assert.IsFalse(graphe.EstPondere, "Par défaut, le graphe ne doit pas être pondéré");
        }
        
        [TestMethod]
        public void TestAjoutNoeudsEtLiens()
        {
            // Arrange
            Graphe<int> graphe = new Graphe<int>();
            
            // Act
            graphe.AjouterNoeud(1, 1);
            graphe.AjouterNoeud(2, 2);
            graphe.AjouterNoeud(3, 3);
            graphe.AjouterLien(1, 2);
            graphe.AjouterLien(2, 3);
            
            // Assert
            Assert.AreEqual(3, graphe.ObtenirOrdre(), "Le graphe doit avoir 3 noeuds");
            Assert.AreEqual(2, graphe.ObtenirTaille(), "Le graphe doit avoir 2 liens");
        }
        
        [TestMethod]
        public void TestParcoursLargeur()
        {
            // Arrange
            Graphe<int> graphe = new Graphe<int>();
            graphe.AjouterNoeud(1, 1);
            graphe.AjouterNoeud(2, 2);
            graphe.AjouterNoeud(3, 3);
            graphe.AjouterNoeud(4, 4);
            graphe.AjouterLien(1, 2);
            graphe.AjouterLien(1, 3);
            graphe.AjouterLien(2, 4);
            
            // Act
            List<int> parcours = graphe.ParcoursLargeur(1);
            
            // Assert
            Assert.AreEqual(4, parcours.Count, "Le parcours doit visiter tous les noeuds");
            Assert.AreEqual(1, parcours[0], "Le premier noeud visité doit être le noeud de départ");
            
            // Vérifier que les voisins directs sont visités avant les autres
            bool voisinsDirectsAvantAutres = 
                parcours.IndexOf(2) < parcours.IndexOf(4) && 
                parcours.IndexOf(3) < parcours.IndexOf(4);
            
            Assert.IsTrue(voisinsDirectsAvantAutres, "Les voisins directs doivent être visités avant les autres");
        }
        
        [TestMethod]
        public void TestEstConnexe()
        {
            // Arrange
            Graphe<int> grapheConnexe = new Graphe<int>();
            grapheConnexe.AjouterNoeud(1, 1);
            grapheConnexe.AjouterNoeud(2, 2);
            grapheConnexe.AjouterNoeud(3, 3);
            grapheConnexe.AjouterLien(1, 2);
            grapheConnexe.AjouterLien(2, 3);
            
            Graphe<int> grapheNonConnexe = new Graphe<int>();
            grapheNonConnexe.AjouterNoeud(1, 1);
            grapheNonConnexe.AjouterNoeud(2, 2);
            grapheNonConnexe.AjouterNoeud(3, 3);
            grapheNonConnexe.AjouterLien(1, 2);
            // Pas de lien vers le noeud 3
            
            // Act & Assert
            Assert.IsTrue(grapheConnexe.EstConnexe(), "Le graphe avec tous les noeuds reliés doit être connexe");
            Assert.IsFalse(grapheNonConnexe.EstConnexe(), "Le graphe avec un noeud isolé ne doit pas être connexe");
        }
        
        [TestMethod]
        public void TestContientCycles()
        {
            // Arrange
            Graphe<int> grapheSansCycle = new Graphe<int>();
            grapheSansCycle.AjouterNoeud(1, 1);
            grapheSansCycle.AjouterNoeud(2, 2);
            grapheSansCycle.AjouterNoeud(3, 3);
            grapheSansCycle.AjouterLien(1, 2);
            grapheSansCycle.AjouterLien(2, 3);
            
            Graphe<int> grapheAvecCycle = new Graphe<int>();
            grapheAvecCycle.AjouterNoeud(1, 1);
            grapheAvecCycle.AjouterNoeud(2, 2);
            grapheAvecCycle.AjouterNoeud(3, 3);
            grapheAvecCycle.AjouterLien(1, 2);
            grapheAvecCycle.AjouterLien(2, 3);
            grapheAvecCycle.AjouterLien(3, 1); // Crée un cycle 1-2-3-1
            
            // Act & Assert
            Assert.IsFalse(grapheSansCycle.ContientCycles(), "Le graphe linéaire ne doit pas contenir de cycle");
            Assert.IsTrue(grapheAvecCycle.ContientCycles(), "Le graphe avec un cycle doit être détecté");
        }
    }
}