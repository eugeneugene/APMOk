using APMData;
using APMOk.Code;
using System.Globalization;
using System.Windows;
using System.Windows.Input;

namespace APMOk
{
    /// <summary>
    /// Interaction logic for SetAPMCustomValueWindow.xaml
    /// </summary>
    public partial class SetAPMCustomValueWindow : Window
    {
        public APMCommandParameter APMCommandParameter { get; }

        public SetAPMCustomValueWindow(APMCommandParameter parameter)
        {
            InitializeComponent();
            APMCommandParameter = parameter;
        }

        private void ButtonOkCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            e.CanExecute = uint.TryParse(CustomValue.Text, out var val) && val > 0U && val < 255U;
        }

        private void ButtonOkCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            APMCommandParameter.ApmValue = uint.Parse(CustomValue.Text, NumberStyles.Integer, CultureInfo.CurrentCulture);
            DialogResult = true;
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            PowerSource.Content = APMCommandParameter.PowerSource switch
            {
                EPowerSource.Battery => "Batteries",
                EPowerSource.Mains => "OnMains",
                _ => "Unknown",
            };
            CustomValue.Text = APMCommandParameter.ApmValue.ToString();
        }
    }
}
