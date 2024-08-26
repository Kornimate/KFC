namespace KFC.Model.Pipe {
    public class PipeNetworkNode
    {
        public (int, int) location;
        public int id;
        public int network_id;
        public int[] connections;
        public PipeNetworkNode()
        {
            this.connections = new int[4] { -1, -1, -1, -1 };
        }
        public PipeNetworkNode((int, int) location, int id, int network_id, int[] connections)
        {
            this.location = location;
            this.id = id;
            this.network_id = network_id;
            this.connections = connections;
        }
        public PipeNetworkNode(int id, int network_id, (int, int) location)
        {
            this.id = id;
            this.network_id = network_id;
            this.location = location;
            this.connections = new int[4] { -1, -1, -1, -1 };
        }
    }
}