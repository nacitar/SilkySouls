using SilkySouls.ViewModels;

namespace SilkySouls.Views
{
    public partial class EventTab
    {
        public EventTab(EventViewModel eventViewModel)
        {
            InitializeComponent();
            DataContext = eventViewModel;
        }
    }
}
