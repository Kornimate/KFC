using KFC.Model;
using KFC.Model.Logging;
using KFC.Model.Pipe;
using KFC.Model.Road;

namespace KFC.Persistence {
    public class GameInfo
    {
        public string city_name { get; set; }
        public long money { get; set; }
        public long income { get; set; }
        public long upkeep { get; set; }
        public long time { get; set; }
        public (int, int, int) converted_time { get; set; }
        public long population { get; set; }
        public Tile[,] map { get; set; }
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

        public GameInfo() {
            city_name = "new city";
            money = 70_000;
            income = 0;
            upkeep = 0;
            time = 0;
            converted_time = (0, 0, 0);
            population = 0;
            total_public_satisfaction_rate = 1.0f;
            residential_tax_rate = 10;
            industrial_tax_rate = 20;
            service_tax_rate = 15;
            new_city = true;
            road_network = new List<RoadNetwork>();
            total_oxygen_requirement = 0;
            total_oxygen_supply = 0;
            pipe_network = new List<PipeNetwork>();
            spending_log = new List<SpendingInfo>();
            monthly_log = new List<MonthlyLog>();
            seed = 100_000 + new Random().Next() % 1_000_000;

            map = new Tile[30, 25];
            for(int i = 0; i < map.GetLength(0); i++) {
                for(int j = 0; j < map.GetLength(1); j++) {
                    map[i, j] = new Tile();
                    map[i, j].location = (i, j);
                }
            }
        }
    }
}
