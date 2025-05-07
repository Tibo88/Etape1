using Etape2;

namespace Test_unitaire_rendu_3
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestCalculerDistance()
        {
            var graphe = new Graph<int>();
            graphe.AjouterNoeud(1, "Station A", "Ligne 1", 2.3522, 48.8566, "Paris", "75000"); 
            graphe.AjouterNoeud(2, "Station B", "Ligne 1", 2.295, 48.8738, "Paris", "75016");  

            double distance = graphe.CalculerDistance(1, 2);

            Assert.IsTrue(distance > 4 && distance < 5, $"Distance = {distance} km (attendue ~4.7 km)");
        }

    }
}
