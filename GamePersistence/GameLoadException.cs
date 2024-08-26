namespace KFC.Persistence {
    public class GameLoadException : Exception {
        public readonly string? file_name;

        public GameLoadException() { }

        public GameLoadException(string message) : base(message) { }

        public GameLoadException(string message, Exception inner) : base(message, inner) { }

        public GameLoadException(string message, string fname) : base(message) {
            this.file_name = fname;
        }

        public GameLoadException(string message, Exception inner, string fname) : base(message, inner) {
            this.file_name = fname;
        }
    }
}
