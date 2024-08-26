namespace KFC.Model.Buildings {
    public interface IBuilding
    {
        public BuildingType building_type { get; set; }
        public (int,int) building_size { get; }
        public SubBuildingPositionType sub_building_position { get; set; }
        public int building_upkeep { get; }
        public int building_price { get; }
        public int radius { get; }
        public bool connected { get; set; }
        public int damage { get; set; }
        public bool targeted { get; set; }
        public int oxygen_requirement { get; }
        public int oxygen_supply { get; set; }
    }
}
