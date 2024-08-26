using KFC.Model;
using KFC.Model.Buildings;
using KFC.Model.Zones;
using KFC.Persistence;

namespace KFC.UnitTests {
    [TestClass]
    public class ZonePlacementTest {

        private GameModel _model = null!;
        private int axis0 = 30;
        private int axis1 = 25;

        [TestInitialize]
        public void Init() {
            GameInfo game_inf = new GameInfo();
            game_inf.city_name = "test_city";
            game_inf.money = 100000;
            _model = new GameModel(game_inf);
            _model.PlaceBuilding((10, 10), BuildingType.CITY_HALL);
        }

        [TestMethod]
        public void SanityCheck() {
            // Tests if model initializes correctly
            Assert.AreEqual("test_city", _model.city_name);

            Assert.AreEqual(BuildingType.CITY_HALL, _model.map[10, 10].building!.building_type);

            Assert.AreEqual(axis0, _model.map.GetLength(0));
            Assert.AreEqual(axis1, _model.map.GetLength(1));
        }

        #region No Overlap
        [TestMethod]
        public void ResidentialNoOverlap() {
            // Single tile
            _model.SetZone((23, 23), (23, 23), ZoneType.RESIDENTIAL);
            Assert.AreEqual(ZoneType.RESIDENTIAL, _model.map[23, 23].zone_type);
            Assert.AreNotEqual(ZoneType.SERVICE, _model.map[23, 24].zone_type);
            Assert.AreNotEqual(ZoneType.INDUSTRIAL, _model.map[24, 23].zone_type);

            // Single row & column
            _model.SetZone((2, 10), (2, 15), ZoneType.RESIDENTIAL);
            for (int i = 10; i <= 15; i++) {
                Assert.AreEqual(ZoneType.RESIDENTIAL, _model.map[2, i].zone_type, $"(2;{i})");
            }

            _model.SetZone((13, 4), (16, 4), ZoneType.RESIDENTIAL);
            for (int i = 13; i <= 16; i++) {
                Assert.AreEqual(ZoneType.RESIDENTIAL, _model.map[i, 4].zone_type, $"({i};4)");
            }

            // Block
            _model.SetZone((1, 1), (5, 6), ZoneType.RESIDENTIAL);
            for(int i = 1; i <= 5; i++) {
                for(int j = 1; j <= 6; j++) {
                    Assert.AreEqual(ZoneType.RESIDENTIAL, _model.map[i, j].zone_type, $"({i};{j})");
                }
            }
        }

        [TestMethod]
        public void IndustrialNoOverlap() {
            // Single tile
            _model.SetZone((20, 20), (20, 20), ZoneType.INDUSTRIAL);
            Assert.AreEqual(ZoneType.INDUSTRIAL, _model.map[20, 20].zone_type);
            Assert.AreNotEqual(ZoneType.RESIDENTIAL, _model.map[21, 20].zone_type);
            Assert.AreNotEqual(ZoneType.SERVICE, _model.map[20, 21].zone_type);

            // Single row & column
            _model.SetZone((0, 15), (0, 20), ZoneType.INDUSTRIAL);
            for (int i = 15; i <= 20; i++) {
                Assert.AreEqual(ZoneType.INDUSTRIAL, _model.map[0, i].zone_type, $"(0;{i})");
            }

            _model.SetZone((15, 0), (20, 0), ZoneType.INDUSTRIAL);
            for (int i = 15; i <= 20; i++) {
                Assert.AreEqual(ZoneType.INDUSTRIAL, _model.map[i, 0].zone_type, $"({i};0)");
            }

            // Block
            _model.SetZone((0, 0), (5, 6), ZoneType.INDUSTRIAL);
            for (int i = 0; i <= 5; i++) {
                for (int j = 0; j <= 6; j++) {
                    Assert.AreEqual(ZoneType.INDUSTRIAL, _model.map[i, j].zone_type, $"({i};{j})");
                }
            }
        }

