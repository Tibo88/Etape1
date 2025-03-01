using Etape1;

namespace TestProject
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestAjoutNoeuLiens()
        {
            var graphe = new Graph<int>();
            graphe.AjouterLien(1, 2);

            Assert.IsTrue(graphe.noeuds.ContainsKey(1));
            Assert.IsTrue(graphe.noeuds.ContainsKey(2));
            Assert.AreEqual(1, graphe.noeuds[1].Liens.Count);
            Assert.AreEqual(1, graphe.noeuds[2].Liens.Count);
        }

        [TestMethod]
        public void TestDFS()
        {
            var graphe = new Graph<int>();

            
            graphe.listeAdjacence = new Dictionary<int, List<int>>()
            {
            { 1, new List<int> { 2, 3 } },
            { 2, new List<int> { 4 } },
            { 3, new List<int> { 5 } },
            { 4, new List<int>() },
            { 5, new List<int>() }
            };

            List<int> resultat = graphe.DFS(1);
            Assert.IsTrue(resultat.Contains(1));
            Assert.IsTrue(resultat.Contains(2));
            Assert.IsTrue(resultat.Contains(3));
            Assert.IsTrue(resultat.Contains(4));
            Assert.IsTrue(resultat.Contains(5));
            Assert.AreEqual(5, resultat.Count);
        }

        [TestMethod]
        public void TestBFS()
        {
            var graphe = new Graph<int>();

            // Initialisation manuelle de la liste d'adjacence
            graphe.listeAdjacence = new Dictionary<int, List<int>>()
            {
            { 1, new List<int> { 2, 3 } },
            { 2, new List<int> { 4 } },
            { 3, new List<int> { 5 } },
            { 4, new List<int>() },
            { 5, new List<int>() }
            };

            List<int> resultat = graphe.BFS(1);
            Assert.IsTrue(resultat.Contains(1));
            Assert.IsTrue(resultat.Contains(2));
            Assert.IsTrue(resultat.Contains(3));
            Assert.IsTrue(resultat.Contains(4));
            Assert.IsTrue(resultat.Contains(5));
            Assert.AreEqual(5, resultat.Count);
        }

        [TestMethod]
        public void TestContientCycle()
        {
            //Création d'un graphe avec un petit cycle 1 -> 2 -> 3 -> 1
            var graphe = new Graph<int>();

            graphe.listeAdjacence = new Dictionary<int, List<int>>()
            {
            { 1, new List<int> { 2 } },
            { 2, new List<int> { 3 } },
            { 3, new List<int> { 1 } },
            };
            graphe.AjouterLien(1, 2);
            graphe.AjouterLien(2, 3);
            graphe.AjouterLien(3, 1);

            bool resultat = graphe.ContientCycle();

            Assert.IsTrue(resultat, "Le graphe devrait contenir un cycle");
        }

        [TestMethod]
        public void TestContientPasCycle()
        {
            //Création d'un graphe sans cycle 1 -> 2 -> 3
            var graphe = new Graph<int>();
            graphe.listeAdjacence = new Dictionary<int, List<int>>()
            {
            { 1, new List<int> { 2 } },
            { 2, new List<int> { 3 } },
            { 3, new List<int> {  } },
            };
            graphe.AjouterLien(1, 2);
            graphe.AjouterLien(2, 3);

            bool resultat = graphe.ContientCycle();

            Assert.IsFalse(resultat, "Le graphe ne devrait pas contenir de cycle");
        }
    }
}
