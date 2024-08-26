using KFC.Model;
using KFC.Model.Buildings;
using KFC.Model.Zones;
using KFC.Persistence;

namespace KFC.UnitTests {
    [TestClass]
    public class RoadPlacementTest {
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
        public void StraightRoadPlacement() {
            // this test only makes roads that don't overlap in any way
            // single tile
            _model.PlaceRoad((23, 23), (23, 23));
            Assert.IsNotNull(_model.map[23, 23].road);
            Assert.IsNull(_model.map[23, 24].road);
            Assert.IsNull(_model.map[24, 23].road);

            // column
            _model.PlaceRoad((5, 10), (5, 15));
            for (int i = 10; i <= 15; i++) {
                Assert.IsNotNull(_model.map[5, i].road, $"(5;{i})");

                Assert.AreEqual(-1, _model.map[5, i].road!.connections[0], $"(5;{i}) RIGHT");

                if (i != 10) Assert.AreNotEqual(-1, _model.map[5, i].road!.connections[1], $"(5;{i}) TOP");
                else Assert.AreEqual(-1, _model.map[5, i].road!.connections[1], $"(5;{i}) TOP");

                Assert.AreEqual(-1, _model.map[5, i].road!.connections[2], $"(5;{i}) LEFT");
                
                if (i != 15) Assert.AreNotEqual(-1, _model.map[5, i].road!.connections[3], $"(5;{i}) BOTTOM");
                else Assert.AreEqual(-1, _model.map[5, i].road!.connections[3], $"(5;{i}) BOTTOM");
            }

            // row
            _model.PlaceRoad((14, 16), (20, 16));
            for (int i = 14; i <= 20; i++) {
                Assert.IsNotNull(_model.map[i, 16].road, $"({i};16)");

                if (i != 20) Assert.AreNotEqual(-1, _model.map[i, 16].road!.connections[0], $"({i};16) RIGHT");
                else Assert.AreEqual(-1, _model.map[i, 16].road!.connections[0], $"({i};16) RIGHT");

                Assert.AreEqual(-1, _model.map[i, 16].road!.connections[1], $"({i};16) TOP");

                if (i != 14) Assert.AreNotEqual(-1, _model.map[i, 16].road!.connections[2], $"({i};16) LEFT");
                else Assert.AreEqual(-1, _model.map[i, 16].road!.connections[2], $"({i};16) LEFT");

                Assert.AreEqual(-1, _model.map[i, 16].road!.connections[3], $"({i};16) BOTTOM");
            }
        }

        [TestMethod]
        public void OverlappingRoads() {
            // shape: +
            _model.PlaceRoad((5, 10), (5, 20));
            _model.PlaceRoad((2, 13), (8, 13));
            Assert.AreNotEqual(-1, _model.map[5, 13].road!.connections[0], $"(5,13) RIGHT");
            Assert.AreNotEqual(-1, _model.map[5, 13].road!.connections[1], $"(5,13) TOP");
            Assert.AreNotEqual(-1, _model.map[5, 13].road!.connections[2], $"(5,13) LEFT");
            Assert.AreNotEqual(-1, _model.map[5, 13].road!.connections[3], $"(5,13) BOTTOM");

            // shape: |-
            _model.PlaceRoad((20, 18), (5, 18));
            Assert.AreNotEqual(-1, _model.map[5, 18].road!.connections[0], $"(5,18) RIGHT");
            Assert.AreNotEqual(-1, _model.map[5, 18].road!.connections[1], $"(5,18) TOP");
            Assert.AreEqual(-1, _model.map[5, 18].road!.connections[2], $"(5,18) LEFT");
            Assert.AreNotEqual(-1, _model.map[5, 18].road!.connections[3], $"(5,18) BOTTOM");

            // shape: -|
            _model.PlaceRoad((2, 19), (5, 19));
            Assert.AreEqual(-1, _model.map[5, 19].road!.connections[0], $"(5,19) RIGHT");
            Assert.AreNotEqual(-1, _model.map[5, 19].road!.connections[1], $"(5,19) TOP");
            Assert.AreNotEqual(-1, _model.map[5, 19].road!.connections[2], $"(5,19) LEFT");
            Assert.AreNotEqual(-1, _model.map[5, 19].road!.connections[3], $"(5,19) BOTTOM");

            // shape: ---
            //         |
            _model.PlaceRoad((18, 24), (18, 18));
            Assert.AreNotEqual(-1, _model.map[18, 18].road!.connections[0], $"(18,18) RIGHT");
            Assert.AreEqual(-1, _model.map[18, 18].road!.connections[1], $"(18,18) TOP");
            Assert.AreNotEqual(-1, _model.map[18, 18].road!.connections[2], $"(18,18) LEFT");
            Assert.AreNotEqual(-1, _model.map[18, 18].road!.connections[3], $"(18,18) BOTTOM");

            // shape:  |
            //        ---
            _model.PlaceRoad((19, 15), (19, 18));
            Assert.AreNotEqual(-1, _model.map[19, 18].road!.connections[0], $"(18,18) RIGHT");
            Assert.AreNotEqual(-1, _model.map[19, 18].road!.connections[1], $"(18,18) TOP");
            Assert.AreNotEqual(-1, _model.map[19, 18].road!.connections[2], $"(18,18) LEFT");
            Assert.AreEqual(-1, _model.map[19, 18].road!.connections[3], $"(18,18) BOTTOM");
        }

