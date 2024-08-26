using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace KFC.Controls {
    /// <summary>
    /// Interaction logic for MenuButtonRed.xaml
    /// </summary>
    public partial class MenuButtonRed : UserControl
    {
        public MenuButtonRed()
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
        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
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

        #region DependencyProperties
        private static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(
                name: "Label",
                propertyType: typeof(string),
                ownerType: typeof(MenuButtonRed),
                typeMetadata: new PropertyMetadata(null)
                );

        private static readonly DependencyProperty CommandProperty = 
            DependencyProperty.Register(
                name: "Command", 
                propertyType: typeof(ICommand), 
                ownerType: typeof(MenuButtonRed), 
                typeMetadata: new PropertyMetadata(null)
                );
       
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(
                name: "CommandParameter", 
                propertyType: typeof(object), 
                ownerType: typeof(MenuButtonRed), 
                typeMetadata: new PropertyMetadata(null)
                );
        #endregion
    }
}
