using System.Windows.Controls;
using SilkySouls.ViewModels;

namespace SilkySouls.Views;

public partial class TargetTab : UserControl
{
    public TargetTab(TargetViewModel targetViewModel)
    {
        InitializeComponent();
        DataContext = targetViewModel;
    }
}