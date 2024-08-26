using KFC.Model;
using KFC.Model.Buildings;
using KFC.Model.Zones;
using KFC.Persistence;

namespace KFC.UnitTests {
    [TestClass]
    public class SatisfactionTest {

        private GameModel _model = null!;
        private int axis0 = 30;
        private int axis1 = 25;
        private float base_satisfaction = 0;

        [TestInitialize]
        public void Init() {
            GameInfo game_inf = new GameInfo();
            game_inf.city_name = "test_city";
            game_inf.money = 100000;
            _model = new GameModel(game_inf);
            _model.PlaceBuilding((5, 5), BuildingType.CITY_HALL);
            _model.GameTick(1); // for city hall placement
            base_satisfaction = _model.total_public_satisfaction;

            _model.PlaceRoad((5, 7), (16, 7));
            _model.PlaceRoad((5, 9), (16, 9));
            _model.PlaceRoad((5, 12), (16, 12));
            _model.PlaceRoad((4, 7), (4, 12));

            _model.SetZone((5, 8), (14, 8), ZoneType.RESIDENTIAL);
            _model.SetZone((5, 10), (14, 11), ZoneType.SERVICE);

            _model.PlaceBuilding((6, 13), BuildingType.OXYGEN_GENERATOR);
            _model.PlaceBuilding((5, 13), BuildingType.OXYGEN_DIFFUSER);
            _model.PlaceBuilding((9, 13), BuildingType.OXYGEN_DIFFUSER);
            _model.PlaceBuilding((14, 13), BuildingType.OXYGEN_DIFFUSER);
            _model.PlacePipe((5, 13), (14, 13));
            _model.PlaceBuilding((7, 6), BuildingType.OXYGEN_DIFFUSER);
            _model.PlaceBuilding((12, 6), BuildingType.OXYGEN_DIFFUSER);
            _model.PlacePipe((6, 13), (7, 6));
            _model.PlacePipe((12, 6), (7, 6));

            for(int p = 0; p < 10; p++)
            {
                _model.GameTick(1);
            } // for moving out of city hall
            _model.GameTick(1); // for satisfaction to be calculated after moving out
        }

        [TestMethod]
        public void InitTest() {
            Assert.AreEqual("test_city", _model.city_name);

            Assert.AreEqual(BuildingType.CITY_HALL, _model.map[5, 5].building?.building_type);

            Assert.AreEqual(axis0, _model.map.GetLength(0));
            Assert.AreEqual(axis1, _model.map.GetLength(1));

            // placements are already tested in other tests
            for(int i = 5; i <= 14; i++) {
                Assert.IsTrue(_model.map[i, 8].zone_info!.people.Count > 0, $"{i};8");
            }

            for (int i = 5; i <= 14; i++) {
                Assert.IsTrue(_model.map[i, 10].zone_info!.people.Count > 0, $"{i};10");
                Assert.IsTrue(_model.map[i, 11].zone_info!.people.Count > 0, $"{i};11");
            }
        }

        [TestMethod]
        public void OutOfCityHall() {
            Assert.IsTrue(base_satisfaction < _model.total_public_satisfaction);
        }

        [TestMethod]
        public void ParkIncreasesSatisfaction() {
            float satisfaction = _model.total_public_satisfaction;
            _model.PlaceBuilding((8, 5), BuildingType.PARK);
            _model.GameTick(1); // for new satisfaction calculation
            Assert.IsTrue(satisfaction < _model.total_public_satisfaction, $"{satisfaction},{_model.total_public_satisfaction}");
        }

        [TestMethod]
        public void SafetyIncreasesSatisfaction() {
            float satisfaction = _model.total_public_satisfaction;
            _model.PlaceBuilding((8, 6), BuildingType.POLICE_STATION);
            _model.GameTick(1); // for new satisfaction calculation
            Assert.IsTrue(satisfaction < _model.total_public_satisfaction);
        }

        [TestMethod]
        public void LowTaxesIncreaseSatisfaction() {
            _model.SetTaxRate(0, ZoneType.RESIDENTIAL);
            _model.SetTaxRate(0, ZoneType.SERVICE);
            _model.GameTick(44);
            float satisfaction = _model.total_public_satisfaction;
            _model.GameTick(1); // for new satisfaction calculation
            Assert.IsTrue(satisfaction < _model.total_public_satisfaction);
        }

        [TestMethod]
        public void HighTaxesDecreaseSatisfaction() {
            _model.SetTaxRate(100, ZoneType.RESIDENTIAL);
            _model.SetTaxRate(100, ZoneType.SERVICE);
            _model.GameTick(44);
            float satisfaction = _model.total_public_satisfaction;
            _model.GameTick(1); // for new satisfaction calculation
            Assert.IsTrue(satisfaction > _model.total_public_satisfaction);
        }

        [TestMethod]
        public void CloseByIndustrailDecreaseSatisfaction() {
            _model.SetZone((15, 6), (15, 6), ZoneType.INDUSTRIAL);
            _model.SetZone((16, 6), (16, 6), ZoneType.RESIDENTIAL);
            _model.GameTick(44);
            float satisfaction = _model.total_public_satisfaction;
            _model.GameTick(1);
            // see if they actually moved in
            Assert.IsTrue(_model.map[15, 6].zone_info!.people.Count > 1);
            Assert.IsTrue(_model.map[16, 6].zone_info!.people.Count > 1);
            Assert.IsTrue(satisfaction > _model.total_public_satisfaction);
        }
    }
}
