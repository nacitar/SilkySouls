using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SilkySouls.Memory;
using SilkySouls.ViewModels;

namespace SilkySouls.Views
{
    /// <summary>
    /// Interaction logic for UtilityTab.xaml
    /// </summary>
    public partial class UtilityTab
    {
        private readonly UtilityViewModel _utilityViewModel;
        private string _lastValidText;

        public UtilityTab(UtilityViewModel utilityViewModel)
        {
            InitializeComponent();
            _utilityViewModel = utilityViewModel;
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
        

        private void LevelUpMenu_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.ShowLevelUpMenu();
        }

        private void AttunementMenu_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.ShowAttunementMenu();
        }

        private void UpgradeWeapon_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.ShowUpgradeMenu(isWeapon: true);
        }

        private void UpgradeArmor_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.ShowUpgradeMenu(isWeapon: false);
        }
        

        private void MaleUdMerchant_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.OpenShop(GameIdsOld.ShopParams.MaleUdMerchant);
        }

        private void FemaleUdMerchant_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.OpenShop(GameIdsOld.ShopParams.FemaleUdMerchant);
        }

        private void Zena_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.OpenShop(GameIdsOld.ShopParams.Zena);
        }

        private void Patches_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.OpenShop(GameIdsOld.ShopParams.Patches);
        }

        private void Shiva_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.OpenShop(GameIdsOld.ShopParams.Shiva);
        }

        private void Griggs_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.OpenShop(GameIdsOld.ShopParams.Griggs);
        }

        private void Dusk_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.OpenShop(GameIdsOld.ShopParams.Dusk);
        }

        private void Ingward_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.OpenShop(GameIdsOld.ShopParams.Ingward);
        }

        private void Laurentius_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.OpenShop(GameIdsOld.ShopParams.Laurentius);
        }

        private void Eingyi_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.OpenShop(GameIdsOld.ShopParams.Eingyi);
        }

        private void Quelana_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.OpenShop(GameIdsOld.ShopParams.Quelana);
        }

        private void Petrus_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.OpenShop(GameIdsOld.ShopParams.Petrus);
        }

        private void Reah_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.OpenShop(GameIdsOld.ShopParams.Reah);
        }

        private void Oswald_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.OpenShop(GameIdsOld.ShopParams.Oswald);
        }

        private void Logan_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.OpenShop(GameIdsOld.ShopParams.Logan);
        }

        private void CrestfallenMerchant_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.OpenShop(GameIdsOld.ShopParams.CrestfallenMerchant);
        }

        private void Chester_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.OpenShop(GameIdsOld.ShopParams.Chester);
        }

        private void Elizabeth_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.OpenShop(GameIdsOld.ShopParams.Elizabeth);
        }

        private void Gough_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.OpenShop(GameIdsOld.ShopParams.Gough);
        }
        
        private void Feed_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.OpenFeedMenu();
        }

        private void Travel_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.OpenWarpMenu();
        }

        private void Bottomless_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.OpenBottomlessBox();
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