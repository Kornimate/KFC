namespace KFC.Model {
    public class GameEventArgs : EventArgs
    {
        public EventStatus status { get; set; }
        public EventUpdate update { get; set; }
        public string message { get; set; }
        public GameEventArgs()
        {
            status = EventStatus.SUCCESS;
            update = EventUpdate.NO_UPDATE;
            message = string.Empty;
        }

        public GameEventArgs(EventStatus status, EventUpdate update)
        { 
            this.status = status;
            this.update = update;
            message = string.Empty;
        }

        public GameEventArgs(EventStatus status, EventUpdate update, string message)
        {
            this.status = status;
            this.update = update;
            this.message = message;
        }
    }
}
