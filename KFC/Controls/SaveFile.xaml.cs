using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KFC.Controls {
    /// <summary>
    /// Interaction logic for SaveFile.xaml
    /// </summary>
    public partial class SaveFile : UserControl
    {
        public SaveFile()
        {
            InitializeComponent();
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
        public ICommand DeleteCommand
        {
            get { return (ICommand)GetValue(DeleteCommandProperty); }
            set { SetValue(DeleteCommandProperty, value); }
        }
        public object DeleteCommandParameter
        {
            get { return GetValue(DeleteCommandParameterProperty); }
            set { SetValue(DeleteCommandParameterProperty, value); }
        }

        #region Dependency Properties
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(
                name: "Label",
                propertyType: typeof(string),
                ownerType: typeof(SaveFile), 
                typeMetadata: new PropertyMetadata(null)
                );

        private static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(
                name: "Command",
                propertyType: typeof(ICommand),
                ownerType: typeof(SaveFile),
                typeMetadata: new PropertyMetadata(null)
                );

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(
                name: "CommandParameter",
                propertyType: typeof(object),
                ownerType: typeof(SaveFile),
                typeMetadata: new PropertyMetadata(null)
                );

        private static readonly DependencyProperty DeleteCommandProperty =
            DependencyProperty.Register(
                name: "DeleteCommand",
                propertyType: typeof(ICommand),
                ownerType: typeof(SaveFile),
                typeMetadata: new PropertyMetadata(null)
                );

        public static readonly DependencyProperty DeleteCommandParameterProperty =
            DependencyProperty.Register(
                name: "DeleteCommandParameter",
                propertyType: typeof(object),
                ownerType: typeof(SaveFile),
                typeMetadata: new PropertyMetadata(null)
                );
        #endregion

    }
}
