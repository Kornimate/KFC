using System.Windows;
using System.Windows.Controls;

namespace KFC.Controls {
    /// <summary>
    /// Interaction logic for TaxSlider.xaml
    /// </summary>
    public partial class TaxSlider : UserControl
    {
        public TaxSlider()
        {
            InitializeComponent();
        }
        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }


        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        #region DependencyProperties
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(
                name: "Label",
                propertyType: typeof(string),
                ownerType: typeof(TaxSlider), 
                typeMetadata: new PropertyMetadata(null)
                );

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                name: "Value",
                propertyType: typeof(double),
                ownerType: typeof(TaxSlider),
                typeMetadata: new PropertyMetadata(0.0)
                );
        #endregion

    }
}
