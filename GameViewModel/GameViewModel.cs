using KFC.Model;
using KFC.Model.Buildings;
using KFC.Model.Zones;
using KFC.Persistence;
using LiveCharts;
using System.Collections.ObjectModel;
using System.Timers;

namespace KFC.ViewModel {
    public class GameViewModelKFC : ViewModelBase
    {
        private const int timeseriessize = 15;
        private const int meteoranimationtime = 200;
        private int MatrixWidth
        {
            get
            {
                return 30; // 30x25
            }
        }
        private int MatrixHeight
        {
            get
            {
                return 25; // 30x25
            }
        }
        private int Scale
        {
            get
            {
                return 100;
            }
        }
        public ObservableCollection<ViewModelTile> TileMatrix { get; set; }
        public ObservableCollection<ViewModelSavedGame> Saves { get; set; }
        public ObservableCollection<Packet> Packets { get; set; }
        public ObservableCollection<ViewModelHeatMap> HeatMaps { get; set; }
        public ObservableCollection<FinanceMenuItem> Finances
        {
            get
            {
                if(model.current_game == null) return new();
                return new ObservableCollection<FinanceMenuItem>(model.current_game.spending_log.Select(x => new FinanceMenuItem(x.amount,TupleToStringTimeConverter(model.current_game.ConvertTime(x.time_stamp)),x.spending_message)));
            }
        }

        ChartValues<long> moneytimeseries;
        ChartValues<long> peopletimeseries;
        int helpercounter;

