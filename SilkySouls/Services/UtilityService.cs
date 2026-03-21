using System;
using System.Collections.Generic;
using System.Linq;
using SilkySouls.Enums;
using SilkySouls.Interfaces;
using SilkySouls.Memory;
using SilkySouls.Utilities;
using static SilkySouls.memory.Offsets;

namespace SilkySouls.Services
{
    public class UtilityService(IMemoryService memoryService, HookManager hookManager)
    {
        private IntPtr _targetView;
        private IntPtr _draw;
        private nint _drawOrigin;
        private IntPtr _emevdCodeLoc;
        private bool _isEmevdCodeWritten;

        private readonly byte[] _drawOriginBytes = { 0x44, 0x8B, 0xC6, 0xBA, 0x16, 0x00, 0x00, 0x00 };

        private List<nint> _noClipHooks;

        internal bool EnableDraw()
        {
            if (!IsDrawOriginInitialized()) return false;
            _draw = CodeCaveOffsets.Base + CodeCaveOffsets.EnableDraw;

            var ezDraw = memoryService.FollowPointers(memoryService.Read<nint>(HgDraw.Base), new[] { HgDraw.EzDraw }, true);
            long drawFunc1 = _drawOrigin + 11 + 5 + memoryService.Read<int>((IntPtr)(_drawOrigin + 11) + 1);
            long drawFunc2 = _drawOrigin + 43 + 5 + memoryService.Read<int>((IntPtr)(_drawOrigin + 43) + 1);

            byte[] drawBytes = AsmLoader.GetAsmBytes(AsmScript.EnableDraw);
            byte[] bytes = BitConverter.GetBytes(ezDraw);
            Array.Copy(bytes, 0, drawBytes, 2, 8);
            bytes = BitConverter.GetBytes(drawFunc1);
            Array.Copy(bytes, 0, drawBytes, 26, 8);
            bytes = BitConverter.GetBytes(drawFunc2);
            Array.Copy(bytes, 0, drawBytes, 85, 8);
            bytes = BitConverter.GetBytes(drawFunc1);
            Array.Copy(bytes, 0, drawBytes, 108, 8);
            bytes = BitConverter.GetBytes(drawFunc2);
            Array.Copy(bytes, 0, drawBytes, 147, 8);
            byte[] jumpBytes = BitConverter.GetBytes((int)(_drawOrigin + 8 - (_draw.ToInt64() + 170)));
            Array.Copy(jumpBytes, 0, drawBytes, 166, 4);
            memoryService.WriteBytes(_draw, drawBytes);

            hookManager.InstallHook(_draw, _drawOrigin, _drawOriginBytes);
            return true;
        }

        private bool IsDrawOriginInitialized()
        {
            _drawOrigin = Hooks.Draw;
            var originBytes = memoryService.ReadBytes((IntPtr)_drawOrigin, 8);
            return originBytes.SequenceEqual(_drawOriginBytes);
        }

        internal void DisableDraw()
        {
            hookManager.UninstallHook(_draw);
        }

        internal void EnableHitboxView()
        {
            var hitboxAddr =
                memoryService.FollowPointers(memoryService.Read<nint>(DamageMan.Base), new[] { DamageMan.HitboxFlag }, false);
            memoryService.Write(hitboxAddr, 1);
        }

        internal void DisableHitboxView()
        {
            var hitboxAddr =
                memoryService.FollowPointers(memoryService.Read<nint>(DamageMan.Base), new[] { DamageMan.HitboxFlag }, false);
            memoryService.Write(hitboxAddr, 0);
        }

        internal void EnableSoundView()
        {
            memoryService.Write(Patches.DrawSoundViewPatch, (byte)1);
        }

        internal void DisableSoundView()
        {
            memoryService.Write(Patches.DrawSoundViewPatch, (byte)0);
        }

        public void EnableDrawEvent()
        {
            memoryService.Write(Patches.DrawEventPatch, (byte)1);
        }

        public void DisableDrawEvent()
        {
            memoryService.Write(Patches.DrawEventPatch, (byte)0);
        }

        private bool _targetViewIsInstalled;

