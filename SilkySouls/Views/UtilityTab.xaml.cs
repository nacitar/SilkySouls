using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SilkySouls.ViewModels;

namespace SilkySouls.Views
{
    public partial class UtilityTab
    {
        private string _lastValidText;

        public UtilityTab(UtilityViewModel utilityViewModel)
        {
            InitializeComponent();
            DataContext = utilityViewModel;
        }

        private void DrawInfoBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show(
                "DRAW INFORMATION\n\n" +
                "Enable Draw Instructions:\n" +
                "• You must exit to the main menu before you can view hitboxes, draw events, and other visual elements.\n\n" +
                "Known Display Issues:\n" +
                "• Temporal Anti-Aliasing (default setting) will prevent hitboxes from displaying properly.\n" +
                "• Solution: Select any other anti-aliasing option in quality settings to fix the display.",
                "Info",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }

        private void WarpLocationsCombo_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is ComboBox combo)) return;
            _lastValidText = combo.Text;

            combo.PreviewMouseDown -= WarpLocationsCombo_PreviewMouseDown;
            combo.DropDownClosed += WarpLocationsCombo_DropDownClosed;

            combo.Dispatcher.BeginInvoke(new Action(() =>
            {
                combo.IsEditable = true;
                combo.Focus();
                combo.IsDropDownOpen = true;
            }), System.Windows.Threading.DispatcherPriority.Input);

        }

        private void WarpLocationsCombo_DropDownClosed(object sender, EventArgs e)
        {
            if (!(sender is ComboBox combo)) return;

            if (string.IsNullOrWhiteSpace(combo.Text))
            {
                combo.Text = _lastValidText;
            }

            combo.IsEditable = false;
            combo.DropDownClosed -= WarpLocationsCombo_DropDownClosed;
            combo.PreviewMouseDown += WarpLocationsCombo_PreviewMouseDown;
        }
    }
}
