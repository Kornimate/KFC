using KFC.Model.Buildings;
using KFC.Model.Pipe;
using KFC.Model.Road;
using KFC.Model.Zones;

namespace KFC.Model {
    public class Tile
    {
        public (int, int) location;
        public ZoneType zone_type;
        public ZoneInfo? zone_info;
        public IBuilding? building;
        public RoadNetworkNode? road;
        public PipeNetworkNode? pipe;
        public Tile()
        {
            zone_type = ZoneType.NONE;
            zone_info = new ZoneInfo();
            building = null;
            road = null;
            pipe = null;
        }
    }
}
