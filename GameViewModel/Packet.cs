namespace KFC.ViewModel {
    public class Packet : ViewModelBase
    {
        public string Label { get; set; }
        public DelegateCommand? PacketCommand { get; set; }
        public string PacketParameter { get; set; }
        public Uri PacketImage { get; set; }
        public Packet() {
            Label = string.Empty;
            PacketParameter = string.Empty;
            PacketImage = new(Directory.GetCurrentDirectory() + "/Resources/empty.png");
        }
    }
}
