using KFC.Model.Buildings;
using KFC.Model.Logging;
using KFC.Model.Pipe;
using KFC.Model.Road;
using KFC.Model.Zones;
using KFC.Persistence;

namespace KFC.Model {
    public class GameModel
    {
        #region Head
        public string city_name { get; private set; }
        public long money { get; private set; }
        public long income { get; private set; }
        public long upkeep { get; private set; }
        public (int, int, int) converted_time { get; private set; }
        public long population { get; private set; }
        public Tile[,] map { get; private set; }
        public (int, int) map_size { get; private set; }
        public float total_public_satisfaction { get; private set; }
        public int residental_tax_rate { get; private set; }
        public int industrial_tax_rate { get; private set; }
        public int service_tax_rate { get; private set; }
        public int new_residental_tax_rate { get; private set; }
        public int new_industrial_tax_rate { get; private set; }
        public int new_service_tax_rate { get; private set; }
        public int total_oxygen_supply { get; private set; }
        public int total_oxygen_requirement { get; private set; }
        public List<RoadNetwork> road_network { get; private set; }
        public List<PipeNetwork> pipe_network { get; private set; }
        private (string, int, int)[] calendar;
        public long time { get; private set; }
        public bool city_hall_placed { get; private set; }
        public Random m_rand { get; set; }

