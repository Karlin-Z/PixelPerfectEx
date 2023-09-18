using Dalamud.Game.ClientState.Conditions;
using Dalamud.Hooking;
using Dalamud.Interface;
using Dalamud.Logging;
using Dalamud.Utility.Numerics;
using FFXIVClientStructs.Interop;
using ImGuiNET;
using PixelPerfectEx;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PiPiPlugin.PluginModule
{
    internal class AutoMoveHack : IPluginModule
    {
        [StructLayout(LayoutKind.Explicit, Size = 0x1000)]
        public struct MovingController
        {
            [FieldOffset(0)]
            public unsafe IntPtr VTable;
            [FieldOffset(0x30)]
            public unsafe IntPtr Heap;
            [FieldOffset(0x490)]
            public unsafe uint FollowObjectID;
            [FieldOffset(0x551)]
            public unsafe ControlType MovingMode;

            public ControlType Mode
            {
                get { return MovingMode; }
                set { MovingMode = value; }
            }
        }
        public enum ControlType
        {
            None,
            Normal,
            Casting,
            AutoForward,
            Follow
        }

        public unsafe static MovingController* MoveControl;
        public delegate void SetFacingDelegate(IntPtr actorPointer, float radian);
        public static SetFacingDelegate SetFace;

        public static bool Enabled { get; private set; }
        
        internal static ConcurrentQueue<Vector3> WayPoints = new ConcurrentQueue<Vector3>();
        private static Vector3 NextWayPoint;
        private static bool AutoMove;

        private static Vector3 DemoPos;
        private static Vector3 LastDemoPos;
        private static bool Pause { get => Service.KeyState[0x14]; } 

        public delegate void ChangeMoveModeDelegate(IntPtr a1, IntPtr a2);
        private static Hook<ChangeMoveModeDelegate> ChangeMoveModeHook;
        private static Hook<ChangeMoveModeDelegate> ChangeMoveModeHook2;
        private static void ChangeMoveModeHookDetour(IntPtr a1, IntPtr a2)
        {

            if (AutoMove && !Pause)
            {
                return;
            }
            ChangeMoveModeHook.Original(a1, a2);
        }
        private static void ChangeMoveModeHook2Detour(IntPtr a1, IntPtr a2)
        {
            if (AutoMove && !Pause)
            {
                //PluginLog.Debug($"跳过");
                return;
            }
            ChangeMoveModeHook2.Original(a1, a2);
        }

        public static void DrawSetting()
        {
        }

        public static void Init()
        {
            try
            {
                unsafe
                {
                    MoveControl = (MovingController*)(Service.Scanner.GetStaticAddressFromSig("48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 84 C0 74 2C 80 3D ?? ?? ?? ?? ??"));
                }
                PluginLog.Debug($"MoveControl {(Service.Scanner.GetStaticAddressFromSig("48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 84 C0 74 2C 80 3D ?? ?? ?? ?? ??")) - Service.Address.BaseAddress:X8}");
                SetFace = Marshal.GetDelegateForFunctionPointer<SetFacingDelegate>(Service.Scanner.ScanText("E8 ?? ?? ?? ?? 83 FE 4F"));
                Service.Framework.Update += Framework_Update;
                Service.Interface.UiBuilder.Draw += UiBuilder_Draw;


                ChangeMoveModeHook = Hook<ChangeMoveModeDelegate>.FromAddress(Service.Scanner.ScanText("E8 ?? ?? ?? ?? 4C 8D 4D 68"), ChangeMoveModeHookDetour);
                ChangeMoveModeHook.Enable();
                ChangeMoveModeHook2 = Hook<ChangeMoveModeDelegate>.FromAddress(Service.Scanner.ScanText("E8 ?? ?? ?? ?? C6 43 3E 00"), ChangeMoveModeHook2Detour);
                ChangeMoveModeHook2.Enable();

            }
            catch (Exception ex)
            {

                PluginLog.Error(ex.Message);
                PluginLog.Error(ex.StackTrace);
            }

        }

        private static void UiBuilder_Draw()
        {
            try
            {
                if (Service.Condition[ConditionFlag.DutyRecorderPlayback])
                {
                    RecorderPlaybackMoveDemo();
                }
            }
            catch (Exception ex)
            {

                PluginLog.Error(ex.Message);
                PluginLog.Error(ex.StackTrace);
            }
        }

        private static void Framework_Update(Dalamud.Game.Framework framework)
        {
            try
            {
                if (!Service.Condition[ConditionFlag.DutyRecorderPlayback])
                {
                    DealAutoMove();
                }
            }
            catch (Exception ex)
            {

                PluginLog.Error(ex.Message);
                PluginLog.Error(ex.StackTrace);
            }
        }
        private unsafe static void DealAutoMove()
        {
            if (Pause) return;
            var player = Service.ClientState.LocalPlayer;
            if (player == null) return;
            if (AutoMove && (NextWayPoint - player.Position).WithY(0).Length() < 0.15)
            {
                ObjPosHack.SetPos(player, NextWayPoint);
                if (!WayPoints.TryDequeue(out NextWayPoint))
                {
                    AutoMove = false;
                    MoveControl->Mode = ControlType.None;
                }
                    
            }
            if (!AutoMove)
            {
                if(WayPoints.TryDequeue(out NextWayPoint))
                {
                    AutoMove = true;
                }
                else
                {
                    return;
                }    
            }
            

            SetFace(Service.ClientState.LocalPlayer.Address, GetTargetPosAngle(NextWayPoint));
            MoveControl->Mode = ControlType.AutoForward;

        }
        private static void RecorderPlaybackMoveDemo()
        {
            var player = Service.ClientState.LocalPlayer;
            if (!AutoMove)
            {
                if (WayPoints.TryDequeue(out NextWayPoint))
                {
                    AutoMove = true;
                    DemoPos = player.Position;
                    LastDemoPos = player.Position;
                }
                else
                {
                    return;
                }
            }



            if (AutoMove && (NextWayPoint - DemoPos).WithY(0).Length() < 0.15)
            {
                DemoPos = NextWayPoint;
                if (!WayPoints.TryDequeue(out NextWayPoint))
                {
                    AutoMove = false;
                }
                else
                {
                    LastDemoPos = DemoPos;
                }

            }

            if (!AutoMove) return;
            if (float.IsNaN(DemoPos.X))
            {
                DemoPos = player.Position;
            }

            Vector3 offset = Vector3.Normalize(NextWayPoint - DemoPos);
            DemoPos += offset / 1000 * (float)Service.Framework.UpdateDelta.TotalMilliseconds * 6;

            ImGui.SetNextWindowSize(ImGui.GetMainViewport().Size);
            ImGuiHelpers.ForceNextWindowMainViewport();
            ImGuiHelpers.SetNextWindowPosRelativeMainViewport(Vector2.Zero);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
            if (ImGui.Begin("AutoMoveDemoDisplay", ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoBackground))
            {
                ImGui.GetBackgroundDrawList().Flags &= ~ImDrawListFlags.AntiAliasedFill;
                ImGui.GetWindowDrawList().Flags &= ~ImDrawListFlags.AntiAliasedFill;
                ImGui.GetBackgroundDrawList().Flags &= ~ImDrawListFlags.AllowVtxOffset;
                ImGui.GetWindowDrawList().Flags &= ~ImDrawListFlags.AllowVtxOffset;


                var drawList = ImGui.GetWindowDrawList();
                var col = ImGui.ColorConvertFloat4ToU32(new(1, 0, 0, 1));
                var col2 = ImGui.ColorConvertFloat4ToU32(new(0, 1, 0, 1));
                float l = 20;


                if (Service.GameGui.WorldToScreen(DemoPos, out var pos))
                {

                    drawList.PathLineToMergeDuplicate(pos - new Vector2(l, l));
                    drawList.PathLineToMergeDuplicate(pos + new Vector2(l, l));
                    drawList.PathStroke(col, ImDrawFlags.None, l / 4);
                    drawList.PathClear();
                    drawList.PathLineToMergeDuplicate(pos - new Vector2(-l, l));
                    drawList.PathLineToMergeDuplicate(pos + new Vector2(-l, l));
                    drawList.PathStroke(col, ImDrawFlags.None, l / 4);
                    drawList.PathClear();
                }


                Service.GameGui.WorldToScreen(LastDemoPos, out var pos2);
                drawList.PathLineToMergeDuplicate(pos2);
                Service.GameGui.WorldToScreen(NextWayPoint, out pos2);
                drawList.PathLineToMergeDuplicate(pos2);
                var e = WayPoints.GetEnumerator();
                while (e.MoveNext())
                {
                    Service.GameGui.WorldToScreen(e.Current, out pos2);
                    drawList.PathLineToMergeDuplicate(pos2);
                }
                drawList.PathStroke(col2, ImDrawFlags.None, l / 5);
                drawList.PathClear();
                ImGui.End();
            }
            ImGui.PopStyleVar();
        }
        public unsafe static void ClearAll()
        {
            AutoMove = false;
            WayPoints.Clear();
            MoveControl->Mode = ControlType.None;
            
        }
        public unsafe static void test()
        {
            
            MoveControl->Mode = ControlType.AutoForward;

        }
        private static float GetTargetPosAngle(Vector3 targetPos)
        {
            Vector3 playerPos = Service.ClientState.LocalPlayer?.Position ?? Vector3.Zero;
            Vector3 offset = targetPos - playerPos;
            return MathF.Atan2(offset.X, offset.Z);
        }
        public static void Dispose()
        {
            ChangeMoveModeHook.Disable();
            ChangeMoveModeHook.Dispose();
            ChangeMoveModeHook2.Disable();
            ChangeMoveModeHook2.Dispose();

            Service.Framework.Update -= Framework_Update;
            Service.Interface.UiBuilder.Draw -= UiBuilder_Draw;
            Enabled = false;
            
        }

    }
}
