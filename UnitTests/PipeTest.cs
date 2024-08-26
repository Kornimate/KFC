using KFC.Model;
using KFC.Model.Buildings;
using KFC.Model.Zones;
using KFC.Persistence;

namespace KFC.UnitTests {
    [TestClass]
    public class PipeTest {
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

        [TestMethod]
        public void SanityCheck() {
            // Tests if model initializes correctly
            Assert.AreEqual("test_city", _model.city_name);

            Assert.AreEqual(BuildingType.CITY_HALL, _model.map[0, 0].building!.building_type);

            Assert.AreEqual(axis0, _model.map.GetLength(0));
            Assert.AreEqual(axis1, _model.map.GetLength(1));
        }

        [TestMethod]
        public void StraightPipePlacement() {
            // this test only makes pipe that don't overlap in any way
            // single tile
            _model.PlacePipe((23, 23), (23, 23));
            Assert.IsNotNull(_model.map[23, 23].pipe);
            Assert.IsNull(_model.map[23, 24].pipe);
            Assert.IsNull(_model.map[24, 23].pipe);

            // column
            _model.PlacePipe((5, 10), (5, 15));
            for (int i = 10; i <= 15; i++) {
                Assert.IsNotNull(_model.map[5, i].pipe, $"(5;{i})");

                Assert.AreEqual(-1, _model.map[5, i].pipe!.connections[0], $"(5;{i}) RIGHT");

                if (i != 10) Assert.AreNotEqual(-1, _model.map[5, i].pipe!.connections[1], $"(5;{i}) TOP");
                else Assert.AreEqual(-1, _model.map[5, i].pipe!.connections[1], $"(5;{i}) TOP");

                Assert.AreEqual(-1, _model.map[5, i].pipe!.connections[2], $"(5;{i}) LEFT");

                if (i != 15) Assert.AreNotEqual(-1, _model.map[5, i].pipe!.connections[3], $"(5;{i}) BOTTOM");
                else Assert.AreEqual(-1, _model.map[5, i].pipe!.connections[3], $"(5;{i}) BOTTOM");
            }

            // row
            _model.PlacePipe((14, 16), (20, 16));
            for (int i = 14; i <= 20; i++) {
                Assert.IsNotNull(_model.map[i, 16].pipe, $"({i};16)");

                if (i != 20) Assert.AreNotEqual(-1, _model.map[i, 16].pipe!.connections[0], $"({i};16) RIGHT");
                else Assert.AreEqual(-1, _model.map[i, 16].pipe!.connections[0], $"({i};16) RIGHT");

                Assert.AreEqual(-1, _model.map[i, 16].pipe!.connections[1], $"({i};16) TOP");

                if (i != 14) Assert.AreNotEqual(-1, _model.map[i, 16].pipe!.connections[2], $"({i};16) LEFT");
                else Assert.AreEqual(-1, _model.map[i, 16].pipe!.connections[2], $"({i};16) LEFT");

                Assert.AreEqual(-1, _model.map[i, 16].pipe!.connections[3], $"({i};16) BOTTOM");
            }
        }

