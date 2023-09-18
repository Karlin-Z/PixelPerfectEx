using Dalamud.Game;
using Dalamud.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelPerfectEx
{
    internal class PluginAddressResolver : BaseAddressResolver
    {

        internal IntPtr BaseAddress;
        public IntPtr ActionManagerPtr;
        public IntPtr PlayerTargetPtr;
        public IntPtr PlayerTargetDisPtr => ActionManagerPtr + 0x7D8;

        protected override void Setup64Bit(SigScanner scanner)
        {
            PluginLog.Verbose("===== PixelPerfectEx =====");
            BaseAddress = Process.GetCurrentProcess().MainModule.BaseAddress;
            unsafe
            {
                ActionManagerPtr = new IntPtr(FFXIVClientStructs.FFXIV.Client.Game.ActionManager.Instance());
                PluginLog.Debug($"ActionManagerPtr-{ActionManagerPtr:X}|{ActionManagerPtr.ToInt64() - BaseAddress.ToInt64():X}");
                PlayerTargetPtr = new IntPtr(FFXIVClientStructs.FFXIV.Client.Game.Control.TargetSystem.Instance()) +0x80;
                PluginLog.Debug($"PlayerTargetPtr-{PlayerTargetPtr:X}|{PlayerTargetPtr.ToInt64() - BaseAddress.ToInt64():X}");
            }
            
        }


    }
}
