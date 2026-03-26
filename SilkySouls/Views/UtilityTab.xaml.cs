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
