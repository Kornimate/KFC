using KFC.Model;
using KFC.Model.Buildings;
using KFC.Model.Zones;
using KFC.Persistence;

namespace KFC.UnitTests {
    [TestClass]
    public class SaveLoadTest {
        private Application _app = null!;
        private GameModel _model = null!;
        private string? prev_city_name;
        private long prev_money;
        private long prev_income;
        private long prev_upkeep;
        private long prev_time;
        private long prev_population;
        private float prev_total_public_satisfaction_rate;
        private int prev_service_tax_rate;
        private int prev_residential_tax_rate;
        private int prev_industrial_tax_rate;
        private int prev_total_oxygen_supply;
        private int prev_total_oxygen_requirement;
        private int axis0 = 30;
        private int axis1 = 25;

        private void SaveModel() {
            GameInfo game_info = new GameInfo();
            prev_city_name = game_info.city_name = _model.city_name;
            prev_money = game_info.money = _model.money;
            prev_income = game_info.income = _model.income;
            prev_upkeep = game_info.upkeep = _model.upkeep;
            prev_time = game_info.time = _model.time;
            game_info.converted_time = _model.converted_time;
            prev_population = game_info.population = _model.population;
            game_info.map = _model.map;
            prev_total_public_satisfaction_rate = game_info.total_public_satisfaction_rate = _model.total_public_satisfaction;
            prev_service_tax_rate = game_info.service_tax_rate = _model.service_tax_rate;
            prev_residential_tax_rate = game_info.residential_tax_rate = _model.residental_tax_rate;
            prev_industrial_tax_rate = game_info.industrial_tax_rate = _model.industrial_tax_rate;
            game_info.new_city = false;
            game_info.road_network = _model.road_network;
            game_info.seed = _app.seed;
            prev_total_oxygen_supply = game_info.total_oxygen_supply = _model.total_oxygen_supply;
            prev_total_oxygen_requirement = game_info.total_oxygen_requirement = _model.total_oxygen_requirement;
            game_info.pipe_network = _model.pipe_network;

            GamePersistence.SaveGameOverwrite("saves/test.json", game_info);
        }

        [TestInitialize]
        public void Init() {
            _app = new Application();
            _app.NewGame("test_city");
            _model = _app.current_game;

            _model.PlaceBuilding((0, 0), BuildingType.CITY_HALL);
            _model.PlaceRoad((0, 2), (15, 2));
            _model.PlaceBuilding((2, 0), BuildingType.PARK);
            _model.PlaceBuilding((4, 1), BuildingType.POLICE_STATION);
            _model.SetZone((0, 3), (3, 3), ZoneType.RESIDENTIAL);
            _model.SetZone((4, 3), (5, 3), ZoneType.SERVICE);
            _model.SetZone((6, 3), (6, 3), ZoneType.INDUSTRIAL);

            // have to bypass setting curr_path in app
            SaveModel();

            _app.NewGame("other_city");
            GameInfo game_info = GamePersistence.LoadSavedGame("saves/test.json");
            _app.LoadGame("saves/test.json");
            _model = _app.current_game;
        }

        [TestCleanup]
        public void Cleanup() {
            // corrupted files get cleaned up
            using (StreamWriter sw = File.AppendText("saves/test.json")) {
                sw.Write("Corrupting json.");
            }
            _app.LoadGame("saves/test.json");
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
        public void BaseTest() {
            Assert.AreEqual(prev_city_name, _model.city_name);
            Assert.AreEqual(prev_money, _model.money);
            Assert.AreEqual(prev_income, _model.income);
            Assert.AreEqual(prev_upkeep, _model.upkeep);
            Assert.AreEqual(prev_time, _model.time);
            Assert.AreEqual(prev_population, _model.population);
            Assert.AreEqual(prev_total_public_satisfaction_rate, _model.total_public_satisfaction, 0.000001);
            Assert.AreEqual(prev_service_tax_rate, _model.service_tax_rate);
            Assert.AreEqual(prev_residential_tax_rate, _model.residental_tax_rate);
            Assert.AreEqual(prev_industrial_tax_rate, _model.industrial_tax_rate);
            Assert.AreEqual(prev_total_oxygen_requirement, _model.total_oxygen_requirement);
            Assert.AreEqual(prev_total_oxygen_supply, _model.total_oxygen_supply);

            Assert.AreEqual(BuildingType.CITY_HALL, _model.map[0, 0].building?.building_type);
            Assert.AreEqual(BuildingType.PARK, _model.map[2, 1].building?.building_type);
            Assert.AreEqual(BuildingType.POLICE_STATION, _model.map[5, 1].building?.building_type);
            Assert.IsNotNull(_model.map[8, 2].road);
            Assert.AreEqual(ZoneType.RESIDENTIAL, _model.map[2, 3].zone_type);
            Assert.AreEqual(ZoneType.SERVICE, _model.map[5, 3].zone_type);
            Assert.AreEqual(ZoneType.INDUSTRIAL, _model.map[6, 3].zone_type);
        }

        [TestMethod]
        public void RenameCity() {
            // from User story
            _app.SaveExistingGame("new_name");
            GameInfo game_info = GamePersistence.LoadSavedGame("saves/test.json");
            _app.LoadGame("saves/test.json");
            _model = _app.current_game;

            Assert.AreEqual("new_name", _model.city_name);
        }

        [TestMethod]
        public void RoadConnection() {
            // testing because this caused issues
            _model.PlaceRoad((17, 2), (16, 2));
            Assert.IsNotNull(_model.map[16, 2].road);
            Assert.IsNotNull(_model.map[17, 2].road);
            Assert.AreNotEqual(-1, _model.map[15, 2].road!.connections[0]);
            Assert.AreEqual(-1, _model.map[15, 2].road!.connections[1]);
            Assert.AreNotEqual(-1, _model.map[15, 2].road!.connections[2]);
            Assert.AreEqual(-1, _model.map[15, 2].road!.connections[3]);

            Assert.AreNotEqual(-1, _model.map[16, 2].road!.connections[0]);
            Assert.AreEqual(-1, _model.map[16, 2].road!.connections[1]);
            Assert.AreNotEqual(-1, _model.map[16, 2].road!.connections[2]);
            Assert.AreEqual(-1, _model.map[16, 2].road!.connections[3]);
        }

        [TestMethod]
        public void OxygenEdgeCase() {
            // residential zone doesn't reinit oxygen
            _model.PlaceBuilding((6, 1), BuildingType.OXYGEN_DIFFUSER);
            _model.PlaceBuilding((7, 0), BuildingType.OXYGEN_GENERATOR);
            _model.PlacePipe((6, 1), (7, 1));
            _model.Delete((4, 3), 0);
            _model.Delete((5, 3), 0);
            _model.Delete((6, 3), 0);
            _model.GameTick(1); // oxygen

            SaveModel();

            _app.NewGame("other_city");
            GameInfo game_info = GamePersistence.LoadSavedGame("saves/test.json");
            _app.LoadGame("saves/test.json");
            _model = _app.current_game;

            _model.SetZone((4, 3), (5, 3), ZoneType.SERVICE);
            _model.SetZone((6, 3), (6, 3), ZoneType.INDUSTRIAL);
            _model.GameTick(1); // moving in
            Assert.IsTrue(_model.map[3, 3].zone_info!.people.Count > 0);
        }
    }
}
