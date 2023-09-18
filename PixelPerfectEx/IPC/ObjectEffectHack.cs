using Dalamud.Hooking;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using H.Pipes;
using Lumina.Excel.GeneratedSheets;
using PixelPerfectEx;
using PixelPerfectEx.IPC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PiPiPlugin.PluginModule
{
    public class ObjectEffectHack : IPluginModule
    {


        public static bool Enabled { get; private set; }

        private static IntPtr ObjectEffectHackHookIntptr = IntPtr.Zero;
        private static Hook<ObjectEffectHackHookDelegate> ObjectEffectHook;
        private unsafe delegate long ObjectEffectHackHookDelegate(GameObject* a1, ushort a2, ushort a3, long a4);


        private unsafe static long ObjectEffectHackHookDetour(GameObject* a1, ushort a2, ushort a3, long a4)
        {
            try
            {
                PluginLog.Debug($"{a1->ObjectID:X8}:{a2}:{a3}:{a4:X8}");
                Service.LogSender.Send(new() { Type = LogSender.LogMessageType.Debug, Message = $"ObjectEffect:{a1->ObjectID:X8}:{a2}:{a3}:{a4:X8}" });
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex.Message);
                PluginLog.Error(ex.StackTrace);
            }

            return ObjectEffectHook.Original(a1, a2, a3, a4);


        }


        public static void DrawSetting()
        {
        }

        public unsafe static void Init()
        {
            try
            {
                ObjectEffectHackHookIntptr = PixelPerfectEx.Service.Scanner.ScanText("40 53 55 56 57 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 84 24 ?? ?? ?? ?? 0F B7 FA");
                PluginLog.Debug($"ObjectEffectHack_ObjectEffectHookIntptr:{ObjectEffectHackHookIntptr:X8}|{ObjectEffectHackHookIntptr:X8}");
                if (ObjectEffectHackHookIntptr != IntPtr.Zero)
                {
                    ObjectEffectHook ??= Hook<ObjectEffectHackHookDelegate>.FromAddress(ObjectEffectHackHookIntptr, ObjectEffectHackHookDetour);
                    ObjectEffectHook.Enable();
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
            ObjectEffectHook.Disable();
            ObjectEffectHook.Dispose();
            Enabled = false;
        }

        public static void Test()
        {
            Service.LogSender.Send(new() { Type=LogSender.LogMessageType.Debug, Message="这是一个测试信息"});
        }

        
    }

    
    
}
