namespace SilkySouls.Memory
{
    public static class GameIdsOld
    {
        public static class EventFlags
        {
            public static readonly int[] UnlockKalameet = { 0xAB0F60, 0x71D };
            public const int GargBell = 0xA8028C;
            public const int QuelaagBell = 0xADF408;
            public const int Sens = 0xAF7AA8;
            public const int PlaceLordVessel = 0xB40E24;
            public const int DukesAfterLordVessel = 0xB28776;
            public const int NewLondoWater = 0xB100E5;
            public const int LaurentiusToFirelink = 0x4E3;
            public static readonly int[] GriggsToFirelink = {0x458, 0xA82996};
            public static readonly int[] LoganToFirelink = {0x443, 0xAF7C32};
        }

        

        public static class EmevdCommands
        {
            public static readonly int[] ReproduceObjectAnimation = { 0x7D5, 0x7 };
            public static readonly int[] DeactiveObject = { 0x7D5, 0x3 };
            public static readonly int[] DeleteMapSfx = { 0x7D6, 0x1 };

        }

        public static class EmevdCommandParams
        {
            public static readonly int[] SensDoor = { 0x16E748, 0x0 };
            public static readonly int[] DukesFogDeactiveObject = { 0x19F74E, 0x0 };
            public static readonly int[] DukesFogDeleteMapSfx = { 0x19F74F, 0x0 };
            public static readonly int[] DemonRuinsFogDeactiveObject = { 0x158A7E, 0x0 };
            public static readonly int[] DemonRuinsFogDeleteMapSfx = { 0x158A7F, 0x0 };
            public static readonly int[] NitoFogDeactiveObject = { 0x1403DE, 0x0 };
            public static readonly int[] NitoFogDeleteMapSfx = { 0x1403DF, 0x0 };
        }
    }
}