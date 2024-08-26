namespace KFC.Model.Logging {
    public class MonthlyLog
    {
        public long time_stamp {  get; set; }
        public long money { get; set; }
        public long income { get; set; }
        public long upkeep { get; set; }
        public long population { get; set; }
        public float total_public_satisfaction { get; set; }
        public int total_oxygen_supply { get; set; }
        public int total_oxygen_requirement { get; set; }
        public MonthlyLog()
        {
            this.time_stamp = 0;
            this.money = 0;
            this.income = 0;
            this.upkeep = 0;
            this.population = 0;
            this.total_public_satisfaction = 0;
            this.total_oxygen_supply = 0;
            this.total_oxygen_requirement = 0;
        }

        public MonthlyLog(long time_stamp, long money, long income, long upkeep, long population, float total_public_satisfaction, int total_oxygen_supply, int total_oxygen_requirement)
        {
            this.time_stamp = time_stamp;
            this.money = money;
            this.income = income;
            this.upkeep = upkeep;
            this.population = population;
            this.total_public_satisfaction = total_public_satisfaction;
            this.total_oxygen_supply = total_oxygen_supply;
            this.total_oxygen_requirement = total_oxygen_requirement;
        }
    }
}