        [TestMethod]
        public void RoadCostsMoney() {
            long money = _model.money;
            _model.PlaceRoad((5, 5), (10, 10));
            Assert.IsTrue(money > _model.money);

            money = _model.money;
            _model.PlaceRoad((20,20), (20, 20));
            Assert.IsTrue(money > _model.money);
        }

        [TestMethod]
        public void ZoneBlocksRoadPlacement() {
            _model.SetZone((5, 5), (10, 10), ZoneType.RESIDENTIAL);
            _model.PlaceRoad((4, 7), (11, 7));
            for (int i = 5; i <= 10; i++) {
                for (int j = 5; j <= 10; j++) {
                    Assert.IsNull(_model.map[i, j].road, $"({i};{j})");
                }
            }
            Assert.IsNotNull(_model.map[4, 7]);
            Assert.IsNotNull(_model.map[11, 7]);
        }

        [TestMethod]
        public void BuildingBlocksRoadPlacement() {
            // can't place road on top of building
            _model.PlaceBuilding((10, 10), BuildingType.PARK);
            _model.PlaceRoad((10, 10), (15, 15));
            for (int i = 10; i <= 15; i++) {
                for (int j = 10; j <= 15; j++) {
                    Assert.IsNull(_model.map[i, j].road, $"({i};{j})");
                }
            }

            // road avoids building, adding a police station to make it harder
            _model.PlaceBuilding((12, 10), BuildingType.POLICE_STATION);
            _model.PlaceBuilding((12, 11), BuildingType.POLICE_STATION);
            _model.PlaceRoad((9, 9), (13, 13));
            for (int i = 10; i <= 13; i++) {
                for (int j = 10; j <= 11; j++) {
                    Assert.IsNull(_model.map[i, j].road, $"({i};{j})");
                }
            }
            Assert.IsNotNull(_model.map[9, 9]);
            Assert.IsNotNull(_model.map[13, 13]);
        }

        [TestMethod]
        public void UnreachableSpot() {
            _model.SetZone((10, 0), (15, 24), ZoneType.SERVICE);
            _model.PlaceRoad((6, 10), (18, 23));
            for (int i = 0; i <= 29; i++) {
                for (int j = 0; j <= 24; j++) {
                    Assert.IsNull(_model.map[i, j].road, $"({i};{j})");
                }
            }
        }

        [TestMethod]
        public void EdgeCase1() {
            // testing for incorrect merges we encountered
            _model.PlaceRoad((6, 6), (6, 7));
            _model.PlaceRoad((6, 9), (6, 8));
            Assert.AreEqual(-1, _model.map[6, 7].road!.connections[0]);
            Assert.AreNotEqual(-1, _model.map[6, 7].road!.connections[1]);
            Assert.AreEqual(-1, _model.map[6, 7].road!.connections[2]);
            Assert.AreNotEqual(-1, _model.map[6, 7].road!.connections[3]);

            Assert.AreEqual(-1, _model.map[6, 8].road!.connections[0]);
            Assert.AreNotEqual(-1, _model.map[6, 8].road!.connections[1]);
            Assert.AreEqual(-1, _model.map[6, 8].road!.connections[2]);
            Assert.AreNotEqual(-1, _model.map[6, 8].road!.connections[3]);

            Assert.AreEqual(1, _model.road_network.Count);
        }

        [TestMethod]
        public void EdgeCase2() {
            // testing for incorrect merges we encountered
            _model.PlaceRoad((3, 1), (3, 2));
            _model.PlaceRoad((1, 3), (2, 3));
            _model.PlaceRoad((5, 3), (3, 3));

            Assert.AreNotEqual(-1, _model.map[3, 3].road!.connections[0]);
            Assert.AreNotEqual(-1, _model.map[3, 3].road!.connections[1]);
            Assert.AreNotEqual(-1, _model.map[3, 3].road!.connections[2]);
            Assert.AreEqual(-1, _model.map[3, 3].road!.connections[3]);

            Assert.AreEqual(1, _model.road_network.Count);
        }

    }
}
