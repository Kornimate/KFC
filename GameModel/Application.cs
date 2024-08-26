using KFC.Persistence;

namespace KFC.Model {
    public class Application {
        public GameModel current_game { get; private set; }
        public int seed { get; private set; }
        public string? curr_path { get; private set; }
        public Application() {
            current_game = null!;
        }
        public void NewGame(string city_name) {
            GameInfo game_inf = new GameInfo();
            game_inf.city_name = city_name;
            game_inf.money = 70000;
            current_game = new GameModel(game_inf);
            this.seed = game_inf.seed;
            curr_path = null;
        }
        
        private GameInfo GetGameInfoFromModel() {
            GameInfo game_info = new GameInfo();
            game_info.city_name = current_game.city_name;
            game_info.money = current_game.money;
            game_info.income = current_game.income;
            game_info.upkeep = current_game.upkeep;
            game_info.time = current_game.time;
            game_info.converted_time = current_game.converted_time;
            game_info.population = current_game.population;
            game_info.map = current_game.map;
            game_info.total_public_satisfaction_rate = current_game.total_public_satisfaction;
            game_info.service_tax_rate = current_game.service_tax_rate;
            game_info.residential_tax_rate = current_game.residental_tax_rate;
            game_info.industrial_tax_rate = current_game.industrial_tax_rate;
            game_info.new_city = false;
            game_info.road_network = current_game.road_network;
            game_info.total_oxygen_requirement = current_game.total_oxygen_requirement;
            game_info.total_oxygen_supply = current_game.total_oxygen_supply;
            game_info.pipe_network = current_game.pipe_network;
            game_info.spending_log = current_game.spending_log;
            game_info.monthly_log = current_game.monthly_log;
            game_info.seed = this.seed;

            return game_info;
        }


        public event EventHandler<string>? SavingError;
        public event EventHandler<string>? SavingSuccess;
        public event EventHandler<string>? LoadingError;
        public event EventHandler<string>? LoadingSuccess;
        public event EventHandler<string>? DeleteError;
        public event EventHandler<string>? DeleteSuccess;
        public event EventHandler<Dictionary<string, SaveInfo>>? SaveDataRetrieved;


        /// <summary>
        /// Function for VM to interact with Persistence
        /// Overwrites current save, changes it's city name to <paramref name="city_name"/>
        /// Invokes <c>SavingError</c> or <c>SavingSuccess</c> event
        /// </summary>
        /// <param name="city_name"></param>
        public void SaveExistingGame(string city_name) {
            GameInfo game_info = GetGameInfoFromModel();
            game_info.city_name = city_name;

            try {
                GamePersistence.SaveGameOverwrite(curr_path!, game_info);
            } catch (GameSaveException) {
                SavingError?.Invoke(this, "Couldn't save game!");
            }
            SavingSuccess?.Invoke(this, "Game saved successfully!");
        }


        /// <summary>
        /// Function for VM to interact with Persistence
        /// Creates new save with <paramref name="city_name"/>
        /// Invokes <c>SavingError</c> or <c>SavingSuccess</c> event
        /// </summary>
        /// <param name="city_name"></param>
        public void SaveGame(string city_name)
        {
            GameInfo game_info = GetGameInfoFromModel();
            game_info.city_name = city_name;

            try {
                curr_path = GamePersistence.SaveGame(game_info);
            }
            catch (GameSaveException) {
                SavingError?.Invoke(this, "Couldn't save game!");
            }
            SavingSuccess?.Invoke(this, "Game saved successfully!");
        }


        /// <summary>
        /// Loads game on <paramref name="load_path"/>
        /// Invokes <c>LoadingError</c> or <c>LoadingSuccess</c> event
        /// </summary>
        /// <param name="load_path"></param>
        public void LoadGame(string load_path)
        {
            GameInfo game_info;
            try {
                game_info = GamePersistence.LoadSavedGame(load_path);
                seed = game_info.seed;
                current_game = new GameModel(game_info);
                curr_path = load_path;
                LoadingSuccess?.Invoke(this, "Save loaded successfully!");
            } catch (GameLoadException) {
                LoadingError?.Invoke(this, "Gamesave corrupted!");
            }
        }

        /// <summary>
        /// Deletes game on <paramref name="load_path"/>
        /// Invokes <c>DeleteError</c> or <c>DeleteSuccess</c> event
        /// </summary>
        /// <param name="save_path"></param>
        public void DeleteSave(string save_path) {
            try {
                GamePersistence.DeleteSave(save_path);
                DeleteSuccess?.Invoke(this, "Save deleted!");
            } catch (GameSaveException) {
                DeleteError?.Invoke(this, "Can't delete save!");
            }
        }

        /// <summary>
        /// Returns dictionary of saves available
        /// </summary>
        public void GetSavedGamesInfo() {
            Dictionary<string, SaveInfo> saved_games = GamePersistence.GetSavedGames();
            SaveDataRetrieved?.Invoke(this, saved_games);
        }
    }
}