        [TestMethod]
        public void ServiceNoOverlap() {
            // Single tile
            _model.SetZone((17, 16), (17, 16), ZoneType.SERVICE);
            Assert.AreEqual(_model.map[17, 16].zone_type, ZoneType.SERVICE);
            Assert.AreNotEqual(_model.map[17, 17].zone_type, ZoneType.INDUSTRIAL);
            Assert.AreNotEqual(_model.map[17, 15].zone_type, ZoneType.RESIDENTIAL);

            // Single row & column
            _model.SetZone((5, 15), (5, 20), ZoneType.SERVICE);
            for (int i = 15; i <= 20; i++) {
                Assert.AreEqual(_model.map[5, i].zone_type, ZoneType.SERVICE, $"(5;{i})");
            }

            _model.SetZone((15, 2), (20, 2), ZoneType.SERVICE);
            for (int i = 15; i <= 20; i++) {
                Assert.AreEqual(_model.map[i, 2].zone_type, ZoneType.SERVICE, $"({i};2)");
            }

            // Block
            _model.SetZone((0, 0), (3, 4), ZoneType.SERVICE);
            for (int i = 0; i <= 3; i++) {
                for (int j = 0; j <= 4; j++) {
                    Assert.AreEqual(_model.map[i, j].zone_type, ZoneType.SERVICE, $"({i};{j})");
                }
            }
        }
        #endregion

        #region Cost
        [TestMethod]
        public void ZoneCostsMoney() {
            long money = _model.money;
            _model.SetZone((0, 0), (5, 5), ZoneType.RESIDENTIAL);
            Assert.IsTrue(money > _model.money);

            money = _model.money;
            _model.SetZone((6, 6), (6, 6), ZoneType.INDUSTRIAL);
            Assert.IsTrue(money > _model.money);

            money = _model.money;
            _model.SetZone((15, 15), (18, 18), ZoneType.SERVICE);
            Assert.IsTrue(money > _model.money);
        }

        [TestMethod]
        public void ZoneOverlapCost() {
            _model.SetZone((0, 0), (5, 5), ZoneType.RESIDENTIAL);
            long money = _model.money;
            _model.SetZone((2, 2), (4, 4), ZoneType.RESIDENTIAL);
            Assert.AreEqual(money, _model.money);

            _model.SetZone((16, 16), (20, 18), ZoneType.SERVICE);
            money = _model.money;
            _model.SetZone((17, 17), (19, 18), ZoneType.SERVICE);
            Assert.AreEqual(money, _model.money);

            _model.SetZone((25, 20), (26, 23), ZoneType.INDUSTRIAL);
            money = _model.money;
            _model.SetZone((25, 22), (26, 22), ZoneType.INDUSTRIAL);
            Assert.AreEqual(money, _model.money);

            money = _model.money;
            _model.SetZone((1, 1), (6, 4), ZoneType.RESIDENTIAL);
            Assert.IsTrue(money > _model.money);

            money = _model.money;
            _model.SetZone((17, 17), (21, 19), ZoneType.SERVICE);
            Assert.IsTrue(money > _model.money);

            money = _model.money;
            _model.SetZone((26, 22), (27, 24), ZoneType.INDUSTRIAL);
            Assert.IsTrue(money > _model.money);
        }

        [TestMethod]
        public void ZoneDeletionRefund() {
            _model.SetZone((0, 0), (5, 5), ZoneType.RESIDENTIAL);
            long money = _model.money;
            _model.SetZone((1, 1), (4, 4), ZoneType.NONE);
            Assert.IsTrue(money < _model.money);

            _model.SetZone((16, 16), (20, 18), ZoneType.SERVICE);
            money = _model.money;
            _model.SetZone((17, 17), (21, 19), ZoneType.NONE);
            Assert.IsTrue(money < _model.money);

            _model.SetZone((25, 20), (26, 23), ZoneType.INDUSTRIAL);
            money = _model.money;
            _model.SetZone((26, 22), (27, 24), ZoneType.NONE);
            Assert.IsTrue(money < _model.money);
        }
        #endregion

