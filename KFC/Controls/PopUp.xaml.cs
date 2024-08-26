using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace KFC.Controls {
    /// <summary>
    /// Interaction logic for PopUp.xaml
    /// </summary>
    public partial class PopUp : UserControl
    {
        public PopUp()
        {
            InitializeComponent();
        }
        public event RoutedEventHandler Click
        {
            add => AddHandler(ButtonBase.ClickEvent, value);
            remove => RemoveHandler(ButtonBase.ClickEvent, value);
        }
        public string PopUpMessage
        {
            get { return (string)GetValue(PopUpMessageProperty); }
            set { SetValue(PopUpMessageProperty, value); }
        }
        public SolidColorBrush PopUpTextColor
        {
            get { return (SolidColorBrush)GetValue(PopUpTextColorProperty); }
            set { SetValue(PopUpTextColorProperty, value); }
        }

        #region DependencyProperties
        public static readonly DependencyProperty PopUpMessageProperty =
            DependencyProperty.Register(
                name: "PopUpMessage", 
                propertyType: typeof(string),
                ownerType: typeof(PopUp),
                typeMetadata: new PropertyMetadata(null)
                );

        public static readonly DependencyProperty PopUpTextColorProperty =
            DependencyProperty.Register(
                name: "PopUpTextColor",
                propertyType: typeof(SolidColorBrush),
                ownerType: typeof(PopUp),
                typeMetadata: new PropertyMetadata(Brushes.Transparent)
                );
        #endregion
    }
}