        public void EnableTargetingView()
        {
            if (!_targetViewIsInstalled)
            {
                nint targetViewOrigin = Hooks.TargetingView;
                _targetView = CodeCaveOffsets.Base + CodeCaveOffsets.TargetView;

                byte[] targetViewBytes =
                {
                    0xC6, 0x41, 0x48, 0x02, // mov    BYTE PTR [rcx+0x48],0x2
                    0x40, 0x53, // push   rbx
                    0x48, 0x83, 0xEC, 0x20, // sub    rsp,0x20
                    0xE9,
                };

                int originOffset = (int)(targetViewOrigin + 6 -
                                         (_targetView.ToInt64() + targetViewBytes.Length + 4));
                targetViewBytes = targetViewBytes.Concat(BitConverter.GetBytes(originOffset)).ToArray();

                memoryService.WriteBytes(_targetView, targetViewBytes);
                hookManager.InstallHook(_targetView, targetViewOrigin,
                    new byte[] { 0x40, 0x53, 0x48, 0x83, 0xEC, 0x20 });

                _targetViewIsInstalled = true;
            }
            else
            {
                IntPtr valueAddr = _targetView + 3;
                memoryService.WriteBytes(valueAddr, new byte[] { 0x02 });
            }
        }

        public void DisableTargetingView()
        {
            IntPtr valueAddr = _targetView + 3;
            memoryService.WriteBytes(valueAddr, new byte[] { 0x00 });
        }

        public void ResetBools()
        {
            _targetViewIsInstalled = false;
            _isEmevdCodeWritten = false;
        }

