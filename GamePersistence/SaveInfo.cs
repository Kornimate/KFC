namespace KFC.Persistence {
    public class SaveInfo {
        public string city_name { get; private set; }
        public string time { get; private set; }

        public SaveInfo(string city_name, string time) {
            this.city_name = city_name;
            this.time = time;
        }
    }
}
