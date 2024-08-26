using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace KFC.Controls {
    /// <summary>
    /// Interaction logic for Tile.xaml
    /// </summary>
    public partial class Tile : UserControl
    {
        public Tile()
        {
            InitializeComponent();
        }
        public event RoutedEventHandler Click
        {
            add => AddHandler(ButtonBase.ClickEvent, value);
            remove => RemoveHandler(ButtonBase.ClickEvent, value);
        }
        public ImageSource TileImageBuildingSource
        {
            get { return (ImageSource)GetValue(TileImageBuildingSourceProperty); }
            set { SetValue(TileImageBuildingSourceProperty, value); }
        }
        public ImageSource TileImageOxygenSource
        {
            get { return (ImageSource)GetValue(TileImageOxygenSourceProperty); }
            set { SetValue(TileImageOxygenSourceProperty, value); }
        }
        public double ZoneOpacity
        {
            get { return (double)GetValue(ZoneOpacityProperty); }
            set { SetValue(ZoneOpacityProperty, value); }
        }
        public SolidColorBrush ZoneColor
        {
            get { return (SolidColorBrush)GetValue(ZoneColorProperty); }
            set { SetValue(ZoneColorProperty, value); }
        }
        public SolidColorBrush FilterColor
        {
            get { return (SolidColorBrush)GetValue(FilterColorProperty); }
            set { SetValue(FilterColorProperty, value); }
        }
        public double FilterOpacity
        {
            get { return (double)GetValue(ZoneOpacityProperty); }
            set { SetValue(ZoneOpacityProperty, value); }
        }
        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }
        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }
        public bool IsMeteorFalling
        {
            get { return (bool)GetValue(IsMeteorFallingProperty); }
            set { SetValue(IsMeteorFallingProperty, value); }
        }

        #region DependencyProperties
        public static readonly DependencyProperty TileImageBuildingSourceProperty =
            DependencyProperty.Register(
                name:"TileImageBuildingSource", 
                propertyType: typeof(ImageSource), 
                ownerType: typeof(Tile), 
                typeMetadata: new PropertyMetadata(null)
                );               

        public static readonly DependencyProperty TileImageOxygenSourceProperty =
            DependencyProperty.Register(
                name: "TileImageOxygenSource",
                propertyType: typeof(ImageSource),
                ownerType: typeof(Tile),
                typeMetadata: new PropertyMetadata(null)
                );        

        public static readonly DependencyProperty ZoneOpacityProperty =
            DependencyProperty.Register(
                name: "ZoneOpacity", 
                propertyType: typeof(double), 
                ownerType: typeof(Tile), 
                typeMetadata: new PropertyMetadata(0.0)
                );        

        public static readonly DependencyProperty ZoneColorProperty =
            DependencyProperty.Register(
                name: "ZoneColor", 
                propertyType: typeof(SolidColorBrush), 
                ownerType: typeof(Tile),
                typeMetadata: new PropertyMetadata(Brushes.Transparent)
                );        

        public static readonly DependencyProperty FilterColorProperty =
            DependencyProperty.Register(
                name: "FilterColor",
                propertyType: typeof(SolidColorBrush),
                ownerType: typeof(Tile),
                typeMetadata: new PropertyMetadata(Brushes.Transparent)
                );
        
        private static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(
                name: "Command",
                propertyType: typeof(ICommand),
                ownerType: typeof(Tile),
                typeMetadata: new PropertyMetadata(null)
                );

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(
                name: "CommandParameter",
                propertyType: typeof(object),
                ownerType: typeof(Tile),
                typeMetadata: new PropertyMetadata(null)
                );
        public static readonly DependencyProperty IsMeteorFallingProperty =
            DependencyProperty.Register(
                name: "IsMeteorFalling", 
                propertyType: typeof(bool), 
                ownerType: typeof(Tile), 
                typeMetadata: new PropertyMetadata(false));
        #endregion
    }
}
