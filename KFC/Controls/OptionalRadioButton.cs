using System.Windows;
using System.Windows.Controls;

namespace KFC.Controls {
    public class OptionalRadioButton : RadioButton
    {
        #region bool IsOptional dependency property
        public static readonly DependencyProperty IsOptionalProperty =
            DependencyProperty.Register(
                name: "IsOptional",
                propertyType: typeof(bool),
                ownerType: typeof(OptionalRadioButton),
                typeMetadata: new PropertyMetadata(true,
                    (obj, args) =>
                    {
                        ((OptionalRadioButton)obj).OnIsOptionalChanged(args);
                    }));
        public bool IsOptional
        {
            get { return (bool)GetValue(IsOptionalProperty); }
            set { SetValue(IsOptionalProperty, value); }
        }
        private void OnIsOptionalChanged(DependencyPropertyChangedEventArgs args)
        {
        }
        #endregion

        protected override void OnClick()
        {
            bool? wasChecked = IsChecked;
            base.OnClick();
            if (IsOptional && wasChecked == true) IsChecked = false;
        }
    }
}
