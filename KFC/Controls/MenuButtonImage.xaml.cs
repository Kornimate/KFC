using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace KFC.Controls {
    /// <summary>
    /// Interaction logic for MenuButtonImage.xaml
    /// </summary>
    public partial class MenuButtonImage : UserControl
    {
        public MenuButtonImage()
        {
            InitializeComponent();
        }
        public event RoutedEventHandler Click
        {
            add => AddHandler(ButtonBase.ClickEvent, value);
            remove => RemoveHandler(ButtonBase.ClickEvent, value);
        }
        public new double Width
        {
            get { return (double)GetValue(WidthProperty); }
            set { SetValue(WidthProperty, value); }
        }
        public new double Height
        {
            get { return (double)GetValue(HeightProperty); }
            set { SetValue(HeightProperty, value); }
        }
        public ImageSource ImageSource
        {
            get { return (ImageSource)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
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
        public bool IsHighPriority
        {
            get { return (bool)GetValue(IsHighPriorityProperty); }
            set { SetValue(IsHighPriorityProperty, value); }
        }
        
        #region DependencyProperties
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register(
                name: "ImageSource", 
                propertyType: typeof(ImageSource),
                ownerType: typeof(MenuButtonImage),
                typeMetadata: new PropertyMetadata(null)
                );

        private static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(
                name: "Command",
                propertyType: typeof(ICommand),
                ownerType: typeof(MenuButtonImage),
                typeMetadata: new PropertyMetadata(null)
                );

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(
                name: "CommandParameter",
                propertyType: typeof(object),
                ownerType: typeof(MenuButtonImage),
                typeMetadata: new PropertyMetadata(null)
                );

        public static readonly DependencyProperty IsHighPriorityProperty =
            DependencyProperty.Register(
                name: "IsHighPriority",
                propertyType: typeof(bool),
                ownerType: typeof(MenuButtonImage),
                typeMetadata: new PropertyMetadata(false)
                );
        #endregion
    }
}
