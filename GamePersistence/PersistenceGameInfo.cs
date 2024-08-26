using KFC.Model;
using KFC.Model.Logging;
using KFC.Model.Pipe;
using KFC.Model.Road;

namespace KFC.Persistence {
    internal class PersistenceGameInfo {
        public string city_name { get; set; }
        public long money { get; set; }
        public long income { get; set; }
        public long upkeep { get; set; }
        public long time { get; set; }
        public (int, int, int) converted_time { get; set; }
        public long population { get; set; }
        public Tile[] map { get; set; }
        public float total_public_satisfaction_rate { get; set; }
        public int residential_tax_rate { get; set; }
        public int industrial_tax_rate { get; set; }
        public int service_tax_rate { get; set; }
        public bool new_city { get; set; }
        public List<RoadNetwork> road_network { get; set; }
        public int total_oxygen_supply { get; set; }
        public int total_oxygen_requirement { get; set; }
        public List<PipeNetwork> pipe_network { get; set; }
        public List<SpendingInfo> spending_log { get; set; }
        public List<MonthlyLog> monthly_log { get; set; }
        public int seed { get; set; }
        public bool city_hall_placed { get; set; }
        public int axis0 { get; set; }
        public int axis1 { get; set; }

        public PersistenceGameInfo() {
            city_name = string.Empty;
            map = new Tile[1];
            spending_log = new();
            monthly_log = new();
            road_network = new();
            pipe_network = new();
        }

        public PersistenceGameInfo(GameInfo info) {
            city_name = info.city_name;
            money = info.money;
            income = info.income;
            upkeep = info.upkeep;
            time = info.time;
            converted_time = info.converted_time;
            population = info.population;
            total_public_satisfaction_rate = info.total_public_satisfaction_rate;
            residential_tax_rate = info.residential_tax_rate;
            industrial_tax_rate = info.industrial_tax_rate;
            service_tax_rate = info.service_tax_rate;
            new_city = info.new_city;
            road_network = info.road_network;
            total_oxygen_requirement = info.total_oxygen_requirement;
            total_oxygen_supply = info.total_oxygen_supply;
            pipe_network = info.pipe_network;
            spending_log = info.spending_log;
            monthly_log = info.monthly_log;
            seed = info.seed;

            axis0 = info.map.GetLength(0);
            axis1 = info.map.GetLength(1);

            map = new Tile[axis0 * axis1];
            for(int i = 0; i < axis0; i++) {
                for(int j = 0; j < axis1; j++) {
                    map[i * axis1 + j] = info.map[i, j];
                }
            }
        }

        public GameInfo CreateGameInfo() {
            GameInfo info = new GameInfo();
            info.city_name = city_name;
            info.money = money;
            info.income = income;
            info.upkeep = upkeep;
            info.time = time;
            info.converted_time = converted_time;
            info.population = population;
            info.total_public_satisfaction_rate = total_public_satisfaction_rate;
            info.residential_tax_rate = residential_tax_rate;
            info.industrial_tax_rate = industrial_tax_rate;
            info.service_tax_rate = service_tax_rate;
            info.new_city = new_city;
            info.road_network = road_network;
            info.total_oxygen_requirement = total_oxygen_requirement;
            info.total_oxygen_supply = total_oxygen_supply;
            info.pipe_network = pipe_network;
            info.spending_log = spending_log;
            info.monthly_log = monthly_log;
            info.seed = seed;

            info.map = new Tile[axis0, axis1];
            for (int i = 0; i < axis0; i++) {
                for (int j = 0; j < axis1; j++) {
                    info.map[i, j] = map[i * axis1 + j];
                }
            }

            return info;
        }
    }
}
