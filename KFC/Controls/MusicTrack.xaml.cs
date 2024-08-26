using System.Windows;
using System.Windows.Controls;

namespace KFC.Controls {
    /// <summary>
    /// Interaction logic for MusicTrack.xaml
    /// </summary>
    public partial class MusicTrack : UserControl
    {
        public MusicTrack()
        {
            InitializeComponent();
        }

        public string TrackName
        {
            get { return (string)GetValue(TrackNameProperty); }
            set { SetValue(TrackNameProperty, value); }
        }

        public bool IsPlaying
        {
            get { return (bool)GetValue(IsPlayingProperty); }
            set { SetValue(IsPlayingProperty, value); }
        }

        #region Dependency Properties
        public static readonly DependencyProperty IsPlayingProperty =
            DependencyProperty.Register(
                name: "IsPlaying",
                propertyType: typeof(bool),
                ownerType: typeof(MusicTrack), 
                typeMetadata: new PropertyMetadata(null)
                );

        public static readonly DependencyProperty TrackNameProperty =
            DependencyProperty.Register(
                name:"TrackName", 
                propertyType: typeof(string),
                ownerType: typeof(MusicTrack),
                typeMetadata: new PropertyMetadata(null)
                );
        #endregion
    }
}
