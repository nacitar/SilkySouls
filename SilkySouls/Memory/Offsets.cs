using System;
using SilkySouls.Memory;

namespace SilkySouls.memory
{
    public static class Offsets
    {
        public static class WorldChrMan
        {
            public static IntPtr Base;
            
            public enum BaseOffsets
            {
                UpdateCoordsBasePtr = 0x40,
                PlayerIns = 0x68,
                DeathCam = 0x70
            }
            
            public const int UpdateCoords = 0x28;
            
            public enum PlayerInsOffsets
            {
                CoordsPtr1 = 0x18,
                PlayerCtrl = 0x68,
                PadMan = 0x70,
                InfinitePoise = 0x2A6,
                ReadOnlyCoords = 0x2C0,
                Health = 0x3E8,
                MaxHealth = 0x3EC,
                Stamina = 0x3F8,
                
                NoDamage = 0x524,
                ChrFlags = 0x525,
                NoGoodsConsume = 0x527,
            }

            public const byte InfinitePoise = 1 << 0;
            public const byte NoDamage = 1 << 6;

            public enum ChrFlags : byte
            {
                InfiniteStam = 1 << 2,
            }
            
            public const byte NoGoodsConsume = 1 << 0;
            
            public const int ChrAnim = 0x18;
            public const int ChrAnimSpeed = 0xA8;
            
            public const int CoordsPtr2 = 0x28;
            public const int CoordsPtr3 = 0x50;
            public const int CoordsPtr4 = 0x20;

            public enum Coords
            {
                X = 0x120,
                Z = 0x124,
                Y = 0x128,
            }
        }

        public static class ChrIns
        {
            public const int Handle = 0x8;

            public const int ChrCtrl = 0x68;
            
            public const int CurrentPoise = 0x250;
            public const int MaxPoise = 0x254;
            public const int PoiseTimer = 0x25C;
            public const int Coords = 0x2C0;
            public const int Health = 0x3E8;
            public const int MaxHealth = 0x3EC;
            public const int PoisonCurrent = 0x418;
            public const int ToxicCurrent = 0x41C;
            public const int BleedCurrent = 0x420;
            public const int PoisonMax = 0x428;
            public const int ToxicMax = 0x42C;
            public const int BleedMax = 0x430;

            public static readonly BitFlag NoDamage = new(0x524, 1 << 6);
            public static readonly BitFlag NoUpdate = new(0x525, 1 << 7);

            public static readonly int[] AnimSpeed = [ChrCtrl, 0x18, 0xA8];
            public static readonly int[] NpcParam = [0x580, 0x8];

            public enum NpcParamOffsets
            {
                AuxImmunitySpEffect = 0x50,
            }
        }

        public static class DebugEventMan
        {
            public static IntPtr Base;
            public const int DisableEvents = 0xDC;
        }

        public static class DebugFlags
        {
            public static IntPtr Base;
            
            public const int NoDeath = 0x0;
            public const int OneShot = 0x1;
            public const int NoAmmoConsume = 0x4;
            public const int InfiniteCasts = 0x5;
            public const int Invisible = 0x6;
            public const int Silent = 0x7;
            public const int AllNoDeath = 0x8;
            public const int AllNoDamage = 0x9;
            public const int DisableAi = 0xD;
        }

        public static class Cam
        {
            public static IntPtr Base;
            
            public const int ChrCam = 0x60;
            public const int ChrExFollowCam = 0x60;
        }
        
        public static class GameDataMan
        {
            public static IntPtr Base;
            
            public enum GameDataOffsets
            {
                PlayerGameData = 0x10,
                Ng = 0x78,
                InGameTime = 0xA4,
            }
            
            public enum PlayerGameData
            {
                Vitality = 0x40,
                Attunement = 0x48,
                Endurance = 0x50,
                Strength = 0x58,
                Dexterity = 0x60,
                Intelligence = 0x68,
                Faith = 0x70,
                Humanity = 0x84,
                Resistance = 0x88,
                SoulLevel = 0x90,
                Souls = 0x94,
                TotalSouls = 0x98,
                EquipMagicData = 0x418,
                EquipGameData = 0x430,
            }
        }
        
        public static nint ItemGet;
        public static IntPtr ItemGetMenuMan;
        public static nint ItemDlgFunc;
        public static nint LevelUpFunc;
        public static nint RestoreCastsFunc;

        public static class FieldArea
        {
            public static IntPtr Base;
            public const int RenderPtr = 0x28;
            public const int FilterRemoval = 0x34D;
            public const int Brightness = 0x350;
        }

