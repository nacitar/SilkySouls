namespace SilkySouls.Models
{
    public class Item(int id, int stackSize, UpgradeType upgradeType, string name)
    {
        public int Id { get; set; } = id;
        public string Name { get; set; } = name;
        public UpgradeType UpgradeType { get; set; } = upgradeType;
        public int StackSize { get; set; } = stackSize;
        public string CategoryName { get; set; }
    }
}