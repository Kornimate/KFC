﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace KFC.Controls {
    /// <summary>
    /// Interaction logic for Packet.xaml
    /// </summary>
    public partial class Packet : UserControl
    {
        public Packet()
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

        public string GroupName
        {
            get { return (string)GetValue(GroupNameProperty); }
            set { SetValue(GroupNameProperty, value); }
        }

        public bool IsChecked
        {
            get { return (bool)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }
        public ImageSource ImageSource
        {
            get { return (ImageSource)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        #region DependencyProperties
        private static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(
                name: "Label",
                propertyType: typeof(string),
                ownerType: typeof(Packet),
                typeMetadata: new PropertyMetadata(null)
                );

        private static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(
                name: "Command",
                propertyType: typeof(ICommand),
                ownerType: typeof(Packet),
                typeMetadata: new PropertyMetadata(null)
                );

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(
                name: "CommandParameter",
                propertyType: typeof(object),
                ownerType: typeof(Packet),
                typeMetadata: new PropertyMetadata(null)
                );

        public static readonly DependencyProperty GroupNameProperty =
            DependencyProperty.Register(
                name: "GroupName",
                propertyType: typeof(string),
                ownerType: typeof(Packet),
                typeMetadata: new PropertyMetadata(null)
                );

        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register(
                name: "IsChecked",
                propertyType: typeof(bool),
                ownerType: typeof(Packet),
                typeMetadata: new PropertyMetadata(false)
                );

        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register(
                name: "ImageSource",
                propertyType: typeof(ImageSource),
                ownerType: typeof(Packet),
                typeMetadata: new PropertyMetadata(null)
                );
        #endregion
    }
}
