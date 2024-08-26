using KFC.Model;
using KFC.Model.Buildings;
using KFC.Model.Zones;
using KFC.Persistence;

namespace KFC.UnitTests {
    [TestClass]
    public class BuildingTest {

        private GameModel _model = null!;
        private int axis0 = 30;
        private int axis1 = 25;

        [TestInitialize]
        public void Init() {
            GameInfo game_inf = new GameInfo();
            game_inf.city_name = "test_city";
            game_inf.money = 100000;
            _model = new GameModel(game_inf);
        }

        [TestMethod]
        public void SanityCheck() {
            // Tests if model initializes correctly
            Assert.AreEqual("test_city", _model.city_name);
            Assert.AreEqual(axis0, _model.map.GetLength(0));
            Assert.AreEqual(axis1, _model.map.GetLength(1));
        }

        #region City Hall
        [TestMethod]
        public void CityHallBlocksCorrectly() {
            // Test if we can place things before City Hall is placed
            _model.PlaceRoad((2, 2), (2, 2));
            Assert.IsNull(_model.map[2, 2].road);

            _model.PlaceBuilding((4, 4), BuildingType.PARK);
            Assert.AreNotEqual(BuildingType.PARK, _model.map[4, 4].building?.building_type);

            _model.PlaceBuilding((7, 7), BuildingType.POLICE_STATION);
            Assert.AreNotEqual(BuildingType.POLICE_STATION, _model.map[7, 7].building?.building_type);

            _model.SetZone((15, 15), (17, 17), ZoneType.RESIDENTIAL);
            Assert.AreEqual(ZoneType.NONE, _model.map[16, 16].zone_type);

            // should be able to place anything after this
            _model.PlaceBuilding((10, 10), BuildingType.CITY_HALL);
            Assert.AreEqual(BuildingType.CITY_HALL, _model.map[10, 10].building?.building_type);

            _model.PlaceBuilding((4, 4), BuildingType.PARK);
            Assert.AreEqual(BuildingType.PARK, _model.map[4, 4].building?.building_type);
        }

        [TestMethod]
        public void CityHallCornerPlacement() {
            _model.PlaceBuilding((29, 24), BuildingType.CITY_HALL);
            Assert.AreNotEqual(BuildingType.CITY_HALL, _model.map[29, 24].building?.building_type);
        }

        [TestMethod]
        public void CityHallStartingPopulation() {
            long population = _model.population;
            _model.PlaceBuilding((10, 10), BuildingType.CITY_HALL);

            Assert.IsTrue(population < _model.population);
        }

        [TestMethod]
        public void CityHallUpgradeable() {     
            _model.PlaceBuilding((10, 10), BuildingType.CITY_HALL);

            int level = ((CityHall)(_model.map[10, 10].building!)).level;
            _model.Upgrade((10, 10));
            Assert.IsTrue(level < ((CityHall)(_model.map[10, 10].building!)).level);

            level = ((CityHall)_model.map[10, 10].building!).level;
            _model.Upgrade((11, 11));
            Assert.IsTrue(level < ((CityHall)(_model.map[10, 10].building!)).level);
        }

        [TestMethod]
        public void CityHallDeletion() {
            // can't delete it
            _model.PlaceBuilding((10, 10), BuildingType.CITY_HALL);
            _model.Delete((10, 10), 0);
            Assert.AreEqual(BuildingType.CITY_HALL, _model.map[10, 10].building?.building_type);
        }
        #endregion

        #region Park
        [TestMethod]
        public void ParkPlacement() {
            _model.PlaceBuilding((0, 0), BuildingType.CITY_HALL);

            // correct spot
            _model.PlaceBuilding((6, 6), BuildingType.PARK);
            Assert.AreEqual(BuildingType.PARK, _model.map[6, 6].building?.building_type);
            Assert.AreEqual(BuildingType.PARK, _model.map[6, 7].building?.building_type);
            Assert.AreEqual(BuildingType.PARK, _model.map[7, 6].building?.building_type);
            Assert.AreEqual(BuildingType.PARK, _model.map[7, 7].building?.building_type);

            // bottom right corner
            _model.PlaceBuilding((29, 24), BuildingType.PARK);
            Assert.AreNotEqual(BuildingType.PARK, _model.map[29, 24].building?.building_type);

            // try inside a park
            _model.PlaceBuilding((7, 7), BuildingType.PARK);
            Assert.AreNotEqual(BuildingType.PARK, _model.map[7, 8].building?.building_type);
            Assert.AreNotEqual(BuildingType.PARK, _model.map[8, 7].building?.building_type);
            Assert.AreNotEqual(BuildingType.PARK, _model.map[8, 8].building?.building_type);

            // try inside road
            _model.PlaceRoad((10, 1), (15, 1));
            _model.PlaceBuilding((10, 0), BuildingType.PARK);
            Assert.AreNotEqual(BuildingType.PARK, _model.map[10, 0].building?.building_type);

            // try inside zone
            _model.SetZone((0, 5), (5, 8), ZoneType.SERVICE);
            _model.PlaceBuilding((3, 6), BuildingType.PARK);
            Assert.AreNotEqual(BuildingType.PARK, _model.map[3, 7].building?.building_type);
        }

