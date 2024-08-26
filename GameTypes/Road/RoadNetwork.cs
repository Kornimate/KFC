namespace KFC.Model.Road {
    public class RoadNetwork
    {
        public int network_id;
        public Dictionary<int,RoadNetworkNode> road_network;
        public bool connected_to_city_hall;
        public RoadNetwork()
        {
            this.network_id = -1;
            road_network = new Dictionary<int, RoadNetworkNode>();
        }
        public RoadNetwork(int network_id)
        {
            this.network_id = network_id;
            road_network = new Dictionary<int,RoadNetworkNode>();
        }
        public RoadNetwork(int network_id, List<RoadNetworkNode> contained_nodes)
        {
            this.network_id = network_id;
            road_network = new Dictionary<int, RoadNetworkNode>();
            foreach(RoadNetworkNode node in contained_nodes)
            {
                node.network_id = network_id;
                road_network[node.id] = node; 
            }
        }
        public void Merge(RoadNetwork rn)
        {
            int offset = this.road_network.Count;
            foreach (KeyValuePair<int, RoadNetworkNode> rnn in rn.road_network)
            {
                this.road_network[rnn.Key + offset] = rnn.Value;
                rnn.Value.network_id = this.network_id;
                rnn.Value.id += offset;
            }
            if(rn.connected_to_city_hall)
            {
                this.connected_to_city_hall = true;
            }
        }
        public void RecalculateNetworkConnections()
        {
            
        }

        public void RefreshNetworkID(int new_network_id)
        {
            network_id = new_network_id;
            foreach(KeyValuePair<int, RoadNetworkNode> rnn in road_network)
            {
                rnn.Value.network_id = new_network_id;
            }
        }

        public void ReshuffleNetwork()
        {
            int cnt = 0;
            Dictionary<int,RoadNetworkNode> reshuffled_road_network= new Dictionary<int, RoadNetworkNode> ();
            foreach (KeyValuePair<int, RoadNetworkNode> rnn in road_network)
            {
                rnn.Value.id = cnt;
                reshuffled_road_network.Add(cnt, rnn.Value);
                cnt++;
            }
            road_network = reshuffled_road_network;
        }
    }
}
