namespace SilkySouls.Models
{
    public class InfusionType(int offset, int maxUpgrade, bool limited)
    {

        public readonly int Offset = offset;
        public readonly int MaxUpgrade = maxUpgrade;
        public readonly bool Limited = limited;
    }
}