        [TestMethod]
        public void ParkDeletion() {
            _model.PlaceBuilding((0, 0), BuildingType.CITY_HALL);
            _model.PlaceBuilding((6, 6), BuildingType.PARK);
            _model.Delete((6, 6), 0);
            Assert.AreNotEqual(BuildingType.PARK, _model.map[6, 6].building?.building_type);
            Assert.AreNotEqual(BuildingType.PARK, _model.map[6, 7].building?.building_type);
            Assert.AreNotEqual(BuildingType.PARK, _model.map[7, 6].building?.building_type);
            Assert.AreNotEqual(BuildingType.PARK, _model.map[7, 7].building?.building_type);

            _model.PlaceBuilding((6, 6), BuildingType.PARK);
            _model.Delete((7, 7), 0);
            Assert.AreNotEqual(BuildingType.PARK, _model.map[6, 6].building?.building_type);
            Assert.AreNotEqual(BuildingType.PARK, _model.map[6, 7].building?.building_type);
            Assert.AreNotEqual(BuildingType.PARK, _model.map[7, 6].building?.building_type);
            Assert.AreNotEqual(BuildingType.PARK, _model.map[7, 7].building?.building_type);
        }

        [TestMethod]
        public void ParkCostsMoney() {
            _model.PlaceBuilding((0, 0), BuildingType.CITY_HALL);
            long money = _model.money;
            _model.PlaceBuilding((15, 15), BuildingType.PARK);
            Assert.IsTrue(money > _model.money);
        }

        [TestMethod]
        public void ParkRefund() {
            _model.PlaceBuilding((0, 0), BuildingType.CITY_HALL);
            _model.PlaceBuilding((15, 15), BuildingType.PARK);
            long money = _model.money;
            _model.Delete((15, 15), 0);
            Assert.IsTrue(money < _model.money);
        }
        #endregion

        #region Police Station
        [TestMethod]
        public void PoliceStationPlacement() {
            _model.PlaceBuilding((0, 0), BuildingType.CITY_HALL);

            // correct spot
            _model.PlaceBuilding((5, 5), BuildingType.POLICE_STATION);
            Assert.AreEqual(BuildingType.POLICE_STATION, _model.map[5, 5].building?.building_type);
            Assert.AreEqual(BuildingType.POLICE_STATION, _model.map[6, 5].building?.building_type);

            // bottom right corner
            _model.PlaceBuilding((29, 24), BuildingType.PARK);
            Assert.AreNotEqual(BuildingType.POLICE_STATION, _model.map[29, 24].building?.building_type);

            // try inside a park
            _model.PlaceBuilding((6, 5), BuildingType.POLICE_STATION);
            Assert.AreNotEqual(BuildingType.POLICE_STATION, _model.map[7, 5].building?.building_type);

            // try inside road
            _model.PlaceRoad((10, 0), (15, 0));
            _model.PlaceBuilding((9, 0), BuildingType.POLICE_STATION);
            _model.PlaceBuilding((15, 0), BuildingType.POLICE_STATION);
            Assert.AreNotEqual(BuildingType.POLICE_STATION, _model.map[9, 0].building?.building_type);
            Assert.AreNotEqual(BuildingType.POLICE_STATION, _model.map[16, 0].building?.building_type);

            // try inside zone
            _model.SetZone((0, 5), (5, 8), ZoneType.SERVICE);
            _model.PlaceBuilding((3, 6), BuildingType.POLICE_STATION);
            Assert.AreNotEqual(BuildingType.POLICE_STATION, _model.map[4, 6].building?.building_type);
        }

        [TestMethod]
        public void PoliceStationDeletion() {
            _model.PlaceBuilding((0, 0), BuildingType.CITY_HALL);
            _model.PlaceBuilding((11, 11), BuildingType.POLICE_STATION);
            _model.Delete((11, 11), 0);
            Assert.AreNotEqual(BuildingType.POLICE_STATION, _model.map[11, 11].building?.building_type);
            Assert.AreNotEqual(BuildingType.POLICE_STATION, _model.map[12, 11].building?.building_type);

            _model.PlaceBuilding((11, 11), BuildingType.POLICE_STATION);
            _model.Delete((12, 11), 0);
            Assert.AreNotEqual(BuildingType.PARK, _model.map[11, 11].building?.building_type);
            Assert.AreNotEqual(BuildingType.PARK, _model.map[12, 11].building?.building_type);
        }

