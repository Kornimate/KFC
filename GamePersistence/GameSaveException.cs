namespace KFC.Persistence {
    public class GameSaveException : Exception {
        string? file_name = null;

        public GameSaveException () { }

        public GameSaveException (string message) : base(message) { }

        public GameSaveException (string message, Exception inner) : base(message, inner) { }

        public GameSaveException(string message, string fname) : base(message) {
            this.file_name = fname;
        }

        public GameSaveException(string message, Exception inner, string fname) : base(message, inner) {
            this.file_name = fname;
        }
    }
}
