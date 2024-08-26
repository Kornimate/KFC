using KFC.Model.Buildings;
using KFC.Model.Pipe;
using KFC.Model.Road;
using KFC.Model.Zones;

namespace KFC.ViewModel {
    public class ViewModelTile : ViewModelBase
    {
        private int upkeep;
        private int population;
        private int capacity;
        private double publicorder;
        private double publicsatisfaction;
        private bool connected;

        public int UpKeep
        {
            get
            {
                return upkeep;
            }
            set
            {
                if (value != upkeep)
                {
                    upkeep = value;
                    OnPropertyChanged();
                }
            }
        }
        public int Population
        {
            get
            {
                return population;
            }
            set
            {
                if (value != population)
                {
                    population = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool Connected
        {
            get
            {
                return connected;
            }
            set
            {
                if (value != connected)
                {
                    connected = value;
                    OnPropertyChanged();
                }
            }
        }
        public int Capacity
        {
            get
            {
                return capacity;
            }
            set
            {
                if (value != capacity)
                {
                    capacity = value;
                    OnPropertyChanged();
                }
            }
        }
        public double PublicSatisfaction
        {
            get
            {
                return publicsatisfaction;
            }
            set
            {
                if (value != publicsatisfaction)
                {
                    publicsatisfaction = value;
                    OnPropertyChanged();
                }
            }
        }
        public double PublicOrder
        {
            get
            {
                return publicorder;
            }
            set
            {
                if (value != publicorder)
                {
                    publicorder = value;
                    OnPropertyChanged();
                }
            }
        }
        public int Location { get; set; }

        private Uri tileimagebindingsource;

        public Uri TileImageBuildingSource
        {
            get
            {
                return tileimagebindingsource;
            }
            set
            {
                if (tileimagebindingsource != value)
                {
                    tileimagebindingsource = value;
                    OnPropertyChanged();
                }
            }
        }

        private IBuilding building;
        public IBuilding Building
        {
            get { return building; }
            set
            {
                if (value != building)
                {
                    building = value;
                }
            }
        }

        private RoadNetworkNode road;
        public RoadNetworkNode Road
        {
            get { return road; }
            set
            {
                if (road != value)
                {
                    road = value;
                }
            }
        }

        private PipeNetworkNode pipenode;
        public PipeNetworkNode PipeNode
        {
            get
            {
                return pipenode;
            }
            set
            {
                if(value != pipenode)
                {
                    pipenode = value;
                }
            }
        }

        private ZoneType zone;
        public ZoneType Zone
        {
            get
            {
                return zone;
            }
            set
            {
                if (zone != value)
                {
                    zone = value;
                    OnPropertyChanged();
                }
            }
        }
        private Uri tileimageoxygensource;
        public Uri TileImageOxygenSource
        {
            get
            {
                return tileimageoxygensource;
            }
            set
            {
                if(value != tileimageoxygensource)
                {
                    tileimageoxygensource= value;
                    OnPropertyChanged();
                }
            }
        }

        private double opacity;
        public double Opacity
        {
            get { return opacity; }
            set
            {
                if (opacity != value)
                {
                    opacity = value;
                    OnPropertyChanged();
                }
            }
        }
        private string zonecolor;
        public string ZoneColor
        {
            get { return zonecolor; }
            set
            {
                if (zonecolor != value)
                {
                    zonecolor = value;
                    OnPropertyChanged(); //not to show color
                }
            }
        }
        private int level;
        public int Level
        {
            get
            {
                return level;
            }
            set
            {
                if (level != value)
                {
                    level = value;
                    OnPropertyChanged();
                }
            }
        }
        private string filtercolor;
        public string FilterColor
        {
            get
            {
                return filtercolor;
            }
            set
            {
                if (value != filtercolor)
                {
                    filtercolor = value;
                    OnPropertyChanged();
                }
            }
        }
        private bool ismeteorfalling;
        public bool IsMeteorFalling
        {
            get
            {
                return ismeteorfalling;
            }
            set
            {
                if (value != ismeteorfalling)
                {
                    ismeteorfalling = value;
                    OnPropertyChanged();
                }
            }
        }
        private int damage;
        public int Damage
        {
            get
            {
                return damage;
            }
            set
            {
                if (value != damage)
                {
                    damage = value;
                    OnPropertyChanged();
                }
            }
        }
        public double OxygenOpacity { get; set; }

        private int oxygenrequirement;
        public int OxygenRequirement
        {
            get
            {
                return oxygenrequirement;
            }
            set
            {
                if (value != oxygenrequirement)
                {
                    oxygenrequirement = value;
                    OnPropertyChanged();
                }
            }
        }

        private int oxygensupply;
        public int OxygenSupply
        {
            get
            {
                return oxygensupply;
            }
            set
            {
                if (value != oxygensupply)
                {
                    oxygensupply = value;
                    OnPropertyChanged();
                }
            }
        }

        public DelegateCommand? OnClick { get; set; }

        public ViewModelTile()
        {
            UpKeep = 0;
            Population = 0;
            PublicSatisfaction = 0;
            opacity = 0.0;
            zonecolor = "Transparent";
            FilterColor = "Transparent";
            OxygenOpacity = 0.0;
            Road = null!;
            building = null!;
            damage = 0;
            ismeteorfalling = false;
            oxygensupply = 0;
            oxygenrequirement = 0;
            tileimagebindingsource = new(Directory.GetCurrentDirectory() + "/Resources/empty.png");
            road = new();
            pipenode = new();
            tileimageoxygensource = new(Directory.GetCurrentDirectory() + "/Resources/empty.png");
            filtercolor = "Transparent";
        }
    }
}