        [TestMethod]
        public void OverlappingPipes() {
            // shape: +
            _model.PlacePipe((5, 10), (5, 20));
            _model.PlacePipe((2, 13), (8, 13));
            Assert.AreNotEqual(-1, _model.map[5, 13].pipe!.connections[0], $"(5,13) RIGHT");
            Assert.AreNotEqual(-1, _model.map[5, 13].pipe!.connections[1], $"(5,13) TOP");
            Assert.AreNotEqual(-1, _model.map[5, 13].pipe!.connections[2], $"(5,13) LEFT");
            Assert.AreNotEqual(-1, _model.map[5, 13].pipe!.connections[3], $"(5,13) BOTTOM");

            // shape: |-
            _model.PlacePipe((20, 18), (5, 18));
            Assert.AreNotEqual(-1, _model.map[5, 18].pipe!.connections[0], $"(5,18) RIGHT");
            Assert.AreNotEqual(-1, _model.map[5, 18].pipe!.connections[1], $"(5,18) TOP");
            Assert.AreEqual(-1, _model.map[5, 18].pipe!.connections[2], $"(5,18) LEFT");
            Assert.AreNotEqual(-1, _model.map[5, 18].pipe!.connections[3], $"(5,18) BOTTOM");

            // shape: -|
            _model.PlacePipe((2, 19), (5, 19));
            Assert.AreEqual(-1, _model.map[5, 19].pipe!.connections[0], $"(5,19) RIGHT");
            Assert.AreNotEqual(-1, _model.map[5, 19].pipe!.connections[1], $"(5,19) TOP");
            Assert.AreNotEqual(-1, _model.map[5, 19].pipe!.connections[2], $"(5,19) LEFT");
            Assert.AreNotEqual(-1, _model.map[5, 19].pipe!.connections[3], $"(5,19) BOTTOM");

            // shape: ---
            //         |
            _model.PlacePipe((18, 24), (18, 18));
            Assert.AreNotEqual(-1, _model.map[18, 18].pipe!.connections[0], $"(18,18) RIGHT");
            Assert.AreEqual(-1, _model.map[18, 18].pipe!.connections[1], $"(18,18) TOP");
            Assert.AreNotEqual(-1, _model.map[18, 18].pipe!.connections[2], $"(18,18) LEFT");
            Assert.AreNotEqual(-1, _model.map[18, 18].pipe!.connections[3], $"(18,18) BOTTOM");

            // shape:  |
            //        ---
            _model.PlacePipe((19, 15), (19, 18));
            Assert.AreNotEqual(-1, _model.map[19, 18].pipe!.connections[0], $"(18,18) RIGHT");
            Assert.AreNotEqual(-1, _model.map[19, 18].pipe!.connections[1], $"(18,18) TOP");
            Assert.AreNotEqual(-1, _model.map[19, 18].pipe!.connections[2], $"(18,18) LEFT");
            Assert.AreEqual(-1, _model.map[19, 18].pipe!.connections[3], $"(18,18) BOTTOM");
        }

        [TestMethod]
        public void PipeCostsMoney() {
            long money = _model.money;
            _model.PlacePipe((5, 5), (10, 10));
            Assert.IsTrue(money > _model.money);

            money = _model.money;
            _model.PlacePipe((20, 20), (20, 20));
            Assert.IsTrue(money > _model.money);
        }

        [TestMethod]
        public void ZoneAllowsPipePlacement() {
            _model.SetZone((5, 5), (10, 10), ZoneType.RESIDENTIAL);
            _model.PlacePipe((4, 7), (11, 7));
            for (int i = 4; i <= 11; i++) {
                Assert.IsNotNull(_model.map[i, 7].pipe, $"({i};7)");
            }
        }

        [TestMethod]
        public void BuildingAllowsPipePlacement() {
            // can place pipe on top of building
            _model.PlaceBuilding((10, 10), BuildingType.PARK);
            _model.PlacePipe((9, 10), (14, 10));
            for (int i = 10; i <= 14; i++) {
                Assert.IsNotNull(_model.map[i, 10].pipe, $"({i};10)");
            }
        }

        [TestMethod]
        public void EdgeCase1() {
            // testing for incorrect merges we encountered with roads
            _model.PlacePipe((6, 6), (6, 7));
            _model.PlacePipe((6, 9), (6, 8));
            Assert.AreEqual(-1, _model.map[6, 7].pipe!.connections[0]);
            Assert.AreNotEqual(-1, _model.map[6, 7].pipe!.connections[1]);
            Assert.AreEqual(-1, _model.map[6, 7].pipe!.connections[2]);
            Assert.AreNotEqual(-1, _model.map[6, 7].pipe!.connections[3]);

            Assert.AreEqual(-1, _model.map[6, 8].pipe!.connections[0]);
            Assert.AreNotEqual(-1, _model.map[6, 8].pipe!.connections[1]);
            Assert.AreEqual(-1, _model.map[6, 8].pipe!.connections[2]);
            Assert.AreNotEqual(-1, _model.map[6, 8].pipe!.connections[3]);

            Assert.AreEqual(1, _model.pipe_network.Count);
        }

        [TestMethod]
        public void EdgeCase2() {
            // testing for incorrect merges we encountered with roads
            _model.PlacePipe((3, 1), (3, 2));
            _model.PlacePipe((1, 3), (2, 3));
            _model.PlacePipe((5, 3), (3, 3));

            Assert.AreNotEqual(-1, _model.map[3, 3].pipe!.connections[0]);
            Assert.AreNotEqual(-1, _model.map[3, 3].pipe!.connections[1]);
            Assert.AreNotEqual(-1, _model.map[3, 3].pipe!.connections[2]);
            Assert.AreEqual(-1, _model.map[3, 3].pipe!.connections[3]);

            Assert.AreEqual(1, _model.pipe_network.Count);
        }
    }
}