        public void EnableNoClip()
        {
            var zDirectionAddr = CodeCaveOffsets.Base + (int)CodeCaveOffsets.NoClip.ZDirectionVariable;

            var playerCoordsBase = memoryService.FollowPointers(memoryService.Read<nint>(WorldChrMan.Base),
                new[]
                {
                    (int)WorldChrMan.BaseOffsets.UpdateCoordsBasePtr, WorldChrMan.UpdateCoords
                },
                true);

            var inAirTimerOrigin = Hooks.InAirTimer;
            IntPtr inAirTimerBlock = CodeCaveOffsets.Base + (int)CodeCaveOffsets.NoClip.InAirTimer;
            byte[] inAirTimerCodeBytes = AsmLoader.GetAsmBytes(AsmScript.NoClip_InAirTimer);

            byte[] bytes = BitConverter.GetBytes(playerCoordsBase);
            Array.Copy(bytes, 0, inAirTimerCodeBytes, 11, 8);
            bytes = BitConverter.GetBytes(3);
            Array.Copy(bytes, 0, inAirTimerCodeBytes, 24, 4);
            bytes = BitConverter.GetBytes(inAirTimerOrigin + 5 - (inAirTimerBlock.ToInt64() + 37));
            Array.Copy(bytes, 0, inAirTimerCodeBytes, 33, 4);

            memoryService.WriteBytes(inAirTimerBlock, inAirTimerCodeBytes);

            IntPtr zDirectionKbCheck = CodeCaveOffsets.Base + (int)CodeCaveOffsets.NoClip.ZDirectionKbCheck;
            var keyOrigin = Hooks.Keyboard;

            byte[] zDirectKbBytes = AsmLoader.GetAsmBytes(AsmScript.NoClip_ZDirection_KB);
            bytes = BitConverter.GetBytes(25);
            Array.Copy(bytes, 0, zDirectKbBytes, 6, 4);
            bytes = BitConverter.GetBytes(39);
            Array.Copy(bytes, 0, zDirectKbBytes, 19, 4);
            int originOffset = (int)(keyOrigin + 7 - (zDirectionKbCheck.ToInt64() + 35));
            bytes = BitConverter.GetBytes(originOffset);
            Array.Copy(bytes, 0, zDirectKbBytes, 31, 4);
            bytes = BitConverter.GetBytes(zDirectionAddr);
            Array.Copy(bytes, 0, zDirectKbBytes, 38, 8);
            originOffset = (int)(keyOrigin + 7 - (zDirectionKbCheck.ToInt64() + 62));
            bytes = BitConverter.GetBytes(originOffset);
            Array.Copy(bytes, 0, zDirectKbBytes, 58, 4);
            bytes = BitConverter.GetBytes(zDirectionAddr);
            Array.Copy(bytes, 0, zDirectKbBytes, 65, 8);
            originOffset = (int)(keyOrigin + 7 - (zDirectionKbCheck.ToInt64() + 89));
            bytes = BitConverter.GetBytes(originOffset);
            Array.Copy(bytes, 0, zDirectKbBytes, 85, 4);

            memoryService.WriteBytes(zDirectionKbCheck, zDirectKbBytes);

            IntPtr zDirectionR2Check = CodeCaveOffsets.Base + (int)CodeCaveOffsets.NoClip.ZDirectionR2Check;
            var r2Origin = Hooks.ControllerR2;

            byte[] r2Bytes = AsmLoader.GetAsmBytes(AsmScript.NoClip_ZDirection_R2);

            bytes = BitConverter.GetBytes(17);
            Array.Copy(bytes, 0, r2Bytes, 9, 4);
            bytes = BitConverter.GetBytes(zDirectionAddr);
            Array.Copy(bytes, 0, r2Bytes, 16, 8);
            originOffset = (int)(r2Origin + 5 - (zDirectionR2Check.ToInt64() + 35));
            bytes = BitConverter.GetBytes(originOffset);
            Array.Copy(bytes, 0, r2Bytes, 31, 4);

            memoryService.WriteBytes(zDirectionR2Check, r2Bytes);

            IntPtr zDirectionL2Check = CodeCaveOffsets.Base + (int)CodeCaveOffsets.NoClip.ZDirectionL2Check;
            var l2Origin = Hooks.ControllerL2;

            byte[] l2Bytes = AsmLoader.GetAsmBytes(AsmScript.NoClip_ZDirection_L2);

            bytes = BitConverter.GetBytes(17);
            Array.Copy(bytes, 0, l2Bytes, 9, 4);
            bytes = BitConverter.GetBytes(zDirectionAddr);
            Array.Copy(bytes, 0, l2Bytes, 16, 8);
            originOffset = (int)(l2Origin + 5 - (zDirectionL2Check.ToInt64() + 35));
            bytes = BitConverter.GetBytes(originOffset);
            Array.Copy(bytes, 0, l2Bytes, 31, 4);

            memoryService.WriteBytes(zDirectionL2Check, l2Bytes);

            IntPtr updateCoordsBlock = CodeCaveOffsets.Base + (int)CodeCaveOffsets.NoClip.UpdateCoords;

            var coordsPtr = memoryService.FollowPointers(memoryService.Read<nint>(WorldChrMan.Base), new[]
            {
                (int)WorldChrMan.BaseOffsets.PlayerIns,
                (int)WorldChrMan.PlayerInsOffsets.CoordsPtr1,
                WorldChrMan.CoordsPtr2,
                WorldChrMan.CoordsPtr3,
                WorldChrMan.CoordsPtr4,
            }, true);

            var updateCoordsOrigin = Hooks.UpdateCoords;
            var padManPtr = memoryService.FollowPointers(memoryService.Read<nint>(WorldChrMan.Base),
                new[]
                {
                    (int)WorldChrMan.BaseOffsets.PlayerIns,
                    (int)WorldChrMan.PlayerInsOffsets.PadMan
                }, true);

            var camPtr = memoryService.FollowPointers(memoryService.Read<nint>(Cam.Base), new[] { Cam.ChrCam, Cam.ChrExFollowCam }, true);

            byte[] updateCoordsCodeBytes = AsmLoader.GetAsmBytes(AsmScript.NoClip_UpdateCoords);

            bytes = BitConverter.GetBytes(coordsPtr);
            Array.Copy(bytes, 0, updateCoordsCodeBytes, 3, 8);
            bytes = BitConverter.GetBytes(247);
            Array.Copy(bytes, 0, updateCoordsCodeBytes, 16, 4);
            bytes = BitConverter.GetBytes(padManPtr);
            Array.Copy(bytes, 0, updateCoordsCodeBytes, 31, 8);
            bytes = BitConverter.GetBytes(camPtr);
            Array.Copy(bytes, 0, updateCoordsCodeBytes, 84, 8);
            bytes = BitConverter.GetBytes(padManPtr);
            Array.Copy(bytes, 0, updateCoordsCodeBytes, 106, 8);
            bytes = BitConverter.GetBytes(camPtr);
            Array.Copy(bytes, 0, updateCoordsCodeBytes, 159, 8);
            bytes = BitConverter.GetBytes(zDirectionAddr);
            Array.Copy(bytes, 0, updateCoordsCodeBytes, 181, 8);
            bytes = BitConverter.GetBytes(10);
            Array.Copy(bytes, 0, updateCoordsCodeBytes, 196, 4);
            bytes = BitConverter.GetBytes(14);
            Array.Copy(bytes, 0, updateCoordsCodeBytes, 206, 4);
            bytes = BitConverter.GetBytes(5);
            Array.Copy(bytes, 0, updateCoordsCodeBytes, 215, 4);
            bytes = BitConverter.GetBytes(updateCoordsOrigin + 7 - (updateCoordsBlock.ToInt64() + 267));
            Array.Copy(bytes, 0, updateCoordsCodeBytes, 263, 4);
            bytes = BitConverter.GetBytes(updateCoordsOrigin + 7 - (updateCoordsBlock.ToInt64() + 273));
            Array.Copy(bytes, 0, updateCoordsCodeBytes, 269, 4);

            memoryService.WriteBytes(updateCoordsBlock, updateCoordsCodeBytes);

            _noClipHooks = new List<nint>
            {
                hookManager.InstallHook(inAirTimerBlock, inAirTimerOrigin,
                    new byte[] { 0xF3, 0x0F, 0x58, 0x9B, 0xB0, 0x01, 0x00, 0x00 }),
                hookManager.InstallHook(zDirectionKbCheck, keyOrigin,
                    new byte[] { 0xC6, 0x43, 0xF0, 0x01, 0xC6, 0x00, 0x01 }),
                hookManager.InstallHook(zDirectionR2Check, r2Origin,
                    new byte[] { 0x0F, 0xB6, 0x44, 0x24, 0x27 }),
                hookManager.InstallHook(zDirectionL2Check, l2Origin,
                    new byte[] { 0x0F, 0xB6, 0x44, 0x24, 0x26 }),
                hookManager.InstallHook(updateCoordsBlock, updateCoordsOrigin,
                    new byte[] { 0x0F, 0x29, 0x81, 0x20, 0x01, 0x00, 0x00 })
            };
        }

