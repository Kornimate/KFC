using KFC.Model;
using KFC.Model.Buildings;
using KFC.Model.Zones;
using KFC.Persistence;

namespace KFC.UnitTests {
    [TestClass]
    public class MeteorTest {
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

            _model.PlaceRoad((5, 7), (17, 7));
            _model.PlaceRoad((5, 9), (17, 9));
            _model.PlaceRoad((5, 12), (17, 12));
            _model.PlaceRoad((4, 7), (4, 12));

            _model.SetZone((5, 8), (17, 8), ZoneType.RESIDENTIAL);
            _model.SetZone((5, 10), (14, 11), ZoneType.SERVICE);
            _model.SetZone((15, 10), (17, 11), ZoneType.INDUSTRIAL);
            _model.PlaceBuilding((7, 5), BuildingType.PARK);
            _model.PlaceBuilding((9, 6), BuildingType.POLICE_STATION);
            _model.PlaceBuilding((11, 5), BuildingType.OXYGEN_GENERATOR);
            _model.PlaceBuilding((14, 6), BuildingType.OXYGEN_DIFFUSER);
            _model.PlaceBuilding((17, 6), BuildingType.OXYGEN_DIFFUSER);
            _model.PlaceBuilding((3, 6), BuildingType.OXYGEN_DIFFUSER);
            _model.PlacePipe((11, 5), (14, 6));
            _model.PlacePipe((17, 6), (14, 6));
            _model.PlacePipe((11, 5), (3, 6));

            _model.GameTick(1); // for moving out of city hall
            _model.GameTick(1); // for satisfaction to be calculated after moving out
        }

        [TestMethod]
        public void InitTest() {
            Assert.AreEqual("test_city", _model.city_name);

            Assert.AreEqual(BuildingType.CITY_HALL, _model.map[5, 5].building?.building_type);

            Assert.AreEqual(axis0, _model.map.GetLength(0));
            Assert.AreEqual(axis1, _model.map.GetLength(1));

            // placements are already tested in other tests
            for (int i = 5; i <= 14; i++) {
                Assert.IsTrue(_model.map[i, 8].zone_info!.people.Count >= 0, $"{i};8");
            }

            for (int i = 5; i <= 14; i++) {
                Assert.IsTrue(_model.map[i, 10].zone_info!.people.Count >= 0, $"{i};10");
                Assert.IsTrue(_model.map[i, 11].zone_info!.people.Count >= 0, $"{i};11");
                //As the industrial and service zones are chosen equally now,
                //there are no longer enough residents to fill up every service zone.
            }
        }

        [TestMethod]
        public void MeteorLands() {
            _model.m_rand = new Random(12);
            _model.InvokeCatastrophe(2);
            _model.RegisterCatastrophe();

            bool damaged = false;
            for(int i = 0; i < axis0; i++) {
                for(int j = 0; j < axis1; j++) {
                    damaged |= _model.map[i, j].zone_info?.damage > 0;
                    damaged |= _model.map[i, j].building?.damage > 0;
                }
            }
            Assert.IsTrue(damaged);
        }

        public void MeteorDamages(int intensity, string expected) {
            _model.InvokeCatastrophe(intensity);
            _model.RegisterCatastrophe();

            (int, int) damaged = (-1, -1);
            for (int i = 0; i < axis0; i++) {
                for (int j = 0; j < axis1; j++) {
                    if (damaged == (-1, -1) &&
                        (_model.map[i, j].zone_info?.damage > 0 || _model.map[i, j].building?.damage > 0) &&
                        (_model.map[i, j].zone_type != ZoneType.NONE || _model.map[i, j].building != null))
                        damaged = (i, j);
                }
            }

            Assert.IsTrue(damaged != (-1, -1), $"Testing for: {expected}");
            Assert.IsTrue(_model.map[damaged.Item1, damaged.Item2].zone_info?.damage > 0 ||
                          _model.map[damaged.Item1, damaged.Item2].building?.damage > 0, $"Testing for: {expected}");

            _model.Repair(damaged);
            Assert.IsTrue(_model.map[damaged.Item1, damaged.Item2].zone_info?.damage == 0 ||
                          _model.map[damaged.Item1, damaged.Item2].building?.damage == 0, $"Testing for: {expected}");

        }

        [TestMethod]
        public void MeteorDamagesAndRepair() {
            Dictionary<string, int> random_seeds = new Dictionary<string, int> {
                { "residential_zone", 13 },
                { "service_zone", 0 },
                { "park", 2 },
                { "city_hall", 24 },
                { "police_station", 420 },
            };

            foreach (var (k, v) in random_seeds) {
                Init();
                _model.m_rand = new Random(v);
                if (k == "city_hall") {
                    for (int i = 0; i <= 3; i++) {
                        for (int j = 0; j < i; j++) _model.Upgrade((5, 6));
                        MeteorDamages(3, k);
                    }
                }
                else {
                    MeteorDamages(3, k);
                }
            }
        }
    }
}
