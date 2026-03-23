// 

using System.IO;

namespace SilkySouls.GameIds;

public static class Emevd
{
    public class EmevdCommand(int groupId, int commandId, params object[] args)
    {
        public int GroupId { get; } = groupId;
        public int CommandId { get; } = commandId;
        public byte[] ParamData { get; } = Pack(args);

        private static byte[] Pack(object[] args)
        {
            if (args.Length == 0) return [];

            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);
            int offset = 0;

            foreach (var arg in args)
            {
                int alignment = arg is sbyte or byte ? 1 : arg is short or ushort ? 2 : 4;
                int padding = (alignment - (offset % alignment)) % alignment;
                offset += padding + alignment;

                for (int i = 0; i < padding; i++) bw.Write((byte)0);

                switch (arg)
                {
                    case sbyte v: bw.Write(v); break;
                    case byte v: bw.Write(v); break;
                    case short v: bw.Write(v); break;
                    case ushort v: bw.Write(v); break;
                    case int v: bw.Write(v); break;
                    case uint v: bw.Write(v); break;
                    case float v: bw.Write(v); break;
                }
            }

            return ms.ToArray();
        }
    }

    public static class EmevdCommands
    {
        
    }
}