        public void DisableNoClip()
        {
            for (int i = _noClipHooks.Count - 1; i >= 0; i--)
            {
                hookManager.UninstallHook(_noClipHooks[i]);
            }

            _noClipHooks.Clear();
            memoryService.WriteBytes(CodeCaveOffsets.Base + (int)CodeCaveOffsets.NoClip.ZDirectionVariable, new byte[641]);
        }

        public void ToggleFilter(bool value)
        {
            if (value)
            {
                var filterPtr = memoryService.FollowPointers(memoryService.Read<nint>(FieldArea.Base), new[]
                    { FieldArea.RenderPtr, FieldArea.FilterRemoval }, false);
                memoryService.Write(filterPtr, (byte)1);
                var brightnessPtr = memoryService.FollowPointers(memoryService.Read<nint>(FieldArea.Base), new[]
                    { FieldArea.RenderPtr, FieldArea.Brightness }, false);
                var bytes = new byte[12];
                var floatBytes = BitConverter.GetBytes(5.0f);
                Buffer.BlockCopy(floatBytes, 0, bytes, 0, 4);
                Buffer.BlockCopy(floatBytes, 0, bytes, 4, 4);
                Buffer.BlockCopy(floatBytes, 0, bytes, 8, 4);

                memoryService.WriteBytes(brightnessPtr, bytes);
            }
            else
            {
                var filterPtr = memoryService.FollowPointers(memoryService.Read<nint>(FieldArea.Base), new[]
                    { FieldArea.RenderPtr, FieldArea.FilterRemoval }, false);
                memoryService.Write(filterPtr, (byte)0);
                var brightnessPtr = memoryService.FollowPointers(memoryService.Read<nint>(FieldArea.Base), new[]
                    { FieldArea.RenderPtr, FieldArea.Brightness }, false);
                var bytes = new byte[12];
                var floatBytes = BitConverter.GetBytes(1.0f);
                Buffer.BlockCopy(floatBytes, 0, bytes, 0, 4);
                Buffer.BlockCopy(floatBytes, 0, bytes, 4, 4);
                Buffer.BlockCopy(floatBytes, 0, bytes, 8, 4);

                memoryService.WriteBytes(brightnessPtr, bytes);
            }
        }

