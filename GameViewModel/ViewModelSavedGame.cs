namespace KFC.ViewModel {
    public class ViewModelSavedGame : ViewModelBase
    {
        public string SaveName { get; set; }
        public DelegateCommand? LoadCommand { get; set; }
        public DelegateCommand? DeleteCommand { get; set; }
        public ViewModelSavedGame(string name) {
            SaveName = name;
        }
    }
}