        [TestMethod]
        public void PoliceStationCostsMoney() {
            _model.PlaceBuilding((0, 0), BuildingType.CITY_HALL);
            long money = _model.money;
            _model.PlaceBuilding((15, 15), BuildingType.POLICE_STATION);
            Assert.IsTrue(money > _model.money);
        }

        [TestMethod]
        public void PoliceStationRefund() {
            _model.PlaceBuilding((0, 0), BuildingType.CITY_HALL);
            _model.PlaceBuilding((15, 15), BuildingType.POLICE_STATION);
            long money = _model.money;
            _model.Delete((15, 15), 0);
            Assert.IsTrue(money < _model.money);
        }

        [TestMethod]
        public void PublicOrderRisesAroundPoliceStation() {
            _model.PlaceBuilding((13, 15), BuildingType.CITY_HALL);
            _model.PlaceBuilding((15, 15), BuildingType.POLICE_STATION);
            _model.PlaceRoad((10, 14), (21, 14));
            _model.SetZone((13, 13), (20, 13), ZoneType.RESIDENTIAL);
            _model.SetZone((17, 15), (20, 15), ZoneType.INDUSTRIAL);
            _model.PlaceBuilding((10, 15), BuildingType.OXYGEN_GENERATOR);
            _model.PlaceBuilding((12, 13), BuildingType.OXYGEN_DIFFUSER);
            _model.PlaceBuilding((21, 13), BuildingType.OXYGEN_DIFFUSER);
            _model.PlacePipe((10, 15), (12, 13));
            _model.PlacePipe((12, 13), (21, 13));
            _model.GameTick(1);
            for (int p = 0; p < 10; p++)
            {
                _model.GameTick(1);
            }
            Assert.IsTrue(0 < _model.map[16, 13].zone_info!.public_order);
            Assert.IsTrue(0 < _model.map[15, 13].zone_info!.public_order);
        }
        #endregion

        #region Oxygen Generator

        public void OxygenGeneratorPlacement() {
            _model.PlaceBuilding((0, 0), BuildingType.CITY_HALL);

            // correct spot
            _model.PlaceBuilding((6, 6), BuildingType.OXYGEN_GENERATOR);
            Assert.AreEqual(BuildingType.OXYGEN_GENERATOR, _model.map[6, 6].building?.building_type);
            Assert.AreEqual(BuildingType.OXYGEN_GENERATOR, _model.map[6, 7].building?.building_type);
            Assert.AreEqual(BuildingType.OXYGEN_GENERATOR, _model.map[7, 6].building?.building_type);
            Assert.AreEqual(BuildingType.OXYGEN_GENERATOR, _model.map[7, 7].building?.building_type);
            Assert.AreEqual(BuildingType.OXYGEN_GENERATOR, _model.map[8, 6].building?.building_type);
            Assert.AreEqual(BuildingType.OXYGEN_GENERATOR, _model.map[8, 7].building?.building_type);

            // bottom right corner
            _model.PlaceBuilding((29, 24), BuildingType.OXYGEN_GENERATOR);
            Assert.AreNotEqual(BuildingType.OXYGEN_GENERATOR, _model.map[29, 24].building?.building_type);

            // try inside another generator
            _model.PlaceBuilding((7, 7), BuildingType.OXYGEN_GENERATOR);
            Assert.AreNotEqual(BuildingType.OXYGEN_GENERATOR, _model.map[7, 8].building?.building_type);
            Assert.AreNotEqual(BuildingType.OXYGEN_GENERATOR, _model.map[8, 7].building?.building_type);
            Assert.AreNotEqual(BuildingType.OXYGEN_GENERATOR, _model.map[8, 8].building?.building_type);
            Assert.AreEqual(BuildingType.OXYGEN_GENERATOR, _model.map[9, 7].building?.building_type);
            Assert.AreEqual(BuildingType.OXYGEN_GENERATOR, _model.map[9, 8].building?.building_type);

            // try inside road
            _model.PlaceRoad((10, 1), (15, 1));
            _model.PlaceBuilding((10, 0), BuildingType.OXYGEN_GENERATOR);
            Assert.AreNotEqual(BuildingType.OXYGEN_GENERATOR, _model.map[10, 0].building?.building_type);

            // try inside zone
            _model.SetZone((0, 5), (5, 8), ZoneType.SERVICE);
            _model.PlaceBuilding((3, 6), BuildingType.OXYGEN_GENERATOR);
            Assert.AreNotEqual(BuildingType.OXYGEN_GENERATOR, _model.map[3, 7].building?.building_type);
        }

