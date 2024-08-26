using System.Text.Json;

namespace KFC.Persistence {
    public static class GamePersistence
    {
        /// <summary>
        /// Generates file name using current <c>DateTime</c>
        /// </summary>
        /// <returns>Generated file name</returns>
        private static string GeneratePath() {
            string date = DateTime.UtcNow.ToString("yyyymmddhhmmssffff");
            return "./saves/" + date + ".json";
        }

        /// <summary>
        /// Loads saved game from given file described by its name(<paramref name="path"/>)
        /// </summary>
        /// <param name="path">
        /// Name of file to load, includes relative path, retrieve from <c>GetSavedGames()</c>
        /// </param>
        /// <returns></returns>
        /// <exception cref="GameLoadException">
        /// Thrown if file doesn't exist, or cannot be serialized <br/>
        /// If file is corrupted and exists, it gets deleted
        /// </exception>
        public static GameInfo LoadSavedGame(string path) {
            Dictionary<string, SaveInfo> saved_games = GetSavedGames();
            if(!saved_games.ContainsKey(path)) {
                throw new GameLoadException($"File {path} doesn't exist.", path);
            }

            PersistenceGameInfo persistence;
            GameInfo info;
            try {
                CheckSaveDir();
                string json_string = File.ReadAllText(path);
                persistence = (PersistenceGameInfo)Newtonsoft.Json.JsonConvert.DeserializeObject(json_string, typeof(PersistenceGameInfo), new Newtonsoft.Json.JsonSerializerSettings { TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto, PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.Objects })!;
                info = persistence.CreateGameInfo();
            } catch (Exception) {
                if(File.Exists(path)) File.Delete(path);
                saved_games.Remove(path);
                UpdateSavedGames(saved_games);
                throw new GameLoadException($"Corrupted file: {path} has been deleted", path);
            }

            return info;
        }

        /// <summary>
        /// Saves given <c>GameInfo(<paramref name="info"/>)</c> to a file, it's name is automatically generated
        /// </summary>
        /// <param name="info">GameInfo of game to save</param>
        /// <exception cref="GameSaveException">
        /// Thrown if file already exists (only occurs when files were compromised or 2 saves occur in the same milisecond)<br/>
        /// or if <c>GameInfo(<paramref name="info"/>)</c> cannot be serialized
        /// </exception>
        /// <returns>Save path generated</returns>
        public static string SaveGame (GameInfo info) {
            string path = GeneratePath();
            Dictionary<string, SaveInfo> saved_games = GetSavedGames();
            if (saved_games.ContainsKey(path)) {
                throw new GameSaveException($"File {path} already exists.", path);
            }

            SaveGameOverwrite(path, info);
            return path;
        }

        /// <summary>
        /// Saves given <c>GameInfo(<paramref name="info"/>)</c> to file(<paramref name="path"/>) overwriting it <br/>
        /// This method creates the file with given name if ite didn't exist!
        /// </summary>
        /// <param name="path">Name of file to overwrite, should be retrieved from <c>GetSavedGames()</c></param>
        /// <param name="info">GameInfo of game to save</param>
        /// <exception cref="GameSaveException">Thrown if <c>GameInfo(<paramref name="info"/>)</c> cannot be serialized</exception>
        public static void SaveGameOverwrite (string path, GameInfo info) {
            Dictionary<string, SaveInfo> saved_games = GetSavedGames();
            PersistenceGameInfo save = new PersistenceGameInfo(info);

            try {
                CheckSaveDir();
                string json_string = Newtonsoft.Json.JsonConvert.SerializeObject(save, Newtonsoft.Json.Formatting.Indented, new Newtonsoft.Json.JsonSerializerSettings { TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto, PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.Objects }); 
                File.WriteAllText(path, json_string);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException or JsonException) {
                throw new GameSaveException($"Error while saving to: {path}", path);
            }

            string time_display = $"{save.converted_time.Item1}/{save.converted_time.Item2}/{save.converted_time.Item3}";
            SaveInfo shortInfo = new SaveInfo(save.city_name, time_display);
            if(saved_games.ContainsKey(path)) {
                saved_games[path] = shortInfo;
            } else {
                saved_games.Add(path, shortInfo);
            }
            UpdateSavedGames(saved_games);
        }

        /// <summary>
        /// Deletes save specified by <c>(<paramref name="path"/>)</c>
        /// </summary>
        /// <param name="path">Path to save</param>
        public static void DeleteSave(string path) {
            Dictionary<string, SaveInfo> saved_games = GetSavedGames();
            if (!saved_games.ContainsKey(path)) {
                throw new GameLoadException($"File {path} doesn't exist.", path);
            }

            try {
                if (File.Exists(path)) File.Delete(path);
                saved_games.Remove(path);
                UpdateSavedGames(saved_games);
            } catch (Exception) {
                throw new GameSaveException($"Error while deleting save to: {path}", path);
            }
        }

        /// <summary>
        /// Retrieves short info about all saved games <br/>
        /// If save_data is corrupted it gets deleted
        /// </summary>
        /// <returns><c>Dictionary</c> where keys are file names and values are of class <c>SaveInfo</c></returns>
        public static Dictionary<string, SaveInfo> GetSavedGames() {
            string path = "./saves/save_data.json";
            Dictionary<string, SaveInfo> saved_games = new Dictionary<string, SaveInfo>();

            if(!File.Exists(path)) {
                return saved_games;
            }

            try {
                CheckSaveDir();
                string json_string = File.ReadAllText(path);
                saved_games = JsonSerializer.Deserialize<Dictionary<string, SaveInfo>>(json_string, new JsonSerializerOptions { IncludeFields = true })!;
            } catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException or JsonException) {
                if (File.Exists(path)) File.Delete(path);
            }

            return saved_games;
        }

        /// <summary>
        /// Updates saved games shortinfo data
        /// </summary>
        /// <param name="saved_games">New saved games data, retrieve via <c>GetSavedGames</c> edit, then pass to this function</param>
        /// <exception cref="GameSaveException">Thrown if save data cannot be serialized</exception>
        private static void UpdateSavedGames(Dictionary<string, SaveInfo> saved_games) {
            try {
                CheckSaveDir();
                string json_string = JsonSerializer.Serialize<Dictionary<string, SaveInfo>>(saved_games, new JsonSerializerOptions { WriteIndented = true, IncludeFields = true });
                File.WriteAllText("./saves/save_data.json", json_string);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException or JsonException) {
                throw new GameSaveException("Error while updating save data.");
            }
        }

        /// <summary>
        /// Generates save directory if it doesn't exist.
        /// </summary>
        private static void CheckSaveDir() {
            string dir = "./saves";
            if (!Directory.Exists(dir)) {
                Directory.CreateDirectory(dir);
            }
        }
    }
}
