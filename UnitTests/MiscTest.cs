using KFC.Model;
using KFC.Model.Buildings;
using KFC.Model.Zones;
using KFC.Persistence;

namespace KFC.UnitTests {
    [TestClass]
    public class MiscTest {
        private GameModel _model = null!;
        private int city_hall_price;
        private int axis0 = 30;
        private int axis1 = 25;

        [TestInitialize]
        public void Init() {
            GameInfo game_inf = new GameInfo();
            game_inf.city_name = "test_city";
            game_inf.money = 100000;
            _model = new GameModel(game_inf);
            _model.PlaceBuilding((0, 0), BuildingType.CITY_HALL);
            city_hall_price = _model.map[0, 0].building!.building_price;
        }

        [TestMethod]
        public void SanityCheck() {
            // Tests if model initializes correctly
            Assert.AreEqual("test_city", _model.city_name);

            Assert.AreEqual(BuildingType.CITY_HALL, _model.map[0, 0].building!.building_type);

            Assert.AreEqual(axis0, _model.map.GetLength(0));
            Assert.AreEqual(axis1, _model.map.GetLength(1));
        }

        [TestMethod]
        public void DebtCheck() {
            GameInfo game_inf = new GameInfo();
            game_inf.city_name = "test_city";
            game_inf.money = city_hall_price + 1;
            _model = new GameModel(game_inf);
            _model.PlaceBuilding((0, 0), BuildingType.CITY_HALL);

            Assert.AreEqual(1, _model.money);

            _model.SetZone((4,4), (5,5), Model.Zones.ZoneType.RESIDENTIAL);
            Assert.IsTrue(0 > _model.money);

            long money = _model.money;
            _model.PlaceBuilding((0, 2), BuildingType.PARK);
            Assert.IsTrue(money > _model.money);

            // second chunk
            game_inf = new GameInfo();
            game_inf.city_name = "test_city";
            game_inf.money = city_hall_price + 1;
            _model = new GameModel(game_inf);
            _model.PlaceBuilding((0, 0), BuildingType.CITY_HALL);

            _model.PlaceRoad((2, 3), (6, 6));
            Assert.IsTrue(0 > _model.money);

            money = _model.money;
            _model.SetZone((0, 3), (0, 5), Model.Zones.ZoneType.RESIDENTIAL);
            Assert.IsTrue(money > _model.money);
        }

        [TestMethod]
        public void UpgradeZone() {
            _model.PlaceRoad((0, 2), (10, 2));
            _model.SetZone((0, 3), (2, 3), ZoneType.RESIDENTIAL);
            _model.SetZone((3, 3), (4, 3), ZoneType.SERVICE);
            _model.SetZone((5, 3), (5, 3), ZoneType.INDUSTRIAL);

            _model.PlaceBuilding((4, 0), BuildingType.OXYGEN_GENERATOR);
            _model.PlaceBuilding((3, 1), BuildingType.OXYGEN_DIFFUSER);
            _model.PlacePipe((3, 1), (4, 1));
            _model.GameTick(1); // city hall
            _model.GameTick(1); // oxygen
            _model.GameTick(1); // moving in

            _model.Upgrade((0, 0));
            _model.Upgrade((0, 0));

            _model.Upgrade((2,3));
            Assert.AreEqual(2, _model.map[2, 3].zone_info!.level);
            _model.Upgrade((2, 3));
            Assert.AreEqual(3, _model.map[2, 3].zone_info!.level);

            _model.Upgrade((3, 3));
            Assert.AreEqual(2, _model.map[3, 3].zone_info!.level);

            _model.Upgrade((5, 3));
            Assert.AreEqual(2, _model.map[5, 3].zone_info!.level);
        }

        [TestMethod]
        public void NewResidentsMoveIn() {

            _model.GameTick(1); // for city hall placement

            _model.PlaceRoad((1, 2), (14, 2));
            _model.PlaceRoad((1, 4), (14, 4));
            _model.PlaceRoad((1, 7), (14, 7));
            _model.PlaceRoad((0, 2), (0, 7));
            Assert.IsNotNull(_model.map[0, 2].road);

            _model.PlaceBuilding((2, 0), BuildingType.OXYGEN_GENERATOR);
            _model.PlaceBuilding((5, 1), BuildingType.OXYGEN_DIFFUSER);
            _model.PlaceBuilding((10, 1), BuildingType.OXYGEN_DIFFUSER);
            _model.PlaceBuilding((14, 1), BuildingType.OXYGEN_DIFFUSER);
            _model.PlacePipe((2, 1), (14, 1));

            _model.SetZone((1, 3), (14, 3), ZoneType.RESIDENTIAL);
            _model.SetZone((1, 5), (10, 6), ZoneType.SERVICE);
            _model.SetZone((11, 5), (14, 6), ZoneType.INDUSTRIAL);

            for (int p = 0; p < 10; p++)
            {
                _model.GameTick(1);
            }  // for moving out of city hall
            Assert.AreEqual(0, _model.city_hall_residents);

            long people = _model.population;
            _model.GameTick(46); // new month
            Assert.IsTrue(people < _model.population, $"{_model.total_public_satisfaction}");
        }
    }
}
