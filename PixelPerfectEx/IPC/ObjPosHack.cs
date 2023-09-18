
using Dalamud.Hooking;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.Graphics;
using ImGuiNET;
using PixelPerfectEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;


namespace PiPiPlugin.PluginModule
{
    [RequiresPreviewFeatures]
    internal class ObjPosHack : IPluginModule
    {


        public static bool Enabled { get;private set; }
        private static IntPtr updatePosIntptr=IntPtr.Zero;
        private static Hook<SetPosDelegate> setPosHook;
        private delegate IntPtr SetPosDelegate(IntPtr address, float x, float y, float z);
        private static IntPtr SetPosDetour(IntPtr address, float x, float y, float z)
        {
            return setPosHook.Original(address, x, y, z);
        }

        public static void DrawSetting()
        {
        }

        public static void Init()
        {
            try
            {
                updatePosIntptr = Service.Scanner.ScanText("40 53 48 83 EC ?? F3 0F 11 89 ?? ?? ?? ?? 48 8B D9 F3 0F 11 91 ?? ?? ?? ??");
                PluginLog.Debug($"ObjPosHack_UpdatePosIntptr:{updatePosIntptr:X8}|{updatePosIntptr.ToInt64() - Service.Address.BaseAddress.ToInt64():X8}");
                if (updatePosIntptr!=IntPtr.Zero)
                {
                    setPosHook ??= Hook<SetPosDelegate>.FromAddress(updatePosIntptr, SetPosDetour);
                    setPosHook.Enable();
                    Enabled = true;
                }
                
            }
            catch (Exception ex)
            {

                PluginLog.Error(ex.Message);
                PluginLog.Error(ex.StackTrace);
            }
            
        }
        public static void Dispose()
        {
            setPosHook.Disable();
            setPosHook.Dispose();
            Enabled = false;
        }
        public static void SetPos(Dalamud.Game.ClientState.Objects.Types.GameObject gameObject, System.Numerics.Vector3 pos)
        {
            setPosHook.Original(gameObject.Address, pos.X, pos.Y, pos.Z);

        }
    }
}