        #region Overlap
        [TestMethod]
        public void OverlappingZonePlacement() {
            // Single tile then encapsulating selection
            _model.SetZone((23, 23), (23, 23), ZoneType.RESIDENTIAL);
            _model.SetZone((23, 22), (23, 24), ZoneType.INDUSTRIAL);
            Assert.AreEqual(ZoneType.INDUSTRIAL, _model.map[23, 22].zone_type);
            Assert.AreEqual(ZoneType.RESIDENTIAL, _model.map[23, 23].zone_type);
            Assert.AreEqual(ZoneType.INDUSTRIAL, _model.map[23, 24].zone_type);

            // Single row then intersecting placement
            _model.SetZone((14, 14), (19, 14), ZoneType.SERVICE);
            _model.SetZone((16, 12), (16, 16), ZoneType.INDUSTRIAL);
            for (int i = 14; i <= 19; i++) {
                Assert.AreEqual(ZoneType.SERVICE, _model.map[i, 14].zone_type, $"({i};14)");
            }
            for (int i = 12; i <= 16; i++) {
                if(i == 14) Assert.AreEqual(ZoneType.SERVICE, _model.map[14, i].zone_type, $"(14;{i})");
                else Assert.AreEqual(ZoneType.INDUSTRIAL, _model.map[16, i].zone_type, $"(14;{i})");
            }

            // Block selection then block selection
            _model.SetZone((1, 1), (5, 6), ZoneType.RESIDENTIAL);
            _model.SetZone((2, 2), (7, 8), ZoneType.SERVICE);
            for (int i = 1; i <= 5; i++) {
                for (int j = 1; j <= 6; j++) {
                    Assert.AreEqual(ZoneType.RESIDENTIAL, _model.map[i, j].zone_type, $"({i};{j})");
                }
            }
            for (int i = 6; i <= 7; i++) {
                for (int j = 7; j <= 8; j++) {
                    Assert.AreEqual(ZoneType.SERVICE, _model.map[i, j].zone_type, $"({i};{j})");
                }
            }
        }

        [TestMethod]
        public void ZonePlacementBuildingOverlap() {
            // city hall is on 10;10-11;11 from Init()
            _model.SetZone((8,8), (13,13), ZoneType.RESIDENTIAL);

            for (int i = 8; i <= 13; i++) {
                for (int j = 8; j <= 13; j++) {
                    if (i >= 10 && i <= 11 && j >= 10 && j <= 11) Assert.AreEqual(ZoneType.NONE, _model.map[i, j].zone_type, $"({i};{j})");
                    else Assert.AreEqual(ZoneType.RESIDENTIAL, _model.map[i, j].zone_type, $"({i};{j})");
                }
            }

            _model.PlaceBuilding((20, 20), BuildingType.PARK);
            _model.SetZone((19, 19), (21, 23), ZoneType.SERVICE);
            for (int i = 19; i <= 21; i++) {
                for (int j = 21; j <= 23; j++) {
                    if (i >= 20 && i <= 21 && j >= 20 && j <= 21) Assert.AreEqual(ZoneType.NONE, _model.map[i, j].zone_type, $"({i};{j})");
                    else Assert.AreEqual(ZoneType.SERVICE, _model.map[i, j].zone_type, $"({i};{j})");
                }
            }

            _model.PlaceBuilding((15, 15), BuildingType.POLICE_STATION);
            _model.SetZone((14, 14), (17, 16), ZoneType.INDUSTRIAL);
            for (int i = 14; i <= 17; i++) {
                for (int j = 14; j <= 16; j++) {
                    if (i >= 15 && i <= 16 && j == 15) Assert.AreEqual(ZoneType.NONE, _model.map[i, j].zone_type, $"({i};{j})");
                    else Assert.AreEqual(ZoneType.INDUSTRIAL, _model.map[i, j].zone_type, $"({i};{j})");
                }
            }
        }
        #endregion
    }
}