        public void ShowMenu(MenuMan.MenuManData menuType)
        {
            var menuPtr = memoryService.FollowPointers(memoryService.Read<nint>(MenuMan.Base), new[] { (int)menuType }, false);
            memoryService.Write(menuPtr, menuType == MenuMan.MenuManData.Warp ? (byte)2 : (byte)1);
        }

        public void ShowUpgradeMenu(bool isWeapon)
        {
            byte[] upgradeBytes = AsmLoader.GetAsmBytes(AsmScript.OpenEnhanceShop);
            var playerGameData = memoryService.FollowPointers(memoryService.Read<nint>(GameDataMan.Base),
                new[] { (int)GameDataMan.GameDataOffsets.PlayerGameData }, true);
            byte[] bytes = BitConverter.GetBytes(playerGameData);
            Array.Copy(bytes, 0, upgradeBytes, 2, bytes.Length);
            bytes = BitConverter.GetBytes(isWeapon ? OpenEnhanceShopWeapon : OpenEnhanceShopArmor);
            Array.Copy(bytes, 0, upgradeBytes, 16, bytes.Length);
            memoryService.AllocateAndExecute(upgradeBytes);
        }

        public void ToggleDeathCam(bool isDeathCamEnabled) =>
            memoryService.Write(memoryService.Read<nint>(WorldChrMan.Base) + (int)WorldChrMan.BaseOffsets.DeathCam,
                isDeathCamEnabled ? (byte)1 : (byte)0);

        
        public void OpenRegularShop(ulong[] shopParams)
        {
            var openRegularShopBytes = AsmLoader.GetAsmBytes(AsmScript.OpenRegularShop);
            var bytes = BitConverter.GetBytes(shopParams[0]);
            Array.Copy(bytes, 0, openRegularShopBytes, 0x0 + 2, 8);
            bytes = BitConverter.GetBytes(shopParams[1]);
            Array.Copy(bytes, 0, openRegularShopBytes, 0xA + 2, 8);
            bytes = BitConverter.GetBytes(Funcs.ShopParamSave);
            Array.Copy(bytes, 0, openRegularShopBytes, 0x14 + 2, 8);
            bytes = BitConverter.GetBytes(Funcs.OpenRegularShop);
            Array.Copy(bytes, 0, openRegularShopBytes, 0x24 + 2, 8);
            memoryService.AllocateAndExecute(openRegularShopBytes);
        }

        public void OpenAttunement()
        {
            var codeBytes = AsmLoader.GetAsmBytes(AsmScript.OpenAttunement);
            var bytes = BitConverter.GetBytes(Funcs.AttunementWindowPrep);
            Array.Copy(bytes, 0, codeBytes, 0xE + 2, 8);
            bytes = BitConverter.GetBytes(Funcs.OpenAttunement);
            Array.Copy(bytes, 0, codeBytes, 0x22 + 2, 8);
            memoryService.AllocateAndExecute(codeBytes);
        }

        public void SetGuaranteedBkhDrop(bool setValue)
        {
            var bkhPtr = memoryService.FollowPointers(memoryService.Read<nint>(SoloParamMan.Base), new[]
            {
                SoloParamMan.ParamResCap,
                SoloParamMan.ItemLot,
                SoloParamMan.BkhDropRateBase
            }, false);

            if (setValue)
            {
                memoryService.Write(bkhPtr + (int)SoloParamMan.BkhDropRateSlots.Nothing, (byte)0);
                memoryService.Write(bkhPtr + (int)SoloParamMan.BkhDropRateSlots.Bkh, (byte)0x64);
                memoryService.Write(bkhPtr + (int)SoloParamMan.BkhDropRateSlots.Bks, (byte)0);
            }
            else
            {
                memoryService.Write(bkhPtr + (int)SoloParamMan.BkhDropRateSlots.Nothing, (byte)0x4B);
                memoryService.Write(bkhPtr + (int)SoloParamMan.BkhDropRateSlots.Bkh, (byte)0x14);
                memoryService.Write(bkhPtr + (int)SoloParamMan.BkhDropRateSlots.Bks, (byte)0x5);
            }
        }
    }
}