        [TestMethod]
        public void OxygenGeneratorDeletion() {
            _model.PlaceBuilding((0, 0), BuildingType.CITY_HALL);
            _model.PlaceBuilding((10, 10), BuildingType.OXYGEN_GENERATOR);
            _model.Delete((10, 10), 0);
            Assert.AreNotEqual(BuildingType.OXYGEN_GENERATOR, _model.map[10, 10].building?.building_type);
            Assert.AreNotEqual(BuildingType.OXYGEN_GENERATOR, _model.map[10, 11].building?.building_type);
            Assert.AreNotEqual(BuildingType.OXYGEN_GENERATOR, _model.map[11, 10].building?.building_type);
            Assert.AreNotEqual(BuildingType.OXYGEN_GENERATOR, _model.map[11, 11].building?.building_type);
            Assert.AreNotEqual(BuildingType.OXYGEN_GENERATOR, _model.map[12, 10].building?.building_type);
            Assert.AreNotEqual(BuildingType.OXYGEN_GENERATOR, _model.map[12, 11].building?.building_type);

            _model.PlaceBuilding((10, 10), BuildingType.PARK);
            _model.Delete((12, 11), 0);
            Assert.AreNotEqual(BuildingType.OXYGEN_GENERATOR, _model.map[10, 10].building?.building_type);
            Assert.AreNotEqual(BuildingType.OXYGEN_GENERATOR, _model.map[10, 11].building?.building_type);
            Assert.AreNotEqual(BuildingType.OXYGEN_GENERATOR, _model.map[11, 10].building?.building_type);
            Assert.AreNotEqual(BuildingType.OXYGEN_GENERATOR, _model.map[11, 11].building?.building_type);
            Assert.AreNotEqual(BuildingType.OXYGEN_GENERATOR, _model.map[12, 10].building?.building_type);
            Assert.AreNotEqual(BuildingType.OXYGEN_GENERATOR, _model.map[12, 11].building?.building_type);
        }

        [TestMethod]
        public void OxygenGeneratorCostsMoney() {
            _model.PlaceBuilding((0, 0), BuildingType.CITY_HALL);
            long money = _model.money;
            _model.PlaceBuilding((12, 12), BuildingType.OXYGEN_GENERATOR);
            Assert.IsTrue(money > _model.money);
        }

        [TestMethod]
        public void OxygenGeneratorStationRefund() {
            _model.PlaceBuilding((0, 0), BuildingType.CITY_HALL);
            _model.PlaceBuilding((12, 12), BuildingType.OXYGEN_GENERATOR);
            long money = _model.money;
            _model.Delete((14, 13), 0);
            Assert.IsTrue(money < _model.money);
        }

        #endregion

        #region Oxygen Diffuser

        [TestMethod]
        public void OxygenDiffuserPlacement() {
            _model.PlaceBuilding((0, 0), BuildingType.CITY_HALL);

            _model.PlaceBuilding((11, 11), BuildingType.OXYGEN_DIFFUSER);
            //Assert.AreEqual(BuildingType.OXYGEN_DIFFUSER, _model.map[11, 11].building?.building_type);

            var reference = _model.map[11, 11].building;
            _model.PlaceBuilding((11, 11), BuildingType.OXYGEN_DIFFUSER);
            Assert.AreEqual(reference, _model.map[11, 11].building);
        }

        [TestMethod]
        public void OxygenDiffuserDeletion() {
            _model.PlaceBuilding((0, 0), BuildingType.CITY_HALL);
            _model.PlaceBuilding((21, 21), BuildingType.OXYGEN_DIFFUSER);
            _model.Delete((21, 21), 0);
            Assert.AreNotEqual(BuildingType.OXYGEN_DIFFUSER, _model.map[21, 21].building?.building_type);
        }

        [TestMethod]
        public void OxygenDiffuserCostsMoney() {
            _model.PlaceBuilding((0, 0), BuildingType.CITY_HALL);
            long money = _model.money;
            _model.PlaceBuilding((23, 23), BuildingType.OXYGEN_DIFFUSER);
            Assert.IsTrue(money > _model.money);
        }

        [TestMethod]
        public void OxygenDiffuserStationRefund() {
            _model.PlaceBuilding((0, 0), BuildingType.CITY_HALL);
            _model.PlaceBuilding((24, 24), BuildingType.OXYGEN_DIFFUSER);
            long money = _model.money;
            _model.Delete((24, 24), 0);
            Assert.IsTrue(money < _model.money);
        }

        #endregion
    }
}
