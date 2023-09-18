using Dalamud.Hooking;
using Dalamud.Logging;
using Dalamud.Utility.Signatures;
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
using Character = FFXIVClientStructs.FFXIV.Client.Game.Character.Character;

namespace PiPiPlugin.PluginModule
{
    public unsafe class ObjectTetherHack : IPluginModule
    {


        public static bool Enabled { get; private set; }

        static IntPtr ProcessTetherIntptr;
        delegate long ProcessTetherDelegate(Character* a1, byte a2, byte a3, long targetOID, byte a5);
        static Hook<ProcessTetherDelegate> ProcessTetherHook = null;

        static IntPtr ProcessTetherRemovalIntptr;
        delegate long ProcessTetherRemovalDelegate(Character* a1, byte a2, byte a3, byte a4, byte a5);
        static Hook<ProcessTetherRemovalDelegate> ProcessTetherRemovalHook = null;



        static long ProcessTetherDetour(Character* a1, byte a2, byte a3, long targetOID, byte a5)
        {
            try
            {
                //PluginLog.Debug($"ProcessTether:{a1->GameObject.ObjectID:X8}:{a2}:{a3}:{targetOID:X8}:{a5}");
                //if (targetOID == 0xE0000000)
                //{
                //    ScriptingProcessor.OnTetherRemoval(a1->GameObject.ObjectID, a2, a3, a5);
                //}
                //else
                //{
                //    ScriptingProcessor.OnTetherCreate(a1->GameObject.ObjectID, (uint)targetOID, a2, a3, a5);
                //}
            }
            catch (Exception e)
            {
                PluginLog.Error($"{e.Message}\n{e.StackTrace}");
            }
            return ProcessTetherHook.Original(a1, a2, a3, targetOID, a5);
        }

        static long ProcessTetherRemovalDetour(Character* a1, byte a2, byte a3, byte a4, byte a5)
        {
            try
            {
                //PluginLog.Debug($"ProcessTetherRemoval:{a1->GameObject.ObjectID:X8}:{a2}:{a3}:{a4}:{a5}");
                //ScriptingProcessor.OnTetherRemoval(a1->GameObject.ObjectID, a2, a3, a5);
            }
            catch (Exception e)
            {
                PluginLog.Error($"{e.Message}\n{e.StackTrace}");
            }
            return ProcessTetherRemovalHook.Original(a1, a2, a3, a4, a5);
        }


        public static void DrawSetting()
        {
        }

        public unsafe static void Init()
        {
            try
            {
                ProcessTetherIntptr = Service.Scanner.ScanText("E8 ?? ?? ?? ?? EB 48 41 81 FF");
                PluginLog.Debug($"ObjectTetherHack_ProcessTetherIntptr:{ProcessTetherIntptr:X8}");
                if (ProcessTetherIntptr != 0)
                {
                    ProcessTetherHook ??= Hook<ProcessTetherDelegate>.FromAddress(ProcessTetherIntptr, ProcessTetherDetour);
                    ProcessTetherHook.Enable();
                }

                ProcessTetherRemovalIntptr = Service.Scanner.ScanText("E8 ?? ?? ?? ?? EB 64 F3 0F 10 05");
                PluginLog.Debug($"ObjectTetherHack_ProcessTetherRemovalIntptr:{ProcessTetherRemovalIntptr:X8}");
                if (ProcessTetherRemovalIntptr != IntPtr.Zero)
                {
                    ProcessTetherRemovalHook ??= Hook<ProcessTetherRemovalDelegate>.FromAddress(ProcessTetherRemovalIntptr, ProcessTetherRemovalDetour);
                    ProcessTetherRemovalHook.Enable();
                }
                Enabled = true;
            }
            catch (Exception ex)
            {

                PluginLog.Error(ex.Message);
                PluginLog.Error(ex.StackTrace);
            }

        }
        public static void Dispose()
        {
            ProcessTetherHook.Disable();
            ProcessTetherHook.Dispose();
            ProcessTetherRemovalHook.Disable();
            ProcessTetherRemovalHook.Dispose();
            Enabled = false;
        }

        

        
    }

    
    
}
