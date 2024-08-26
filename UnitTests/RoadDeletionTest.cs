using KFC.Model.Buildings;
using KFC.Model.Zones;
using KFC.Model;
using KFC.Persistence;

namespace KFC.UnitTests {
    [TestClass]
    public class RoadDeletionTest {
        private GameModel _model = null!;
        private int axis0 = 30;
        private int axis1 = 25;

        [TestInitialize]
        public void Init() {
            GameInfo game_inf = new GameInfo();
            game_inf.city_name = "test_city";
            game_inf.money = 100000;
            _model = new GameModel(game_inf);
            _model.PlaceBuilding((0, 0), BuildingType.CITY_HALL);
        }

        public void SetUpScenario() {
            _model.PlaceRoad((0, 2), (20, 2));
            _model.PlaceRoad((8, 3), (8, 4));
            _model.PlaceRoad((9, 4), (20, 4));
            _model.SetZone((10, 1), (14, 1), ZoneType.RESIDENTIAL);
            _model.SetZone((10, 3), (12, 3), ZoneType.SERVICE);
            _model.SetZone((13, 3), (14, 3), ZoneType.INDUSTRIAL);
            _model.PlaceBuilding((13, 5), BuildingType.OXYGEN_GENERATOR);
            _model.PlaceBuilding((12, 5), BuildingType.OXYGEN_DIFFUSER);
            _model.PlacePipe((13, 5), (12, 5));
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
        public void RoadDeletionRefund() {
            _model.PlaceRoad((0, 2), (10, 2));
            long money = _model.money;
            for(int i = 0; i <= 10; i++) {
                _model.Delete((i, 2), 0);
            }
            Assert.IsTrue(money < _model.money);
        }

        [TestMethod]
        public void RoadSimpleDelete() {
            _model.PlaceRoad((0, 2), (10, 2));
            for (int i = 0; i <= 10; i++) {
                _model.Delete((i, 2), 0);
            }
            for(int i = 0; i <=10; i++) {
                Assert.IsNull(_model.map[i, 2].road, $"({i}, 2)");
            }
        }

        [TestMethod]
        public void RoadNonOccupiedDisconnect() {
            SetUpScenario();
            _model.Delete((9, 2), 0);
            Assert.IsNull(_model.map[9, 2].road);
        }

        [TestMethod]
        public void RoadOccupiedCantDisconnect() {
            SetUpScenario();
            _model.GameTick(1); // oxygen
            _model.GameTick(1); // moving in
            _model.Delete((9, 2), 0);
            Assert.IsNotNull(_model.map[9, 2].road);
        }

        [TestMethod]
        public void RoadNonNetworkBreakingDelete() {
            SetUpScenario();
            _model.PlaceRoad((20, 3), (20, 3));
            _model.GameTick(1); // oxygen
            _model.GameTick(1); // moving in
            _model.Delete((9, 2), 0);
            Assert.IsNull(_model.map[9, 2].road);
        }

        [TestMethod]
        public void RoadMultipleNetworkDelete() {
            SetUpScenario();
            _model.PlaceRoad((9, 1), (9, 0));
            _model.Delete((9, 2), 0);
            Assert.IsNull(_model.map[9, 2].road);
            Assert.IsTrue(_model.road_network.Count > 1);
            // don't allow break, even if 1 network could be broken off
            _model.PlaceRoad((9, 2), (9, 2));
            _model.GameTick(1); // oxygen
            _model.GameTick(1); // moving in
            _model.Delete((9, 2), 0);
            Assert.IsNotNull(_model.map[9, 2].road);
        }

        [TestMethod]
        public void RoadBigBuildingDelete() {
            SetUpScenario();
            _model.GameTick(1); // oxygen
            _model.GameTick(1); // moving in
            _model.Delete((0, 2), 0);
            Assert.IsNull(_model.map[0, 2].road);
        }

        [TestMethod]
        public void RoadDeleteConnectionsTest() {
            SetUpScenario();
            _model.Delete((9, 2), 0);
            Assert.AreEqual(-1, _model.map[8, 2].road!.connections[0]);
            Assert.AreEqual(-1, _model.map[10, 2].road!.connections[2]);
        }

        [TestMethod]
        public void RoadDeleteConnectedNetwork() {
            // this is only possible if you have a single road next to the city hall
            _model.PlaceRoad((0, 2), (0, 2));
            Assert.IsNotNull(_model.map[0, 2].road);
            _model.Delete((0, 2), 0);
            Assert.IsNull(_model.map[0, 2].road);
        }
    }
}
