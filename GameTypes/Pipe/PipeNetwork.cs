namespace KFC.Model.Pipe {
    public class PipeNetwork
    {
        public int network_id;
        public Dictionary<int, PipeNetworkNode> pipe_network;
        public int total_oxygen_supply;
        public HashSet<(int,int)> connected_generators;
        public HashSet<(int,int)> connected_diffusers;
        public HashSet<(int, int)> supplied_zones;
        public PipeNetwork()
        {
            this.network_id = -1;
            pipe_network = new Dictionary<int, PipeNetworkNode>();
            connected_generators = new HashSet<(int, int)>();
            connected_diffusers = new HashSet<(int,int)> ();
            supplied_zones = new HashSet<(int, int)>();
        }
        public PipeNetwork(int network_id)
        {
            this.network_id = network_id;
            pipe_network = new Dictionary<int, PipeNetworkNode>();
            connected_generators = new HashSet<(int, int)>();
            connected_diffusers = new HashSet<(int, int)>();
            supplied_zones= new HashSet<(int, int)>();
        }
        public PipeNetwork(int network_id, List<PipeNetworkNode> contained_nodes)
        {
            this.network_id = network_id;
            pipe_network = new Dictionary<int, PipeNetworkNode>();
            foreach (PipeNetworkNode node in contained_nodes)
            {
                node.network_id = network_id;
                pipe_network[node.id] = node;
            }
            connected_generators = new HashSet<(int, int)>();
            connected_diffusers = new HashSet<(int, int)>();
            supplied_zones = new HashSet<(int, int)>();
        }
        public void RecalculateOxygenSupply()
        {
            
        }
        public void RecalculateNetworkConnections()
        {

        }
        public void Merge(PipeNetwork pn)
        {
            int offset = this.pipe_network.Count;
            foreach (KeyValuePair<int, PipeNetworkNode> pnn in pn.pipe_network)
            {
                this.pipe_network[pnn.Key + offset] = pnn.Value;
                pnn.Value.network_id = this.network_id;
                pnn.Value.id += offset;
            }
            this.connected_generators.UnionWith(pn.connected_generators);
            this.connected_diffusers.UnionWith(pn.connected_diffusers);
            this.supplied_zones.UnionWith(pn.supplied_zones);
            this.total_oxygen_supply += pn.total_oxygen_supply;
        }
        public void RefreshNetworkID(int new_network_id)
        {
            network_id = new_network_id;
            foreach (KeyValuePair<int, PipeNetworkNode> pnn in pipe_network)
            {
                pnn.Value.network_id = new_network_id;
            }
        }
        public void ReshuffleNetwork()
        {
            int cnt = 0;
            Dictionary<int, PipeNetworkNode> reshuffled_pipe_network = new Dictionary<int, PipeNetworkNode>();
            foreach (KeyValuePair<int, PipeNetworkNode> rnn in pipe_network)
            {
                rnn.Value.id = cnt;
                reshuffled_pipe_network.Add(cnt, rnn.Value);
                cnt++;
            }
            pipe_network = reshuffled_pipe_network;
        }
    }
}