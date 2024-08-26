namespace KFC.Model.Buildings {
    public class OxygenDiffuser : IBuilding
    {
        public BuildingType building_type { get; set; }
        public (int, int) building_size { get { return (1,1); } }
        public SubBuildingPositionType sub_building_position { get; set; }
        public int building_upkeep { get { return 500; } }
        public int building_price { get { return 1000; } }
        public int radius { get { return 5; } }
        public bool connected { get; set; }
        public int network_id { get; set; }
        public int damage { get; set; }
        public bool targeted { get; set; }
        public int oxygen_requirement { get { return 0; } }
        public int oxygen_supply { get; set; }
        public OxygenDiffuser(SubBuildingPositionType sbpt)
        {
            building_type = BuildingType.OXYGEN_DIFFUSER;
            sub_building_position = sbpt;
            damage = 0;
            targeted = false;
            network_id = -1;
            oxygen_supply = 0;
        }
    }
}