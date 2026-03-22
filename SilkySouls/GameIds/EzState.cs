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
        public static TalkCommand OpenDialog(int type, int textId, int result, int style, int unk) =>
            new(17, [type, textId, result, style, unk]);

        public static readonly TalkCommand OpenUpgrade = new(24, [0]);
        public static readonly TalkCommand LevelUp = new(31, []);
        public static readonly TalkCommand OpenAttunement = new(28, [-1, -1]);
        public static readonly TalkCommand OpenChest = new(30, []);
        public static readonly TalkCommand OpenSell = new(46, [-1, -1]);
        public static readonly TalkCommand OpenAow = new(48, []);
        
    }
}