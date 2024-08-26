namespace KFC.Model.Buildings {
    public class PoliceStation : IBuilding
    {
        public BuildingType building_type { get; set; }
        public (int, int) building_size { get { return (2, 1); } }
        public SubBuildingPositionType sub_building_position { get; set; }
        public int building_upkeep { get { return 1500; } }
        public int building_price { get { return 4000; } }
        public int radius { get { return 5; } }
        public bool connected { get; set; }
        public int public_order_modifier { get { return 2; } }
        public int damage { get; set; }
        public bool targeted { get; set; }
        public int oxygen_requirement { get { return 100; } }
        public int oxygen_supply { get; set; }
        public PoliceStation(SubBuildingPositionType sbpt)
        {
            building_type = BuildingType.POLICE_STATION;
            sub_building_position = sbpt;
            damage = 0;
            targeted = false;
            oxygen_supply = 0;
        }
    }
}
