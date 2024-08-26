namespace KFC.Model.Buildings {
    public class CityHall : IBuilding
    {
        public BuildingType building_type { get; set; }
        public (int, int) building_size { get { return (2, 2); } }
        public SubBuildingPositionType sub_building_position { get; set; }
        public int building_upkeep { get { return 5000; } }
        public int building_price { get { return 10000; } }
        public int radius { get { return 3; } }
        public bool connected { get; set; }
        public int level { get; set; }
        public int public_satisfaction_modifier { get { return 2; } }
        public int damage { get; set; }
        public bool targeted { get; set; }
        public int oxygen_requirement { get { return 0; } }
        public int oxygen_supply { get; set; }
        public CityHall(SubBuildingPositionType sbpt)
        {
            building_type = BuildingType.CITY_HALL;
            sub_building_position = sbpt;
            level = 1;
            damage = 0;
            connected = true;
            targeted = false;
            oxygen_supply = 0;
        }
    }
}