using SilkySouls.ViewModels;

namespace SilkySouls.Views
{
    /// <summary>
    /// Interaction logic for TargetTab.xaml
    /// </summary>
    public partial class EnemyTab
    {
        public EnemyTab(EnemyViewModel enemyViewModel)
        {
            InitializeComponent();
            DataContext = enemyViewModel;
        }
    }
}