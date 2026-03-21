namespace SilkySouls.Models
{
    public class EquippedWeapon(string displayName, int slotOffset)
    {
        public string DisplayName { get; set; } = displayName;
        public int SlotOffset { get; set; } = slotOffset;
    }
}