        public List<SpendingInfo> spending_log { get; private set; }
        public List<MonthlyLog> monthly_log { get; private set; }
        private int[] zone_price;
        private int[,] zone_capacity;
        private int[] base_tax_rate;
        private int[] base_income_rate;
        private int city_hall_resident_income_rate;
        private int road_price;
        private int pipe_price;
        private int base_arrival_rate;
        private int arrival_rate_modifier;
        private HashSet<(int, int)> connected_zones;
        private int[,] satisfaction_modifiers;
        private int[,] public_order_modifiers;
        public int city_hall_residents;
        private int zone_reach;
        private int road_upkeep;
        private int pipe_upkeep;
        private int number_of_colonists_per_unit_of_oxygen;
        private int city_wide_satisfaction_modifier;
        private int income_modifier;
        private (int, int)[] catastrophe_scale;
        private (int, int, int)[] targeted_zones;
        private HashSet<(int, int)> buildings_with_modifiers;
        private int city_hall_level;
        private int place_road_count;
        public EventHandler<GameEventArgs>? StatusUpdate;
        public EventHandler<GameEventArgs>? GameOver;
        public EventHandler<GameEventArgs>? LogUpdate;
        /// <summary>
        /// Creates a game-session based on the information in <paramref name="game_info"/>.
        /// </summary>
        /// <param name="game_info">The information to construct/reconstruct a session.</param>
        public GameModel(GameInfo game_info)
        {
            city_name = game_info.city_name;
            calendar = new (string, int, int)[15]{
                ("1",45,0),
                ("2",44,45),
                ("3",45,89),
                ("4",44,134),
                ("5",45,178),
                ("6",44,223),
                ("7",45,267),
                ("8",44,312),
                ("9",45,356),
                ("10",44,401),
                ("11",45,445),
                ("12",44,490),
                ("13",45,534),
                ("14",44,579),
                ("15",45,623)
            };
            zone_price = new int[4]{
                0,
                100,
                500,
                200
            };
            zone_capacity = new int[4, 4];
            zone_capacity[0, 0] = 0;
            zone_capacity[0, 1] = 0;
            zone_capacity[0, 2] = 0;
            zone_capacity[0, 3] = 0;
            zone_capacity[1, 0] = 100;
            zone_capacity[1, 1] = 250;
            zone_capacity[1, 2] = 500;
            zone_capacity[1, 3] = 1000;
            zone_capacity[2, 0] = 200;
            zone_capacity[2, 1] = 400;
            zone_capacity[2, 2] = 800;
            zone_capacity[2, 3] = 1600;
            zone_capacity[3, 0] = 50;
            zone_capacity[3, 1] = 200;
            zone_capacity[3, 2] = 400;
            zone_capacity[3, 3] = 600;
            city_hall_level = 1;
            base_income_rate = new int[4]
            {
                0,
                7,
                25,
                15
            };
            base_tax_rate = new int[4]
            {
                0,
                10,
                20,
                15
            };
            city_hall_resident_income_rate = 3;
            city_wide_satisfaction_modifier = 0;
            income_modifier = 100;
            number_of_colonists_per_unit_of_oxygen = 3;
            road_price = 10;
            road_upkeep = 2;
            pipe_price = 20;
            pipe_upkeep = 4;
            map = game_info.map;
            map_size = (game_info.map.GetLength(0), game_info.map.GetLength(1));
            city_hall_placed = false;
            money = game_info.money;
            income = game_info.income;
            upkeep = game_info.upkeep;
            road_network = game_info.road_network;
            pipe_network = game_info.pipe_network;
            base_arrival_rate = 100;
            arrival_rate_modifier = 2;
            total_public_satisfaction = game_info.total_public_satisfaction_rate;
            residental_tax_rate = game_info.residential_tax_rate;
            industrial_tax_rate = game_info.industrial_tax_rate;
            service_tax_rate = game_info.service_tax_rate;
            new_residental_tax_rate = residental_tax_rate;
            new_industrial_tax_rate = industrial_tax_rate;
            new_service_tax_rate = service_tax_rate;
            population = game_info.population;
            connected_zones = new HashSet<(int, int)>();
            buildings_with_modifiers = new HashSet<(int, int)>();
            place_road_count = 0;
            satisfaction_modifiers = new int[map_size.Item1, map_size.Item2];
            targeted_zones = new (int, int, int)[0];
            m_rand = new Random();
            spending_log = game_info.spending_log;
            monthly_log = game_info.monthly_log;
            catastrophe_scale = new (int, int)[6]
            {
                (0,0),
                (1,10),
                (2,20),
                (3,30),
                (4,40),
                (5,50)
            };
            time = game_info.time;
            zone_reach = 5;
            total_oxygen_supply = game_info.total_oxygen_supply;
            total_oxygen_requirement = game_info.total_oxygen_requirement;
            converted_time = ConvertTime(time);
            for (int x = 0; x < map_size.Item1; x++)
            {
                for (int y = 0; y < map_size.Item2; y++)
                {
                    satisfaction_modifiers[x, y] = 0;
                }
            }
            public_order_modifiers = new int[map_size.Item1, map_size.Item2];
            for (int x = 0; x < map_size.Item1; x++)
            {
                for (int y = 0; y < map_size.Item2; y++)
                {
                    public_order_modifiers[x, y] = -1;
                }
            }
            if (!game_info.new_city)
            {
                _PreProcess();
            }
            else
            {
                monthly_log.Add(new MonthlyLog(time, money, income, upkeep, population, total_public_satisfaction, total_oxygen_supply, total_oxygen_requirement));
                LogUpdate?.Invoke(this, new GameEventArgs { status = EventStatus.SUCCESS, update = EventUpdate.UPDATE, message = "Log updated!" });
            }
        }
        private void _PreProcess()
        {
            long population_sum = 0;
            bool[,] prep = new bool[map_size.Item1, map_size.Item2];
            for (int x = 0; x < map_size.Item1; x++)
            {
                for (int y = 0; y < map_size.Item2; y++)
                {
                    if (prep[x, y])
                    {
                        continue;
                    }
                    if (map[x, y].building != null)
                    {
                        if (map[x, y].building!.building_type == BuildingType.CITY_HALL)
                        {
                            city_hall_placed = true;
                            CityHall ch = (CityHall)map[x, y].building!;
                            for (int p = 1; p <= ch.level; p++)
                            {
                                _UpgradeCityHall(p);
                            }
                        }
                        if (map[x, y].building!.connected)
                        {
                            map[x, y].zone_info!.connected = false;
                            map[x, y].building!.connected = false;
                            _ConnectBuilding((x, y));
                            for (int x2 = x; x2 < x + map[x, y].building!.building_size.Item1; x2++)
                            {
                                for (int y2 = y; y2 < y + map[x, y].building!.building_size.Item2; y2++)
                                {
                                    prep[x2, y2] = true;
                                }
                            }
                        }
                    }
                    if (map[x, y].road != null)
                    {
                        place_road_count++;
                    }
                    if (map[x, y].zone_type == ZoneType.RESIDENTIAL)
                    {
                        population_sum += map[x, y].zone_info!.people.Count;
                    }
                    if (map[x, y].zone_type != ZoneType.NONE)
                    {
                        map[x, y].zone_info!.connected = false;
                    }
                }
            }
            city_hall_residents = (int)(population - population_sum);
            foreach (RoadNetwork rn in road_network)
            {
                _RecalculateRoadNetworkConnections(rn.network_id);
            }
            foreach (PipeNetwork pn in pipe_network)
            {
                _RecalculatePipeNetworkConnections(pn.network_id);
            }
            _RecalculateModifiers();
        }
        #endregion
        #region Game_Tick
        /// <summary>
        /// Advances the game-state by one cycle.<br/>
        /// </summary>
        /// <param name="tick_size">Sets the number of days this cycle represents (almost always 1).</param>
        public void GameTick(int tick_size)
        {
            if (tick_size <= 0)
            {
                return;
            }
            time += tick_size;
            (int, int, int) last_time = converted_time;
            converted_time = ConvertTime(time);
            if (city_hall_placed)
            {
                _RefreshOxygenSuppliers();
                _CalculateOxygenSupply();
                _CalculateSatisfactionRates();
                if (city_hall_residents > 0)
                {
                    _RelocateFromCityHall();
                }
            }
            if (converted_time.Item1 > last_time.Item1)
            {
                _NewYear();
            }
            else
            {
                if (converted_time.Item2 > last_time.Item2)
                {
                    _NewMonth();
                }
            }
            StatusUpdate?.Invoke(this, new GameEventArgs { status = EventStatus.SUCCESS, update = EventUpdate.UPDATE, message = "New day" });
        }
        private void _CollectTaxes()
        {
            float tax = 0.0f;
            foreach ((int, int) pos in connected_zones.ToList())
            {
                switch (map[pos.Item1, pos.Item2].zone_type)
                {
                    case ZoneType.RESIDENTIAL:
                        foreach (KeyValuePair<int, Person.Person> ps in map[pos.Item1, pos.Item2].zone_info!.people)
                        {
                            Person.Person person = ps.Value;
                            tax += (base_income_rate[1] * (residental_tax_rate / 100.0f));
                            switch (map[person.workplace.Item1, person.workplace.Item2].zone_type)
                            {
                                case ZoneType.INDUSTRIAL:
                                    tax += (base_income_rate[2] * (industrial_tax_rate / 100.0f));
                                    break;
                                case ZoneType.SERVICE:
                                    tax += (base_income_rate[3] * (service_tax_rate / 100.0f));
                                    break;
                            }
                        }
                        break;
                }
            }
            money += (long)(tax) * (long)(income_modifier / 100.0f);
            money += city_hall_resident_income_rate * Math.Min(city_hall_residents, population);
            income = (long)(tax) + city_hall_resident_income_rate * Math.Min(city_hall_residents, population) - upkeep;
        }
        private void _NearIndustrial((int, int) position)
        {
            if (map[position.Item1, position.Item2].zone_info!.near_active_industrial == 0)
            {
                foreach ((int, int) loc in map[position.Item1, position.Item2].zone_info!.reachable_connected_zones)
                {
                    switch (map[loc.Item1, loc.Item2].zone_type)
                    {
                        case ZoneType.INDUSTRIAL:
                            {
                                if (map[loc.Item1, loc.Item2].zone_info!.people.Count > 0)
                                {
                                    map[position.Item1, position.Item2].zone_info!.near_active_industrial = 1;
                                    break;
                                }
                            }
                            break;
                    }
                }
            }
        }
        private void _CalculateSatisfactionRates()
        {
            if (!city_hall_placed)
            {
                total_public_satisfaction = 1.0f;
                return;
            }
            float city_satisfaction_sum = 0.0f;
            int city_wide_satm = 0 + city_wide_satisfaction_modifier;
            if (money < 0)
            {
                city_wide_satm = -10;
            }
            foreach ((int, int) position in connected_zones.ToList())
            {
                switch (map[position.Item1, position.Item2].zone_type)
                {
                    case ZoneType.RESIDENTIAL:
                        {
                            if(map[position.Item1, position.Item2].zone_info!.people.Count <= 0)
                            {
                                map[position.Item1, position.Item2].zone_info!.public_satisfaction = 0.0f;
                                continue;
                            }
                            _NearIndustrial(position);
                            float zone_satisfaction_sum = 0.0f;
                            map[position.Item1, position.Item2].zone_info!.public_order = Math.Min(1.0f, (public_order_modifiers[position.Item1, position.Item2] + 1) * 0.34f);
                            int zone_satisfaction_modifier = satisfaction_modifiers[position.Item1, position.Item2] + public_order_modifiers[position.Item1, position.Item2] + city_wide_satm - map[position.Item1, position.Item2].zone_info!.near_active_industrial * 3 - ((residental_tax_rate - base_tax_rate[1]) / 10) - Math.Sign(map[position.Item1, position.Item2].zone_info!.damaged_nearby_buildings) * 4 - Math.Sign(map[position.Item1, position.Item2].zone_info!.damage) * 10;
                            if(map[position.Item1, position.Item2].zone_info!.oxygen_supply < map[position.Item1, position.Item2].zone_info!.oxygen_requirement)
                            {
                                zone_satisfaction_modifier -= 5;
                            }
                            foreach (KeyValuePair<int,Person.Person> ps in map[position.Item1, position.Item2].zone_info!.people)
                            {
                                Person.Person person = ps.Value;
                                switch (map[person.workplace.Item1, person.workplace.Item2].zone_type)
                                {
                                    case ZoneType.INDUSTRIAL:
                                        {
                                            person.satisfaction_target = ((zone_satisfaction_modifier - ((industrial_tax_rate - base_tax_rate[2]) / 10)) / 10.0f) + 0.5f;
                                        }
                                        break;
                                    case ZoneType.SERVICE:
                                        {
                                            person.satisfaction_target = ((zone_satisfaction_modifier - ((service_tax_rate - base_tax_rate[3]) / 10)) / 10.0f) + 0.5f;
                                        }
                                        break;
                                }
                                person.satisfaction_target = Math.Max(Math.Min(person.satisfaction_target, 1.0f), 0.0f);
                                if (Math.Abs(person.satisfaction_target - person.satisfaction_rate) < 0.001f)
                                {
                                    person.satisfaction_rate = person.satisfaction_target;
                                }
                                else
                                {
                                    if (person.satisfaction_rate < person.satisfaction_target)
                                    {
                                        person.satisfaction_rate += (0.025f) / 44.533f;
                                    }
                                    else
                                    {
                                        person.satisfaction_rate -= (0.025f) / 44.533f;
                                    }
                                }
                                person.satisfaction_rate = Math.Max(Math.Min(person.satisfaction_rate, 1.0f), 0.0f);

                                zone_satisfaction_sum += person.satisfaction_rate;
                            }
                            map[position.Item1, position.Item2].zone_info!.public_satisfaction = zone_satisfaction_sum / map[position.Item1, position.Item2].zone_info!.people.Count;
                            city_satisfaction_sum += zone_satisfaction_sum;
                        }
                        break;
                }
            }
            foreach ((int, int) position in connected_zones.ToList())
            {
                switch (map[position.Item1, position.Item2].zone_type)
                {
                    case ZoneType.SERVICE:
                    case ZoneType.INDUSTRIAL:
                        {
                            if (map[position.Item1, position.Item2].zone_info!.people.Count <= 0)
                            {
                                map[position.Item1, position.Item2].zone_info!.public_satisfaction = 0.0f;
                                continue;
                            }
                            float zone_satisfaction_sum = 0.0f;
                            foreach (KeyValuePair<int, Person.Person> ps in map[position.Item1, position.Item2].zone_info!.people)
                            {
                                Person.Person person = ps.Value;
                                zone_satisfaction_sum += person.satisfaction_rate;
                            }
                            map[position.Item1, position.Item2].zone_info!.public_satisfaction = zone_satisfaction_sum / map[position.Item1, position.Item2].zone_info!.people.Count;
                        }
                        break;
                }
            }
            city_satisfaction_sum += (city_hall_residents * 0.35f);
            total_public_satisfaction = city_satisfaction_sum / population;
            if (total_public_satisfaction <= 0.1f)
            {
                StatusUpdate?.Invoke(this, new GameEventArgs { status = EventStatus.GAME_OVER, update = EventUpdate.UPDATE, message = "Game over!" });
            }
        }
        private void _RecalculateModifiers()
        {
            for (int x = 0; x < map_size.Item1; x++)
            {
                for (int y = 0; y < map_size.Item2; y++)
                {
                    satisfaction_modifiers[x, y] = 0;
                }
            }
            public_order_modifiers = new int[map_size.Item1, map_size.Item2];
            for (int x = 0; x < map_size.Item1; x++)
            {
                for (int y = 0; y < map_size.Item2; y++)
                {
                    public_order_modifiers[x, y] = -1;
                }
            }
            foreach((int,int) location in buildings_with_modifiers)
            {
                switch (map[location.Item1, location.Item2].building!.building_type)
                {
                    case BuildingType.PARK:
                        {
                            int sm = 0;
                            sm = 2;
                            if (map[location.Item1, location.Item2].building!.damage > 0)
                            {
                                sm--;
                            }
                            if (map[location.Item1, location.Item2].building!.damage >= 50)
                            {
                                sm--;
                            }
                            if (map[location.Item1, location.Item2].zone_info!.oxygen_supply < map[location.Item1, location.Item2].zone_info!.oxygen_requirement)
                            {
                                sm--;
                            }
                            if (map[location.Item1, location.Item2].zone_info!.oxygen_supply < map[location.Item1, location.Item2].zone_info!.oxygen_requirement / 2)
                            {
                                sm--;
                            }
                            sm = Math.Max(0, sm);
                            for (int x = location.Item1 - map[location.Item1, location.Item2].building!.radius; x < location.Item1 + map[location.Item1, location.Item2].building!.building_size.Item1 + map[location.Item1, location.Item2].building!.radius; x++)
                            {
                                for (int y = location.Item2 - map[location.Item1, location.Item2].building!.radius; y < location.Item2 + map[location.Item1, location.Item2].building!.building_size.Item2 + map[location.Item1, location.Item2].building!.radius; y++)
                                {
                                    if (_InsideMap((x, y)))
                                    {
                                        satisfaction_modifiers[x, y] += sm;
                                    }
                                }
                            }
                        }
                        break;
                    case BuildingType.POLICE_STATION:
                        {
                            int sm = 0;
                            sm = 2;
                            if (map[location.Item1, location.Item2].building!.damage > 0)
                            {
                                sm--;
                            }
                            if (map[location.Item1, location.Item2].building!.damage >= 50)
                            {
                                sm--;
                            }
                            if (map[location.Item1, location.Item2].zone_info!.oxygen_supply < map[location.Item1, location.Item2].zone_info!.oxygen_requirement)
                            {
                                sm--;
                            }
                            if (map[location.Item1, location.Item2].zone_info!.oxygen_supply < map[location.Item1, location.Item2].zone_info!.oxygen_requirement / 2)
                            {
                                sm--;
                            }
                            sm = Math.Max(0, sm);
                            for (int x = location.Item1 - map[location.Item1, location.Item2].building!.radius; x < location.Item1 + map[location.Item1, location.Item2].building!.building_size.Item1 + map[location.Item1, location.Item2].building!.radius; x++)
                            {
                                for (int y = location.Item2 - map[location.Item1, location.Item2].building!.radius; y < location.Item2 + map[location.Item1, location.Item2].building!.building_size.Item2 + map[location.Item1, location.Item2].building!.radius; y++)
                                {
                                    if (_InsideMap((x, y)))
                                    {
                                        public_order_modifiers[x, y] += sm;
                                    }
                                }
                            }
                        }
                        break;
                }
            }
        }
        private void _CalculateUpkeep()
        {
            money -= upkeep;
        }
        private void _CalculateOxygenSupply()
        {
            total_oxygen_supply = 0;
            total_oxygen_requirement = 0;
            if(population == 0 || city_hall_residents >= 1000)
            {
                return;
            }
            int[] network_oxygen_supply = new int[pipe_network.Count];
            foreach((int,int) location in connected_zones.ToList())
            {
                map[location.Item1, location.Item2].zone_info!.oxygen_supply = 0;
                map[location.Item1, location.Item2].zone_info!.oxygen_requirement = map[location.Item1, location.Item2].zone_info!.people.Count / number_of_colonists_per_unit_of_oxygen;
                total_oxygen_requirement += map[location.Item1, location.Item2].zone_info!.people.Count / number_of_colonists_per_unit_of_oxygen;
            }
            foreach((int,int) location in buildings_with_modifiers)
            {
                map[location.Item1, location.Item2].zone_info!.oxygen_supply = 0;
                map[location.Item1, location.Item2].zone_info!.oxygen_requirement = map[location.Item1, location.Item2].building!.oxygen_requirement;
                total_oxygen_requirement += map[location.Item1, location.Item2].building!.oxygen_requirement;
            }
            foreach (PipeNetwork pn in pipe_network)
            {
                total_oxygen_supply += pn.total_oxygen_supply;
                network_oxygen_supply[pn.network_id] = pn.total_oxygen_supply;
            }
            //SINGLE SUPPLIER
            foreach (PipeNetwork pn in pipe_network)
            {
                int required_oxygen = 0;
                foreach ((int, int) location in pn.supplied_zones.ToList())
                {
                    if(map[location.Item1, location.Item2].zone_info!.connected_oxygen_diffusers.Count == 1)
                    {
                        required_oxygen += map[location.Item1, location.Item2].zone_info!.oxygen_requirement;
                    }
                }
                if(required_oxygen <= pn.total_oxygen_supply)
                {
                    foreach ((int, int) location in pn.supplied_zones.ToList())
                    {
                        if (map[location.Item1, location.Item2].zone_info!.connected_oxygen_diffusers.Count == 1)
                        {
                            map[location.Item1, location.Item2].zone_info!.oxygen_supply = map[location.Item1, location.Item2].zone_info!.oxygen_requirement;
                            network_oxygen_supply[pn.network_id] -= map[location.Item1, location.Item2].zone_info!.oxygen_requirement;
                        }
                    }
                }
                else
                {
                    float oxygen_ratio = network_oxygen_supply[pn.network_id] / (float)(required_oxygen);
                    foreach ((int, int) location in pn.supplied_zones)
                    {
                        if (map[location.Item1, location.Item2].zone_info!.connected_oxygen_diffusers.Count == 1)
                        {
                            map[location.Item1, location.Item2].zone_info!.oxygen_supply = (int)(map[location.Item1, location.Item2].zone_info!.oxygen_requirement * oxygen_ratio);
                        }
                    }
                    network_oxygen_supply[pn.network_id] = 0;
                }
            }
            //MULTIPLE SUPPLIERS
            foreach (PipeNetwork pn in pipe_network)
            {
                int required_oxygen = 0;
                foreach ((int, int) location in pn.supplied_zones)
                {
                    if (map[location.Item1, location.Item2].zone_info!.connected_oxygen_diffusers.Count > 1)
                    {
                        required_oxygen += (map[location.Item1, location.Item2].zone_info!.oxygen_requirement - map[location.Item1, location.Item2].zone_info!.oxygen_supply);
                    }
                }
                if (required_oxygen <= network_oxygen_supply[pn.network_id])
                {
                    foreach ((int, int) location in pn.supplied_zones)
                    {
                        if (map[location.Item1, location.Item2].zone_info!.connected_oxygen_diffusers.Count > 1)
                        {
                            network_oxygen_supply[pn.network_id] -= (map[location.Item1, location.Item2].zone_info!.oxygen_requirement - map[location.Item1, location.Item2].zone_info!.oxygen_supply);
                            map[location.Item1, location.Item2].zone_info!.oxygen_supply = map[location.Item1, location.Item2].zone_info!.oxygen_requirement;
                        }
                    }
                }
                else
                {
                    float oxygen_ratio = network_oxygen_supply[pn.network_id] / (float)(required_oxygen);
                    foreach ((int, int) location in pn.supplied_zones)
                    {
                        if (map[location.Item1, location.Item2].zone_info!.connected_oxygen_diffusers.Count == 1)
                        {
                            if(map[location.Item1, location.Item2].zone_info!.oxygen_requirement > map[location.Item1, location.Item2].zone_info!.oxygen_supply)
                            {
                                map[location.Item1, location.Item2].zone_info!.oxygen_supply = Math.Min((int)((map[location.Item1, location.Item2].zone_info!.oxygen_requirement - map[location.Item1, location.Item2].zone_info!.oxygen_supply) * oxygen_ratio), map[location.Item1, location.Item2].zone_info!.oxygen_requirement);
                            }
                        }
                    }
                    network_oxygen_supply[pn.network_id] = 0;
                }
            }
            bool should_recalculate_modifiers = false;
            foreach ((int, int) location in buildings_with_modifiers)
            {
                for(int x = location.Item1; x < location.Item1 + map[location.Item1, location.Item2].building!.building_size.Item1; x++)
                {
                    for (int y = location.Item2; y < location.Item2 + map[location.Item1, location.Item2].building!.building_size.Item2; y++)
                    {
                        map[x, y].zone_info!.oxygen_supply = map[location.Item1, location.Item2].zone_info!.oxygen_supply;
                        map[x, y].zone_info!.oxygen_requirement = map[location.Item1, location.Item2].building!.oxygen_requirement;
                    }
                }
                if(map[location.Item1, location.Item2].zone_info!.oxygen_supply < map[location.Item1, location.Item2].zone_info!.oxygen_requirement)
                {
                    should_recalculate_modifiers = true;
                }
            }
            if(should_recalculate_modifiers)
            {
                _RecalculateModifiers();
            }
        }
        private int _ResidentialUnitComparison((int, int) a, (int, int) b)
        {
            int comp = map[a.Item1, a.Item2].zone_info!.people.Count - map[b.Item1, b.Item2].zone_info!.people.Count;
            return -Math.Sign(comp);
        }
        private bool _SufficientOxygenSupply((int,int) position)
        {
            int networks_with_accessible_generators = 0;
            foreach(int network_id in map[position.Item1, position.Item2].zone_info!.connected_oxygen_diffusers)
            {
                if(network_id == -1)
                {
                    continue;
                }
                if (pipe_network[network_id].connected_generators.Count > 0)
                {
                    networks_with_accessible_generators++;
                }
            }
            return (map[position.Item1, position.Item2].zone_info!.oxygen_supply >= map[position.Item1, position.Item2].zone_info!.oxygen_requirement && networks_with_accessible_generators > 0);
        }
        private (int, int) _ChooseWorkPlace((int, int) position, int mode)
        {
            (int, int) wp = (-1, -1);
            foreach ((int, int) workp in map[position.Item1, position.Item2].zone_info!.reachable_connected_zones)
            {
                if (mode == 0 && map[workp.Item1, workp.Item2].zone_type == ZoneType.INDUSTRIAL)
                {
                    if (map[workp.Item1, workp.Item2].zone_info!.people.Count < map[workp.Item1, workp.Item2].zone_info!.max_capacity)
                    {
                        if (_SufficientOxygenSupply(workp))
                        {
                            wp = workp;
                            break;
                        }
                    }
                }
                if (mode == 1 && map[workp.Item1, workp.Item2].zone_type == ZoneType.SERVICE)
                {
                    if (map[workp.Item1, workp.Item2].zone_info!.people.Count < map[workp.Item1, workp.Item2].zone_info!.max_capacity)
                    {
                        if (_SufficientOxygenSupply(workp))
                        {
                            wp = workp;
                            break;
                        }
                    }
                }
            }
            if(wp != (-1,-1))
            {
                return wp;
            }
            foreach((int, int) workp in map[position.Item1, position.Item2].zone_info!.reachable_connected_zones)
            {
                if (mode == 1 && map[workp.Item1, workp.Item2].zone_type == ZoneType.INDUSTRIAL)
                {
                    if (map[workp.Item1, workp.Item2].zone_info!.people.Count < map[workp.Item1, workp.Item2].zone_info!.max_capacity)
                    {
                        if (_SufficientOxygenSupply(workp))
                        {
                            wp = workp;
                            break;
                        }
                    }
                }
                if (mode == 0 && map[workp.Item1, workp.Item2].zone_type == ZoneType.SERVICE)
                {
                    if (map[workp.Item1, workp.Item2].zone_info!.people.Count < map[workp.Item1, workp.Item2].zone_info!.max_capacity)
                    {
                        if (_SufficientOxygenSupply(workp))
                        {
                            wp = workp;
                            break;
                        }
                    }
                }
            }
            return wp;
        }
        private void _RelocateFromCityHall()
        {
            int colonists_to_house = Math.Min(100,city_hall_residents);
            city_hall_residents -= colonists_to_house;
            List<(int, int)> available_residential_units = new List<(int, int)>();
            foreach ((int, int) pos in connected_zones.ToList())
            {
                switch (map[pos.Item1, pos.Item2].zone_type)
                {
                    case ZoneType.RESIDENTIAL:
                        {
                            if (!_SufficientOxygenSupply(pos))
                            {
                                continue;
                            }
                            available_residential_units.Add(pos);
                        }
                        break;
                }
            }
            available_residential_units.Sort(_ResidentialUnitComparison);
            int wp_mode = 0;
            foreach ((int, int) pos in available_residential_units)
            {
                (int, int) wp = (-1,-1);
                for (int p = map[pos.Item1, pos.Item2].zone_info!.people.Count; p < map[pos.Item1, pos.Item2].zone_info!.max_capacity && colonists_to_house > 0; p++)
                {
                    wp = _ChooseWorkPlace(pos, wp_mode);
                    if (wp == (-1, -1))
                    {
                        break;
                    }
                    Person.Person ps = new Person.Person(pos, wp, 0.5f, 0.5f);
                    if (map[pos.Item1, pos.Item2].zone_info!.people.Count == 0)
                    {
                        map[pos.Item1, pos.Item2].zone_info!.damage = 0;
                    }
                    if (map[wp.Item1, wp.Item2].zone_info!.people.Count == 0)
                    {
                        map[wp.Item1, wp.Item2].zone_info!.damage = 0;
                    }
                    map[pos.Item1, pos.Item2].zone_info!.people[ps.id] = ps;
                    map[wp.Item1, wp.Item2].zone_info!.people[ps.id] = ps;
                    colonists_to_house--;
                    wp_mode = 1 - wp_mode;
                }
                if (colonists_to_house <= 0)
                {
                    break;
                }
            }
            foreach ((int, int) pos in connected_zones.ToList())
            {
                map[pos.Item1, pos.Item2].zone_info!.oxygen_requirement = map[pos.Item1, pos.Item2].zone_info!.people.Count / number_of_colonists_per_unit_of_oxygen;
            }
            city_hall_residents += colonists_to_house;
        }
        private void _CalculateGrowth()
        {
            if (city_hall_residents > 0)
            {
                return;
            }
            int new_colonists = base_arrival_rate + (int)(population * (arrival_rate_modifier / 100.0f));
            List<(int, int)> available_residential_units = new List<(int, int)>();
            foreach ((int, int) pos in connected_zones.ToList())
            {
                switch (map[pos.Item1, pos.Item2].zone_type)
                {
                    case ZoneType.RESIDENTIAL:
                        {
                            if (!_SufficientOxygenSupply(pos))
                            {
                                continue;
                            }
                            available_residential_units.Add(pos);
                        }
                        break;
                }
            }
            available_residential_units.Sort(_ResidentialUnitComparison);
            int wp_mode = 0;
            foreach ((int, int) pos in available_residential_units)
            {
                (int, int) wp = (-1, -1);
                for (int p = map[pos.Item1, pos.Item2].zone_info!.people.Count; p < map[pos.Item1, pos.Item2].zone_info!.max_capacity && new_colonists > 0; p++)
                {
                    wp = _ChooseWorkPlace(pos, wp_mode);
                    if (wp == (-1, -1))
                    {
                        break;
                    }
                    Person.Person ps = new Person.Person(pos, wp, 0.5f, 0.5f);
                    if(map[pos.Item1, pos.Item2].zone_info!.people.Count == 0)
                    {
                        map[pos.Item1, pos.Item2].zone_info!.damage = 0;
                    }
                    if (map[wp.Item1, wp.Item2].zone_info!.people.Count == 0)
                    {
                        map[wp.Item1, wp.Item2].zone_info!.damage = 0;
                    }
                    map[pos.Item1, pos.Item2].zone_info!.people[ps.id] = ps;
                    map[wp.Item1, wp.Item2].zone_info!.people[ps.id] = ps;
                    population++;
                    new_colonists--;
                    wp_mode = 1 - wp_mode;
                }
                if (new_colonists <= 0)
                {
                    break;
                }
            }
            foreach ((int, int) pos in connected_zones.ToList())
            {
                map[pos.Item1, pos.Item2].zone_info!.oxygen_requirement = map[pos.Item1, pos.Item2].zone_info!.people.Count / number_of_colonists_per_unit_of_oxygen;
            }
        }
        private void _NewMonth()
        {
            if (!city_hall_placed)
            {
                monthly_log.Add(new MonthlyLog(time, money, income, upkeep, population, total_public_satisfaction, total_oxygen_supply, total_oxygen_requirement));
                LogUpdate?.Invoke(this, new GameEventArgs { status = EventStatus.SUCCESS, update = EventUpdate.UPDATE, message = "Log updated!" });
                return;
            }
            _CollectTaxes();
            _CalculateUpkeep();
            _CalculateGrowth();
            residental_tax_rate = new_residental_tax_rate;
            industrial_tax_rate = new_industrial_tax_rate;
            service_tax_rate = new_service_tax_rate;
            monthly_log.Add(new MonthlyLog(time, money, income, upkeep, population, total_public_satisfaction, total_oxygen_supply, total_oxygen_requirement));
            LogUpdate?.Invoke(this, new GameEventArgs { status = EventStatus.SUCCESS, update = EventUpdate.UPDATE, message = "Log updated!" });
        }
        private void _NewYear()
        {
            _NewMonth();
        }
        #endregion
        #region Building
        /// <summary>
        /// Attempts to put down a building of the specified type at the specified location.
        /// </summary>
        /// <param name="location">The location of the top left corner of the new building.</param>
        /// <param name="building_type">Specifies the type of the new building.</param>
        public int PlaceBuilding((int, int) location, BuildingType building_type)
        {
            bool can_place = true;
            (int, int) building_size = (1, 1);
            bool is_city_hall = false;
            switch (building_type)
            {
                case BuildingType.PARK:
                    building_size = (2, 2);
                    break;
                case BuildingType.POLICE_STATION:
                    building_size = (2, 1);
                    break;
                case BuildingType.CITY_HALL:
                    building_size = (2, 2);
                    is_city_hall = true;
                    break;
                case BuildingType.OXYGEN_GENERATOR:
                    building_size = (3, 2);
                    break;
                case BuildingType.OXYGEN_DIFFUSER:
                    building_size = (1, 1);
                    break;
            }
            if (!city_hall_placed)
            {
                if (!is_city_hall)
                {
                    StatusUpdate?.Invoke(this, new GameEventArgs { status = EventStatus.ERROR, update = EventUpdate.NO_UPDATE, message = "You need to place the city hall first!" });
                    return -1;
                }
            }
            else
            {
                if (is_city_hall)
                {
                    StatusUpdate?.Invoke(this, new GameEventArgs { status = EventStatus.ERROR, update = EventUpdate.NO_UPDATE, message = "You can only have one city hall!" });
                    return -1;
                }
            }
            for (int x = location.Item1; x < location.Item1 + building_size.Item1; x++)
            {
                for (int y = location.Item2; y < location.Item2 + building_size.Item2; y++)
                {
                    if(!_InsideMap((x,y)))
                    {
                        can_place = false;
                        break;
                    }
                    if (map[x, y].zone_type != ZoneType.NONE || map[x, y].building != null || map[x, y].road != null)
                    {
                        can_place = false;
                    }
                }
            }
            if (can_place)
            {
                switch (building_type)
                {
                    case BuildingType.PARK:
                        {
                            bool is_connected = false;
                            map[location.Item1, location.Item2].building = new Park(SubBuildingPositionType.MAIN);
                            map[location.Item1 + 1, location.Item2].building = new Park(SubBuildingPositionType.TOP_RIGHT);
                            map[location.Item1 + 1, location.Item2 + 1].building = new Park(SubBuildingPositionType.BOTTOM_RIGHT);
                            map[location.Item1, location.Item2 + 1].building = new Park(SubBuildingPositionType.BOTTOM_LEFT);
                            for (int x = location.Item1; x < location.Item1 + building_size.Item1; x++)
                            {
                                for (int y = location.Item2; y < location.Item2 + building_size.Item2; y++)
                                {
                                    map[x, y].zone_info!.damage = 0;
                                }
                            }
                            money -= map[location.Item1, location.Item2].building!.building_price;
                            spending_log.Add(new SpendingInfo(map[location.Item1, location.Item2].building!.building_price, time, "Park placed"));
                            upkeep += map[location.Item1, location.Item2].building!.building_upkeep;
                            income -= map[location.Item1, location.Item2].building!.building_upkeep;
                            for (int x = location.Item1 - 1; x < location.Item1 + building_size.Item1 + 1; x++)
                            {
                                for (int y = location.Item2 - 1; y < location.Item2 + building_size.Item2 + 1; y++)
                                {
                                    if (_InsideMap((x, y)))
                                    {
                                        if (map[x, y].road != null)
                                        {
                                            if (road_network[map[x, y].road!.network_id].connected_to_city_hall)
                                            {
                                                is_connected = true;
                                            }
                                        }
                                        if (map[x, y].building != null)
                                        {
                                            if (map[x, y].building!.building_type == BuildingType.CITY_HALL)
                                            {
                                                is_connected = true;
                                            }
                                        }
                                    }
                                }
                            }
                            if (is_connected)
                            {
                                _ConnectBuilding(location);
                            }
                        }
                        break;
                    case BuildingType.POLICE_STATION:
                        {
                            bool is_connected = false;
                            map[location.Item1, location.Item2].building = new PoliceStation(SubBuildingPositionType.MAIN);
                            map[location.Item1 + 1, location.Item2].building = new PoliceStation(SubBuildingPositionType.TOP_RIGHT);
                            for (int x = location.Item1; x < location.Item1 + building_size.Item1; x++)
                            {
                                for (int y = location.Item2; y < location.Item2 + building_size.Item2; y++)
                                {
                                    map[x, y].zone_info!.damage = 0;
                                }
                            }
                            money -= map[location.Item1, location.Item2].building!.building_price;
                            spending_log.Add(new SpendingInfo(map[location.Item1, location.Item2].building!.building_price, time, "Police station placed"));
                            upkeep += map[location.Item1, location.Item2].building!.building_upkeep;
                            income -= map[location.Item1, location.Item2].building!.building_upkeep;
                            for (int x = location.Item1 - 1; x < location.Item1 + building_size.Item1 + 1; x++)
                            {
                                for (int y = location.Item2 - 1; y < location.Item2 + building_size.Item2 + 1; y++)
                                {
                                    if (_InsideMap((x, y)))
                                    {
                                        if (map[x, y].road != null)
                                        {
                                            if (road_network[map[x, y].road!.network_id].connected_to_city_hall)
                                            {
                                                is_connected = true;
                                            }
                                        }
                                        if (map[x, y].building != null)
                                        {
                                            if (map[x, y].building!.building_type == BuildingType.CITY_HALL)
                                            {
                                                is_connected = true;
                                            }
                                        }
                                    }
                                }
                            }
                            if (is_connected)
                            {
                                _ConnectBuilding(location);
                            }
                        }
                        break;
                    case BuildingType.CITY_HALL:
                        {
                            map[location.Item1, location.Item2].building = new CityHall(SubBuildingPositionType.MAIN);
                            map[location.Item1 + 1, location.Item2].building = new CityHall(SubBuildingPositionType.TOP_RIGHT);
                            map[location.Item1 + 1, location.Item2 + 1].building = new CityHall(SubBuildingPositionType.BOTTOM_RIGHT);
                            map[location.Item1, location.Item2 + 1].building = new CityHall(SubBuildingPositionType.BOTTOM_LEFT);
                            for (int x = location.Item1; x < location.Item1 + building_size.Item1; x++)
                            {
                                for (int y = location.Item2; y < location.Item2 + building_size.Item2; y++)
                                {
                                    map[x, y].zone_info!.damage = 0;
                                }
                            }
                            money -= map[location.Item1, location.Item2].building!.building_price;
                            spending_log.Add(new SpendingInfo(map[location.Item1, location.Item2].building!.building_price, time, "City hall placed"));
                            upkeep += map[location.Item1, location.Item2].building!.building_upkeep;
                            income -= map[location.Item1, location.Item2].building!.building_upkeep;
                        }
                        break;
                    case BuildingType.OXYGEN_GENERATOR:
                        {
                            bool is_connected = false;
                            map[location.Item1, location.Item2].building = new OxygenGenerator(SubBuildingPositionType.MAIN);
                            map[location.Item1 + 1, location.Item2].building = new OxygenGenerator(SubBuildingPositionType.TOP_CENTER);
                            map[location.Item1 + 2, location.Item2].building = new OxygenGenerator(SubBuildingPositionType.TOP_RIGHT);
                            map[location.Item1 + 2, location.Item2 + 1].building = new OxygenGenerator(SubBuildingPositionType.BOTTOM_RIGHT);
                            map[location.Item1 + 1, location.Item2 + 1].building = new OxygenGenerator(SubBuildingPositionType.BOTTOM_CENTER);
                            map[location.Item1, location.Item2 + 1].building = new OxygenGenerator(SubBuildingPositionType.BOTTOM_LEFT);
                            for (int x = location.Item1; x < location.Item1 + building_size.Item1; x++)
                            {
                                for (int y = location.Item2; y < location.Item2 + building_size.Item2; y++)
                                {
                                    map[x, y].zone_info!.damage = 0;
                                }
                            }
                            money -= map[location.Item1, location.Item2].building!.building_price;
                            spending_log.Add(new SpendingInfo(map[location.Item1, location.Item2].building!.building_price, time, "Oxygen generator placed"));
                            upkeep += map[location.Item1, location.Item2].building!.building_upkeep;
                            income -= map[location.Item1, location.Item2].building!.building_upkeep;
                            for (int x = location.Item1 - 1; x < location.Item1 + building_size.Item1 + 1; x++)
                            {
                                for (int y = location.Item2 - 1; y < location.Item2 + building_size.Item2 + 1; y++)
                                {
                                    if (_InsideMap((x, y)))
                                    {
                                        if (map[x, y].road != null)
                                        {
                                            if (road_network[map[x, y].road!.network_id].connected_to_city_hall)
                                            {
                                                is_connected = true;
                                            }
                                        }
                                        if (map[x, y].building != null)
                                        {
                                            if (map[x, y].building!.building_type == BuildingType.CITY_HALL)
                                            {
                                                is_connected = true;
                                            }
                                        }
                                    }
                                }
                            }
                            if (is_connected)
                            {
                                _ConnectBuilding(location);
                            }
                        }
                        break;
                    case BuildingType.OXYGEN_DIFFUSER:
                        {
                            bool is_connected = false;
                            map[location.Item1, location.Item2].building = new OxygenDiffuser(SubBuildingPositionType.MAIN);
                            for (int x = location.Item1; x < location.Item1 + building_size.Item1; x++)
                            {
                                for (int y = location.Item2; y < location.Item2 + building_size.Item2; y++)
                                {
                                    map[x, y].zone_info!.damage = 0;
                                }
                            }
                            money -= map[location.Item1, location.Item2].building!.building_price;
                            spending_log.Add(new SpendingInfo(map[location.Item1, location.Item2].building!.building_price, time, "Oxygen diffuser placed"));
                            upkeep += map[location.Item1, location.Item2].building!.building_upkeep;
                            income -= map[location.Item1, location.Item2].building!.building_upkeep;
                            for (int x = location.Item1 - 1; x < location.Item1 + building_size.Item1 + 1; x++)
                            {
                                for (int y = location.Item2 - 1; y < location.Item2 + building_size.Item2 + 1; y++)
                                {
                                    if (_InsideMap((x, y)))
                                    {
                                        if (map[x, y].road != null)
                                        {
                                            if (road_network[map[x, y].road!.network_id].connected_to_city_hall)
                                            {
                                                is_connected = true;
                                            }
                                        }
                                        if (map[x, y].building != null)
                                        {
                                            if (map[x, y].building!.building_type == BuildingType.CITY_HALL)
                                            {
                                                is_connected = true;
                                            }
                                        }
                                    }
                                }
                            }
                            if (is_connected)
                            {
                                _ConnectBuilding(location);
                            }
                        }
                        break;
                }
                if (is_city_hall)
                {
                    city_hall_placed = true;
                    population += 1000;
                    city_hall_residents = 1000;
                }
                StatusUpdate?.Invoke(this, new GameEventArgs { status = EventStatus.SUCCESS, update = EventUpdate.UPDATE, message = "City hall placed successfully!" });
            }
            else
            {
                StatusUpdate?.Invoke(this, new GameEventArgs { status = EventStatus.ERROR, update = EventUpdate.NO_UPDATE, message = "This spot is blocked!" });
            }
            return 0;
        }
        #endregion
        private bool _InsideMap((int, int) position)
        {
            return (position.Item1 >= 0 && position.Item1 < map_size.Item1 && position.Item2 >= 0 && position.Item2 < map_size.Item2);
        }
        private (int, int) _JumpToMain((int, int) position)
        {
            if(map[position.Item1, position.Item2].building == null)
            {
                return position;
            }
            (int, int) location = position;
            switch (map[position.Item1, position.Item2].building!.sub_building_position)
            {
                case SubBuildingPositionType.TOP_CENTER:
                    location.Item1--;
                    break;
                case SubBuildingPositionType.TOP_RIGHT:
                    location.Item1 -= map[position.Item1, position.Item2].building!.building_size.Item1 - 1;
                    break;
                case SubBuildingPositionType.BOTTOM_RIGHT:
                    location.Item1 -= map[position.Item1, position.Item2].building!.building_size.Item1 - 1;
                    location.Item2 -= map[position.Item1, position.Item2].building!.building_size.Item2 - 1;
                    break;
                case SubBuildingPositionType.BOTTOM_CENTER:
                    location.Item1--;
                    location.Item2--;
                    break;
                case SubBuildingPositionType.BOTTOM_LEFT:
                    location.Item2 -= map[position.Item1, position.Item2].building!.building_size.Item2 - 1;
                    break;
            }
            return location;
        }
        private bool _AbleToPlaceRoad((int,int) position)
        {
            if(!_InsideMap(position))
            {
                return false;
            }
            if(map[position.Item1,position.Item2].building != null)
            {
                return false;
            }
            if (map[position.Item1, position.Item2].zone_type != ZoneType.NONE)
            {
                return false;
            }
            return true;
        }
        private void _ConnectZone((int,int) position)
        {
            map[position.Item1, position.Item2].zone_info!.reachable_connected_zones.Clear();
            for (int x = position.Item1 - zone_reach; x <= position.Item1 + zone_reach; x++)
            {
                for (int y = position.Item2 - zone_reach; y <= position.Item2 + zone_reach; y++)
                {
                    if (_InsideMap((x,y)) && (x != position.Item1 || y != position.Item2))
                    {
                        if (map[x,y].zone_type != ZoneType.NONE)
                        {
                            if (map[x,y].zone_info!.connected)
                            {
                                map[position.Item1, position.Item2].zone_info!.reachable_connected_zones.Add((x, y));
                                map[x,y].zone_info!.reachable_connected_zones.Add((position.Item1, position.Item2));
                                if(map[x, y].zone_info!.damage > 0)
                                {
                                    map[position.Item1, position.Item2].zone_info!.damaged_nearby_buildings++;
                                }
                            }
                        }
                    }
                }
            }
            OxygenDiffuser generic_diffuser = new OxygenDiffuser(SubBuildingPositionType.MAIN);
            for (int x = position.Item1 - generic_diffuser.radius; x <= position.Item1 + generic_diffuser.radius; x++)
            {
                for (int y = position.Item2 - generic_diffuser.radius; y <= position.Item2 + generic_diffuser.radius; y++)
                {
                    if (_InsideMap((x, y)) && (x != position.Item1 || y != position.Item2))
                    {
                        if (map[x, y].building != null)
                        {
                            if (map[x, y].building!.connected)
                            {
                                switch(map[x, y].building!.building_type)
                                {
                                    case BuildingType.OXYGEN_DIFFUSER:
                                        {
                                            OxygenDiffuser od = (OxygenDiffuser)(map[x, y].building!);
                                            if(od.network_id != -1)
                                            {
                                                pipe_network[od.network_id].supplied_zones.Add(position);
                                                map[position.Item1, position.Item2].zone_info!.connected_oxygen_diffusers.Add(od.network_id);
                                            }
                                        }
                                        break;
                                }
                            }
                        }
                    }
                }
            }
        }
        private void _DisconnectZone((int,int) position)
        {
            map[position.Item1, position.Item2].zone_info!.connected = false;
            foreach ((int, int) reachable_zone in map[position.Item1, position.Item2].zone_info!.reachable_connected_zones)
            {
                map[reachable_zone.Item1, reachable_zone.Item2].zone_info!.reachable_connected_zones.Remove(position);
            }
            foreach(int network_id in map[position.Item1,position.Item2].zone_info!.connected_oxygen_diffusers)
            {
                pipe_network[network_id].supplied_zones.Remove(position);
            }
            connected_zones.Remove(position);
        }
        private void _ConnectDiffuser((int,int) position)
        {
            OxygenDiffuser od = (OxygenDiffuser)(map[position.Item1, position.Item2].building!);
            pipe_network[od.network_id].connected_diffusers.Add(position);
            for (int x = position.Item1 - map[position.Item1, position.Item2].building!.radius; x < position.Item1 + map[position.Item1, position.Item2].building!.building_size.Item1 + map[position.Item1, position.Item2].building!.radius; x++)
            {
                for (int y = position.Item2 - map[position.Item1, position.Item2].building!.radius; y < position.Item2 + map[position.Item1, position.Item2].building!.building_size.Item2 + map[position.Item1, position.Item2].building!.radius; y++)
                {
                    if(position.Item1 == x && position.Item2 == y)
                    {
                        continue;
                    }
                    if (_InsideMap((x, y)))
                    {
                        if (map[x,y].building == null)
                        {
                            if (map[x, y].zone_info!.connected)
                            {
                                map[x, y].zone_info!.connected_oxygen_diffusers.Add(od.network_id);
                                pipe_network[od.network_id].supplied_zones.Add((x, y));
                            }
                        }
                        else
                        {
                            if (map[x, y].building!.connected)
                            {
                                (int, int) location = _JumpToMain((x, y));
                                if(map[location.Item1, location.Item2].building!.building_type == BuildingType.PARK || map[location.Item1, location.Item2].building!.building_type == BuildingType.POLICE_STATION)
                                {
                                    map[location.Item1, location.Item2].zone_info!.connected_oxygen_diffusers.Add(od.network_id);
                                    pipe_network[od.network_id].supplied_zones.Add(location);
                                }
                            }
                        }
                    }
                }
            }
        }
        private void _ConnectGenerator((int, int) position, int network_id)
        {
            (int, int) location = _JumpToMain(position);
            OxygenGenerator og = (OxygenGenerator)(map[location.Item1, location.Item2].building!);
            for (int x = location.Item1; x < location.Item1 + map[location.Item1, location.Item2].building!.building_size.Item1; x++)
            {
                for (int y = location.Item2; y < location.Item2 + map[location.Item1, location.Item2].building!.building_size.Item2; y++)
                {
                    OxygenGenerator osg = (OxygenGenerator)(map[x,y].building!);
                    osg.network_id = network_id;
                }
            }
            if(network_id != -1)
            {
                pipe_network[network_id].total_oxygen_supply += og.oxygen_generation;
                pipe_network[network_id].connected_generators.Add(position);
            }
        }
        private bool _SearchForPipeNetwork((int, int) position)
        {
            position = _JumpToMain(position);
            int mode = -1;
            switch(map[position.Item1,position.Item2].building!.building_type)
            {
                case BuildingType.OXYGEN_GENERATOR:
                    {
                        mode = 0;
                    }
                    break;
                case BuildingType.OXYGEN_DIFFUSER:
                    {
                        mode = 1;
                    }
                    break;
            }
            SortedSet<int> networks_to_connect = new SortedSet<int>();
            if (mode == 0)
            {
                for (int x = position.Item1; x < position.Item1 + map[position.Item1, position.Item2].building!.building_size.Item1; x++)
                {
                    for (int y = position.Item2; y < position.Item2 + map[position.Item1, position.Item2].building!.building_size.Item2; y++)
                    {
                        if (map[x, y].pipe != null)
                        {
                            networks_to_connect.Add(map[x, y].pipe!.network_id);
                        }
                    }
                }
            }
            if (mode == 1)
            {
                if (map[position.Item1,position.Item2].pipe != null)
                {
                    networks_to_connect.Add(map[position.Item1, position.Item2].pipe!.network_id);
                }
                for (int x = position.Item1 - 1; x < position.Item1 + map[position.Item1, position.Item2].building!.building_size.Item1 + 1; x++)
                {
                    for (int y = position.Item2 - 1; y < position.Item2 + map[position.Item1, position.Item2].building!.building_size.Item2 + 1; y++)
                    {
                        if (_InsideMap((x, y)))
                        {
                            if (map[x, y].building != null)
                            {
                                if (map[x, y].building!.building_type == BuildingType.OXYGEN_GENERATOR && map[x, y].building!.connected)
                                {
                                    OxygenGenerator og = (OxygenGenerator)(map[x, y].building!);
                                    if (og.network_id != -1)
                                    {
                                        networks_to_connect.Add(og.network_id);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (networks_to_connect.Count == 1)
            {
                if (mode == 0)
                {
                    OxygenGenerator og = (OxygenGenerator)(map[position.Item1, position.Item2].building!);//POSITION TO JUMP MAIN
                    _ConnectGenerator(position,networks_to_connect.First());
                }
                if (mode == 1)
                {
                    OxygenDiffuser od = (OxygenDiffuser)(map[position.Item1, position.Item2].building!);
                    od.network_id = networks_to_connect.First();
                    _ConnectDiffuser(position);
                }
            }
            if (networks_to_connect.Count > 1)
            {
                int network_id = networks_to_connect.Min();
                foreach (int network_to_merge in networks_to_connect.Reverse())
                {
                    if (network_to_merge != network_id)
                    {
                        pipe_network[network_id].Merge(pipe_network[network_to_merge]);
                    }
                }
                int del_cnt = 0;
                for (int p = 0; p < pipe_network.Count; p++)
                {
                    if (networks_to_connect.Contains(p))
                    {
                        if (p != network_id)
                        {
                            del_cnt++;
                        }
                    }
                    else
                    {
                        pipe_network[p].RefreshNetworkID(pipe_network[p].network_id - del_cnt);
                    }
                }
                foreach (int network_to_merge in networks_to_connect.Reverse())
                {
                    if (network_to_merge != network_id)
                    {
                        pipe_network.Remove(pipe_network[network_to_merge]);
                    }
                }
                if (mode == 0)
                {
                    OxygenGenerator og = (OxygenGenerator)(map[position.Item1, position.Item2].building!);
                    _ConnectGenerator(position, networks_to_connect.Min());
                }
                if (mode == 1)
                {
                    OxygenDiffuser od = (OxygenDiffuser)(map[position.Item1, position.Item2].building!);
                    od.network_id = networks_to_connect.Min();
                    _ConnectDiffuser(position);
                }
                _RecalculatePipeNetworkConnections(network_id);
                return true;
            }
            return false;
        }
        private void _ConnectBuilding((int,int) position)
        {
            (int, int) location = _JumpToMain(position);
            if (!(map[location.Item1,location.Item2].building!.connected))
            {
                map[location.Item1, location.Item2].building!.connected = true;
                switch (map[position.Item1, position.Item2].building!.building_type)
                {
                    case BuildingType.PARK:
                        {
                            buildings_with_modifiers.Add(location);
                            _RecalculateModifiers();
                            OxygenDiffuser generic_diffuser = new OxygenDiffuser(SubBuildingPositionType.MAIN);
                            for (int x = location.Item1 - generic_diffuser.radius; x <= location.Item1 + map[location.Item1,location.Item2].building!.building_size.Item1 + generic_diffuser.radius; x++)
                            {
                                for (int y = location.Item2 - generic_diffuser.radius; y <= location.Item2 + map[location.Item1, location.Item2].building!.building_size.Item2 + generic_diffuser.radius; y++)
                                {
                                    if (_InsideMap((x, y)) && (x != location.Item1 || y != location.Item2))
                                    {
                                        if (map[x, y].building != null)
                                        {
                                            if (map[x, y].building!.connected && map[x, y].building!.building_type == BuildingType.OXYGEN_DIFFUSER)
                                            {
                                                OxygenDiffuser od = (OxygenDiffuser)(map[x, y].building!);
                                                if (od.network_id != -1)
                                                {
                                                    pipe_network[od.network_id].supplied_zones.Add(location);
                                                    map[location.Item1, location.Item2].zone_info!.connected_oxygen_diffusers.Add(od.network_id);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case BuildingType.POLICE_STATION:
                        {
                            buildings_with_modifiers.Add(location);
                            _RecalculateModifiers();
                            OxygenDiffuser generic_diffuser = new OxygenDiffuser(SubBuildingPositionType.MAIN);
                            for (int x = location.Item1 - generic_diffuser.radius; x <= location.Item1 + map[location.Item1, location.Item2].building!.building_size.Item1 + generic_diffuser.radius; x++)
                            {
                                for (int y = location.Item2 - generic_diffuser.radius; y <= location.Item2 + map[location.Item1, location.Item2].building!.building_size.Item2 + generic_diffuser.radius; y++)
                                {
                                    if (_InsideMap((x, y)) && (x != location.Item1 || y != location.Item2))
                                    {
                                        if (map[x, y].building != null)
                                        {
                                            if (map[x, y].building!.connected && map[x, y].building!.building_type == BuildingType.OXYGEN_DIFFUSER)
                                            {
                                                OxygenDiffuser od = (OxygenDiffuser)(map[x, y].building!);
                                                if (od.network_id != -1)
                                                {
                                                    pipe_network[od.network_id].supplied_zones.Add(location);
                                                    map[location.Item1, location.Item2].zone_info!.connected_oxygen_diffusers.Add(od.network_id);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case BuildingType.OXYGEN_DIFFUSER:
                        {
                            _SearchForPipeNetwork(position);
                        }
                        break;
                    case BuildingType.OXYGEN_GENERATOR:
                        {
                            _SearchForPipeNetwork(position);
                        }
                        break;
                }
            }
        }
        #region Road
        private void _RecalculateRoadNetworkConnections(int network_id)
        {
            (int, int) location;
            List<(int, int)> newly_connected_zones = new List<(int, int)>();
            foreach (KeyValuePair<int, RoadNetworkNode> rnn in road_network[network_id].road_network)
            {
                location = rnn.Value.location;
                location.Item1--;
                if (_InsideMap(location))
                {
                    if (map[location.Item1, location.Item2].road != null)
                    {
                        rnn.Value.connections[2] = map[location.Item1, location.Item2].road!.id;
                        rnn.Value.connected_tiles[2] = (-1, -1);
                    }
                    else
                    {
                        rnn.Value.connections[2] = -1;
                        rnn.Value.connected_tiles[2] = location;
                        if (map[location.Item1, location.Item2].zone_type != ZoneType.NONE)
                        {
                            if(road_network[network_id].connected_to_city_hall)
                            {
                                if(!map[location.Item1, location.Item2].zone_info!.connected)
                                {
                                    map[location.Item1, location.Item2].zone_info!.connected = true;
                                    connected_zones.Add(location);
                                    newly_connected_zones.Add(location);
                                }
                            }
                        }
                        if (map[location.Item1, location.Item2].building != null)
                        {
                            if (road_network[network_id].connected_to_city_hall)
                            {
                                _ConnectBuilding(location);
                            }
                        }
                    }
                }
                location = rnn.Value.location;
                location.Item1++;
                if (_InsideMap(location))
                {
                    if (map[location.Item1, location.Item2].road != null)
                    {
                        rnn.Value.connections[0] = map[location.Item1, location.Item2].road!.id;
                        rnn.Value.connected_tiles[0] = (-1, -1);
                    }
                    else
                    {
                        rnn.Value.connections[0] = -1;
                        rnn.Value.connected_tiles[0] = location;
                        if (map[location.Item1, location.Item2].zone_type != ZoneType.NONE)
                        {
                            if (road_network[network_id].connected_to_city_hall)
                            {
                                if (!map[location.Item1, location.Item2].zone_info!.connected)
                                {
                                    map[location.Item1, location.Item2].zone_info!.connected = true;
                                    connected_zones.Add(location);
                                    newly_connected_zones.Add(location);
                                }
                            }
                        }
                        if (map[location.Item1, location.Item2].building != null)
                        {
                            if (road_network[network_id].connected_to_city_hall)
                            {
                                _ConnectBuilding(location);
                            }
                        }
                    }
                }
                location = rnn.Value.location;
                location.Item2--;
                if (_InsideMap(location))
                {
                    if (map[location.Item1, location.Item2].road != null)
                    {
                        rnn.Value.connections[1] = map[location.Item1, location.Item2].road!.id;
                        rnn.Value.connected_tiles[1] = (-1, -1);
                    }
                    else
                    {
                        rnn.Value.connections[1] = -1;
                        rnn.Value.connected_tiles[1] = location;
                        if (map[location.Item1, location.Item2].zone_type != ZoneType.NONE)
                        {
                            if (road_network[network_id].connected_to_city_hall)
                            {
                                if (!map[location.Item1, location.Item2].zone_info!.connected)
                                {
                                    map[location.Item1, location.Item2].zone_info!.connected = true;
                                    connected_zones.Add(location);
                                    newly_connected_zones.Add(location);
                                }
                            }
                        }
                        if (map[location.Item1, location.Item2].building != null)
                        {
                            if (road_network[network_id].connected_to_city_hall)
                            {
                                _ConnectBuilding(location);
                            }
                        }
                    }
                }
                location = rnn.Value.location;
                location.Item2++;
                if (_InsideMap(location))
                {
                    if (map[location.Item1, location.Item2].road != null)
                    {
                        rnn.Value.connections[3] = map[location.Item1, location.Item2].road!.id;
                        rnn.Value.connected_tiles[3] = (-1, -1);
                    }
                    else
                    {
                        rnn.Value.connections[3] = -1;
                        rnn.Value.connected_tiles[3] = location;
                        if (map[location.Item1, location.Item2].zone_type != ZoneType.NONE)
                        {
                            if (road_network[network_id].connected_to_city_hall)
                            {
                                if (!map[location.Item1, location.Item2].zone_info!.connected)
                                {
                                    map[location.Item1, location.Item2].zone_info!.connected = true;
                                    connected_zones.Add(location);
                                    newly_connected_zones.Add(location);
                                }
                            }
                        }
                        if (map[location.Item1, location.Item2].building != null)
                        {
                            if (road_network[network_id].connected_to_city_hall)
                            {
                                _ConnectBuilding(location);
                            }
                        }
                    }
                }
            }
            foreach ((int, int) loc in newly_connected_zones)
            {
                _ConnectZone(loc);
            }
        }
        private List<(int, int)> _FindRoute((int, int) starting_from, (int, int) finishing_at)
        {
            List<(int,int)> route = new List<(int,int)>();
            (int,int)[,] parent = new (int,int)[map_size.Item1, map_size.Item2];
            for(int x = 0; x < map_size.Item1; x++)
            {
                for(int y = 0; y < map_size.Item2; y++)
                {
                    parent[x, y] = (-1,-1);
                }
            }
            parent[starting_from.Item1, starting_from.Item2] = starting_from;
            Queue<(int,int)> route_queue = new Queue<(int,int)>();
            route_queue.Enqueue(starting_from);
            while (route_queue.Count != 0)
            {
                (int,int) act = route_queue.Dequeue();
                if(act == finishing_at)
                {
                    break;
                }
                if(_AbleToPlaceRoad((act.Item1 - 1, act.Item2)))
                {
                    if(parent[act.Item1 - 1, act.Item2] == (-1,-1))
                    {
                        route_queue.Enqueue((act.Item1 - 1, act.Item2));
                        parent[act.Item1 - 1, act.Item2] = act;
                    }
                }
                if (_AbleToPlaceRoad((act.Item1 + 1, act.Item2)))
                {
                    if (parent[act.Item1 + 1, act.Item2] == (-1, -1))
                    {
                        route_queue.Enqueue((act.Item1 + 1, act.Item2));
                        parent[act.Item1 + 1, act.Item2] = act;
                    }
                }
                if (_AbleToPlaceRoad((act.Item1, act.Item2 - 1)))
                {
                    if (parent[act.Item1, act.Item2 - 1] == (-1, -1))
                    {
                        route_queue.Enqueue((act.Item1, act.Item2 - 1));
                        parent[act.Item1, act.Item2 - 1] = act;
                    }
                }
                if (_AbleToPlaceRoad((act.Item1, act.Item2 + 1)))
                {
                    if (parent[act.Item1, act.Item2 + 1] == (-1, -1))
                    {
                        route_queue.Enqueue((act.Item1, act.Item2 + 1));
                        parent[act.Item1, act.Item2 + 1] = act;
                    }
                }
            }
            (int, int) position = finishing_at;
            if (parent[position.Item1, position.Item2] != (-1,-1))
            {
                route.Add(position);
                while (position != starting_from)
                {
                    position = parent[position.Item1, position.Item2];
                    route.Add(position);
                }
            }
            return route;
        }
        /// <summary>
        /// Attempts to place a road connecting <paramref name="starting_from"/> with <paramref name="finishing_at"/>.<br/>
        /// The placed roads either create a new road-network or merge into one.<br/>
        /// </summary>
        /// <param name="starting_from">The starting location.</param>
        /// <param name="finishing_at">The finishing location.</param>
        public int PlaceRoad((int, int) starting_from, (int, int) finishing_at)
        {
            if(!city_hall_placed)
            {
                StatusUpdate?.Invoke(this, new GameEventArgs { status = EventStatus.ERROR, update = EventUpdate.NO_UPDATE, message = "You need to place the city hall first!" });
                return -1;
            }
            if (!_AbleToPlaceRoad((starting_from.Item1, starting_from.Item2)) ||
                !_AbleToPlaceRoad((finishing_at.Item1, finishing_at.Item2)))
            {
                StatusUpdate?.Invoke(this, new GameEventArgs { status = EventStatus.ERROR, update = EventUpdate.NO_UPDATE, message = "Road ending(s) blocked!" });
                return -2;
            }
            List<(int,int)> route = _FindRoute(starting_from, finishing_at);
            if(route.Count == 0)
            {
                StatusUpdate?.Invoke(this, new GameEventArgs { status = EventStatus.ERROR, update = EventUpdate.NO_UPDATE, message = "Can not find suitable route!" });
                return -3;
            }
            SortedSet<int> intersected_networks = new SortedSet<int>();
            (int, int) location;
            bool is_connected = false;
            foreach ((int, int) position in route)
            {
                if (map[position.Item1,position.Item2].road != null)
                {
                    RoadNetworkNode rnn = map[position.Item1, position.Item2].road!;
                    intersected_networks.Add(rnn.network_id);
                }
                location = position;
                location.Item1--;
                if (_InsideMap(location))
                {
                    if (map[location.Item1, location.Item2].road != null)
                    {
                        RoadNetworkNode rnn = map[location.Item1, location.Item2].road!;
                        intersected_networks.Add(rnn.network_id);
                    }
                    if (map[location.Item1, location.Item2].building != null)
                    {
                        if (map[location.Item1, location.Item2].building!.building_type == BuildingType.CITY_HALL)
                        {
                            is_connected = true;
                        }
                    }
                }
                location = position;
                location.Item1++;
                if (_InsideMap(location))
                {
                    if (map[location.Item1, location.Item2].road != null)
                    {
                        RoadNetworkNode rnn = map[location.Item1, location.Item2].road!;
                        intersected_networks.Add(rnn.network_id);
                    }
                    if (map[location.Item1, location.Item2].building != null)
                    {
                        if (map[location.Item1, location.Item2].building!.building_type == BuildingType.CITY_HALL)
                        {
                            is_connected = true;
                        }
                    }
                }
                location = position;
                location.Item2--;
                if (_InsideMap(location))
                {
                    if (map[location.Item1, location.Item2].road != null)
                    {
                        RoadNetworkNode rnn = map[location.Item1, location.Item2].road!;
                        intersected_networks.Add(rnn.network_id);
                    }
                    if (map[location.Item1, location.Item2].building != null)
                    {
                        if (map[location.Item1, location.Item2].building!.building_type == BuildingType.CITY_HALL)
                        {
                            is_connected = true;
                        }
                    }
                }
                location = position;
                location.Item2++;
                if (_InsideMap(location))
                {
                    if (map[location.Item1, location.Item2].road != null)
                    {
                        RoadNetworkNode rnn = map[location.Item1, location.Item2].road!;
                        intersected_networks.Add(rnn.network_id);
                    }
                    if (map[location.Item1, location.Item2].building != null)
                    {
                        if (map[location.Item1, location.Item2].building!.building_type == BuildingType.CITY_HALL)
                        {
                            is_connected = true;
                        }
                    }
                }
            }
            if (intersected_networks.Count == 0)
            {
                int new_network_id = road_network.Count;
                road_network.Add(new RoadNetwork(new_network_id));
                int cnt = 0;
                int placed_road_count = 0;
                foreach ((int, int) pos in route)
                {
                    road_network[new_network_id].road_network[cnt] = new RoadNetworkNode(cnt, new_network_id, pos);
                    map[pos.Item1, pos.Item2].road = road_network[new_network_id].road_network[cnt];
                    cnt++;
                    placed_road_count++;
                    money -= road_price;
                    upkeep += road_upkeep;
                    income -= road_upkeep;
                }
                if (placed_road_count > 0)
                {
                    spending_log.Add(new SpendingInfo(placed_road_count * road_price, time, placed_road_count.ToString() + " road(s) placed"));
                }
                road_network[new_network_id].connected_to_city_hall = is_connected;
                _RecalculateRoadNetworkConnections(new_network_id);
            }
            else
            {
                int network_id = intersected_networks.Min();
                foreach(int network_to_merge in intersected_networks.Reverse())
                {
                    if (network_to_merge != network_id)
                    {
                        road_network[network_id].Merge(road_network[network_to_merge]);
                    }
                }
                int del_cnt = 0;
                for(int p = 0; p < road_network.Count; p++)
                {
                    if(intersected_networks.Contains(p))
                    {
                        if (p != network_id)
                        {
                            del_cnt++;
                        }
                    }
                    else
                    {
                        road_network[p].RefreshNetworkID(road_network[p].network_id - del_cnt);
                    }
                }
                foreach (int network_to_merge in intersected_networks.Reverse())
                {
                    if (network_to_merge != network_id)
                    {
                        road_network.Remove(road_network[network_to_merge]);
                    }
                }
                if (is_connected)
                {
                    road_network[network_id].connected_to_city_hall = true;
                }
                int cnt = road_network[network_id].road_network.Count;
                int placed_road_count = 0;
                foreach ((int, int) loc in route)
                {
                    if (map[loc.Item1, loc.Item2].road == null)
                    {
                        road_network[network_id].road_network[cnt] = new RoadNetworkNode(cnt, network_id, loc);
                        map[loc.Item1, loc.Item2].road = road_network[network_id].road_network[cnt];
                        cnt++;
                        placed_road_count++;
                        money -= road_price;
                        upkeep += road_upkeep;
                        income -= road_upkeep;
                    }
                }
                if(placed_road_count > 0)
                {
                    spending_log.Add(new SpendingInfo(placed_road_count * road_price, time, placed_road_count.ToString() + " road(s) placed"));
                }
                _RecalculateRoadNetworkConnections(network_id);
            }
            StatusUpdate?.Invoke(this, new GameEventArgs { status = EventStatus.SUCCESS, update = EventUpdate.UPDATE, message = "Road placed successfully!" });
            return 0;
        }
        private List<int> _GetReachableRoadNetworkNodes(int network_id,int starting_node_id)
        {
            List<int> result = new List<int>();
            Queue<int> rrn_queue = new Queue<int>();
            result.Add(starting_node_id);
            rrn_queue.Enqueue(starting_node_id);
            int act_rrn;
            while(rrn_queue.Count > 0)
            {
                act_rrn = rrn_queue.Dequeue();
                for(int p = 0; p < 4; p++)
                {
                    if (road_network[network_id].road_network[act_rrn].connections[p] != -1)
                    {
                        if(!result.Contains(road_network[network_id].road_network[act_rrn].connections[p]))
                        {
                            result.Add(road_network[network_id].road_network[act_rrn].connections[p]);
                            rrn_queue.Enqueue(road_network[network_id].road_network[act_rrn].connections[p]);
                        }
                    }
                }
            }
            return result;
        }
        private bool _CanDeleteRoad(int network_id,int node_id)
        {
            foreach ((int, int) leaf in road_network[network_id].road_network[node_id].connected_tiles)
            {
                if (leaf == (-1, -1))
                {
                    continue;
                }
                if (map[leaf.Item1, leaf.Item2].building != null)
                {
                    if (map[leaf.Item1, leaf.Item2].building!.building_type != BuildingType.CITY_HALL)
                    {
                        (int, int) location = _JumpToMain(leaf);
                        int connections = 0;
                        for(int x = location.Item1 - 1; x < location.Item1 + map[leaf.Item1, leaf.Item2].building!.building_size.Item1 + 1; x++)
                        {
                            for (int y = location.Item2; y < location.Item2 + map[leaf.Item1, leaf.Item2].building!.building_size.Item2; y++)
                            {
                                if (map[x,y].road != null)
                                {
                                    if (road_network[map[x,y].road!.network_id].connected_to_city_hall)
                                    {
                                        connections++;
                                    }
                                }
                            }
                        }
                        for (int x = location.Item1; x < location.Item1 + map[leaf.Item1, leaf.Item2].building!.building_size.Item1; x++)
                        {
                            for (int y = location.Item2 - 1; y < location.Item2 + map[leaf.Item1, leaf.Item2].building!.building_size.Item2 + 1; y++)
                            {
                                if (map[x, y].road != null)
                                {
                                    if (road_network[map[x, y].road!.network_id].connected_to_city_hall)
                                    {
                                        connections++;
                                    }
                                }
                            }
                        }
                        if(connections <= 1)
                        {
                            return false;
                        }
                    }
                }
                if (map[leaf.Item1, leaf.Item2].zone_type != ZoneType.NONE)
                {
                    if (map[leaf.Item1, leaf.Item2].zone_info!.people.Count != 0)
                    {
                        int connections = 0;
                        if(_InsideMap((leaf.Item1 + 1, leaf.Item2)))
                        {
                            if (map[leaf.Item1 + 1, leaf.Item2].road != null)
                            {
                                if (road_network[map[leaf.Item1 + 1, leaf.Item2].road!.network_id].connected_to_city_hall)
                                {
                                    connections++;
                                }
                            }
                        }
                        if (_InsideMap((leaf.Item1, leaf.Item2 - 1)))
                        {
                            if (map[leaf.Item1, leaf.Item2 - 1].road != null)
                            {
                                if (road_network[map[leaf.Item1, leaf.Item2 - 1].road!.network_id].connected_to_city_hall)
                                {
                                    connections++;
                                }
                            }
                        }
                        if (_InsideMap((leaf.Item1 - 1, leaf.Item2)))
                        {
                            if (map[leaf.Item1 - 1, leaf.Item2].road != null)
                            {
                                if (road_network[map[leaf.Item1 - 1, leaf.Item2].road!.network_id].connected_to_city_hall)
                                {
                                    connections++;
                                }
                            }
                        }
                        if (_InsideMap((leaf.Item1, leaf.Item2 + 1)))
                        {
                            if (map[leaf.Item1, leaf.Item2 + 1].road != null)
                            {
                                if (road_network[map[leaf.Item1, leaf.Item2 + 1].road!.network_id].connected_to_city_hall)
                                {
                                    connections++;
                                }
                            }
                        }
                        if (connections <= 1)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        private void _RestoreDeletedRoad(int road_network_id, int deletion_id)
        {
            (int, int) position = road_network[road_network_id].road_network[deletion_id].location;
            for (int p = 0; p < 4; p++)
            {
                if (map[position.Item1, position.Item2].road!.connections[p] != -1)
                {
                    road_network[road_network_id].road_network[map[position.Item1, position.Item2].road!.connections[p]].connections[(p + 2) % 4] = deletion_id;
                }
            }
        }
        private bool _HasDirectConnectionToCityHall(int road_network_id, List<int> road_network_node_list)
        {
            foreach(int node in road_network_node_list)
            {
                foreach((int,int) leaf in road_network[road_network_id].road_network[node].connected_tiles)
                {
                    if (leaf == (-1, -1))
                    {
                        continue;
                    }
                    if (map[leaf.Item1, leaf.Item2].building != null)
                    {
                        if (map[leaf.Item1, leaf.Item2].building!.building_type == BuildingType.CITY_HALL)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        private bool _HasDirectConnectionToCityHall(int road_network_id)
        {
            foreach (KeyValuePair<int,RoadNetworkNode> rnn in road_network[road_network_id].road_network)
            {
                foreach ((int, int) leaf in rnn.Value.connected_tiles)
                {
                    if (leaf == (-1, -1))
                    {
                        continue;
                    }
                    if (map[leaf.Item1, leaf.Item2].building != null)
                    {
                        if (map[leaf.Item1, leaf.Item2].building!.building_type == BuildingType.CITY_HALL)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        private bool _HasConnectionReliantLeaf(int road_network_id, List<int> road_network_node_list)
        {
            foreach(int node in road_network_node_list)
            {
                foreach ((int, int) leaf in road_network[road_network_id].road_network[node].connected_tiles)
                {
                    if (leaf == (-1, -1))
                    {
                        continue;
                    }
                    if (map[leaf.Item1, leaf.Item2].building != null)
                    {
                        if (map[leaf.Item1, leaf.Item2].building!.building_type != BuildingType.CITY_HALL)
                        {
                            (int, int) location = _JumpToMain(leaf);
                            HashSet<int> connections = new HashSet<int>();
                            for(int x = location.Item1 - 1; x < location.Item1 + map[leaf.Item1, leaf.Item2].building!.building_size.Item1 + 1; x++)
                            {
                                for (int y = location.Item2; y < location.Item2 + map[leaf.Item1, leaf.Item2].building!.building_size.Item2; y++)
                                {
                                    if (map[x,y].road != null)
                                    {
                                        if (road_network[map[x,y].road!.network_id].connected_to_city_hall)
                                        {
                                            connections.Add(map[x, y].road!.network_id);
                                        }
                                    }
                                }
                            }
                            for (int x = location.Item1; x < location.Item1 + map[leaf.Item1, leaf.Item2].building!.building_size.Item1; x++)
                            {
                                for (int y = location.Item2 - 1; y < location.Item2 + map[leaf.Item1, leaf.Item2].building!.building_size.Item2 + 1; y++)
                                {
                                    if (map[x, y].road != null)
                                    {
                                        if (road_network[map[x, y].road!.network_id].connected_to_city_hall)
                                        {
                                            connections.Add(map[x, y].road!.network_id);
                                        }
                                    }
                                }
                            }
                            if(connections.Count <= 1)
                            {
                                return true;
                            }
                        }
                    }
                    if (map[leaf.Item1, leaf.Item2].zone_type != ZoneType.NONE)
                    {
                        if (map[leaf.Item1, leaf.Item2].zone_info!.people.Count != 0)
                        {
                            HashSet<int> connections = new HashSet<int>();
                            if (_InsideMap((leaf.Item1 + 1, leaf.Item2)))
                            {
                                if (map[leaf.Item1 + 1, leaf.Item2].road != null)
                                {
                                    if (road_network[map[leaf.Item1 + 1, leaf.Item2].road!.network_id].connected_to_city_hall)
                                    {
                                        connections.Add(map[leaf.Item1 + 1, leaf.Item2].road!.network_id);
                                    }
                                }
                            }
                            if (_InsideMap((leaf.Item1, leaf.Item2 - 1)))
                            {
                                if (map[leaf.Item1, leaf.Item2 - 1].road != null)
                                {
                                    if (road_network[map[leaf.Item1, leaf.Item2 - 1].road!.network_id].connected_to_city_hall)
                                    {
                                        connections.Add(map[leaf.Item1, leaf.Item2 - 1].road!.network_id);
                                    }
                                }
                            }
                            if (_InsideMap((leaf.Item1 - 1, leaf.Item2)))
                            {
                                if (map[leaf.Item1 - 1, leaf.Item2].road != null)
                                {
                                    if (road_network[map[leaf.Item1 - 1, leaf.Item2].road!.network_id].connected_to_city_hall)
                                    {
                                        connections.Add(map[leaf.Item1 - 1, leaf.Item2].road!.network_id);
                                    }
                                }
                            }
                            if (_InsideMap((leaf.Item1, leaf.Item2 + 1)))
                            {
                                if (map[leaf.Item1, leaf.Item2 + 1].road != null)
                                {
                                    if (road_network[map[leaf.Item1, leaf.Item2 + 1].road!.network_id].connected_to_city_hall)
                                    {
                                        connections.Add(map[leaf.Item1, leaf.Item2 + 1].road!.network_id);
                                    }
                                }
                            }
                            if (connections.Count <= 1)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
        private int _DeleteRoad((int,int) position)
        {
            int road_network_id = map[position.Item1, position.Item2].road!.network_id;
            int deletion_id = map[position.Item1, position.Item2].road!.id;
            if (road_network[road_network_id].connected_to_city_hall)
            {
                for (int p = 0; p < 4; p++)
                {
                    if (map[position.Item1, position.Item2].road!.connections[p] != -1)
                    {
                        road_network[road_network_id].road_network[map[position.Item1, position.Item2].road!.connections[p]].connections[(p + 2) % 4] = -1;
                    }
                }
                List<List<int>> sub_networks = new List<List<int>>();
                for (int p = 0; p < 4; p++)
                {
                    if (map[position.Item1, position.Item2].road!.connections[p] != -1)
                    {
                        sub_networks.Add(_GetReachableRoadNetworkNodes(road_network_id, map[position.Item1, position.Item2].road!.connections[p]));
                    }
                }
                for (int p = 0; p < sub_networks.Count; p++)
                {
                    for (int s = p + 1; s < sub_networks.Count; s++)
                    {
                        if (sub_networks[s].Contains(sub_networks[p].First()))
                        {
                            sub_networks.RemoveAt(s);
                            s--;
                        }
                    }
                }
                if (sub_networks.Count == 0)
                {
                    bool can_delete = _CanDeleteRoad(road_network_id, deletion_id);
                    if (can_delete)
                    {
                        foreach ((int, int) leaf in road_network[road_network_id].road_network[deletion_id].connected_tiles)
                        {
                            if (leaf == (-1, -1))
                            {
                                continue;
                            }
                            if (map[leaf.Item1, leaf.Item2].zone_type != ZoneType.NONE)
                            {
                                _DisconnectZone(leaf);
                            }
                        }
                        map[position.Item1, position.Item2].road = null;
                        road_network[road_network_id].road_network.Remove(deletion_id);
                        int del_cnt = 0;
                        for (int p = 0; p < road_network.Count; p++)
                        {
                            if (p == road_network_id)
                            {
                                del_cnt++;
                            }
                            else
                            {
                                road_network[p].RefreshNetworkID(road_network[p].network_id - del_cnt);
                            }
                        }
                        road_network.RemoveAt(road_network_id);
                        return 0;
                    }
                    else
                    {
                        _RestoreDeletedRoad(road_network_id, deletion_id);
                        return -1;
                    }
                }
                if (sub_networks.Count == 1)
                {
                    bool can_delete = _CanDeleteRoad(road_network_id, deletion_id);
                    if (can_delete)
                    {
                        if (_HasConnectionReliantLeaf(road_network_id, sub_networks[0]))
                        {
                            if (!_HasDirectConnectionToCityHall(road_network_id, sub_networks[0]))
                            {
                                _RestoreDeletedRoad(road_network_id, deletion_id);
                                return -1;
                            }
                        }
                        else
                        {
                            if (!_HasDirectConnectionToCityHall(road_network_id, sub_networks[0]))
                            {
                                road_network[road_network_id].connected_to_city_hall = false;
                            }
                        }
                        foreach ((int, int) leaf in road_network[road_network_id].road_network[deletion_id].connected_tiles)
                        {
                            if (leaf == (-1, -1))
                            {
                                continue;
                            }
                            if (map[leaf.Item1, leaf.Item2].zone_type != ZoneType.NONE)
                            {
                                _DisconnectZone(leaf);
                            }
                        }
                        map[position.Item1, position.Item2].road = null;
                        road_network[road_network_id].road_network.Remove(deletion_id);
                        road_network[road_network_id].ReshuffleNetwork();
                        _RecalculateRoadNetworkConnections(road_network_id);
                        return 0;
                    }
                    else
                    {
                        _RestoreDeletedRoad(road_network_id, deletion_id);
                        return -1;
                    }
                }
                if(sub_networks.Count > 1)
                {
                    bool can_delete = true;
                    for (int p = 0; p < sub_networks.Count; p++)
                    {
                        if (_HasConnectionReliantLeaf(road_network_id, sub_networks[p]))
                        {
                            if (!_HasDirectConnectionToCityHall(road_network_id, sub_networks[p]))
                            {
                                can_delete = false;
                                break;
                            }
                        }
                    }
                    if(!_CanDeleteRoad(road_network_id, deletion_id))
                    {
                        can_delete = false;
                    }
                    if (can_delete)
                    {
                        List<List<RoadNetworkNode>> sub_network_nodes = new List<List<RoadNetworkNode>>();
                        for (int p = 0; p < sub_networks.Count; p++)
                        {
                            sub_network_nodes.Add(new List<RoadNetworkNode>());
                            foreach (int node in sub_networks[p])
                            {
                                sub_network_nodes[p].Add(road_network[road_network_id].road_network[node]);
                            }
                        }
                        foreach ((int, int) leaf in road_network[road_network_id].road_network[deletion_id].connected_tiles)
                        {
                            if (leaf == (-1, -1))
                            {
                                continue;
                            }
                            if (map[leaf.Item1, leaf.Item2].zone_type != ZoneType.NONE)
                            {
                                _DisconnectZone(leaf);
                            }
                        }
                        map[position.Item1, position.Item2].road = null;
                        road_network[road_network_id].road_network.Remove(deletion_id);
                        for (int p = 1; p < sub_networks.Count; p++)
                        {
                            road_network.Add(new RoadNetwork(road_network.Count, sub_network_nodes[p]));
                            road_network.Last().ReshuffleNetwork();
                            road_network.Last().connected_to_city_hall = _HasDirectConnectionToCityHall(road_network_id, sub_networks[p]);
                            if(!road_network.Last().connected_to_city_hall)
                            {
                                foreach(RoadNetworkNode rnn in sub_network_nodes[p])
                                {
                                    foreach ((int, int) leaf in rnn.connected_tiles)
                                    {
                                        if (leaf == (-1, -1))
                                        {
                                            continue;
                                        }
                                        if (map[leaf.Item1, leaf.Item2].zone_type != ZoneType.NONE)
                                        {
                                            _DisconnectZone(leaf);
                                        }
                                    }
                                }
                            }
                            _RecalculateRoadNetworkConnections(road_network.Count - 1);
                        }
                        road_network[road_network_id] = new RoadNetwork(road_network_id, sub_network_nodes[0]);
                        road_network[road_network_id].ReshuffleNetwork();
                        road_network[road_network_id].connected_to_city_hall = _HasDirectConnectionToCityHall(road_network_id);
                        if (!road_network[road_network_id].connected_to_city_hall)
                        {
                            foreach (RoadNetworkNode rnn in sub_network_nodes[0])
                            {
                                foreach ((int, int) leaf in rnn.connected_tiles)
                                {
                                    if (leaf == (-1, -1))
                                    {
                                        continue;
                                    }
                                    if (map[leaf.Item1, leaf.Item2].zone_type != ZoneType.NONE)
                                    {
                                        _DisconnectZone(leaf);
                                    }
                                }
                            }
                        }
                        _RecalculateRoadNetworkConnections(road_network_id);
                        return 0;
                    }
                    else
                    {
                        _RestoreDeletedRoad(road_network_id, deletion_id);
                        return -1;
                    }
                }
                return -1;
            }
            else
            {
                for(int p = 0; p < 4; p++)
                {
                    if(map[position.Item1, position.Item2].road!.connections[p] != -1)
                    {
                        road_network[road_network_id].road_network[map[position.Item1, position.Item2].road!.connections[p]].connections[(p + 2) % 4] = -1;
                    }
                }
                List<List<int>> sub_networks = new List<List<int>>(); 
                for (int p = 0; p < 4; p++)
                {
                    if (map[position.Item1, position.Item2].road!.connections[p] != -1)
                    {
                        sub_networks.Add(_GetReachableRoadNetworkNodes(road_network_id, map[position.Item1, position.Item2].road!.connections[p]));
                    }
                }
                for (int p = 0; p < sub_networks.Count; p++)
                {
                    for(int s = p + 1; s < sub_networks.Count; s++)
                    {
                        if (sub_networks[s].Contains(sub_networks[p].First()))
                        {
                            sub_networks.RemoveAt(s);
                            s--;
                        }
                    }
                }
                if(sub_networks.Count == 0)
                {
                    map[position.Item1, position.Item2].road = null;
                    road_network[road_network_id].road_network.Remove(deletion_id);
                    int del_cnt = 0;
                    for (int p = 0; p < road_network.Count; p++)
                    {
                        if (p == road_network_id)
                        {
                            del_cnt++;
                        }
                        else
                        {
                            road_network[p].RefreshNetworkID(road_network[p].network_id - del_cnt);
                        }
                    }
                    road_network.RemoveAt(road_network_id);
                    return 0;
                }
                if (sub_networks.Count == 1)
                {
                    map[position.Item1, position.Item2].road = null;
                    road_network[road_network_id].road_network.Remove(deletion_id);
                    road_network[road_network_id].ReshuffleNetwork();
                    _RecalculateRoadNetworkConnections(road_network_id);
                    return 0;
                }
                if (sub_networks.Count > 1)
                {
                    List<List<RoadNetworkNode>> sub_network_nodes = new List<List<RoadNetworkNode>>();
                    for(int p = 0; p < sub_networks.Count; p++)
                    {
                        sub_network_nodes.Add(new List<RoadNetworkNode>());
                        foreach(int node in sub_networks[p])
                        {
                            sub_network_nodes[p].Add(road_network[road_network_id].road_network[node]);
                        }
                    }
                    map[position.Item1, position.Item2].road = null;
                    road_network[road_network_id].road_network.Remove(deletion_id);
                    road_network[road_network_id] = new RoadNetwork(road_network_id, sub_network_nodes[0]);
                    road_network[road_network_id].ReshuffleNetwork();
                    _RecalculateRoadNetworkConnections(road_network_id);
                    for (int p = 1; p < sub_networks.Count; p++)
                    {
                        road_network.Add(new RoadNetwork(road_network.Count, sub_network_nodes[p]));
                        road_network.Last().ReshuffleNetwork();
                        _RecalculateRoadNetworkConnections(road_network.Count - 1);
                    }
                    return 0;
                }
                return -1;
            }
        }
        #endregion
        #region Pipe
        private bool _AbleToPlacePipe((int, int) position)
        {
            if (!_InsideMap(position))
            {
                return false;
            }
            return true;
        }
        public void _RecalculatePipeNetworkConnections(int network_id)
        {
            (int, int) location;
            pipe_network[network_id].total_oxygen_supply = 0;
            pipe_network[network_id].connected_diffusers.Clear();
            pipe_network[network_id].connected_generators.Clear();
            pipe_network[network_id].supplied_zones.Clear();
            foreach (KeyValuePair<int, PipeNetworkNode> pnn in pipe_network[network_id].pipe_network)
            {
                location = pnn.Value.location;
                if (map[location.Item1, location.Item2].building != null)
                {
                    (int, int) loc = _JumpToMain(location);
                    if (map[loc.Item1, loc.Item2].building!.connected)
                    {
                        switch (map[loc.Item1, loc.Item2].building!.building_type)
                        {
                            case BuildingType.OXYGEN_GENERATOR:
                                {
                                    pipe_network[network_id].connected_generators.Add(loc);
                                }
                                break;
                            case BuildingType.OXYGEN_DIFFUSER:
                                {
                                    pipe_network[network_id].connected_diffusers.Add(loc);
                                }
                                break;
                        }
                    }
                }
                location.Item1--;
                if (_InsideMap(location))
                {
                    if (map[location.Item1, location.Item2].pipe != null)
                    {
                        pnn.Value.connections[2] = map[location.Item1, location.Item2].pipe!.id;
                    }
                    else
                    {
                        pnn.Value.connections[2] = -1;
                    }
                }
                location = pnn.Value.location;
                location.Item1++;
                if (_InsideMap(location))
                {
                    if (map[location.Item1, location.Item2].pipe != null)
                    {
                        pnn.Value.connections[0] = map[location.Item1, location.Item2].pipe!.id;
                    }
                    else
                    {
                        pnn.Value.connections[0] = -1;
                    }
                }
                location = pnn.Value.location;
                location.Item2--;
                if (_InsideMap(location))
                {
                    if (map[location.Item1, location.Item2].pipe != null)
                    {
                        pnn.Value.connections[1] = map[location.Item1, location.Item2].pipe!.id;
                    }
                    else
                    {
                        pnn.Value.connections[1] = -1;
                    }
                }
                location = pnn.Value.location;
                location.Item2++;
                if (_InsideMap(location))
                {
                    if (map[location.Item1, location.Item2].pipe != null)
                    {
                        pnn.Value.connections[3] = map[location.Item1, location.Item2].pipe!.id;
                    }
                    else
                    {
                        pnn.Value.connections[3] = -1;
                    }
                }
            }
            bool recursive = false;
            foreach ((int, int) position in pipe_network[network_id].connected_generators)
            {
                recursive = _SearchForPipeNetwork(position);
                if (recursive)
                {
                    break;
                }
            }
            if (recursive)
            {
                return;
            }
            foreach ((int, int) position in pipe_network[network_id].connected_diffusers)
            {
                recursive = _SearchForPipeNetwork(position);
                if (recursive)
                {
                    break;
                }
            }
        }
        private List<(int, int)> _FindRouteForPipe((int, int) starting_from, (int, int) finishing_at)
        {
            List<(int, int)> route = new List<(int, int)>();
            (int, int)[,] parent = new (int, int)[map_size.Item1, map_size.Item2];
            for (int x = 0; x < map_size.Item1; x++)
            {
                for (int y = 0; y < map_size.Item2; y++)
                {
                    parent[x, y] = (-1, -1);
                }
            }
            parent[starting_from.Item1, starting_from.Item2] = starting_from;
            Queue<(int, int)> route_queue = new Queue<(int, int)>();
            route_queue.Enqueue(starting_from);
            while (route_queue.Count != 0)
            {
                (int, int) act = route_queue.Dequeue();
                if (act == finishing_at)
                {
                    break;
                }
                if (_AbleToPlacePipe((act.Item1 - 1, act.Item2)))
                {
                    if (parent[act.Item1 - 1, act.Item2] == (-1, -1))
                    {
                        route_queue.Enqueue((act.Item1 - 1, act.Item2));
                        parent[act.Item1 - 1, act.Item2] = act;
                    }
                }
                if (_AbleToPlacePipe((act.Item1 + 1, act.Item2)))
                {
                    if (parent[act.Item1 + 1, act.Item2] == (-1, -1))
                    {
                        route_queue.Enqueue((act.Item1 + 1, act.Item2));
                        parent[act.Item1 + 1, act.Item2] = act;
                    }
                }
                if (_AbleToPlacePipe((act.Item1, act.Item2 - 1)))
                {
                    if (parent[act.Item1, act.Item2 - 1] == (-1, -1))
                    {
                        route_queue.Enqueue((act.Item1, act.Item2 - 1));
                        parent[act.Item1, act.Item2 - 1] = act;
                    }
                }
                if (_AbleToPlacePipe((act.Item1, act.Item2 + 1)))
                {
                    if (parent[act.Item1, act.Item2 + 1] == (-1, -1))
                    {
                        route_queue.Enqueue((act.Item1, act.Item2 + 1));
                        parent[act.Item1, act.Item2 + 1] = act;
                    }
                }
            }
            (int, int) position = finishing_at;
            if (parent[position.Item1, position.Item2] != (-1, -1))
            {
                route.Add(position);
                while (position != starting_from)
                {
                    position = parent[position.Item1, position.Item2];
                    route.Add(position);
                }
            }
            return route;
        }
        /// <summary>
        /// Attempts to place a pipe connecting <paramref name="starting_from"/> with <paramref name="finishing_at"/>.<br/>
        /// The placed pipes either create a new pipe-network or merge into one.<br/>
        /// </summary>
        /// <param name="starting_from">The starting location.</param>
        /// <param name="finishing_at">The finishing location.</param>
        public int PlacePipe((int, int) starting_from, (int, int) finishing_at)
        {
            if (!city_hall_placed)
            {
                StatusUpdate?.Invoke(this, new GameEventArgs { status = EventStatus.ERROR, update = EventUpdate.NO_UPDATE, message = "You need to place the city hall first!" });
                return -1;
            }
            if (!_AbleToPlacePipe((starting_from.Item1, starting_from.Item2)) ||
                !_AbleToPlacePipe((finishing_at.Item1, finishing_at.Item2)))
            {
                StatusUpdate?.Invoke(this, new GameEventArgs { status = EventStatus.ERROR, update = EventUpdate.NO_UPDATE, message = "Pipe ending(s) blocked!" });
                return -2;
            }
            List<(int, int)> route = _FindRouteForPipe(starting_from, finishing_at);
            if (route.Count == 0)
            {
                StatusUpdate?.Invoke(this, new GameEventArgs { status = EventStatus.ERROR, update = EventUpdate.NO_UPDATE, message = "Can not find suitable route!" });
                return -3;
            }
            SortedSet<int> intersected_networks = new SortedSet<int>();
            (int, int) location;
            foreach ((int, int) position in route)
            {
                if (map[position.Item1, position.Item2].pipe != null)
                {
                    PipeNetworkNode pnn = map[position.Item1, position.Item2].pipe!;
                    intersected_networks.Add(pnn.network_id);
                }
                location = position;
                location.Item1--;
                if (_InsideMap(location))
                {
                    if (map[location.Item1, location.Item2].pipe != null)
                    {
                        PipeNetworkNode pnn = map[location.Item1, location.Item2].pipe!;
                        intersected_networks.Add(pnn.network_id);
                    }
                }
                location = position;
                location.Item1++;
                if (_InsideMap(location))
                {
                    if (map[location.Item1, location.Item2].pipe != null)
                    {
                        PipeNetworkNode pnn = map[location.Item1, location.Item2].pipe!;
                        intersected_networks.Add(pnn.network_id);
                    }
                }
                location = position;
                location.Item2--;
                if (_InsideMap(location))
                {
                    if (map[location.Item1, location.Item2].pipe != null)
                    {
                        PipeNetworkNode pnn = map[location.Item1, location.Item2].pipe!;
                        intersected_networks.Add(pnn.network_id);
                    }
                }
                location = position;
                location.Item2++;
                if (_InsideMap(location))
                {
                    if (map[location.Item1, location.Item2].pipe != null)
                    {
                        PipeNetworkNode pnn = map[location.Item1, location.Item2].pipe!;
                        intersected_networks.Add(pnn.network_id);
                    }
                }
            }
            if (intersected_networks.Count == 0)
            {
                int new_network_id = pipe_network.Count;
                pipe_network.Add(new PipeNetwork(new_network_id));
                int cnt = 0;
                int placed_pipe_count = 0;
                foreach ((int, int) pos in route)
                {
                    pipe_network[new_network_id].pipe_network[cnt] = new PipeNetworkNode(cnt, new_network_id, pos);
                    map[pos.Item1, pos.Item2].pipe = pipe_network[new_network_id].pipe_network[cnt];
                    cnt++;
                    placed_pipe_count++;
                    money -= pipe_price;
                    upkeep += pipe_upkeep;
                    income -= pipe_upkeep;
                }
                if (placed_pipe_count > 0)
                {
                    spending_log.Add(new SpendingInfo(placed_pipe_count * pipe_price, time, placed_pipe_count.ToString() + " pipe(s) placed"));
                }
                _RecalculatePipeNetworkConnections(new_network_id);
            }
            else
            {
                int network_id = intersected_networks.Min();
                foreach (int network_to_merge in intersected_networks.Reverse())
                {
                    if (network_to_merge != network_id)
                    {
                        pipe_network[network_id].Merge(pipe_network[network_to_merge]);
                        foreach ((int, int) pos in pipe_network[network_id].supplied_zones)
                        {
                            map[pos.Item1, pos.Item2].zone_info!.connected_oxygen_diffusers.Remove(network_to_merge);
                        }
                    }
                }
                int del_cnt = 0;
                for (int p = 0; p < pipe_network.Count; p++)
                {
                    if (intersected_networks.Contains(p))
                    {
                        if (p != network_id)
                        {
                            del_cnt++;
                        }
                    }
                    else
                    {
                        pipe_network[p].RefreshNetworkID(pipe_network[p].network_id - del_cnt);
                    }
                }
                foreach (int network_to_merge in intersected_networks.Reverse())
                {
                    if (network_to_merge != network_id)
                    {
                        pipe_network.Remove(pipe_network[network_to_merge]);
                    }
                }
                int cnt = pipe_network[network_id].pipe_network.Count;
                int placed_pipe_count = 0;
                foreach ((int, int) loc in route)
                {
                    if (map[loc.Item1, loc.Item2].pipe == null)
                    {
                        pipe_network[network_id].pipe_network[cnt] = new PipeNetworkNode(cnt, network_id, loc);
                        map[loc.Item1, loc.Item2].pipe = pipe_network[network_id].pipe_network[cnt];
                        cnt++;
                        placed_pipe_count++;
                        money -= pipe_price;
                        upkeep += pipe_upkeep;
                        income -= pipe_upkeep;
                    }
                }
                if (placed_pipe_count > 0)
                {
                    spending_log.Add(new SpendingInfo(placed_pipe_count * pipe_price, time, placed_pipe_count.ToString() + " pipe(s) placed"));
                }
                _RecalculatePipeNetworkConnections(network_id);
            }
            StatusUpdate?.Invoke(this, new GameEventArgs { status = EventStatus.SUCCESS, update = EventUpdate.UPDATE, message = "Pipe placed successfully!" });
            return 0;
        }
        private List<int> _GetReachablePipeNetworkNodes(int network_id, int starting_node_id)
        {
            List<int> result = new List<int>();
            Queue<int> rpn_queue = new Queue<int>();
            result.Add(starting_node_id);
            rpn_queue.Enqueue(starting_node_id);
            int act_rpn;
            while (rpn_queue.Count > 0)
            {
                act_rpn = rpn_queue.Dequeue();
                for (int p = 0; p < 4; p++)
                {
                    if (pipe_network[network_id].pipe_network[act_rpn].connections[p] != -1)
                    {
                        if (!result.Contains(pipe_network[network_id].pipe_network[act_rpn].connections[p]))
                        {
                            result.Add(pipe_network[network_id].pipe_network[act_rpn].connections[p]);
                            rpn_queue.Enqueue(pipe_network[network_id].pipe_network[act_rpn].connections[p]);
                        }
                    }
                }
            }
            return result;
        }
        private bool _CanDeletePipe(int network_id, int node_id)
        {
            (int, int) position = pipe_network[network_id].pipe_network[node_id].location;
            foreach((int,int) location in pipe_network[network_id].connected_diffusers)
            {
                if(position == location)
                {
                    return false;
                }
            }
            foreach ((int, int) location in pipe_network[network_id].connected_generators)
            {
                if (_JumpToMain(position) == _JumpToMain(location))
                {
                    return false;
                }
            }
            return true;
        }
        private void _RestoreDeletedPipe(int pipe_network_id, int deletion_id)
        {
            (int, int) position = pipe_network[pipe_network_id].pipe_network[deletion_id].location;
            for (int p = 0; p < 4; p++)
            {
                if (map[position.Item1, position.Item2].pipe!.connections[p] != -1)
                {
                    pipe_network[pipe_network_id].pipe_network[map[position.Item1, position.Item2].pipe!.connections[p]].connections[(p + 2) % 4] = deletion_id;
                }
            }
        }
        private bool _HasConnectedBuilding(int pipe_network_id, List<int> pipe_network_node_list)
        {
            (int, int) location;
            foreach (int node in pipe_network_node_list)
            {
                location = pipe_network[pipe_network_id].pipe_network[node].location;
                if (map[location.Item1,location.Item2].building != null && (map[location.Item1, location.Item2].building!.building_type == BuildingType.OXYGEN_GENERATOR || map[location.Item1, location.Item2].building!.building_type == BuildingType.OXYGEN_DIFFUSER))
                {
                    return true;
                }
            }
            return false;
        }
        private void _RefreshOxygenSuppliers()
        {
            foreach((int,int) position in connected_zones.ToList())
            {
                map[position.Item1, position.Item2].zone_info!.connected_oxygen_diffusers.Clear();
                OxygenDiffuser generic_diffuser = new OxygenDiffuser(SubBuildingPositionType.MAIN);
                for (int x = position.Item1 - generic_diffuser.radius; x <= position.Item1 + generic_diffuser.radius; x++)
                {
                    for (int y = position.Item2 - generic_diffuser.radius; y <= position.Item2 + generic_diffuser.radius; y++)
                    {
                        if (_InsideMap((x, y)) && (x != position.Item1 || y != position.Item2))
                        {
                            if (map[x, y].building != null)
                            {
                                if (map[x, y].building!.connected)
                                {
                                    switch (map[x, y].building!.building_type)
                                    {
                                        case BuildingType.OXYGEN_DIFFUSER:
                                            {
                                                OxygenDiffuser od = (OxygenDiffuser)(map[x, y].building!);
                                                if (od.network_id != -1)
                                                {
                                                    pipe_network[od.network_id].supplied_zones.Add(position);
                                                    map[position.Item1, position.Item2].zone_info!.connected_oxygen_diffusers.Add(od.network_id);
                                                }
                                            }
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        private void _RefreshPipeNetworkID(int network_id,int new_network_id)
        {
            OxygenGenerator og;
            foreach ((int, int) generator in pipe_network[network_id].connected_generators)
            {
                og = (OxygenGenerator)(map[generator.Item1, generator.Item2].building!);
                og.network_id = new_network_id;
            }
            OxygenDiffuser od;
            foreach ((int, int) diffuser in pipe_network[network_id].connected_diffusers)
            {
                od = (OxygenDiffuser)(map[diffuser.Item1, diffuser.Item2].building!);
                od.network_id = new_network_id;
            }
        }
        private int _DeletePipe((int, int) position)
        {
            int pipe_network_id = map[position.Item1, position.Item2].pipe!.network_id;
            int deletion_id = map[position.Item1, position.Item2].pipe!.id;
            if (pipe_network[pipe_network_id].supplied_zones.Count > 0)
            {
                for (int p = 0; p < 4; p++)
                {
                    if (map[position.Item1, position.Item2].pipe!.connections[p] != -1)
                    {
                        pipe_network[pipe_network_id].pipe_network[map[position.Item1, position.Item2].pipe!.connections[p]].connections[(p + 2) % 4] = -1;
                    }
                }
                List<List<int>> sub_networks = new List<List<int>>();
                for (int p = 0; p < 4; p++)
                {
                    if (map[position.Item1, position.Item2].pipe!.connections[p] != -1)
                    {
                        sub_networks.Add(_GetReachablePipeNetworkNodes(pipe_network_id, map[position.Item1, position.Item2].pipe!.connections[p]));
                    }
                }
                for (int p = 0; p < sub_networks.Count; p++)
                {
                    for (int s = p + 1; s < sub_networks.Count; s++)
                    {
                        if (sub_networks[s].Contains(sub_networks[p].First()))
                        {
                            sub_networks.RemoveAt(s);
                            s--;
                        }
                    }
                }
                if (sub_networks.Count == 0)
                {
                    bool can_delete = _CanDeletePipe(pipe_network_id, deletion_id);
                    if (can_delete)
                    {
                        map[position.Item1, position.Item2].pipe = null;
                        pipe_network[pipe_network_id].pipe_network.Remove(deletion_id);
                        int del_cnt = 0;
                        for (int p = 0; p < pipe_network.Count; p++)
                        {
                            if (p == pipe_network_id)
                            {
                                del_cnt++;
                            }
                            else
                            {
                                _RefreshPipeNetworkID(p, pipe_network[p].network_id - del_cnt);
                                pipe_network[p].RefreshNetworkID(pipe_network[p].network_id - del_cnt);
                            }
                        }
                        pipe_network.RemoveAt(pipe_network_id);
                        _RefreshOxygenSuppliers();
                        return 0;
                    }
                    else
                    {
                        _RestoreDeletedPipe(pipe_network_id, deletion_id);
                        return -1;
                    }
                }
                if (sub_networks.Count == 1)
                {
                    bool can_delete = _CanDeletePipe(pipe_network_id, deletion_id);
                    if (can_delete)
                    {
                        map[position.Item1, position.Item2].pipe = null;
                        pipe_network[pipe_network_id].pipe_network.Remove(deletion_id);
                        pipe_network[pipe_network_id].ReshuffleNetwork();
                        _RecalculatePipeNetworkConnections(pipe_network_id);
                        return 0;
                    }
                    else
                    {
                        _RestoreDeletedPipe(pipe_network_id, deletion_id);
                        return -1;
                    }
                }
                if (sub_networks.Count > 1)
                {
                    bool can_delete = true;
                    int sub_networks_with_connected_buildings = 0;
                    for (int p = 0; p < sub_networks.Count; p++)
                    {
                        if (_HasConnectedBuilding(pipe_network_id, sub_networks[p]))
                        {
                            sub_networks_with_connected_buildings++;
                        }
                    }
                    can_delete = (sub_networks_with_connected_buildings <= 1);
                    if (!_CanDeletePipe(pipe_network_id, deletion_id))
                    {
                        can_delete = false;
                    }
                    if (can_delete)
                    {
                        List<List<PipeNetworkNode>> sub_network_nodes = new List<List<PipeNetworkNode>>();
                        for (int p = 0; p < sub_networks.Count; p++)
                        {
                            sub_network_nodes.Add(new List<PipeNetworkNode>());
                            foreach (int node in sub_networks[p])
                            {
                                sub_network_nodes[p].Add(pipe_network[pipe_network_id].pipe_network[node]);
                            }
                        }
                        map[position.Item1, position.Item2].pipe = null;
                        pipe_network[pipe_network_id].pipe_network.Remove(deletion_id);
                        for (int p = 1; p < sub_networks.Count; p++)
                        {
                            pipe_network.Add(new PipeNetwork(pipe_network.Count, sub_network_nodes[p]));
                            pipe_network.Last().ReshuffleNetwork();
                            _RecalculatePipeNetworkConnections(pipe_network.Count - 1);
                        }
                        pipe_network[pipe_network_id] = new PipeNetwork(pipe_network_id, sub_network_nodes[0]);
                        pipe_network[pipe_network_id].ReshuffleNetwork();
                        _RecalculatePipeNetworkConnections(pipe_network_id);
                        return 0;
                    }
                    else
                    {
                        _RestoreDeletedPipe(pipe_network_id, deletion_id);
                        return -1;
                    }
                }
                return -1;
            }
            else
            {
                for (int p = 0; p < 4; p++)
                {
                    if (map[position.Item1, position.Item2].pipe!.connections[p] != -1)
                    {
                        pipe_network[pipe_network_id].pipe_network[map[position.Item1, position.Item2].pipe!.connections[p]].connections[(p + 2) % 4] = -1;
                    }
                }
                List<List<int>> sub_networks = new List<List<int>>();
                for (int p = 0; p < 4; p++)
                {
                    if (map[position.Item1, position.Item2].pipe!.connections[p] != -1)
                    {
                        sub_networks.Add(_GetReachablePipeNetworkNodes(pipe_network_id, map[position.Item1, position.Item2].pipe!.connections[p]));
                    }
                }
                for (int p = 0; p < sub_networks.Count; p++)
                {
                    for (int s = p + 1; s < sub_networks.Count; s++)
                    {
                        if (sub_networks[s].Contains(sub_networks[p].First()))
                        {
                            sub_networks.RemoveAt(s);
                            s--;
                        }
                    }
                }
                if (sub_networks.Count == 0)
                {
                    bool can_delete = _CanDeletePipe(pipe_network_id, deletion_id);
                    if (can_delete)
                    {
                        map[position.Item1, position.Item2].pipe = null;
                        pipe_network[pipe_network_id].pipe_network.Remove(deletion_id);
                        int del_cnt = 0;
                        for (int p = 0; p < pipe_network.Count; p++)
                        {
                            if (p == pipe_network_id)
                            {
                                del_cnt++;
                            }
                            else
                            {
                                _RefreshPipeNetworkID(p, pipe_network[p].network_id - del_cnt);
                                pipe_network[p].RefreshNetworkID(pipe_network[p].network_id - del_cnt);
                            }
                        }
                        pipe_network.RemoveAt(pipe_network_id);
                        _RefreshOxygenSuppliers();
                        return 0;
                    }
                    else
                    {
                        _RestoreDeletedPipe(pipe_network_id, deletion_id);
                        return -1;
                    }
                }
                if (sub_networks.Count == 1)
                {
                    bool can_delete = _CanDeletePipe(pipe_network_id, deletion_id);
                    if (can_delete)
                    {
                        map[position.Item1, position.Item2].pipe = null;
                        pipe_network[pipe_network_id].pipe_network.Remove(deletion_id);
                        pipe_network[pipe_network_id].ReshuffleNetwork();
                        _RecalculatePipeNetworkConnections(pipe_network_id);
                        return 0;
                    }
                    else
                    {
                        _RestoreDeletedPipe(pipe_network_id, deletion_id);
                        return -1;
                    }
                }
                if (sub_networks.Count > 1)
                {
                    bool can_delete = _CanDeletePipe(pipe_network_id, deletion_id);
                    if(can_delete)
                    {
                        List<List<PipeNetworkNode>> sub_network_nodes = new List<List<PipeNetworkNode>>();
                        for (int p = 0; p < sub_networks.Count; p++)
                        {
                            sub_network_nodes.Add(new List<PipeNetworkNode>());
                            foreach (int node in sub_networks[p])
                            {
                                sub_network_nodes[p].Add(pipe_network[pipe_network_id].pipe_network[node]);
                            }
                        }
                        map[position.Item1, position.Item2].pipe = null;
                        pipe_network[pipe_network_id].pipe_network.Remove(deletion_id);
                        pipe_network[pipe_network_id] = new PipeNetwork(pipe_network_id, sub_network_nodes[0]);
                        pipe_network[pipe_network_id].ReshuffleNetwork();
                        _RecalculatePipeNetworkConnections(pipe_network_id);
                        for (int p = 1; p < sub_networks.Count; p++)
                        {
                            pipe_network.Add(new PipeNetwork(pipe_network.Count, sub_network_nodes[p]));
                            pipe_network.Last().ReshuffleNetwork();
                            _RecalculatePipeNetworkConnections(pipe_network.Count - 1);
                        }
                        return 0;
                    }
                    else
                    {
                        _RestoreDeletedPipe(pipe_network_id, deletion_id);
                        return -1;
                    }
                }
                return -1;
            }
        }
        #endregion
        /// <summary>
        /// Attempts to delete a building, road or pipe.<br/>
        /// The city hall cannot be deleted.<br/>
        /// The deletion of roads cannot separate buildings or zones from the city hall.<br/>
        /// The deletion of pipes/diffusers/generators cannot break active oxygen-supply-chains.<br/>
        /// </summary>
        /// <param name="position">The location of the deletion.</param>
        /// <param name="level">Sets whether to delete above ground (0) or below (-1).</param>
        public int Delete((int, int) position, int level)
        {
            int deletion_mode = -1;
            (int, int) location = position;
            if(level == 0)
            {
                if (map[position.Item1, position.Item2].building != null)
                {
                    if (map[position.Item1, position.Item2].building!.building_type != BuildingType.CITY_HALL)
                    {
                        deletion_mode = 0;
                    }
                    else
                    {
                        StatusUpdate?.Invoke(this, new GameEventArgs { status = EventStatus.ERROR, update = EventUpdate.NO_UPDATE, message = "You can not destroy the city hall!" });
                        return -1;
                    }
                }
                if (map[position.Item1, position.Item2].road != null)
                {
                    deletion_mode = 1;
                }
            }
            if(level == -1)
            {
                if (map[position.Item1, position.Item2].pipe != null)
                {
                    deletion_mode = 2;
                }
            }
            switch (deletion_mode)
            {
                case 0:
                    {
                        location = _JumpToMain(position);
                        switch (map[position.Item1, position.Item2].building!.building_type)
                        {
                            case BuildingType.PARK:
                                {
                                    buildings_with_modifiers.Remove(location);
                                    foreach(int network_id in map[location.Item1,location.Item2].zone_info!.connected_oxygen_diffusers)
                                    {
                                        if(network_id == -1)
                                        {
                                            continue;
                                        }
                                        pipe_network[network_id].supplied_zones.Remove(location);
                                    }
                                    map[location.Item1, location.Item2].zone_info!.oxygen_requirement = 0;
                                    map[location.Item1, location.Item2].zone_info!.oxygen_supply = 0;
                                    _RecalculateModifiers();
                                }
                                break;
                            case BuildingType.POLICE_STATION:
                                {
                                    buildings_with_modifiers.Remove(location);
                                    foreach (int network_id in map[location.Item1, location.Item2].zone_info!.connected_oxygen_diffusers)
                                    {
                                        if (network_id == -1)
                                        {
                                            continue;
                                        }
                                        pipe_network[network_id].supplied_zones.Remove(location);
                                    }
                                    _RecalculateModifiers();
                                }
                                break;
                            case BuildingType.OXYGEN_GENERATOR:
                                {
                                    OxygenGenerator og = (OxygenGenerator)(map[location.Item1, location.Item2].building!);
                                    if(og.network_id != -1)
                                    {
                                        StatusUpdate?.Invoke(this, new GameEventArgs { status = EventStatus.ERROR, update = EventUpdate.NO_UPDATE, message = "Can not destroy connected oxygen generator!" });
                                        return -1;
                                    }
                                }
                                break;
                            case BuildingType.OXYGEN_DIFFUSER:
                                {
                                    OxygenDiffuser od = (OxygenDiffuser)(map[location.Item1, location.Item2].building!);
                                    if (od.network_id != -1)
                                    {
                                        StatusUpdate?.Invoke(this, new GameEventArgs { status = EventStatus.ERROR, update = EventUpdate.NO_UPDATE, message = "Can not destroy connected oxygen diffuser!" });
                                        return -1;
                                    }
                                }
                                break;
                        }
                        money += map[location.Item1, location.Item2].building!.building_price / 2;
                        upkeep -= map[location.Item1, location.Item2].building!.building_upkeep;
                        income += map[location.Item1, location.Item2].building!.building_upkeep;
                        (int, int) building_size = map[location.Item1, location.Item2].building!.building_size;
                        for (int x = location.Item1; x < location.Item1 + building_size.Item1; x++)
                        {
                            for (int y = location.Item2; y < location.Item2 + building_size.Item2; y++)
                            {
                                map[x, y].building = null;
                            }
                        }
                        StatusUpdate?.Invoke(this, new GameEventArgs { status = EventStatus.SUCCESS, update = EventUpdate.UPDATE, message = "Destroy successful!" });
                    }
                    break;
                case 1:
                    {
                        int deletion_status = _DeleteRoad(position);
                        if (deletion_status == 0)
                        {
                            money += road_price / 2;
                            upkeep -= road_upkeep;
                            income += road_upkeep;
                            StatusUpdate?.Invoke(this, new GameEventArgs { status = EventStatus.SUCCESS, update = EventUpdate.UPDATE, message = "Road deleted successfully!" });
                        }
                        if (deletion_status != 0)
                        {
                            StatusUpdate?.Invoke(this, new GameEventArgs { status = EventStatus.ERROR, update = EventUpdate.NO_UPDATE, message = "Deletion would disconnect building(s)/zone(s)!" });
                        }
                    }
                    break;
                case 2:
                    {
                        int deletion_status = _DeletePipe(position);
                        if (deletion_status == 0)
                        {
                            money += pipe_price / 2;
                            upkeep -= pipe_upkeep;
                            income += pipe_upkeep;
                            StatusUpdate?.Invoke(this, new GameEventArgs { status = EventStatus.SUCCESS, update = EventUpdate.UPDATE, message = "Pipe deleted successfully!" });
                        }
                        if (deletion_status != 0)
                        {
                            StatusUpdate?.Invoke(this, new GameEventArgs { status = EventStatus.ERROR, update = EventUpdate.NO_UPDATE, message = "Deletion would break pipe network!" });
                        }
                    }
                    break;
            }
            if(deletion_mode == -1)
            {
                StatusUpdate?.Invoke(this, new GameEventArgs { status = EventStatus.ERROR, update = EventUpdate.NO_UPDATE, message = "Nothing to delete!" });
            }
            return 0;
        }
        private bool _CheckForConnection((int,int) position)
        {
            (int, int) location;
            location = position;
            location.Item1--;
            if (_InsideMap(location))
            {
                if (map[location.Item1, location.Item2].road != null)
                {
                    if(road_network[map[location.Item1, location.Item2].road!.network_id].connected_to_city_hall)
                    {
                        return true;
                    }
                }
                if (map[location.Item1, location.Item2].building != null)
                {
                    if (map[location.Item1,location.Item2].building!.building_type == BuildingType.CITY_HALL)
                    {
                        return true;
                    }
                }
            }
            location = position;
            location.Item1++;
            if (_InsideMap(location))
            {
                if (map[location.Item1, location.Item2].road != null)
                {
                    if (road_network[map[location.Item1, location.Item2].road!.network_id].connected_to_city_hall)
                    {
                        return true;
                    }
                }
                if (map[location.Item1, location.Item2].building != null)
                {
                    if (map[location.Item1, location.Item2].building!.building_type == BuildingType.CITY_HALL)
                    {
                        return true;
                    }
                }
            }
            location = position;
            location.Item2--;
            if (_InsideMap(location))
            {
                if (map[location.Item1, location.Item2].road != null)
                {
                    if (road_network[map[location.Item1, location.Item2].road!.network_id].connected_to_city_hall)
                    {
                        return true;
                    }
                }
                if (map[location.Item1, location.Item2].building != null)
                {
                    if (map[location.Item1, location.Item2].building!.building_type == BuildingType.CITY_HALL)
                    {
                        return true;
                    }
                }
            }
            location = position;
            location.Item2++;
            if (_InsideMap(location))
            {
                if (map[location.Item1, location.Item2].road != null)
                {
                    if (road_network[map[location.Item1, location.Item2].road!.network_id].connected_to_city_hall)
                    {
                        return true;
                    }
                }
                if (map[location.Item1, location.Item2].building != null)
                {
                    if (map[location.Item1, location.Item2].building!.building_type == BuildingType.CITY_HALL)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// Attempts to fill the selection area with zones of the specified type.<br/>
        /// If placement is not possible, it will skip that tile and move on.<br/>
        /// </summary>
        /// <param name="top_left_corner">Sets the top left corner of the selection.</param>
        /// <param name="bottom_right_corner">Sets the bottom right corner of the selection.</param>
        /// <param name="zone_type">Specifies the type of zone to place. (NONE for deletion).</param>
        public int SetZone((int, int) top_left_corner, (int, int) bottom_right_corner, ZoneType zone_type)
        {
            if (!city_hall_placed)
            {
                StatusUpdate?.Invoke(this, new GameEventArgs { status = EventStatus.ERROR, update = EventUpdate.NO_UPDATE, message = "You need to place the city hall first!" });
                return -1;
            }
            List<(int, int)> newly_connected_zones = new List<(int, int)>();
            bool partial = false;
            int successfully_placed_zones = 0;
            for (int x = top_left_corner.Item1; x <= bottom_right_corner.Item1; x++)
            {
                for (int y = top_left_corner.Item2; y <= bottom_right_corner.Item2; y++)
                {
                    if(zone_type != ZoneType.NONE)
                    {
                        if (map[x, y].zone_type == ZoneType.NONE && map[x, y].building == null && map[x, y].road == null)
                        {
                            map[x, y].zone_type = zone_type;
                            map[x, y].zone_info = new ZoneInfo();
                            map[x, y].zone_info!.connected = _CheckForConnection((x, y));
                            successfully_placed_zones++;
                            if (map[x, y].zone_info!.connected)
                            {
                                newly_connected_zones.Add((x, y));
                                connected_zones.Add((x, y));
                            }
                            switch (zone_type)
                            {
                                case ZoneType.RESIDENTIAL:
                                    {
                                        map[x, y].zone_info!.max_capacity = zone_capacity[1,0];
                                    }
                                    break;
                                case ZoneType.INDUSTRIAL:
                                    {
                                        map[x, y].zone_info!.max_capacity = zone_capacity[2,0];
                                    }
                                    break;
                                case ZoneType.SERVICE:
                                    {
                                        map[x, y].zone_info!.max_capacity = zone_capacity[3,0];
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            partial = true;
                        }
                    }
                    else
                    {
                        if (map[x, y].zone_type != ZoneType.NONE)
                        {
                            if(map[x, y].zone_info!.people.Count == 0)
                            {
                                if (map[x,y].zone_info!.connected)
                                {
                                    _DisconnectZone((x, y));
                                }
                                map[x, y].zone_info = new ZoneInfo();
                                money += zone_price[(int)(map[x, y].zone_type)] / 2;
                                map[x, y].zone_type = ZoneType.NONE;
                                successfully_placed_zones++;
                            }
                            else
                            {
                                partial = true;
                            }
                        }
                        else
                        {
                            partial = true;
                        }
                    }
                }
            }
            foreach((int,int) location in newly_connected_zones)
            {
                _ConnectZone(location);
            }
            money -= zone_price[(int)zone_type] * successfully_placed_zones;
            if(successfully_placed_zones > 0)
            {
                switch(zone_type)
                {
                    case ZoneType.RESIDENTIAL:
                        {
                            spending_log.Add(new SpendingInfo(successfully_placed_zones * zone_price[1], time, successfully_placed_zones.ToString() + " residential zone(s) placed"));
                        }
                        break;
                    case ZoneType.INDUSTRIAL:
                        {
                            spending_log.Add(new SpendingInfo(successfully_placed_zones * zone_price[2], time, successfully_placed_zones.ToString() + " industrial zone(s) placed"));
                        }
                        break;
                    case ZoneType.SERVICE:
                        {
                            spending_log.Add(new SpendingInfo(successfully_placed_zones * zone_price[3], time, successfully_placed_zones.ToString() + " service zone(s) placed"));
                        }
                        break;
                }
            }
            if (partial)
            {
                StatusUpdate?.Invoke(this, new GameEventArgs { status = EventStatus.PARTIAL, update = EventUpdate.UPDATE, message = "Some spots are blocked in the selection!" });
            }
            else
            {
                StatusUpdate?.Invoke(this, new GameEventArgs { status = EventStatus.SUCCESS, update = EventUpdate.UPDATE, message = "Zone placed successfully!" });
            }
            return 0;
        }
        /// <summary>
        /// Sets the new tax rate for a given zone type.<br/>
        /// Takes effect after the next tax collection cycle.<br/>
        /// </summary>
        /// <param name="tax_rate">Tax rate in percentage.</param>
        /// <param name="zone_type">The type of zone (RESIDENTIAL|INDUSTRIAL|SERVICE), that the new tax rate should be applied to.</param>
        public void SetTaxRate(int tax_rate, ZoneType zone_type)
        {
            if(tax_rate < 0 || tax_rate > 100)
            {
                return;
            }
            switch (zone_type)
            {
                case ZoneType.RESIDENTIAL:
                    {
                        new_residental_tax_rate = tax_rate;
                    }
                    break;
                case ZoneType.INDUSTRIAL:
                    {
                        new_industrial_tax_rate = tax_rate;
                    }
                    break;
                case ZoneType.SERVICE:
                    {
                        new_service_tax_rate = tax_rate;
                    }
                    break;
            }
            StatusUpdate?.Invoke(this, new GameEventArgs { status = EventStatus.SUCCESS, update = EventUpdate.UPDATE, message = "New tax rates set successfully!" });
        }
        /// <summary>
        /// Attempts to repair the zone/building at the specified location.<br/>
        /// Restores the capacity/functionality of the repaired structure.<br/>
        /// </summary>
        /// <param name="position">Specifies the position of the tile to repair.</param>
        /// <returns></returns>
        public int Repair((int, int) position)
        {
            if (!city_hall_placed)
            {
                StatusUpdate?.Invoke(this, new GameEventArgs { status = EventStatus.ERROR, update = EventUpdate.NO_UPDATE, message = "You need to place the city hall first!" });
                return -1;
            }
            if (map[position.Item1, position.Item2].building != null)
            {
                (int, int) location = _JumpToMain(position);
                if (map[location.Item1,location.Item2].building!.damage > 0)
                {
                    if (map[location.Item1, location.Item2].building!.connected)
                    {
                        switch (map[position.Item1, position.Item2].building!.building_type)
                        {
                            case BuildingType.PARK:
                                {
                                    _RecalculateModifiers();
                                    spending_log.Add(new SpendingInfo((int)(map[location.Item1, location.Item2].building!.building_price * (map[location.Item1, location.Item2].building!.damage / 100.0)), time, "Park repaired"));
                                }
                                break;
                            case BuildingType.POLICE_STATION:
                                {
                                    _RecalculateModifiers();
                                    spending_log.Add(new SpendingInfo((int)(map[location.Item1, location.Item2].building!.building_price * (map[location.Item1, location.Item2].building!.damage / 100.0)), time, "Police station repaired"));
                                }
                                break;
                            case BuildingType.CITY_HALL:
                                {
                                    spending_log.Add(new SpendingInfo((int)(map[location.Item1, location.Item2].building!.building_price * (map[location.Item1, location.Item2].building!.damage / 100.0)), time, "City hall repaired"));
                                    CityHall ch = (CityHall)(map[location.Item1, location.Item2].building!);
                                    switch (ch.level)
                                    {
                                        case 3:
                                            if (map[location.Item1, location.Item2].building!.damage < 50)
                                            {
                                                city_wide_satisfaction_modifier++;
                                                base_arrival_rate += 50;
                                            }
                                            if (map[location.Item1, location.Item2].building!.damage >= 50)
                                            {
                                                city_wide_satisfaction_modifier += 2;
                                                base_arrival_rate += 100;
                                            }
                                            break;
                                        case 4:
                                            if (map[location.Item1, location.Item2].building!.damage < 50)
                                            {
                                                city_wide_satisfaction_modifier++;
                                                base_arrival_rate += 50;
                                                income_modifier += 2;
                                            }
                                            if (map[location.Item1, location.Item2].building!.damage >= 50)
                                            {
                                                city_wide_satisfaction_modifier += 2;
                                                base_arrival_rate += 100;
                                                income_modifier += 5;
                                                city_wide_satisfaction_modifier++;
                                            }
                                            break;
                                    }
                                }
                                break;
                        }
                    }
                    money -= (long)(map[location.Item1, location.Item2].building!.building_price * (map[location.Item1, location.Item2].building!.damage / 100.0));
                    for (int x = location.Item1; x < location.Item1 + map[location.Item1, location.Item2].building!.building_size.Item1; x++)
                    {
                        for (int y = location.Item2; y < location.Item2 + map[location.Item1, location.Item2].building!.building_size.Item2; y++)
                        {
                            map[x, y].building!.damage = 0;
                        }
                    }
                    for (int x = location.Item1 - zone_reach; x <= location.Item1 + map[location.Item1, location.Item2].building!.building_size.Item1 + zone_reach; x++)
                    {
                        for (int y = location.Item2 - zone_reach; y <= location.Item2 + map[location.Item1, location.Item2].building!.building_size.Item2 + zone_reach; y++)
                        {
                            if (_InsideMap((x, y)))
                            {
                                if (map[x, y].zone_type != ZoneType.NONE)
                                {
                                    map[x, y].zone_info!.damaged_nearby_buildings--;
                                }
                            }
                        }
                    }
                }
                else
                {
                    StatusUpdate?.Invoke(this, new GameEventArgs { status = EventStatus.ERROR, update = EventUpdate.NO_UPDATE, message = "This building is not damaged!" });
                    return -1;
                }
            }
            else
            {
                if (map[position.Item1, position.Item2].zone_info!.damage > 0)
                {
                    map[position.Item1, position.Item2].zone_info!.damage = 0;
                    if (map[position.Item1, position.Item2].zone_type != ZoneType.NONE)
                    {
                        spending_log.Add(new SpendingInfo((int)(zone_price[(int)(map[position.Item1, position.Item2].zone_type)] * (map[position.Item1, position.Item2].zone_info!.damage / 100.0)), time, "Zone repaired"));
                        money -= (long)(zone_price[(int)(map[position.Item1, position.Item2].zone_type)] * (map[position.Item1, position.Item2].zone_info!.damage / 100.0));//SHOULD BE BASED ON THE LEVEL
                        map[position.Item1, position.Item2].zone_info!.max_capacity = zone_capacity[(int)(map[position.Item1, position.Item2].zone_type), map[position.Item1, position.Item2].zone_info!.level - 1];
                        foreach ((int, int) rz in map[position.Item1, position.Item2].zone_info!.reachable_connected_zones)
                        {
                            map[rz.Item1, rz.Item2].zone_info!.damaged_nearby_buildings--;
                        }
                    }
                }
                else
                {
                    StatusUpdate?.Invoke(this, new GameEventArgs { status = EventStatus.ERROR, update = EventUpdate.NO_UPDATE, message = "This tile does not have a zone!" });
                    return -1;
                }
            }
            StatusUpdate?.Invoke(this, new GameEventArgs { status = EventStatus.SUCCESS, update = EventUpdate.UPDATE, message = "Repaired successfully!" });
            return 0;
        }
        private void _ExpireColonists((int,int) position, int number_of_victims)
        {
            int cnt = 0;
            foreach (KeyValuePair<int, Person.Person> ps in map[position.Item1, position.Item2].zone_info!.people)
            {
                if(cnt >= number_of_victims)
                {
                    break;
                }
                map[ps.Value.home.Item1, ps.Value.home.Item2].zone_info!.people.Remove(ps.Key);
                map[ps.Value.workplace.Item1, ps.Value.workplace.Item2].zone_info!.people.Remove(ps.Key);
                population--;
                cnt++;
            }
            foreach ((int, int) pos in connected_zones.ToList())
            {
                map[pos.Item1, pos.Item2].zone_info!.oxygen_requirement = map[pos.Item1, pos.Item2].zone_info!.people.Count / 10;
            }
        }
        private void _ZoneHit((int,int) position,int damage)
        {
            if(map[position.Item1, position.Item2].zone_type != ZoneType.NONE)
            {
                if (map[position.Item1, position.Item2].zone_info!.damage == 0)
                {
                    foreach ((int, int) rz in map[position.Item1, position.Item2].zone_info!.reachable_connected_zones)
                    {
                        map[rz.Item1, rz.Item2].zone_info!.damaged_nearby_buildings++;
                    }
                }
            }
            if(map[position.Item1, position.Item2].zone_info!.damage + damage >= 90)
            {
                damage = 90 - map[position.Item1, position.Item2].zone_info!.damage;
            }
            map[position.Item1, position.Item2].zone_info!.damage += damage;
            if (map[position.Item1, position.Item2].zone_type != ZoneType.NONE)
            {
                int capacity_reduction = Math.Min(map[position.Item1, position.Item2].zone_info!.max_capacity, (int)(zone_capacity[(int)(map[position.Item1, position.Item2].zone_type), map[position.Item1, position.Item2].zone_info!.level - 1] * (damage / 100.0)));
                map[position.Item1, position.Item2].zone_info!.max_capacity -= capacity_reduction;
                int colonist_reduction = Math.Max(map[position.Item1, position.Item2].zone_info!.people.Count - map[position.Item1, position.Item2].zone_info!.max_capacity, 0);
                _ExpireColonists(position, colonist_reduction);
            }
        }
        private void _BuildingHit((int,int) position,int damage)
        {
            (int, int) location = _JumpToMain(position);
            if (map[location.Item1,location.Item2].building!.damage == 0)
            {
                for (int x = location.Item1 - zone_reach; x <= location.Item1 + map[location.Item1, location.Item2].building!.building_size.Item1 + zone_reach; x++)
                {
                    for (int y = location.Item2 - zone_reach; y <= location.Item2 + map[location.Item1, location.Item2].building!.building_size.Item2 + zone_reach; y++)
                    {
                        if (_InsideMap((x, y)))
                        {
                            if (map[x, y].zone_type != ZoneType.NONE)
                            {
                                map[x, y].zone_info!.damaged_nearby_buildings++;
                            }
                        }
                    }
                }
            }
            if (map[location.Item1, location.Item2].building!.damage + damage >= 90)
            {
                damage = 90 - map[location.Item1, location.Item2].building!.damage;
            }
            bool first_half = false;
            bool second_half = false;
            if (map[location.Item1, location.Item2].building!.damage == 0 && map[location.Item1, location.Item2].building!.damage + damage < 50)
            {
                first_half = true;
                if(map[location.Item1, location.Item2].building!.damage + damage >= 50)
                {
                    second_half = true;
                }
            }
            if (map[location.Item1, location.Item2].building!.damage < 50 && map[location.Item1, location.Item2].building!.damage + damage >= 50)
            {
                second_half = true;
            }
            for (int x = location.Item1; x < location.Item1 + map[location.Item1,location.Item2].building!.building_size.Item1; x++)
            {
                for (int y = location.Item2; y < location.Item2 + map[location.Item1, location.Item2].building!.building_size.Item2; y++)
                {
                    map[x, y].building!.damage += damage;
                }
            }
            if (map[location.Item1, location.Item2].building!.connected)
            {
                switch (map[position.Item1, position.Item2].building!.building_type)
                {
                    case BuildingType.CITY_HALL:
                        {
                            if(city_hall_residents > 0)
                            {
                                city_hall_residents -= Math.Min(city_hall_residents, damage * 10);
                                population -= Math.Min(city_hall_residents, damage * 10);
                            }
                            CityHall ch = (CityHall)(map[location.Item1, location.Item2].building!);
                            switch(ch.level)
                            {
                                case 2:
                                    if (first_half)
                                    {
                                        city_wide_satisfaction_modifier--;
                                    }
                                    if (second_half)
                                    {
                                        city_wide_satisfaction_modifier--;
                                    }
                                    break;
                                case 3:
                                    if (first_half)
                                    {
                                        city_wide_satisfaction_modifier--;
                                        base_arrival_rate -= 50;
                                    }
                                    if (second_half)
                                    {
                                        city_wide_satisfaction_modifier--;
                                        base_arrival_rate -= 50;
                                    }
                                    break;
                                case 4:
                                    if (first_half)
                                    {
                                        city_wide_satisfaction_modifier--;
                                        base_arrival_rate -= 50;
                                        income_modifier -= 2;
                                    }
                                    if (second_half)
                                    {
                                        city_wide_satisfaction_modifier--;
                                        base_arrival_rate -= 50;
                                        income_modifier -= 3;
                                        city_wide_satisfaction_modifier--;
                                    }
                                    break;
                            }
                        }
                        break;
                }
            }
        }
        /// <summary>
        /// Selects the targets of the meteor shower.<br/>
        /// A number of efficient buildings and roughly 3 times as many other zones will be targeted, calculated based on catastrophe_intensity.<br/>
        /// </summary>
        /// <param name="catastrophe_intensity">A number between 1 and 5 indicating the desired intensity of the meteor shower.</param>
        public int InvokeCatastrophe(int catastrophe_intensity)
        {
            if(catastrophe_intensity < 0 || catastrophe_intensity > 5)
            {
                return -1;
            }
            HashSet<(int,int)> tg = new HashSet<(int,int)> ();
            int number_of_actual_hits = m_rand.Next(catastrophe_scale[catastrophe_intensity].Item1 - 2, catastrophe_scale[catastrophe_intensity].Item1 + 3);
            int number_of_phony_hits = m_rand.Next(catastrophe_scale[catastrophe_intensity].Item1 * 3 - 2, catastrophe_scale[catastrophe_intensity].Item1 * 3 + 3);
            targeted_zones = new (int, int, int)[number_of_actual_hits + number_of_phony_hits + 1];
            for(int p = 0; (number_of_actual_hits > 0) || (number_of_phony_hits > 0); p++)
            {
                (int, int, int) target = (m_rand.Next(0, map_size.Item1), m_rand.Next(0, map_size.Item2), m_rand.Next(catastrophe_scale[catastrophe_intensity].Item2 - 10, catastrophe_scale[catastrophe_intensity].Item2 + 11));
                if(tg.Contains((target.Item1,target.Item2)))
                {
                    p--;
                    continue;
                }
                tg.Add((target.Item1, target.Item2));
                if (map[target.Item1, target.Item2].road != null || (map[target.Item1, target.Item2].building != null && map[target.Item1,target.Item2].building!.building_type == BuildingType.OXYGEN_DIFFUSER) || (map[target.Item1, target.Item2].building != null && map[target.Item1, target.Item2].building!.building_type == BuildingType.OXYGEN_GENERATOR))
                {
                    p--;
                    continue;
                }
                if (map[target.Item1, target.Item2].building != null || (map[target.Item1,target.Item2].zone_type != ZoneType.NONE && map[target.Item1, target.Item2].zone_info!.people.Count > 0))
                {
                    if (number_of_actual_hits > 0)
                    {
                        targeted_zones[p] = target;
                        if (map[target.Item1,target.Item2].building != null)
                        {
                            map[target.Item1, target.Item2].building!.targeted = true;
                        }
                        map[target.Item1, target.Item2].zone_info!.targeted = true;
                        number_of_actual_hits--;
                    }
                }
                else
                {
                    if (p >= targeted_zones.Length)
                    {
                        break;
                    }
                    if (number_of_phony_hits > 0)
                    {
                        targeted_zones[p] = target;
                        map[target.Item1, target.Item2].zone_info!.targeted = true;
                        number_of_phony_hits--;
                    }
                    else
                    {
                        p--;
                    }
                }
                if(tg.Count - place_road_count >= map_size.Item1 * map_size.Item2)
                {
                    break;
                }
            }
            StatusUpdate?.Invoke(this, new GameEventArgs { status = EventStatus.SUCCESS, update = EventUpdate.UPDATE, message = "Catastrophe invoked successfully!" });
            return 0;
        }
        /// <summary>
        /// Registers the effects of the meteor shower.<br/>
        /// This may include dealing damage and killing colonists.<br/>
        /// Resets the targeted tags.<br/>
        /// </summary>
        public int RegisterCatastrophe()
        {
            if(targeted_zones.Length == 0)
            {
                return -1;
            }
            foreach((int,int,int) target in targeted_zones)
            {
                if (map[target.Item1, target.Item2].building == null && map[target.Item1, target.Item2].road == null)
                {
                    map[target.Item1, target.Item2].zone_info!.targeted = false;
                    _ZoneHit((target.Item1,target.Item2),target.Item3);
                }
                if (map[target.Item1, target.Item2].building != null)
                {
                    map[target.Item1, target.Item2].zone_info!.targeted = false;
                    map[target.Item1, target.Item2].building!.targeted = true;
                    _BuildingHit((target.Item1, target.Item2), target.Item3);
                }
            }
            _RecalculateModifiers();
            StatusUpdate?.Invoke(this, new GameEventArgs { status = EventStatus.SUCCESS, update = EventUpdate.UPDATE, message = "Meteors fell successfully!" });
            return 0;
        }
        private void _UpgradeCityHall(int level)
        {
            switch(level)
            {
                case 1:
                    {
                        city_hall_level = 1;
                    }
                    break;
                case 2:
                    {
                        money -= 25000;
                        spending_log.Add(new SpendingInfo(25000, time, "City hall upgraded to level 2"));
                        city_hall_level = 2;
                    }
                    break;
                case 3:
                    {
                        money -= 50000;
                        spending_log.Add(new SpendingInfo(50000, time, "City hall upgraded to level 3"));
                        city_wide_satisfaction_modifier += 1;
                        base_arrival_rate += 100;
                        city_hall_level = 3;
                    }
                    break;
                case 4:
                    {
                        money -= 100000;
                        spending_log.Add(new SpendingInfo(100000, time, "City hall upgraded to level 4"));
                        city_wide_satisfaction_modifier += 1;
                        income_modifier += 5;
                        city_hall_level = 4;
                    }
                    break;
            }
        }
        /// <summary>
        /// Upgrades the selected tile, if possible.<br/>
        /// The city hall can be upgraded to level 4, while zones can reach level 3.<br/>
        /// No other structure can be upgraded.<br/>
        /// </summary>
        /// <param name="position">The coordinates of the tile to upgrade.</param>
        public int Upgrade((int,int) position)
        {
            if (!city_hall_placed)
            {
                StatusUpdate?.Invoke(this, new GameEventArgs { status = EventStatus.ERROR, update = EventUpdate.NO_UPDATE, message = "You need to place the city hall first!" });
                return -1;
            }
            int upgrade_mode = -1;
            if (map[position.Item1,position.Item2].building != null)
            {
                (int, int) location = _JumpToMain(position);
                switch (map[location.Item1,location.Item2].building!.building_type)
                {
                    case BuildingType.CITY_HALL:
                        {
                            upgrade_mode = 0;
                            CityHall ch = (CityHall)(map[location.Item1, location.Item2].building!);
                            if (ch.level < 4)
                            {
                                for (int x = location.Item1; x < location.Item1 + map[location.Item1, location.Item2].building!.building_size.Item1; x++)
                                {
                                    for (int y = location.Item2; y < location.Item2 + map[location.Item1, location.Item2].building!.building_size.Item2; y++)
                                    {
                                        if (map[x, y].zone_type != ZoneType.NONE || map[x, y].building != null || map[x, y].road != null)
                                        {
                                            CityHall city_hall = (CityHall)(map[x, y].building!);
                                            map[x, y].zone_info!.level++;
                                            city_hall.level++;
                                        }
                                    }
                                }
                                _UpgradeCityHall(ch.level);
                            }
                        }
                        break;
                }
            }
            if (map[position.Item1,position.Item2].zone_type != ZoneType.NONE)
            {
                upgrade_mode = 1;
                if(map[position.Item1, position.Item2].zone_info!.level < 4 && map[position.Item1, position.Item2].zone_info!.level < city_hall_level)
                {
                    money -= 500 * map[position.Item1, position.Item2].zone_info!.level;
                    spending_log.Add(new SpendingInfo(500 * map[position.Item1, position.Item2].zone_info!.level, time, "Zone upgraded to level " + (map[position.Item1, position.Item2].zone_info!.level + 1)));
                    map[position.Item1, position.Item2].zone_info!.level++;
                    map[position.Item1, position.Item2].zone_info!.max_capacity = zone_capacity[(int)(map[position.Item1, position.Item2].zone_type), map[position.Item1, position.Item2].zone_info!.level - 1];
                }
                else
                {
                    StatusUpdate?.Invoke(this, new GameEventArgs { status = EventStatus.ERROR, update = EventUpdate.NO_UPDATE, message = "City hall level can not be less than zone level!" });
                }
            }
            if(upgrade_mode == -1)
            {
                StatusUpdate?.Invoke(this, new GameEventArgs { status = EventStatus.ERROR, update = EventUpdate.NO_UPDATE, message = "You can only upgrade zones and the city hall!" });
            }
            StatusUpdate?.Invoke(this, new GameEventArgs { status = EventStatus.SUCCESS, update = EventUpdate.UPDATE, message = "Upgraded successfully!" });
            return 0;
        }
        /// <summary>
        /// Converts a time stamp into a date, according to the in-game calendar.
        /// </summary>
        /// <param name="time_to_convert">The time stamp to convert</param>
        /// <returns>(Year,Month,Day)</returns>
        public (int, int, int) ConvertTime(long time_to_convert)
        {
            int md = (int)(time_to_convert % 668);
            int mt = 14;
            for (int m = 0; m < 14; m++)
            {
                if (md >= calendar[m].Item3 && md < calendar[m + 1].Item3)
                {
                    mt = m;
                    break;
                }
            }
            return ((int)(time_to_convert / 668), mt, md - calendar[mt].Item3);
        }
    }
}