        public static class GameMan
        {
            public static IntPtr Base;
            public const int BonfireCoords = 0xA80;
            public const int LastBonfire = 0xB34;
        }

        public static IntPtr WarpEvent;
        public static long WarpFunc;

        public static class DamageMan
        {
            public static IntPtr Base;
            public const int HitboxFlag = 0x30;
        }
        
        public static class MenuMan
        {
            public static IntPtr Base;
            
            public enum MenuManData
            {
                LevelUpMenu = 0x8C,
                AttunementMenu = 0x94,
                BottomlessBox = 0x98,
                Warp = 0xC0,
                Feed = 0x130,
                Quitout = 0x24C,
                LoadedFlag = 0x258
            }
        }

        public static long OpenEnhanceShopWeapon;
        public static long OpenEnhanceShopArmor;
        
        public static int ShowEnhancedShopArmorOffset = -0x40;

        public enum LockedTarget
        {
            ForceActPtr = 0xAD0,
        }
        
        public const int BattleGoalId = 0x4;

        public static readonly int[] NpcThinkParam = [0xAD0, 0xC0];
        
        public const int ForceActOffset = 0x360;
        
             
        public static class EventFlagMan
        {
            public static IntPtr Base;
            public const int FlagPtr = 0x0;
            public const int WarpFlag = 0x5B;
            public const int WarpFlagBit1 = 1;
            public const int WarpFlagBit2 = 5;
            
            public const int BonfireFlags = 0x18;

            public const int QuelaagBellPtr1 = 0x40;
            public const int QuelaagBellPtr2 = 0x690;
            public const int QuelaagBellPtr3 = 0x18;
            public const int QuelaagBellBit = 8;
      

            public const int GargOffset = 0xF54;
            public const int GargBellBit = 0x1C;
            
            public enum BonfireBitFlag
            {
                OolaSanc = 13,
                Anorlondo1 = 20,
                Gwyndolin = 15,
                Parish = 8,
                SunlightAltar = 18,
                Depths = 9,
                Quelana = 21,
                AshLake = 22,
                OS = 16,
                PaintedWorld = 7,
                DukesArchives = 5,
                Vamos = 3,
                OolaTown = 12,
                OolaDungeon = 10,
                TombOfTheGiants = 6,
                Nito = 17,
                Seath = 4,
                FourKings = 19,
                Firelink = 23,
                SancGarden = 14,
                Manus = 11
            }
        }

        public static class HgDraw
        {
            public static IntPtr Base;
            public const int EzDraw = 0x58;
        }

        public static class SoloParamMan
        {
            public static IntPtr Base;
            public const int ParamResCap = 0x570;
            public const int ItemLot = 0x38;
            public const int BkhDropRateBase = 0x32C30;

            public enum BkhDropRateSlots
            {
                Nothing = 0x40,
                Bkh = 0x42,
                Bks = 0x44,
            }
        }
        
        public static class WorldAiMan
        {
            public static IntPtr Base;

            public static readonly int[] LuaGlobalTable = [0x17E8, 0x8, 0x28, 0x78];
            
            public const int WorldAiLuaManager = 0x17E8;
            public const int DLLua = 0x8;
            public const int LuaState = 0x28;
        }

        public static class EmkEventIns
        {
            public static IntPtr Base;
        }
        
        public static class Hooks
        {
            public static nint LastLockedTarget;
            public static nint AllNoDamage;
            public static nint ItemSpawn;
            public static nint Draw;
            public static nint TargetingView;
            public static nint InAirTimer;
            public static nint Keyboard;
            public static nint ControllerR2;
            public static nint ControllerL2;
            public static nint UpdateCoords;
            public static nint WarpCoords;
            public static nint LuaIfCase;
            public static nint LuaSwitchCase;
            public static nint BattleActivate;
            public static nint Emevd;
        }

        public static class Patches
        {
            public static IntPtr DrawEventPatch;
            public static IntPtr DrawSoundViewPatch;
            public static IntPtr InfiniteDurabilityPatch;
            public static IntPtr FourKingsPatch;
            public static IntPtr NoRollPatch;
            public static IntPtr QuitoutPatch;
        }

        public static class Funcs
        {
            public static nint SetEvent;
            public static nint GetEvent;
            public static nint ShopParamSave;
            public static nint OpenRegularShop;
            public static nint ProcessEmevdCommand;
            public static nint OpenAttunement;
            public static nint AttunementWindowPrep;
            public static nint GetInventoryIndexByCatAndId;

        }
    }
}