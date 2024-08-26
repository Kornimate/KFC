namespace KFC.Model.Buildings {
    public class Park : IBuilding
    {
        public BuildingType building_type { get; set; }
        public (int, int) building_size { get { return (2, 2); } }
        public SubBuildingPositionType sub_building_position { get; set; }
        public int building_upkeep { get { return 1000; } }
        public int building_price { get { return 2000; } }
        public int radius { get { return 5; } }
        public bool connected { get; set; }
        public int public_satisfaction_modifier { get { return 2; } }
        public int damage { get; set; }
        public bool targeted { get; set; }
        public int oxygen_requirement { get { return 200; } }
        public int oxygen_supply { get; set; }
        public Park(SubBuildingPositionType sbpt)
        {
            building_type = BuildingType.PARK;
            sub_building_position = sbpt;
            damage = 0;
            targeted = false;
            oxygen_supply = 0;
        }
    }
}
