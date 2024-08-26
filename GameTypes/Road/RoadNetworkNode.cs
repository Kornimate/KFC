namespace KFC.Model.Road {
    public class RoadNetworkNode
    {
        public (int, int) location;
        public int id;
        public int network_id;
        public int[] connections;
        public (int, int)[] connected_tiles;
        public RoadNetworkNode()
        {
            this.connections = new int[4] { -1, -1, -1, -1 };
            this.connected_tiles = new (int, int)[4] { (-1, -1), (-1, -1), (-1, -1), (-1, -1) };
        }
        public RoadNetworkNode((int, int) location, int id, int network_id, int[] connections, (int, int)[] connected_tiles)
        {
            this.location = location;
            this.id = id;
            this.network_id = network_id;
            this.connections = connections;
            this.connected_tiles = connected_tiles;
        }

        public RoadNetworkNode(int id,int network_id,(int,int) location)
        {
            this.id = id;
            this.network_id = network_id;
            this.location = location;
            this.connections = new int[4] { -1, -1, -1, -1 };
            this.connected_tiles = new (int, int)[4] { (-1, -1), (-1, -1), (-1, -1), (-1, -1) };
        }
    }
}
