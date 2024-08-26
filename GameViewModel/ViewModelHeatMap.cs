namespace KFC.ViewModel {
    public class ViewModelHeatMap : ViewModelBase
    {
        public string Label { get; set; }
        public DelegateCommand? HeatMapCommand { get; set; }
        public ViewModelHeatMap() {
            Label = string.Empty;
        }
    }
}
