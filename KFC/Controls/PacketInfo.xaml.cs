using System.Windows;
using System.Windows.Controls;

namespace KFC.Controls
{
    /// <summary>
    /// Interaction logic for PacketInfo.xaml
    /// </summary>
    public partial class PacketInfo : UserControl
    {
        public PacketInfo()
        {
            InitializeComponent();
        }

        public string PacketName
        {
            get { return (string)GetValue(PacketNameProperty); }
            set { SetValue(PacketNameProperty, value); }
        }

        public static readonly DependencyProperty PacketNameProperty =
            DependencyProperty.Register(
                name: "PacketName", 
                propertyType: typeof(string), 
                ownerType: typeof(PacketInfo), 
                typeMetadata: new PropertyMetadata(null)
                );
    }
}
