using KFC.Model;
using KFC.Model.Buildings;
using KFC.Model.Zones;
using KFC.Persistence;

namespace KFC.UnitTests {
    [TestClass]
    public class PipeDeletionTest {
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
            _model.PlaceBuilding((0, 3), BuildingType.OXYGEN_GENERATOR);
            _model.PlaceBuilding((6, 3), BuildingType.OXYGEN_DIFFUSER);
            _model.PlaceBuilding((8, 3), BuildingType.OXYGEN_DIFFUSER);
            _model.PlaceBuilding((12, 3), BuildingType.OXYGEN_GENERATOR);
            _model.SetZone((5, 1), (7, 1), ZoneType.RESIDENTIAL);
            _model.SetZone((8, 1), (8, 1), ZoneType.INDUSTRIAL);
            _model.SetZone((9, 1), (10, 1), ZoneType.SERVICE);
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
        public void PipeDeletionRefund() {
            _model.PlacePipe((0, 2), (10, 2));
            long money = _model.money;
            for (int i = 0; i <= 10; i++) {
                _model.Delete((i, 2), -1);
            }
            Assert.IsTrue(money < _model.money);
        }

        [TestMethod]
        public void PipeSimpleDelete() {
            _model.PlacePipe((2, 6), (2, 10));
            for (int i = 6; i <= 10; i++) {
                _model.Delete((2, i), -1);
            }
            for (int i = 6; i <= 10; i++) {
                Assert.IsNull(_model.map[2, i].pipe, $"(2, {i})");
            }
        }

        [TestMethod]
        public void PipeOnlyGeneratorConnectedDelete() {
            SetUpScenario();
            _model.PlacePipe((2, 3), (5, 3));
            for (int i = 5; i >= 2; i--) {
                _model.Delete((i, 3), -1);
            }
            for (int i = 3; i <= 5; i++) {
                Assert.IsNull(_model.map[i, 3].pipe, $"({i}, 3)");
            }
            Assert.IsNotNull(_model.map[2, 3].pipe);
        }

        [TestMethod]
        public void PipeOnlyDiffuserConnectedDelete() {
            SetUpScenario();
            _model.PlacePipe((3, 3), (6, 3));
            for (int i = 3; i <= 6; i++) {
                _model.Delete((i, 3), -1);
            }
            for (int i = 3; i <= 5; i++) {
                Assert.IsNull(_model.map[i, 3].pipe, $"({i}, 3)");
            }
            // we established that if a diffuser could supply zones, you can't delete the pipe from under it
            Assert.IsNotNull(_model.map[6, 3].pipe);
        }

        [TestMethod]
        public void PipeConnectedCantDelete() {
            SetUpScenario();
            _model.PlacePipe((2, 3), (6, 3));
            _model.Delete((4, 3), -1);
            Assert.IsNotNull(_model.map[4, 3].pipe);
        }

        [TestMethod]
        public void PipeEdgeCase() {
            // if you place pipe on a diffuser,
            //      then delete the pipe,
            //      and then place zones in radius of the diffuser,
            //      you get an exception from the model
            _model.PlaceRoad((0, 2), (20, 2));
            _model.PlaceBuilding((5, 3), BuildingType.OXYGEN_DIFFUSER);
            _model.PlacePipe((5, 3), (5, 3));
            _model.Delete((5, 3), -1);
            _model.SetZone((4, 1), (6, 1), ZoneType.RESIDENTIAL);
            // no assert, because we only check if no exception occured
        }

        [TestMethod]
        public void PipeConnectingDiffusersDelete() {
            SetUpScenario();
            _model.PlacePipe((2, 3), (8, 3));
            _model.Delete((7, 3), -1);
            Assert.IsNotNull(_model.map[7, 3].pipe);
        }

        [TestMethod]
        public void PipeDeletingFromNetwork() {
            SetUpScenario();
            _model.PlacePipe((2, 3), (8, 3));
            _model.PlacePipe((4, 4), (4, 6));
            _model.Delete((4, 4), -1);
            Assert.IsNull(_model.map[4, 4].pipe);
        }

        [TestMethod]
        public void PipeNonNetworkBreakingDelete() {
            SetUpScenario();
            _model.PlacePipe((2, 3), (12, 3));
            _model.PlacePipe((6, 4), (6, 5));
            _model.PlacePipe((8, 4), (8, 5));
            _model.PlacePipe((7, 5), (7, 5));
            _model.Delete((7, 5), -1);
            Assert.IsNull(_model.map[7, 5].pipe);
        }

        [TestMethod]
        public void PipeDeleteConnectionsTest() {
            _model.PlacePipe((4, 0), (10, 0));
            _model.PlacePipe((5, 1), (7, 1));
            _model.Delete((6, 0), -1);
            Assert.AreEqual(-1, _model.map[5, 0].pipe!.connections[0]);
            Assert.AreEqual(-1, _model.map[7, 0].pipe!.connections[2]);
        }

    }
}
