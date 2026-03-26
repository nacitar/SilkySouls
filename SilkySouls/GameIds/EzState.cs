// 

namespace SilkySouls.GameIds;

public static class EzState
{
    public class TalkCommand(int commandId, int[] @params)
    {
        public int CommandId { get; } = commandId;
        public int[] Params { get; } = @params;
    }

    public static class TalkCommands
    {
        public static TalkCommand OpenRegularShop(int start, int end) => new(22, [start, end]);
        public static readonly TalkCommand OpenRepairShop = new(23, []);
        public static readonly TalkCommand OpenEnhanceWeapon = new(24, [0]);
        public static readonly TalkCommand OpenEnhanceArmor = new(24, [10]);
        public static readonly TalkCommand OpenAttunement = new(28, [10000, 10099]);
        public static readonly TalkCommand OpenSell = new(46, [-1, -1]);
        
    }
}