        public ChartValues<long> MoneyTimeSeries
        {
            get
            {
                if (moneytimeseries.Count > timeseriessize) moneytimeseries.RemoveAt(0);
                moneytimeseries.Add(model.current_game.monthly_log.Last().money);
                if (moneytimeseries.Count > model.current_game.monthly_log.Count) moneytimeseries.RemoveAt(moneytimeseries.Count - 1);
                return moneytimeseries;
            }
        }
        public ChartValues<long> PeopleTimeSeries
        {
            get
            {
                if (peopletimeseries.Count > timeseriessize) peopletimeseries.RemoveAt(0);
                peopletimeseries.Add(model.current_game.monthly_log.Last().population);
                if (peopletimeseries.Count > model.current_game.monthly_log.Count) peopletimeseries.RemoveAt(peopletimeseries.Count - 1);
                return peopletimeseries;
            }
        }
        private int residentialtaxrate;
        public int ResidentialTaxRate
        {
            get
            {
                return residentialtaxrate;
            }
            set
            {
                if (value != residentialtaxrate)
                {
                    residentialtaxrate = value;
                    OnPropertyChanged();
                }
            }
        }
        private int industrialtaxrate;
        public int IndustrialTaxRate
        {
            get
            {
                return industrialtaxrate;
            }
            set
            {
                if (value != industrialtaxrate)
                {
                    industrialtaxrate = value;
                    OnPropertyChanged();
                }
            }
        }
        private int servicetaxrate;
        public int ServiceTaxRate
        {
            get
            {
                return servicetaxrate;
            }
            set
            {
                if (value != servicetaxrate)
                {
                    servicetaxrate = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Date
        {
            get
            {
                if (model.current_game == null)
                {
                    return "1/1/1";
                }
                return $"{model.current_game.converted_time.Item1 + 1}/{model.current_game.converted_time.Item2 + 1}/{model.current_game.converted_time.Item3 + 1}";
            }
        }
        public long Income
        {
            get
            {
                if (model.current_game == null) return 0;
                return model.current_game.income;
            }
        }
        public string BuildingData { get; private set; }
        public string PublicSatisfaction
        {
            get
            {
                if (model.current_game == null) return 0.ToString();
                return String.Format("{0:0.##}", model.current_game.total_public_satisfaction * 100);
            }
        }
        public long Population
        {
            get
            {
                if (model.current_game == null) return 0;
                return model.current_game.population;
            }
        }
        public int GameSpeed { get; private set; }
        public Uri SpeedImage
        {
            get
            {
                return gameSpeeds[GameSpeed].Key;
            }
        }
        public Uri PauseImage
        {
            get
            {
                return new Uri(Directory.GetCurrentDirectory() + (timer.Enabled ? imagePaths["pause"] : imagePaths["resume"]));
            }
        }
        public string SaveName { get; set; } // CityName
        public int Capacity
        {
            get
            {
                if (currenttile == null) return 0;
                return currenttile!.Capacity;
            }
        }
        public int Occupancy
        {
            get
            {
                if (currenttile == null) return 0;
                return currenttile.Population;
            }
        }
        public string PublicOrder
        {
            get
            {
                if (currenttile == null) return 0.ToString();
                return String.Format("{0:0.##}", currenttile.PublicOrder * 100);
            }
        }
        public string SatisfactionRate
        {
            get
            {
                if (currenttile == null) return 0.ToString();
                return String.Format("{0:0.##}", currenttile.PublicSatisfaction * 100);
            }
        }
        public string OxygenSupply
        {
            get
            {
                if (currenttile is null) return 0.ToString();
                return currenttile.OxygenSupply.ToString();
            }
        }
        public string OxygenRequirement
        {
            get
            {
                if (currenttile is null) return 0.ToString();
                return currenttile.OxygenRequirement.ToString();
            }
        }
        public string Damage
        {
            get
            {
                if (mainforselectedtile is null) return 0.ToString();
                return mainforselectedtile.Damage.ToString();
            }
        }
        public string Level
        {
            get
            {
                if (currenttile is null) return 0.ToString();
                if (currenttile.Building?.building_type == BuildingType.CITY_HALL && currenttile.Level >= 4)
                {
                    return "Max";
                }
                if (currenttile.Building?.building_type != BuildingType.CITY_HALL && currenttile.Level >= 3)
                {
                    return "Max";
                }
                return currenttile.Level.ToString();
            }
        }
        public long Money
        {
            get
            {
                if (model.current_game == null) return 0;
                return model.current_game.money;
            }
        }
        public string Tax
        {
            get
            {
                return String.Format("{0:0.##}", (model?.current_game?.converted_time.Item1 == 0 && model?.current_game?.converted_time.Item2 == 0 ? 0 : Income + UpKeep));
            }
        }
        public long UpKeep
        {
            get
            {
                if (model.current_game == null) return 0;
                return model.current_game.upkeep;
            }
        }
        public bool IsSaved
        {
            get
            {
                return model.curr_path is not null;
            }
        }
        public bool UpgradeEnabled
        {
            get
            {
                if (currenttile is null) return false;
                if (afterdisaster) return false;
                if (!loaded) return false;
                if (currenttile.Building is CityHall && currenttile.Level != 4) return true;
                if (currenttile.Zone == ZoneType.NONE) return false;
                if (currenttile.Population <= 0) return false;
                if (currenttile.Level >= 3) return false;
                return true;
            }
        }
        public bool RepairEnabled
        {
            get
            {
                if (currenttile is null) return false;
                if (afterdisaster) return false;
                if (!loaded) return false;
                if (currenttile.Damage > 0) return true;
                return false;
            }
        }
        public string PopUpMessage { get; set; }
        public string PopUpTextColor { get; set; }

        private int NoiseFunction(int seed, int location, int mod)
        {
            return ((seed + location) * 99999 / 45667 + 345671) % mod + 1; //starts from 1
        }

        public DelegateCommand SetTimerCommand { get; private set; }
        public DelegateCommand BuildingCommand { get; private set; }
        public DelegateCommand ZoneCommand { get; private set; }
        public DelegateCommand NewGameCommand { get; private set; }
        public DelegateCommand DeleteBuildingCommand { get; private set; }
        public DelegateCommand DeleteZoneCommand { get; private set; }
        public DelegateCommand SaveGameCommand { get; private set; }
        public DelegateCommand OverwriteSaveGameCommand { get; private set; }
        public DelegateCommand ExitGameCommand { get; private set; }
        public DelegateCommand CreditsCommand { get; private set; }
        public DelegateCommand HeatMapCommand { get; private set; } // parameter heatmap tipusa
        public DelegateCommand HelpCommand { get; private set; }
        public DelegateCommand SpeedCommand { get; private set; }
        public DelegateCommand MeteorCommand { get; private set; }
        public DelegateCommand UpgradeCommand { get; private set; }
        public DelegateCommand UpgradeAllCommand { get; private set; }
        public DelegateCommand RepairCommand { get; private set; }
        public DelegateCommand RepairAllCommand { get; private set; }
        public DelegateCommand SaveMenuOpened { get; private set; }
        public DelegateCommand SaveMenuClosed { get; private set; }
        public DelegateCommand PipeCommand { get; private set; }
        public DelegateCommand DeletePipeCommand { get; private set; }
        public DelegateCommand HelpMenuClosedCommand { get; private set; }

        private int firstcoordx;
        private int firstcoordy;

        private string currentbuilding = String.Empty;
        private string currentzone = String.Empty;
        private string prevbuilding = String.Empty;
        private string prevzone = String.Empty;
        private string currentpipe = String.Empty;
        private string prevpipe = String.Empty;

        private MapModes currentheatmap = MapModes.Empty;
        private MapModes priorityheatmap = MapModes.None;
        private MapModes prioritymeteorheatmap = MapModes.None;

        private int seed = 0;

        private bool loaded;
        private bool prevtimerstate;
        private bool afterdisaster;

        private OneShot? onetime;
        private CollectionIterator iterator;

        private ViewModelTile currenttile;
        private ViewModelTile prevtile;
        private ViewModelTile mainforselectedtile;

        private GameModes gameMode;
        public string IsConnected
        {
            get
            {
                if (mainforselectedtile is null) return "Gray";
                if (currenttile.Building is null && currenttile.Population <= 0) return "Gray";
                return mainforselectedtile.Connected ? "Green" : "Red";
            }
        }

        public string ConnectedString
        {
            get
            {
                if (mainforselectedtile is null) return "No Info";
                if (currenttile.Building is null && currenttile.Population <= 0) return "No Info";
                return mainforselectedtile.Connected ? "Connected" : "Not Connected";
            }
        }

        public string IncomeColor
        {
            get
            {
                switch (Income)
                {
                    case 0:
                        return "Yellow";
                    case < 0:
                        return "Red";
                    default:
                        return "Green";
                }

            }
        }
        private bool ispacketselected;
        public bool IsPacketSelected
        {
            get
            {
                return ispacketselected;
            }
            set
            {
                if (ispacketselected != value)
                {
                    ispacketselected = value;
                    istileselected = !ispacketselected;
                    CallAllPropertyChanged();
                }
            }
        }

        private bool istileselected;
        public bool IsTileSelected
        {
            get
            {
                return istileselected;
            }
            set
            {
                if (istileselected != value)
                {
                    istileselected = value;
                    ispacketselected = !istileselected;
                    CallAllPropertyChanged();
                }
            }
        }

        public string PacketName
        {
            get
            {
                if (afterdisaster) return "Disaster";
                return gameMode switch
                {
                    GameModes.Playing => "",
                    GameModes.ZonePlacing => currentzone,
                    GameModes.BuildingPlacing => currentbuilding,
                    GameModes.ZoneDeleting => "ZoneDeleting",
                    GameModes.BuildingDeleting => "BuildingDeleting",
                    GameModes.RoadPlacing => currentbuilding != "RoadS" ? currentbuilding : "Road",
                    GameModes.RoadDeleting => "RoadDeleting",
                    GameModes.MeteorDisaster => "Disaster",
                    GameModes.PipePlacing => "Pipe",
                    GameModes.PipeDeleting => "PipeDeleting",
                    _ => "",
                };
            }
        }

        public bool PlayButtonPulse { get; set; }

        private System.Timers.Timer timer;
        private System.Timers.Timer meteortimer;
        private KFC.Model.Application model;

        public event EventHandler? callAppToNewGame;
        public event EventHandler<string>? callAppToLoadGame;
        public event EventHandler<bool>? callAppToSaveGame;
        public event EventHandler<string>? callAppToDeleteGame;
        public event EventHandler? callAppToExitGame;
        public event EventHandler? callAppToLoadSuccess;
        public event EventHandler? callAppToShowCredits;
        public event EventHandler? callAppToShowHelp;
        public event EventHandler<string>? callAppToShowInfo;
        public event EventHandler<string>? callAppToShowGameOver;

        private event EventHandler cityhallplaced;

        public GameViewModelKFC(KFC.Model.Application model)
        {
            this.model = model;

            firstcoordx = -1;
            firstcoordy = -1;

            TileMatrix = new ObservableCollection<ViewModelTile>();
            HeatMaps = new ObservableCollection<ViewModelHeatMap>();
            Saves = new ObservableCollection<ViewModelSavedGame>();
            Packets = new ObservableCollection<Packet>();

            BuildingData = "no data";
            SaveName = "";

            gameMode = GameModes.Playing;

            priorityheatmap = MapModes.None;
            prioritymeteorheatmap = MapModes.None;
            currentheatmap = MapModes.Empty;

            SetTimerCommand = new DelegateCommand((param) => ToggleTimer());
            SpeedCommand = new DelegateCommand((param) => SetGameSpeed());

            HeatMapCommand = new DelegateCommand((param) => SelectHeatMap(param));
            BuildingCommand = new DelegateCommand((param) => BuildingForPlace(param));
            ZoneCommand = new DelegateCommand((param) => ZoneForPlace(param));
            DeleteBuildingCommand = new DelegateCommand((param) => BuildingForDelete());
            DeleteZoneCommand = new DelegateCommand((param) => ZoneForDelete());

            NewGameCommand = new DelegateCommand((param) => NewGameToApp());
            SaveGameCommand = new DelegateCommand((param) => SaveGameToApp(param));
            OverwriteSaveGameCommand = new DelegateCommand((param) => OverwriteGameToApp(param));

            ExitGameCommand = new DelegateCommand((param) => ExitGameToApp());
            CreditsCommand = new DelegateCommand((param) => ShowCreditsToApp());
            HelpCommand = new DelegateCommand((param) => ShowHelpToApp());
            MeteorCommand = new DelegateCommand((param) => StartMeteorDisaster());
            UpgradeCommand = new DelegateCommand((param) => UpgradeCurrentTile());
            UpgradeAllCommand = new DelegateCommand((param) => UpgradeZoneTiles());
            RepairCommand = new DelegateCommand((param) => RepairCurrentTile());
            RepairAllCommand = new DelegateCommand((param) => RepairAllTiles());

            SaveMenuOpened = new DelegateCommand((param) => SaveMenuOpenedPause());
            SaveMenuClosed = new DelegateCommand((param) => SaveMenuClosedUnpause());

            PipeCommand = new DelegateCommand((param) => PipeForPlace(param));
            DeletePipeCommand = new DelegateCommand((param) => PipeForDelete());

            HelpMenuClosedCommand = new DelegateCommand((param) => HelpMenuClosed());

            moneytimeseries = new ChartValues<long>();
            peopletimeseries = new ChartValues<long>();

            PopUpMessage = string.Empty;
            PopUpTextColor = string.Empty;
            iterator = new(new List<int>());
            prevtile = new();
            mainforselectedtile = new();
            timer = new();
            meteortimer = new();

            PlayButtonPulse = false;

            currenttile = new();
            IsPacketSelected = true;

            loaded = false;
            prevtimerstate = false;

            cityhallplaced += new EventHandler(InitializeRemainingButtons);

            model.SaveDataRetrieved += UpdateSavedGames;
            model.GetSavedGamesInfo();
            model.LoadingSuccess += LoadSuccess;
            model.LoadingError += LoadError;
            model.DeleteSuccess += DeleteSuccess;
            model.DeleteError += DeleteError;

            ConfigureTimer();
            InitializeButtons();
            SetGameSpeed(0);
        }
        private void RestoreChartValues()
        {
            moneytimeseries = new ChartValues<long>(model.current_game.monthly_log.TakeLast(timeseriessize).Select(x => x.money).ToList());
            peopletimeseries = new ChartValues<long>(model.current_game.monthly_log.TakeLast(timeseriessize).Select(x => x.population).ToList());
        }

        private string TupleToStringTimeConverter((int,int,int) tuple)
        {
            return $"{tuple.Item1 + 1}/{tuple.Item2 + 1}/{tuple.Item3 + 1}";
        }
        public void FinancesPropertyChanged()
        {
            OnPropertyChanged(nameof(Finances));
            if(helpercounter == 0)
            {
                InitializeChart();
                helpercounter++;
            }
            CallAllPropertyChanged();
            OnPropertyChanged(nameof(MoneyTimeSeries));
            OnPropertyChanged(nameof(PeopleTimeSeries));
        }

        private void PipeForDelete()
        {
            priorityheatmap = MapModes.Oxygen;
            if (gameMode == GameModes.PipeDeleting)
            {
                gameMode = GameModes.Playing;
                IsPacketSelected = false;
                priorityheatmap = MapModes.None;
                CallAllPropertyChanged();
                SetMatrixTileValues();
                return;
            }
            prevpipe = "";
            IsPacketSelected = true;
            gameMode = GameModes.PipeDeleting;
            CallAllPropertyChanged();
            SetMatrixTileValues();
        }

        private void PipeForPlace(object? param)
        {
            currentpipe = param!.ToString()!;
            if (gameMode == GameModes.PipePlacing && currentpipe == prevpipe)
            {
                priorityheatmap = MapModes.None;
                gameMode = GameModes.Playing;
                prevpipe = "";
                IsPacketSelected = false;
                CallAllPropertyChanged();
                SetMatrixTileValues();
                return;
            }
            firstcoordx = -1;
            firstcoordy = -1;
            IsPacketSelected = true;
            priorityheatmap = MapModes.Oxygen;
            prevpipe = currentpipe;
            gameMode = GameModes.PipePlacing;
            CallAllPropertyChanged();
            SetMatrixTileValues();
        }

        public void SetTaxRate(string zone)
        {
            ZoneType newtaxforzone = ZoneTypes[zone];
            switch (newtaxforzone)
            {
                case ZoneType.RESIDENTIAL:
                    model.current_game.SetTaxRate(ResidentialTaxRate, newtaxforzone);
                    break;
                case ZoneType.INDUSTRIAL:
                    model.current_game.SetTaxRate(IndustrialTaxRate, newtaxforzone);
                    break;
                case ZoneType.SERVICE:
                    model.current_game.SetTaxRate(ServiceTaxRate, newtaxforzone);
                    break;
                case ZoneType.NONE: // just do nothing
                    break;
                default: // just do nothing
                    break;
            }
        }

        public void RepairCurrentTile()
        {
            if (!RepairEnabled)
            {
                return;
            }
            int i = currenttile!.Location % Scale;
            int j = currenttile!.Location / Scale;
            model.current_game.Repair((i, j));
            SetMatrixTileValues();
            CallAllPropertyChanged();
        }

        private void RepairAllTiles()
        {
            var prevcurrenttile = currenttile;
            for (int pos = 0; pos < MatrixWidth * MatrixHeight; pos++)
            {
                int j = TileMatrix[pos].Location / Scale;
                int i = TileMatrix[pos].Location % Scale;
                currenttile = TileMatrix[pos];
                if (RepairEnabled)
                {
                    model.current_game.Repair((i, j));
                }
            }
            currenttile = prevcurrenttile;
            SetMatrixTileValues();
            CallAllPropertyChanged();
        }

        public void UpgradeCurrentTile()
        {
            if (!UpgradeEnabled)
            {
                return;
            }
            int i = currenttile!.Location % Scale;
            int j = currenttile!.Location / Scale;
            model.current_game.Upgrade((i, j));
            SetMatrixTileValues();
            CallAllPropertyChanged();
        }

        public void UpgradeZoneTiles() {
            var prevcurrenttile = currenttile;
            for (int pos = 0; pos < MatrixWidth * MatrixHeight; pos++) {
                int j = TileMatrix[pos].Location / Scale;
                int i = TileMatrix[pos].Location % Scale;
                if (TileMatrix[pos].Building?.building_type == BuildingType.CITY_HALL)
                {
                    continue;
                }
                currenttile = TileMatrix[pos];
                if (UpgradeEnabled)
                {
                    model.current_game.Upgrade((i, j));
                }
            }
            currenttile = prevcurrenttile;
            SetMatrixTileValues();
            CallAllPropertyChanged();
        }

        private void StartMeteorDisaster()
        {
            if (afterdisaster)
            {
                ShowMessageForUser("First you have to continue  the game!", EventStatus.ERROR);
                return;
            }
            if (!model.current_game.city_hall_placed)
            {
                ShowMessageForUser("First you have to place the City Hall!", EventStatus.ERROR);
                return;
            }
            if (!loaded)
            {
                ShowMessageForUser("You can't do that now!", EventStatus.ERROR);
                return;
            }
            Random r = new Random();
            timer.Enabled = false;
            prioritymeteorheatmap = MapModes.Empty;
            afterdisaster = true;
            model.current_game.InvokeCatastrophe(r.Next(1, 6));
            SimulateMeteorFalls();
            OnPropertyChanged(nameof(PauseImage));
        }

        private void SimulateMeteorFalls()
        {
            loaded = false;
            List<int> targeted = new List<int>();
            Random r = new Random();
            int pos = 0;
            for (int j = 0; j < MatrixHeight; j++)
            {
                for (int i = 0; i < MatrixWidth; i++)
                {
                    if (model.current_game.map[i, j].zone_info!.targeted)
                    {
                        targeted.Add(pos);
                    }
                    pos++;
                }
            }
            targeted = targeted.OrderBy(_ => r.Next()).ToList();
            iterator = new CollectionIterator(targeted);
            meteortimer.Enabled = true; // starts the animation
        }
        private void GetMeteorFalling(object? sender, ElapsedEventArgs args)
        {
            if (iterator.GetNextItem(out int index))
            {
                TileMatrix[index].IsMeteorFalling = true;
                return;
            }
            loaded = true;
            meteortimer.Enabled = false;
            PlayButtonPulse = true;
            OnPropertyChanged(nameof(PlayButtonPulse));
        }

        private void InitializeRemainingButtons(object? sender, EventArgs e)
        {
            Packets.Clear();
            HeatMaps.Clear();

            HeatMaps.Add(new ViewModelHeatMap()
            {
                Label = "Damage",
                HeatMapCommand = this.HeatMapCommand
            });
            HeatMaps.Add(new ViewModelHeatMap()
            {
                Label = "Oxygen",
                HeatMapCommand = this.HeatMapCommand
            });
            HeatMaps.Add(new ViewModelHeatMap()
            {
                Label = "Occupancy",
                HeatMapCommand = this.HeatMapCommand
            });
            HeatMaps.Add(new ViewModelHeatMap()
            {
                Label = "Satisfaction",
                HeatMapCommand = this.HeatMapCommand
            });
            HeatMaps.Add(new ViewModelHeatMap()
            {
                Label = "Safety",
                HeatMapCommand = this.HeatMapCommand
            });
            HeatMaps.Add(new ViewModelHeatMap()
            {
                Label = "Zone",
                HeatMapCommand = this.HeatMapCommand
            });

            Packets.Add(new Packet()
            {
                Label = "Industrial Zone",
                PacketCommand = ZoneCommand,
                PacketParameter = "Industrial",
                PacketImage = new Uri(Directory.GetCurrentDirectory() + imagePaths["industrialzone_btn"])
            });
            Packets.Add(new Packet()
            {
                Label = "Residential Zone",
                PacketCommand = ZoneCommand,
                PacketParameter = "Residential",
                PacketImage = new Uri(Directory.GetCurrentDirectory() + imagePaths["residentialzone_btn"])
            });
            Packets.Add(new Packet()
            {
                Label = "Service Zone",
                PacketCommand = ZoneCommand,
                PacketParameter = "Service",
                PacketImage = new Uri(Directory.GetCurrentDirectory() + imagePaths["servicezone_btn"])
            });
            Packets.Add(new Packet()
            {
                Label = "Police Station",
                PacketCommand = BuildingCommand,
                PacketParameter = "PoliceStation",
                PacketImage = new Uri(Directory.GetCurrentDirectory() + imagePaths["policestation_btn"])
            });
            Packets.Add(new Packet()
            {
                Label = "Park",
                PacketCommand = BuildingCommand,
                PacketParameter = "Park",
                PacketImage = new Uri(Directory.GetCurrentDirectory() + imagePaths["park_btn"])
            });
            Packets.Add(new Packet()
            {
                Label = "Oxygen Generator",
                PacketCommand = BuildingCommand,
                PacketParameter = "OxygenGenerator",
                PacketImage = new Uri(Directory.GetCurrentDirectory() + imagePaths["oxygengenerator_btn"])
            });
            Packets.Add(new Packet()
            {
                Label = "Pipe",
                PacketCommand = PipeCommand,
                PacketParameter = "Pipe",
                PacketImage = new Uri(Directory.GetCurrentDirectory() + imagePaths["pipe_btn"])
            });
            Packets.Add(new Packet()
            {
                Label = "Pipe xX",
                PacketCommand = PipeCommand,
                PacketParameter = "PipeS",
                PacketImage = new Uri(Directory.GetCurrentDirectory() + imagePaths["pipes_btn"])
            });
            Packets.Add(new Packet()
            {
                Label = "Oxygen Diffuser",
                PacketCommand = BuildingCommand,
                PacketParameter = "OxygenDiffuser",
                PacketImage = new Uri(Directory.GetCurrentDirectory() + imagePaths["oxygendiffuser_btn"])
            });
            Packets.Add(new Packet()
            {
                Label = "Delete Building",
                PacketCommand = DeleteBuildingCommand,
                PacketParameter = "DeleteBuilding",
                PacketImage = new Uri(Directory.GetCurrentDirectory() + imagePaths["destroybuilding_btn"])
            });
            Packets.Add(new Packet()
            {
                Label = "Delete Zone",
                PacketCommand = DeleteZoneCommand,
                PacketParameter = "DeleteZone",
                PacketImage = new Uri(Directory.GetCurrentDirectory() + imagePaths["destroyzone_btn"])
            });
            Packets.Add(new Packet()
            {
                Label = "Delete Pipe",
                PacketCommand = DeletePipeCommand,
                PacketParameter = "Pipe",
                PacketImage = new Uri(Directory.GetCurrentDirectory() + imagePaths["destroypipe_btn"])
            });
            Packets.Add(new Packet()
            {
                Label = "Road",
                PacketCommand = BuildingCommand,
                PacketParameter = "Road",
                PacketImage = new Uri(Directory.GetCurrentDirectory() + imagePaths["road_btn"])
            });
            Packets.Add(new Packet()
            {
                Label = "Road xX",
                PacketCommand = BuildingCommand,
                PacketParameter = "RoadS",
                PacketImage = new Uri(Directory.GetCurrentDirectory() + imagePaths["roads_btn"])
            });
        }

        private void InitializeButtons()
        {
            HeatMaps.Clear();
            Packets.Clear();
            Packets.Add(new Packet()
            {
                Label = "CityHall",
                PacketCommand = BuildingCommand,
                PacketParameter = "CityHall",
                PacketImage = new Uri(Directory.GetCurrentDirectory() + imagePaths["cityhall_btn"])
            });
        }

        private void SaveMenuOpenedPause()
        {
            if (prevtimerstate == true && timer.Enabled == false)
            {
                return;
            }
            prevtimerstate = timer.Enabled;
            timer.Enabled = false;
            OnPropertyChanged(nameof(PauseImage));
        }

        private void SaveMenuClosedUnpause()
        {
            timer.Enabled = prevtimerstate;
            OnPropertyChanged(nameof(PauseImage));
            prevtimerstate = false;
        }

        public void SaveGameToPersistence()
        {
            model.SaveGame(SaveName);
            model.GetSavedGamesInfo();
            timer.Enabled = prevtimerstate;
            OnPropertyChanged(nameof(PauseImage));
            OnPropertyChanged(nameof(IsSaved));
        }

        public void OverwriteToPersistence()
        {
            model.SaveExistingGame(SaveName);
            model.GetSavedGamesInfo();
            timer.Enabled = prevtimerstate;
            OnPropertyChanged(nameof(PauseImage));
            OnPropertyChanged(nameof(IsSaved));
        }

        private void SaveGameToApp(object? param)
        {
            if (!model.current_game.city_hall_placed)
            {
                ShowMessageForUser("First You need to place a City Hall!", EventStatus.ERROR);
                return;
            }
            SaveName = param!.ToString()!;
            callAppToSaveGame?.Invoke(this, false);
            OnPropertyChanged(nameof(SaveName));
        }

        private void OverwriteGameToApp(object? param)
        {
            SaveName = param!.ToString()!;
            callAppToSaveGame?.Invoke(this, true);
            OnPropertyChanged(nameof(SaveName));
        }

        private void UpdateSavedGames(object? sender, Dictionary<string, SaveInfo> savedGames)
        {
            Saves.Clear();
            foreach (var (path, info) in savedGames)
            {
                ViewModelSavedGame viewModelSavedGame = new ViewModelSavedGame($"{FormatTime(info.time)} - {info.city_name}");
                viewModelSavedGame.LoadCommand = new DelegateCommand((param) => LoadGame(path));
                viewModelSavedGame.DeleteCommand = new DelegateCommand((param) => DeleteSave(path));
                Saves.Add(viewModelSavedGame);
            }
        }

        private object FormatTime(string time)
        {
            return String.Join('/', (time.Split('/', StringSplitOptions.RemoveEmptyEntries)).Select(x => int.Parse(x) + 1));
        }

        private void LoadGame(string path)
        {
            callAppToLoadGame?.Invoke(this, path);
        }

        public void LoadGameFromPersistence(string path)
        {
            model.LoadGame(path);
        }

        private void LoadSuccess(object? sender, string message)
        {
            model.current_game.StatusUpdate += new EventHandler<GameEventArgs>(SetViewData);
            model.current_game.LogUpdate += new EventHandler<GameEventArgs>(ChartUpdate);
            InitializeTileMatrix();
            timer.Enabled = false;
            SaveName = model.current_game.city_name;
            ResidentialTaxRate = model.current_game.residental_tax_rate;
            IndustrialTaxRate = model.current_game.industrial_tax_rate;
            ServiceTaxRate = model.current_game.service_tax_rate;
            onetime = new OneShot(cityhallplaced);
            helpercounter = 0;
            if (onetime.Call())
            {
                AfterCityHallPlaced();
            }
            SettingDefaultValues();
            RestoreChartValues();
            ChartUpdate(this, new());
            callAppToLoadSuccess?.Invoke(this, new());
        }

        private void LoadError(object? sender, string message)
        {
            model.GetSavedGamesInfo();
            PopUpTextColor = "Red";
            PopUpMessage = "Save corrupted! Deleting...";
            OnPropertyChanged(nameof(PopUpMessage));
            OnPropertyChanged(nameof(PopUpTextColor));
            callAppToShowInfo?.Invoke(this, String.Empty);
        }

        private void DeleteSave(string path)
        {
            callAppToDeleteGame?.Invoke(this, path);
        }

        public void DeleteSaveFromPersistence(string path)
        {
            model.DeleteSave(path);
        }

        private void DeleteSuccess(object? sender, string message)
        {
            model.GetSavedGamesInfo();
            PopUpTextColor = "Aqua";
            PopUpMessage = "Save deleted successfully!";
            OnPropertyChanged(nameof(PopUpMessage));
            OnPropertyChanged(nameof(PopUpTextColor));
            callAppToShowInfo?.Invoke(this, String.Empty);
        }

        private void DeleteError(object? sender, string message)
        {
            model.GetSavedGamesInfo();
            PopUpTextColor = "Red";
            PopUpMessage = "Save cannot be deleted!";
            OnPropertyChanged(nameof(PopUpMessage));
            OnPropertyChanged(nameof(PopUpTextColor));
            callAppToShowInfo?.Invoke(this, String.Empty);
        }


        private void ShowHelpToApp()
        {
            if (prevtimerstate == true && timer.Enabled == false)
            {
                return;
            }
            prevtimerstate = timer.Enabled;
            timer.Enabled = false;
            OnPropertyChanged(nameof(PauseImage));
            callAppToShowHelp?.Invoke(this, new EventArgs());
        }

        private void HelpMenuClosed()
        {
            timer.Enabled = prevtimerstate;
            OnPropertyChanged(nameof(PauseImage));
            prevtimerstate = false;
        }

        private void ZoneForDelete()
        {
            if (gameMode == GameModes.ZoneDeleting)
            {
                priorityheatmap = MapModes.None;
                gameMode = GameModes.Playing;
                IsPacketSelected = false;
                CallAllPropertyChanged();
                SetMatrixTileValues();
                return;
            }
            firstcoordx = -1;
            firstcoordy = -1;
            IsPacketSelected = true;
            gameMode = GameModes.ZoneDeleting;
            priorityheatmap = MapModes.Zone;
            CallAllPropertyChanged();
            SetMatrixTileValues();
        }

        private void BuildingForDelete()
        {
            priorityheatmap = MapModes.None;
            if (gameMode == GameModes.BuildingDeleting)
            {
                gameMode = GameModes.Playing;
                IsPacketSelected = false;
                CallAllPropertyChanged();
                SetMatrixTileValues();
                return;
            }
            IsPacketSelected = true;
            gameMode = GameModes.BuildingDeleting;
            currentzone = "";
            CallAllPropertyChanged();
            SetMatrixTileValues();
        }

        private void ZoneForPlace(object? param)
        {
            currentzone = param!.ToString()!;
            currentbuilding = "";
            if (gameMode == GameModes.ZonePlacing && currentzone == prevzone)
            {
                priorityheatmap = MapModes.None;
                gameMode = GameModes.Playing;
                prevzone = "";
                IsPacketSelected = false;
                CallAllPropertyChanged();
                SetMatrixTileValues();
                return;
            }
            firstcoordx = -1;
            firstcoordy = -1;
            prevzone = currentzone;
            gameMode = GameModes.ZonePlacing;
            priorityheatmap = MapModes.Zone;
            IsPacketSelected = true;
            CallAllPropertyChanged();
            SetMatrixTileValues();
        }

        private void BuildingForPlace(object? param)
        {
            currentbuilding = param!.ToString()!;
            if (gameMode == GameModes.BuildingPlacing && currentbuilding == prevbuilding)
            {
                priorityheatmap = MapModes.None;
                gameMode = GameModes.Playing;
                prevbuilding = "";
                IsPacketSelected = false;
                CallAllPropertyChanged();
                SetMatrixTileValues();
                return;
            }
            if (gameMode == GameModes.RoadPlacing && currentbuilding == prevbuilding)
            {
                gameMode = GameModes.Playing;
                priorityheatmap = MapModes.None;
                prevbuilding = "";
                IsPacketSelected = false;
                CallAllPropertyChanged();
                SetMatrixTileValues();
                return;
            }
            firstcoordx = -1;
            firstcoordy = -1;
            IsPacketSelected = true;
            priorityheatmap = MapModes.Zone;
            prevbuilding = currentbuilding;
            gameMode = GameModeChooser[currentbuilding];
            CallAllPropertyChanged();
            SetMatrixTileValues();
        }
        public void BackToMainMenu()
        {
            loaded = false;
            timer.Enabled = false;
            SetGameSpeed(0);
            currentheatmap = MapModes.Empty;
            priorityheatmap = MapModes.None;
            prioritymeteorheatmap = MapModes.None;
            OnPropertyChanged(nameof(SpeedImage));
            OnPropertyChanged(nameof(PauseImage));
            IsPacketSelected = true;
            SettingDefaultValues();
        }
        private void ChartUpdate(object? sender, GameEventArgs e)
        {
            OnPropertyChanged(nameof(MoneyTimeSeries));
            OnPropertyChanged(nameof(PeopleTimeSeries));
        }

        private void InitializeChart()
        {
            moneytimeseries = new ChartValues<long>(model.current_game.monthly_log.TakeLast(30).Select(x => x.money).ToList());
            peopletimeseries = new ChartValues<long>(model.current_game.monthly_log.TakeLast(30).Select(x => x.population).ToList());
        }

        public void StartNewGame()
        {
            model.NewGame("New City");
            model.current_game.StatusUpdate += new EventHandler<GameEventArgs>(SetViewData);
            model.current_game.LogUpdate += new EventHandler<GameEventArgs>(ChartUpdate);
            InitializeButtons();
            InitializeTileMatrix();
            InitializeChart();
            helpercounter = 0;
            seed = model.seed;
            onetime = new OneShot(cityhallplaced);
            SaveName = model.current_game.city_name;
            ResidentialTaxRate = model.current_game.residental_tax_rate;
            ServiceTaxRate = model.current_game.service_tax_rate;
            IndustrialTaxRate = model.current_game.industrial_tax_rate;
            SettingDefaultValues();
            ChartUpdate(this,new());
        }

        public void SettingDefaultValues()
        {
            gameMode = GameModes.Playing;
            currentheatmap = MapModes.Empty;
            priorityheatmap = MapModes.None;
            prioritymeteorheatmap = MapModes.None;
            currentbuilding = "";
            currenttile = null!;
            prevtile = new ViewModelTile();
            mainforselectedtile = null!;
            currentzone = "";
            afterdisaster = false;
            loaded = false;
            IsTileSelected = true;
            moneytimeseries = new ChartValues<long>();
            peopletimeseries = new ChartValues<long>();
            SetGameSpeed(0);
            CallAllPropertyChanged();
        }
        public void StartTimer()
        {
            loaded = true;
            timer.Enabled = true;
            OnPropertyChanged(nameof(PauseImage));
        }

        private void SetViewData(object? sender, GameEventArgs e)
        {
            if (e.status == EventStatus.GAME_OVER)
            {
                GameOverProcedure();
            }
            if (e.status == EventStatus.ERROR)
            {
                ShowMessageForUser(e.message, e.status);
            }
            if (e.update == EventUpdate.NO_UPDATE)
            {
                return;
            }
            if (model.current_game.city_hall_placed)
            {
                if (onetime!.Call())
                {
                    AfterCityHallPlaced();
                }
            }
            SetMatrixTileValues();
            CallAllPropertyChanged();
        }

        private void AfterCityHallPlaced()
        {
            gameMode = GameModes.Playing;
            priorityheatmap = MapModes.None;
            prioritymeteorheatmap = MapModes.None;
            currentheatmap = MapModes.Empty;
            IsTileSelected = true;
        }

        private void GameOverProcedure()
        {
            BackToMainMenu();

            CallAllPropertyChanged();

            callAppToShowGameOver?.Invoke(this, String.Empty);
        }

        private void ShowMessageForUser(string message = "", EventStatus e = EventStatus.PARTIAL)
        {
            PopUpMessage = message;
            switch (e)
            {
                case EventStatus.PARTIAL:
                    PopUpTextColor = "Aqua";
                    PopUpMessage = "🌐 " + PopUpMessage;
                    callAppToShowInfo?.Invoke(this, String.Empty);
                    break;
                case EventStatus.ERROR:
                    PopUpTextColor = "Red";
                    PopUpMessage = "🛑 " + PopUpMessage;
                    callAppToShowInfo?.Invoke(this, String.Empty);
                    break;

            }
            OnPropertyChanged(nameof(PopUpMessage));
            OnPropertyChanged(nameof(PopUpTextColor));
        }

        private void CallAllPropertyChanged()
        {
            OnPropertyChanged(nameof(IsTileSelected));
            OnPropertyChanged(nameof(IsPacketSelected));
            OnPropertyChanged(nameof(PacketName));
            OnPropertyChanged(nameof(SaveName));
            OnPropertyChanged(nameof(Date));
            OnPropertyChanged(nameof(Income));
            OnPropertyChanged(nameof(IncomeColor));
            OnPropertyChanged(nameof(PublicSatisfaction));
            OnPropertyChanged(nameof(Population));
            OnPropertyChanged(nameof(Money));
            OnPropertyChanged(nameof(Tax));
            OnPropertyChanged(nameof(UpKeep));
            OnPropertyChanged(nameof(SatisfactionRate));
            OnPropertyChanged(nameof(Capacity));
            OnPropertyChanged(nameof(Occupancy));
            OnPropertyChanged(nameof(IsConnected));
            OnPropertyChanged(nameof(PublicOrder));
            OnPropertyChanged(nameof(OxygenSupply));
            OnPropertyChanged(nameof(OxygenRequirement));
            OnPropertyChanged(nameof(Level));
            OnPropertyChanged(nameof(Damage));
            OnPropertyChanged(nameof(PauseImage));
            OnPropertyChanged(nameof(IsSaved));
            OnPropertyChanged(nameof(SpeedImage));
            OnPropertyChanged(nameof(ConnectedString));
            OnPropertyChanged(nameof(UpgradeEnabled));
            OnPropertyChanged(nameof(RepairEnabled));
        }

        private void NewGameToApp()
        {
            callAppToNewGame?.Invoke(this, new EventArgs());
        }

        private void ShowCreditsToApp()
        {
            callAppToShowCredits?.Invoke(this, new EventArgs());
        }

        private void SelectHeatMap(object? param)
        {
            MapModes heatmaptype = HeatMapChooser[param!.ToString()!];
            if (currentheatmap == heatmaptype)
            {
                currentheatmap = MapModes.Empty;
                SetMatrixTileValues();
                return;
            }
            currentheatmap = heatmaptype;
            SetMatrixTileValues();
        }

        private void ExitGameToApp()
        {
            callAppToExitGame?.Invoke(this, new EventArgs());
        }

        private void SetGameSpeed(int newSpeed = -1)
        {
            GameSpeed = (newSpeed == -1 ? ++GameSpeed % gameSpeeds.Count : newSpeed % gameSpeeds.Count); //3
            timer.Interval = gameSpeeds[GameSpeed].Value;
            OnPropertyChanged(nameof(SpeedImage));
        }

        private void InitializeTileMatrix()
        {
            TileMatrix.Clear();
            for (int i = 0; i < MatrixHeight; i++)
            {
                for (int j = 0; j < MatrixWidth; j++)
                {
                    TileMatrix.Add(new ViewModelTile()
                    {
                        TileImageBuildingSource = new Uri(Directory.GetCurrentDirectory() + imagePaths["empty"]),
                        TileImageOxygenSource = new Uri(Directory.GetCurrentDirectory() + imagePaths["empty"]),
                        Location = i * Scale + j,
                        OnClick = new DelegateCommand((param) => EventChooser(param)),
                    });
                }
            }
            SetMatrixTileValues();
        }
        private void SetMatrixTileValues()
        {
            int i = 0;
            int j = 0;
            for (int pos = 0; pos < MatrixWidth * MatrixHeight; pos++)
            {
                j = TileMatrix[pos].Location / Scale;
                i = TileMatrix[pos].Location % Scale;

                TileMatrix[pos].Population = model.current_game.map[i, j].zone_info!.people.Count!;
                TileMatrix[pos].PublicOrder = model.current_game.map[i, j].zone_info!.public_order;
                TileMatrix[pos].Capacity = model.current_game.map[i, j].zone_info!.max_capacity;
                TileMatrix[pos].Connected = ConnectedChooser(i, j);
                TileMatrix[pos].PublicSatisfaction = model.current_game.map[i, j].zone_info!.public_satisfaction;
                TileMatrix[pos].Level = model.current_game.map[i, j].zone_info!.level;

                TileMatrix[pos].Building = model.current_game.map[i, j].building!;
                TileMatrix[pos].Road = model.current_game.map[i, j].road!;
                TileMatrix[pos].PipeNode = model.current_game.map[i, j].pipe!;

                TileMatrix[pos].Zone = model.current_game.map[i, j].zone_type;
                TileMatrix[pos].ZoneColor = ColorChooser(TileMatrix[pos]);
                TileMatrix[pos].IsMeteorFalling = TargetedChooser(TileMatrix[pos]);
                TileMatrix[pos].Damage = DamageChooser(i, j);
                TileMatrix[pos].FilterColor = FilterColorChooserForCurrentTile(TileMatrix[pos]);

                TileMatrix[pos].OxygenRequirement = model.current_game.map[i, j].zone_info!.oxygen_requirement;
                TileMatrix[pos].OxygenSupply = model.current_game.map[i, j].zone_info!.oxygen_supply;

                TileMatrix[pos].Opacity = OpacityChooser(TileMatrix[pos]);
                TileMatrix[pos].TileImageBuildingSource = ImageChooser(TileMatrix[pos]);
                TileMatrix[pos].TileImageOxygenSource = OxygenImageChooser(TileMatrix[pos]);

                OnPropertyChanged(nameof(Date));
            }
        }

        private bool ConnectedChooser(int i, int j)
        {

            if (model.current_game.map[i, j].building is null) return model.current_game.map[i, j].zone_info!.connected;
            return model.current_game.map[i, j].building!.connected;
        }

        private int DamageChooser(int i, int j)
        {
            if (model.current_game.map[i, j].building is null) return model.current_game.map[i, j].zone_info!.damage;
            return model.current_game.map[i, j].building!.damage;
        }

        private ViewModelTile JumpToMain(int pos)
        {
            //NULL
            int location = pos;
            if (TileMatrix[pos].Building is null) return TileMatrix[pos];
            switch (TileMatrix[pos].Building.sub_building_position)
            {
                case SubBuildingPositionType.TOP_CENTER:
                    location--;
                    break;
                case SubBuildingPositionType.TOP_RIGHT:
                    location -= TileMatrix[pos].Building.building_size.Item1 - 1;
                    break;
                case SubBuildingPositionType.BOTTOM_RIGHT:
                    location -= (MatrixWidth + TileMatrix[pos].Building.building_size.Item1 - 1);
                    break;
                case SubBuildingPositionType.BOTTOM_CENTER:
                    location -= (MatrixWidth + 1);
                    break;
                case SubBuildingPositionType.BOTTOM_LEFT:
                    location -= MatrixWidth;
                    break;
            }
            return TileMatrix[location];
        }
        private string FilterColorChooserForCurrentTile(ViewModelTile tile)
        {
            if (currenttile == null) return "Transparent";
            
            if (currenttile.Building is null)
            {
                if (currenttile == tile) return "Azure";
            }
            else
            {
                if (mainforselectedtile == JumpToMain((tile.Location / Scale) * MatrixWidth + tile.Location % Scale)) return "Azure";
            }
            return "Transparent";
        }

        private Uri OxygenImageChooser(ViewModelTile tile)
        {
            if (prioritymeteorheatmap == MapModes.Empty)
            {
                return new Uri(Directory.GetCurrentDirectory() + imagePaths["empty"]);
            }
            if (currentheatmap == MapModes.Oxygen || priorityheatmap == MapModes.Oxygen || (priorityheatmap == MapModes.Zone && (currentbuilding == "OxygenDiffuser" || currentbuilding == "OxygenGenerator")))
            {
                if (tile.PipeNode != null)
                {
                    string name = "pipe_";
                    name += tile.PipeNode.connections[1] != -1 ? "1" : "0";
                    name += tile.PipeNode.connections[3] != -1 ? "1" : "0";
                    name += tile.PipeNode.connections[2] != -1 ? "1" : "0";
                    name += tile.PipeNode.connections[0] != -1 ? "1" : "0";
                    return new Uri(Directory.GetCurrentDirectory() + imagePaths[name]);
                }
            }
            return new Uri(Directory.GetCurrentDirectory() + imagePaths["empty"]);
        }

        private bool TargetedChooser(ViewModelTile tile)
        {
            if (afterdisaster) return tile.IsMeteorFalling;
            else return false;
        }

        private string ColorChooser(ViewModelTile tile)
        {
            if (prioritymeteorheatmap == MapModes.Empty)
            {
                return "Transparent";
            }
            if (priorityheatmap == MapModes.Oxygen)
            {
                return tile.Opacity switch
                {
                    0 => "Transparent",
                    < 0.21 => "Red",
                    < 0.41 => "Orange",
                    < 0.61 => "Yellow",
                    < 0.81 => "Green",
                    _ => "Lime"
                };
            }
            if (priorityheatmap == MapModes.Zone)
            {
                return ZoneColorChooser[tile.Zone];
            }
            if (priorityheatmap == MapModes.Empty)
            {
                return "Transparent";
            }
            if (currentheatmap == MapModes.Empty)
            {
                return "Transparent";
            }
            if (currentheatmap == MapModes.Zone)
            {
                return ZoneColorChooser[tile.Zone];
            }
            if (currentheatmap == MapModes.Damage)
            {
                return "Red";
            }
            return JumpToMain((tile.Location / Scale) * MatrixWidth + tile.Location % Scale).Opacity switch
            {
                0 => "Transparent",
                < 0.21 => "Red",
                < 0.41 => "Orange",
                < 0.61 => "Yellow",
                < 0.81 => "Green",
                _ => "Lime"
            };
        }

        private double OpacityChooser(ViewModelTile tile)
        {
            if (prioritymeteorheatmap == MapModes.Empty)
            {
                return 0.0;
            }
            if (priorityheatmap == MapModes.Zone)
            {
                return 0.5;
            }
            double opacity = 0.0;
            if (priorityheatmap == MapModes.Oxygen)
            {
                opacity = (double)JumpToMain((tile.Location / Scale) * MatrixWidth + tile.Location % Scale).OxygenSupply / (double)JumpToMain((tile.Location / Scale) * MatrixWidth + tile.Location % Scale).OxygenRequirement;
                return opacity > 0.81 ? 0.81 : opacity;
            }
            if (currentheatmap == MapModes.Empty) return 0.0;
            if (currentheatmap == MapModes.Oxygen)
            {
                opacity = (double)JumpToMain((tile.Location / Scale) * MatrixWidth + tile.Location % Scale).OxygenSupply / (double)JumpToMain((tile.Location / Scale) * MatrixWidth + tile.Location % Scale).OxygenRequirement;
            }
            if (currentheatmap == MapModes.Zone)
            {
                opacity = 0.5;
            }
            else if (currentheatmap == MapModes.Satisfaction && tile.PublicSatisfaction != 0)
            {
                opacity = tile.PublicSatisfaction;
            }
            else if (currentheatmap == MapModes.Safety && tile.PublicOrder != 0)
            {
                opacity = tile.PublicOrder;
            }
            else if (currentheatmap == MapModes.Occupancy && tile.Population != 0)
            {
                opacity = (double)tile.Population / (double)tile.Capacity;
            }
            else if (currentheatmap == MapModes.Damage)
            {
                if (tile.Damage > 0) opacity = 0.5;
            }
            return opacity > 0.81 ? 0.81 : (opacity < 0.21 && opacity != 0.0 ? 0.20 : opacity);
        }

        private Uri ImageChooser(ViewModelTile tile)
        {
            if (tile.Building != null)
            {
                if (tile.Building is CityHall)
                {
                    int levelselector = tile.Level > 2 ? 2 : 1;
                    switch (tile.Building.sub_building_position)
                    {
                        case SubBuildingPositionType.MAIN:
                            return new Uri(Directory.GetCurrentDirectory() + imagePaths[$"city_hall_tl_{levelselector}"]);
                        case SubBuildingPositionType.TOP_LEFT:
                            return new Uri(Directory.GetCurrentDirectory() + imagePaths[$"empty"]);
                        case SubBuildingPositionType.TOP_CENTER:
                            return new Uri(Directory.GetCurrentDirectory() + imagePaths[$"empty"]);
                        case SubBuildingPositionType.TOP_RIGHT:
                            return new Uri(Directory.GetCurrentDirectory() + imagePaths[$"city_hall_tr_{levelselector}"]);
                        case SubBuildingPositionType.BOTTOM_RIGHT:
                            return new Uri(Directory.GetCurrentDirectory() + imagePaths[$"city_hall_br_{levelselector}"]);
                        case SubBuildingPositionType.BOTTOM_CENTER:
                            return new Uri(Directory.GetCurrentDirectory() + imagePaths[$"empty"]);
                        case SubBuildingPositionType.BOTTOM_LEFT:
                            return new Uri(Directory.GetCurrentDirectory() + imagePaths[$"city_hall_bl_{levelselector}"]);
                    }
                }
                if (tile.Building is Park)
                {
                    switch (tile.Building.sub_building_position)
                    {
                        case SubBuildingPositionType.MAIN:
                            return new Uri(Directory.GetCurrentDirectory() + imagePaths["park_tl"]);
                        case SubBuildingPositionType.TOP_LEFT:
                            return new Uri(Directory.GetCurrentDirectory() + imagePaths["empty"]);
                        case SubBuildingPositionType.TOP_CENTER:
                            return new Uri(Directory.GetCurrentDirectory() + imagePaths["empty"]);
                        case SubBuildingPositionType.TOP_RIGHT:
                            return new Uri(Directory.GetCurrentDirectory() + imagePaths["park_tr"]);
                        case SubBuildingPositionType.BOTTOM_RIGHT:
                            return new Uri(Directory.GetCurrentDirectory() + imagePaths["park_br"]);
                        case SubBuildingPositionType.BOTTOM_CENTER:
                            return new Uri(Directory.GetCurrentDirectory() + imagePaths["empty"]);
                        case SubBuildingPositionType.BOTTOM_LEFT:
                            return new Uri(Directory.GetCurrentDirectory() + imagePaths["park_bl"]);
                    }
                }
                if (tile.Building is PoliceStation)
                {
                    switch (tile.Building.sub_building_position)
                    {
                        case SubBuildingPositionType.MAIN:
                            return new Uri(Directory.GetCurrentDirectory() + imagePaths["police_station_l"]);
                        case SubBuildingPositionType.TOP_LEFT:
                            return new Uri(Directory.GetCurrentDirectory() + imagePaths["empty"]);
                        case SubBuildingPositionType.TOP_CENTER:
                            return new Uri(Directory.GetCurrentDirectory() + imagePaths["empty"]);
                        case SubBuildingPositionType.TOP_RIGHT:
                            return new Uri(Directory.GetCurrentDirectory() + imagePaths["police_station_r"]);
                        case SubBuildingPositionType.BOTTOM_RIGHT:
                            return new Uri(Directory.GetCurrentDirectory() + imagePaths["empty"]);
                        case SubBuildingPositionType.BOTTOM_CENTER:
                            return new Uri(Directory.GetCurrentDirectory() + imagePaths["empty"]);
                        case SubBuildingPositionType.BOTTOM_LEFT:
                            return new Uri(Directory.GetCurrentDirectory() + imagePaths["empty"]);
                    }
                }
                if (tile.Building is OxygenGenerator)
                {
                    switch (tile.Building.sub_building_position)
                    {
                        case SubBuildingPositionType.MAIN:
                            return new Uri(Directory.GetCurrentDirectory() + imagePaths["oxygen_generator_tl"]);
                        case SubBuildingPositionType.TOP_LEFT:
                            return new Uri(Directory.GetCurrentDirectory() + imagePaths["empty"]);
                        case SubBuildingPositionType.TOP_CENTER:
                            return new Uri(Directory.GetCurrentDirectory() + imagePaths["oxygen_generator_tm"]);
                        case SubBuildingPositionType.TOP_RIGHT:
                            return new Uri(Directory.GetCurrentDirectory() + imagePaths["oxygen_generator_tr"]);
                        case SubBuildingPositionType.BOTTOM_RIGHT:
                            return new Uri(Directory.GetCurrentDirectory() + imagePaths["oxygen_generator_br"]);
                        case SubBuildingPositionType.BOTTOM_CENTER:
                            return new Uri(Directory.GetCurrentDirectory() + imagePaths["oxygen_generator_bm"]);
                        case SubBuildingPositionType.BOTTOM_LEFT:
                            return new Uri(Directory.GetCurrentDirectory() + imagePaths["oxygen_generator_bl"]);
                    }
                }
                if (tile.Building is OxygenDiffuser)
                {
                    switch (tile.Building.sub_building_position)
                    {
                        case SubBuildingPositionType.MAIN:
                            return new Uri(Directory.GetCurrentDirectory() + imagePaths["oxygen_diffuser"]);
                        case SubBuildingPositionType.TOP_LEFT:
                            return new Uri(Directory.GetCurrentDirectory() + imagePaths["empty"]);
                        case SubBuildingPositionType.TOP_CENTER:
                            return new Uri(Directory.GetCurrentDirectory() + imagePaths["empty"]);
                        case SubBuildingPositionType.TOP_RIGHT:
                            return new Uri(Directory.GetCurrentDirectory() + imagePaths["empty"]);
                        case SubBuildingPositionType.BOTTOM_RIGHT:
                            return new Uri(Directory.GetCurrentDirectory() + imagePaths["empty"]);
                        case SubBuildingPositionType.BOTTOM_CENTER:
                            return new Uri(Directory.GetCurrentDirectory() + imagePaths["empty"]);
                        case SubBuildingPositionType.BOTTOM_LEFT:
                            return new Uri(Directory.GetCurrentDirectory() + imagePaths["empty"]);
                    }
                }
            }
            if (tile.Population > 0)
            {
                int buildingnum = 1;
                switch (tile.Zone)
                {
                    case ZoneType.NONE:
                        return new Uri(Directory.GetCurrentDirectory() + imagePaths["empty"]);
                    case ZoneType.RESIDENTIAL:
                        buildingnum = NoiseFunction(seed, tile.Location, 2);
                        return new Uri(Directory.GetCurrentDirectory() + imagePaths[$"habitat_{buildingnum}_{tile.Level}"]);
                    case ZoneType.INDUSTRIAL:
                        buildingnum = NoiseFunction(seed, tile.Location, 3);
                        return new Uri(Directory.GetCurrentDirectory() + imagePaths[$"industrial_{buildingnum}_{tile.Level}"]);
                    case ZoneType.SERVICE:
                        buildingnum = NoiseFunction(seed, tile.Location, 3);
                        return new Uri(Directory.GetCurrentDirectory() + imagePaths[$"service_{buildingnum}_{tile.Level}"]);
                }
            }
            if (tile.Population == 0 && tile.Damage > 0)
            {
                int craternum = NoiseFunction(seed, tile.Location, 2);
                return new Uri(Directory.GetCurrentDirectory() + imagePaths[$"crater_{craternum}"]);
            }
            if (tile.Road != null)
            {
                string name = "road_";
                name += tile.Road.connections[1] != -1 ? "1" : "0";
                name += tile.Road.connections[3] != -1 ? "1" : "0";
                name += tile.Road.connections[2] != -1 ? "1" : "0";
                name += tile.Road.connections[0] != -1 ? "1" : "0";
                return new Uri(Directory.GetCurrentDirectory() + imagePaths[name]);
            }
            return new Uri(Directory.GetCurrentDirectory() + imagePaths["empty"]);
        }

        private void ConfigureTimer()
        {
            timer = new System.Timers.Timer(gameSpeeds[0].Value);
            timer.AutoReset = true;
            timer.Elapsed += OnGameAdvanced;

            meteortimer = new System.Timers.Timer(meteoranimationtime);
            meteortimer.AutoReset = true;
            meteortimer.Elapsed += GetMeteorFalling;

            timer.Enabled = false;
            meteortimer.Enabled = false;
        }

        private void OnGameAdvanced(object? sender, ElapsedEventArgs e)
        {
            model.current_game.GameTick(1);
            CallAllPropertyChanged();
        }

        public void ToggleTimer()
        {
            if (!loaded) return;
            if (afterdisaster)
            {
                afterdisaster = false;
                PlayButtonPulse = false;
                OnPropertyChanged(nameof(PlayButtonPulse));
                prioritymeteorheatmap = MapModes.None;
                model.current_game.RegisterCatastrophe();
            }
            timer.Enabled = !timer.Enabled;
            OnPropertyChanged(nameof(PauseImage));
            SetMatrixTileValues();
        }
        private void EventChooser(object? param)
        {
            if (!loaded) return;
            if (afterdisaster)
            {
                ShowMessageForUser("First you need to continue the Game!");
                return;
            }
            int y = ((int)param!) / Scale;
            int x = ((int)param!) % Scale;
            int pos = y * MatrixWidth + x;
            switch (gameMode)
            {
                case GameModes.Playing:

                    if (!model.current_game.city_hall_placed)
                    {
                        ShowMessageForUser("First Step is to place the City Hall!");
                        return;
                    }
                    ShowDetails(pos);
                    CallAllPropertyChanged();
                    SetMatrixTileValues();
                    break;
                case GameModes.ZonePlacing:
                    PlaceZone(x, y);
                    break;
                case GameModes.BuildingPlacing:
                    bool notexistingbuilding = currenttile is not null && currenttile.Building is null;
                    model.current_game.PlaceBuilding((x, y), BuildingTypes[currentbuilding]);
                    if(currenttile is not null && notexistingbuilding && currenttile.Building is not null)
                    {
                        prevtile = new ViewModelTile();
                        ShowDetails(pos);
                        CallAllPropertyChanged();
                        SetMatrixTileValues();
                    }
                    break;
                case GameModes.ZoneDeleting:
                    DeleteZone(x, y);
                    break;
                case GameModes.BuildingDeleting:
                    model.current_game.Delete((x, y), 0);
                    break;
                case GameModes.RoadPlacing:
                    RoadPlacing(x, y);
                    break;
                case GameModes.RoadDeleting:
                    model.current_game.Delete((x, y), 0);
                    break;
                case GameModes.PipePlacing:
                    PipePlacing(x, y);
                    break;
                case GameModes.PipeDeleting:
                    PipeDeleting(x, y);
                    break;
                case GameModes.MeteorDisaster: // just do nothing
                    break;
                default: // just do nothing
                    break;
            }
        }

        private void PipeDeleting(int x, int y)
        {
            model.current_game.Delete((x, y), -1);
        }

        private void PipePlacing(int x, int y)
        {
            if (currentpipe == "PipeS")
            {
                if (firstcoordx == -1 && firstcoordy == -1)
                {
                    firstcoordx = x;
                    firstcoordy = y;
                    return;
                }
                model.current_game.PlacePipe((firstcoordx, firstcoordy), (x, y));
                firstcoordx = -1;
                firstcoordy = -1;
                return;
            }
            model.current_game.PlacePipe((x, y), (x, y));
        }

        private void DeleteZone(int x, int y)
        {
            if (firstcoordx == -1 && firstcoordy == -1)
            {
                firstcoordx = x;
                firstcoordy = y;
                return;
            }
            model.current_game.SetZone((Math.Min(firstcoordx, x), Math.Min(firstcoordy, y)), (Math.Max(x, firstcoordx), Math.Max(y, firstcoordy)), ZoneTypes["None"]);
            firstcoordx = -1;
            firstcoordy = -1;
        }

        private void RoadPlacing(int x, int y)
        {
            if (currentbuilding == "RoadS")
            {
                if (firstcoordx == -1 && firstcoordy == -1)
                {
                    firstcoordx = x;
                    firstcoordy = y;
                    return;
                }
                model.current_game.PlaceRoad((firstcoordx, firstcoordy), (x, y));
                firstcoordx = -1;
                firstcoordy = -1;
                return;
            }
            model.current_game.PlaceRoad((x, y), (x, y));
        }

        private void PlaceZone(int x, int y)
        {
            if (firstcoordx == -1 && firstcoordy == -1)
            {
                firstcoordx = x;
                firstcoordy = y;
                return;
            }
            model.current_game.SetZone((Math.Min(firstcoordx, x), Math.Min(firstcoordy, y)), (Math.Max(x, firstcoordx), Math.Max(y, firstcoordy)), ZoneTypes[currentzone]);
            firstcoordx = -1;
            firstcoordy = -1;
        }

        private void ShowDetails(int pos)
        {
            currenttile = TileMatrix[pos];
            mainforselectedtile = JumpToMain(pos);
            if (prevtile == mainforselectedtile)
            {
                currenttile = null!;
                mainforselectedtile = null!;
                prevtile = new ViewModelTile();
                return;
            }
            prevtile = JumpToMain(pos);
        }
        internal enum MapModes
        {
            None = 0,
            Empty = 1,
            Zone = 2,
            Satisfaction = 3,
            Oxygen = 4,
            Safety = 5,
            Occupancy = 6,
            Damage = 7
        }

        internal enum GameModes
        {
            Playing = 0,
            ZonePlacing = 1,
            BuildingPlacing = 2,
            ZoneDeleting = 3,
            BuildingDeleting = 4,
            RoadPlacing = 5,
            RoadDeleting = 6,
            MeteorDisaster = 7,
            PipePlacing = 8,
            PipeDeleting = 9,
        }
        private readonly Dictionary<string, MapModes> HeatMapChooser = new Dictionary<string, MapModes>()
        {
            {"Empty",MapModes.Empty },
            {"Zone",MapModes.Zone },
            {"Satisfaction",MapModes.Satisfaction },
            {"Safety",MapModes.Safety },
            {"Occupancy",MapModes.Occupancy },
            {"Oxygen",MapModes.Oxygen },
            {"Damage",MapModes.Damage }
        };

        private readonly List<KeyValuePair<Uri, int>> gameSpeeds = new List<KeyValuePair<Uri, int>>()
        {
            new KeyValuePair<Uri, int>( new Uri(Directory.GetCurrentDirectory()+"/Resources/b_speed1.png"), 1000 ),
            new KeyValuePair<Uri, int>( new Uri(Directory.GetCurrentDirectory()+"/Resources/b_speed2.png"), 500 ),
            new KeyValuePair<Uri, int>( new Uri(Directory.GetCurrentDirectory()+"/Resources/b_speed3.png"), 200 )
        };

        private readonly Dictionary<ZoneType, string> ZoneColorChooser = new Dictionary<ZoneType, string>()
        {
            { ZoneType.RESIDENTIAL,"Green"},
            { ZoneType.INDUSTRIAL,"Gray" },
            { ZoneType.SERVICE,"Blue" },
            { ZoneType.NONE,"Transparent" }
        };

        private readonly Dictionary<string, GameModes> GameModeChooser = new Dictionary<string, GameModes>()
        {
            {"Road",GameModes.RoadPlacing},
            {"RoadS",GameModes.RoadPlacing},
            {"Pipe",GameModes.PipePlacing},
            {"PipeS",GameModes.PipePlacing},
            {"CityHall",GameModes.BuildingPlacing },
            {"Park",GameModes.BuildingPlacing },
            {"PoliceStation",GameModes.BuildingPlacing },
            {"OxygenDiffuser",GameModes.BuildingPlacing },
            {"OxygenGenerator",GameModes.BuildingPlacing},
            {"Industrial",GameModes.ZonePlacing },
            {"Residential",GameModes.ZonePlacing },
            {"Service",GameModes.ZonePlacing },
            {"None",GameModes.BuildingDeleting },
        };
        private readonly Dictionary<string, BuildingType> BuildingTypes = new Dictionary<string, BuildingType>()
        {
            {"CityHall", BuildingType.CITY_HALL},
            {"OxygenDiffuser", BuildingType.OXYGEN_DIFFUSER},
            {"OxygenGenerator", BuildingType.OXYGEN_GENERATOR},
            {"Park", BuildingType.PARK},
            {"PoliceStation", BuildingType.POLICE_STATION},
            {"Road", BuildingType.NONE},
            {"RoadS", BuildingType.NONE},
            {"Pipe", BuildingType.NONE},
            {"PipeS", BuildingType.NONE},
            {"None", BuildingType.NONE},
        };
        private readonly Dictionary<string, ZoneType> ZoneTypes = new Dictionary<string, ZoneType>()
        {
            {"Residential", ZoneType.RESIDENTIAL},
            {"Industrial", ZoneType.INDUSTRIAL },
            {"Service", ZoneType.SERVICE},
            {"None", ZoneType.NONE}
        };

        private readonly Dictionary<string, string> imagePaths = new Dictionary<string, string>()
        {
            {"city_hall_bl_1","/Resources/city_hall_BL.png" },
            {"city_hall_br_1","/Resources/city_hall_BR.png" },
            {"city_hall_tl_1","/Resources/city_hall_TL.png" },
            {"city_hall_tr_1","/Resources/city_hall_TR.png" },
            {"city_hall_bl_2","/Resources/city_hall_l2_BL.png" },
            {"city_hall_br_2","/Resources/city_hall_l2_BR.png" },
            {"city_hall_tl_2","/Resources/city_hall_l2_TL.png" },
            {"city_hall_tr_2","/Resources/city_hall_l2_TR.png" },
            {"industrial_1_1","/Resources/industrial1_l1.png" },
            {"industrial_1_2","/Resources/industrial1_l2.png" },
            {"industrial_1_3","/Resources/industrial1_l3.png" },
            {"industrial_2_1","/Resources/industrial2_l1.png" },
            {"industrial_2_2","/Resources/industrial2_l2.png" },
            {"industrial_2_3","/Resources/industrial2_l3.png" },
            {"industrial_3_1","/Resources/industrial3_l1.png" },
            {"industrial_3_2","/Resources/industrial3_l2.png" },
            {"industrial_3_3","/Resources/industrial3_l3.png" },
            {"park_bl","/Resources/park_BL.png" },
            {"park_br","/Resources/park_BR.png" },
            {"park_tl","/Resources/park_TL.png" },
            {"park_tr","/Resources/park_TR.png" },
            {"habitat_1_1","/Resources/habitat1_l1.png" },
            {"habitat_1_2","/Resources/habitat1_l2.png" },
            {"habitat_1_3","/Resources/habitat1_l3.png" },
            {"habitat_2_1","/Resources/habitat2_l1.png" },
            {"habitat_2_2","/Resources/habitat2_l2.png" },
            {"habitat_2_3","/Resources/habitat2_l3.png" },
            {"police_station_l","/Resources/police_station_L.png" },
            {"police_station_r","/Resources/police_station_R.png" },
            {"service_1_1","/Resources/service1_l1.png" },
            {"service_1_2","/Resources/service1_l2.png" },
            {"service_1_3","/Resources/service1_l3.png" },
            {"service_2_1","/Resources/service2_l1.png" },
            {"service_2_2","/Resources/service2_l2.png" },
            {"service_2_3","/Resources/service2_l3.png" },
            {"service_3_1","/Resources/service3_l1.png" },
            {"service_3_2","/Resources/service3_l2.png" },
            {"service_3_3","/Resources/service3_l3.png" },
            {"oxygen_generator_bl","/Resources/o2_generator_BL.png" },
            {"oxygen_generator_bm","/Resources/o2_generator_BM.png" },
            {"oxygen_generator_br","/Resources/o2_generator_BR.png" },
            {"oxygen_generator_tl","/Resources/o2_generator_TL.png" },
            {"oxygen_generator_tm","/Resources/o2_generator_TM.png" },
            {"oxygen_generator_tr","/Resources/o2_generator_TR.png" },
            {"oxygen_diffuser","/Resources/o2_diffuser.png" },
            {"road_1000","/Resources/road_1000.png" },
            {"road_0100","/Resources/road_0100.png" },
            {"road_0010","/Resources/road_0010.png" },
            {"road_0001","/Resources/road_0001.png" },
            {"road_0011","/Resources/road_0011.png" },
            {"road_1100","/Resources/road_1100.png" },
            {"road_1001","/Resources/road_1001.png" },
            {"road_1010","/Resources/road_1010.png" },
            {"road_0101","/Resources/road_0101.png" },
            {"road_0110","/Resources/road_0110.png" },
            {"road_1110","/Resources/road_1110.png" },
            {"road_1101","/Resources/road_1101.png" },
            {"road_0111","/Resources/road_0111.png" },
            {"road_1011","/Resources/road_1011.png" },
            {"road_0000","/Resources/road_0000.png" },
            {"road_1111","/Resources/road_1111.png" },
            {"resume","/Resources/b_resume.png" },
            {"pause","/Resources/b_pause.png" },
            {"empty","/Resources/empty.png" },
            {"cityhall_btn","/Resources/b_city_hall.png" },
            {"residentialzone_btn","/Resources/b_residential_zone.png" },
            {"industrialzone_btn","/Resources/b_industrial_zone.png" },
            {"servicezone_btn","/Resources/b_service_zone.png" },
            {"destroybuilding_btn","/Resources/b_destroy_building.png" },
            {"destroyzone_btn","/Resources/b_destroy_zone.png" },
            {"park_btn","/Resources/b_park.png" },
            {"roads_btn","/Resources/b_road_selection.png" },
            {"road_btn","/Resources/b_road_single.png" },
            {"policestation_btn","/Resources/b_police_station.png" },
            {"oxygendiffuser_btn","/Resources/b_o2_diffuser.png" },
            {"oxygengenerator_btn","/Resources/b_o2_generator.png" },
            {"pipe_btn","/Resources/b_pipe_single.png" },
            {"pipes_btn","/Resources/b_pipe_selection.png" },
            {"destroypipe_btn","/Resources/b_destroy_pipe.png" },
            {"pipe_0000","/Resources/pipe_0000.png" },
            {"pipe_0001","/Resources/pipe_0001.png" },
            {"pipe_0010","/Resources/pipe_0010.png" },
            {"pipe_0011","/Resources/pipe_0011.png" },
            {"pipe_0100","/Resources/pipe_0100.png" },
            {"pipe_0101","/Resources/pipe_0101.png" },
            {"pipe_0110","/Resources/pipe_0110.png" },
            {"pipe_0111","/Resources/pipe_0111.png" },
            {"pipe_1000","/Resources/pipe_1000.png" },
            {"pipe_1001","/Resources/pipe_1001.png" },
            {"pipe_1010","/Resources/pipe_1010.png" },
            {"pipe_1011","/Resources/pipe_1011.png" },
            {"pipe_1100","/Resources/pipe_1100.png" },
            {"pipe_1101","/Resources/pipe_1101.png" },
            {"pipe_1110","/Resources/pipe_1110.png" },
            {"pipe_1111","/Resources/pipe_1111.png" },
            {"crater_1","/Resources/crater_1.png" },
            {"crater_2","/Resources/crater_2.png" },
        };
    }
}