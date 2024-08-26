namespace KFC.Model.Zones {
    public class ZoneInfo
    {
        public Dictionary<int,Person.Person> people;
        public int max_capacity;
        public float public_order;
        public float public_satisfaction;
        public Boolean connected;
        public int level;
        public int oxygen_supply;
        public int oxygen_requirement;
        public HashSet<(int, int)> reachable_connected_zones;
        public HashSet<int> connected_oxygen_diffusers;
        public int near_active_industrial;
        public int damage;
        public bool targeted;
        public int damaged_nearby_buildings;
        public ZoneInfo()
        {
            connected = false;
            people = new Dictionary<int,Person.Person>();
            reachable_connected_zones = new HashSet<(int, int)> ();
            connected_oxygen_diffusers = new HashSet<int> ();
            near_active_industrial = 0;
            level = 1;
            damage = 0;
            targeted = false;
            damaged_nearby_buildings = 0;
        }
    }
}
