namespace KFC.Model.Buildings {
    public class OxygenGenerator : IBuilding
    {
        public BuildingType building_type { get; set; }
        public (int, int) building_size { get { return (3, 2); } }
        public SubBuildingPositionType sub_building_position { get; set; }
        public int building_upkeep { get { return 3000; } }
        public int building_price { get { return 6000; } }
        public int radius { get { return 1; } }
        public bool connected { get; set; }
        public int oxygen_generation { get { return 2000; } }
        public int network_id { get; set; }
        public int damage { get; set; }
        public bool targeted { get; set; }
        public int oxygen_requirement { get { return 0; } }
        public int oxygen_supply { get; set; }
        public OxygenGenerator(SubBuildingPositionType sbpt)
        {
            building_type = BuildingType.OXYGEN_GENERATOR;
            sub_building_position = sbpt;
            damage = 0;
            targeted = false;
            network_id = -1;
            oxygen_supply = 0;
        }
    }
}