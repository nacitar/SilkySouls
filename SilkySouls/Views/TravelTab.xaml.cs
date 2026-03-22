using SilkySouls.ViewModels;

namespace SilkySouls.Views
{
    public partial class TravelTab
    {
        public TravelTab(TravelViewModel travelViewModel)
        {
            InitializeComponent();
            DataContext = travelViewModel;
        }
    }
}
