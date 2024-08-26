using KFC.Model;
using KFC.Model.Buildings;
using KFC.Model.Zones;
using KFC.Persistence;

namespace KFC.UnitTests {
    [TestClass]
    public class OxygenTest {
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
            _model.PlaceBuilding((0, 0), BuildingType.CITY_HALL);
            _model.GameTick(1); // for city hall placement
            base_satisfaction = _model.total_public_satisfaction;

            _model.PlaceRoad((1, 2), (14, 2));
            _model.PlaceRoad((1, 4), (14, 4));
            _model.PlaceRoad((1, 7), (14, 7));
            _model.PlaceRoad((0, 2), (0, 7));

            _model.SetZone((1, 3), (14, 3), ZoneType.RESIDENTIAL);
            _model.SetZone((1, 5), (14, 6), ZoneType.SERVICE);

            _model.GameTick(1);
        }

        [TestMethod]
        public void InitTest() {
            Assert.AreEqual("test_city", _model.city_name);

            Assert.AreEqual(BuildingType.CITY_HALL, _model.map[0, 0].building?.building_type);

            Assert.AreEqual(axis0, _model.map.GetLength(0));
            Assert.AreEqual(axis1, _model.map.GetLength(1));

            // placements are already tested in other tests
        }

        [TestMethod]
        public void OxygenSetup() {
            _model.PlaceBuilding((3, 0), BuildingType.OXYGEN_GENERATOR);
            _model.GameTick(1); // oxygen register
            _model.GameTick(1); // moving in?
            Assert.AreEqual(0, _model.map[1, 3].zone_info!.oxygen_supply);
            Assert.AreEqual(0, _model.map[1, 3].zone_info!.people.Count);

            _model.PlaceBuilding((2, 1), BuildingType.OXYGEN_DIFFUSER);
            _model.GameTick(1); // oxygen register
            _model.GameTick(1); // moving in?
            Assert.AreEqual(0, _model.map[1, 3].zone_info!.oxygen_supply);
            Assert.AreEqual(0, _model.map[1, 3].zone_info!.people.Count);

            _model.PlacePipe((2, 1), (3, 0));
            _model.GameTick(1); // oxygen register
            _model.GameTick(1); // moving in?
            Assert.AreNotEqual(0, _model.map[1, 3].zone_info!.oxygen_supply);
            Assert.AreNotEqual(0, _model.map[1, 3].zone_info!.people.Count);
        }

        [TestMethod]
        public void Connect2DiffusersWithGenerator() {
            _model.PlaceBuilding((2, 1), BuildingType.OXYGEN_DIFFUSER);
            _model.PlaceBuilding((10, 1), BuildingType.OXYGEN_DIFFUSER);

            _model.PlacePipe((2, 1), (4, 1));
            _model.PlacePipe((6, 1), (10, 1));

            _model.PlaceBuilding((4, 0), BuildingType.OXYGEN_GENERATOR);

            _model.GameTick(1); // oxygen register
            for (int p = 0; p < 10; p++)
            {
                _model.GameTick(1);
            }  // moving in?

            Assert.AreNotEqual(0, _model.map[2, 3].zone_info!.oxygen_supply);
            Assert.AreNotEqual(0, _model.map[2, 3].zone_info!.people.Count);

            Assert.AreNotEqual(0, _model.map[8, 3].zone_info!.oxygen_supply);
            Assert.AreNotEqual(0, _model.map[8, 3].zone_info!.people.Count);
        }
    }
}
