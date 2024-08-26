using System.Windows;
using System.Windows.Controls;

namespace KFC.Controls {
    /// <summary>
    /// Interaction logic for FinanceData.xaml
    /// </summary>
    public partial class FinanceData : UserControl
    {
        public FinanceData()
        {
            InitializeComponent();
        }
        public string FinanceName
        {
            get { return (string)GetValue(FinanceNameProperty); }
            set { SetValue(FinanceNameProperty, value); }
        }
        public int FinanceCost
        {
            get { return (int)GetValue(FinanceCostProperty); }
            set { SetValue(FinanceCostProperty, value); }
        }
        public string FinanceTime
        {
            get { return (string)GetValue(FinanceTimeProperty); }
            set { SetValue(FinanceTimeProperty, value); }
        }

        #region Dependency Properties
        public static readonly DependencyProperty FinanceNameProperty =
            DependencyProperty.Register(
                name: "FinanceName", 
                propertyType: typeof(string),
                ownerType: typeof(FinanceData),
                typeMetadata: new PropertyMetadata(null)
                );   

        public static readonly DependencyProperty FinanceCostProperty =
            DependencyProperty.Register(
                name: "FinanceCost", 
                propertyType: typeof(int),
                ownerType: typeof(FinanceData),
                typeMetadata: new PropertyMetadata(0)
                );

        public static readonly DependencyProperty FinanceTimeProperty =
            DependencyProperty.Register(
                name: "FinanceTime", 
                propertyType: typeof(string), 
                ownerType: typeof(FinanceData),
                typeMetadata: new PropertyMetadata(null)
                );

        #endregion
    }
}
