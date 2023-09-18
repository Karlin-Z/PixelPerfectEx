using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Network;
using Dalamud.Hooking;
using Dalamud.Interface.Animation;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;
using PixelPerfectEx;
using SharpDX;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static PiPiPlugin.PluginModule.AutoMoveHack;

namespace PiPiPlugin.PluginModule
{
    internal class ObjectFacingHack : IPluginModule
    {


        public static bool Enabled { get; private set; }
        private static bool OwnFaceLocked = false;
        private static float Face;


        private static Hook<SetObjectFacingOneCastDelegate> SetObjectFacingOnCastHook;
        private delegate long SetObjectFacingOneCastDelegate(IntPtr objPointer, IntPtr facing, IntPtr targetId);

        
        

       

        //posUp_op1 UpdatePositionInstance
        //posUp_op2 UpdatePositionHandler
        private const short posUp_op1 = 0x0DE;
        private const short posUp_op2 = 0x08E;
        private static IntPtr ZoneUpA1 = IntPtr.Zero;
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate byte ProcessZonePacketUpDelegate(IntPtr a1, IntPtr dataPtr, IntPtr a3, byte a4);
        private static Hook<ProcessZonePacketUpDelegate> ProcessZonePacketUpHook;



        private static long SetObjectFacingOnCastDetour(IntPtr pointerA1, IntPtr dPosPtr, IntPtr targetId)
        {
            var rst = SetObjectFacingOnCastHook.Original(pointerA1, dPosPtr, targetId);
            if (OwnFaceLocked)
            {
                SendFace(Face);

            }

            return rst;
        }
        private static byte ProcessZonePacketUpDetour(IntPtr a1, IntPtr dataPtr, IntPtr a3, byte a4)
        {
            try
            {
                var opCode = (ushort)Marshal.ReadInt16(dataPtr);
                if (opCode == posUp_op1 || opCode == posUp_op2)
                {
                    if (a4 == 1)
                    {
                        PosPackge.Arg1 = Marshal.PtrToStructure<byte>(dataPtr + 32 + 9);
                        PosPackge.Arg2 = Marshal.PtrToStructure<byte>(dataPtr + 32 + 10);
                    }
                    if (OwnFaceLocked && opCode == posUp_op1)
                    {
                        Marshal.StructureToPtr<float>(Face, dataPtr + 32, true);
                        Marshal.StructureToPtr<float>(Face, dataPtr + 36, true);
                    }
                    //if (opCode == posUp_op1)
                    //{
                    //    var pkg = Marshal.PtrToStructure<PostionPackge>(dataPtr);
                    //    PluginLog.Debug($"{pkg}");
                    //}
                }
                if (ZoneUpA1 == IntPtr.Zero)
                {
                    ZoneUpA1 = a1;
                }
                
                
                
            }
            catch (Exception ex)
            {
                PluginLog.Error($"{ex.Message}\n{ex.StackTrace}");
            }

            return ProcessZonePacketUpHook.Original(a1, dataPtr, a3, a4);
        }
        static Random Random = new Random();

        private static void SendFace(float face)
        {
            if (Service.ClientState.LocalPlayer is not null)
            {
                SendPosToServer(face, Service.ClientState.LocalPlayer.Position);
            }
            
        }
        [StructLayout(LayoutKind.Explicit, Size = 72)]
        public struct PostionPackge
        {
            [FieldOffset(0)]
            public unsafe short OpCode;
            [FieldOffset(8)]
            public unsafe int AdjustLength;
            [FieldOffset(32)]
            public unsafe float Face;
            [FieldOffset(36)]
            public unsafe float Face2;
            [FieldOffset(41)]
            public unsafe byte Arg1;
            [FieldOffset(42)]
            public unsafe byte Arg2;
            [FieldOffset(44)]
            public unsafe System.Numerics.Vector3 CurrentPostion;
            [FieldOffset(56)]
            public unsafe System.Numerics.Vector3 PredictedPostion;
            [FieldOffset(68)]
            public unsafe bool IsMoveStopPackge;

            public override string ToString()
            {
                return $"Arg1:{Arg1} Arg2:{Arg2} Face:{Face:0.00} Face2:{Face2:0.00} CurrentPostion:{CurrentPostion:0.00} PredictedPostion:{PredictedPostion:0.00} IsMoveStopPackge:{IsMoveStopPackge}";

            }

        }
        private static PostionPackge PosPackge=new();
        private static IntPtr PosPackgeBuffer = Marshal.AllocHGlobal(Marshal.SizeOf(PosPackge));
        private static void SendPosToServer(float face, System.Numerics.Vector3 pos)
        {
            if (ZoneUpA1 == IntPtr.Zero) return;
            try
            {

                PosPackge.Face = face;
                PosPackge.Face2 = face;
                PosPackge.CurrentPostion = pos;
                PosPackge.PredictedPostion = pos;
                PosPackge.IsMoveStopPackge = false;

                Marshal.StructureToPtr(PosPackge, PosPackgeBuffer, true);
                
                ProcessZonePacketUpHook.Original(ZoneUpA1, PosPackgeBuffer, PosPackgeBuffer, 1);
                
            }
            catch (Exception ex)
            {

                PluginLog.Error($"{ex.Message}\n{ex.StackTrace}");
            }
            
        }
        public static void Test()
        {

            //SendPosToServer(MathF.PI, Service.ClientState.LocalPlayer.Position);
            SetTo(MathF.PI, 0, 15);
        }
        
        public static void SetTo(float facing, float delay, float during)
        {
            if (!Service.Condition[ConditionFlag.DutyRecorderPlayback])
            {
                Task.Delay((int)Math.Floor(delay * 1000)).ContinueWith(t =>
                {
                    PluginLog.Debug($"锁定至{facing}");
                    OwnFaceLocked = true;
                    Face = facing;
                    SendFace(Face);
                });
                Task.Delay(((int)Math.Floor(delay + during)) * 1000).ContinueWith(t =>
                {
                    PluginLog.Debug($"解除锁定");
                    OwnFaceLocked = false;
                });
            }

        }
        public static void DrawSetting()
        {
        }
        public static void Init()
        {
            try
            {

                SetObjectFacingOnCastHook = Hook<SetObjectFacingOneCastDelegate>.FromAddress(Service.Scanner.ScanText("48 89 5C 24 ?? 57 48 83 EC 30 F3 0F 10 42 ?? 49 8B F8"), SetObjectFacingOnCastDetour);
                SetObjectFacingOnCastHook.Enable();

                ProcessZonePacketUpHook = Hook<ProcessZonePacketUpDelegate>.FromAddress(
                    ((Dalamud.Game.Network.GameNetworkAddressResolver)Service.GameNetwork.GetType().GetField("address", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Service.GameNetwork)).ProcessZonePacketUp,
                    ProcessZonePacketUpDetour);
                ProcessZonePacketUpHook.Enable();

                PosPackge.OpCode = posUp_op1;
                PosPackge.AdjustLength = 0x38;

                Service.Framework.Update += Framework_Update;


            }
            catch (Exception ex)
            {

                PluginLog.Error(ex.Message);
                PluginLog.Error(ex.StackTrace);
            }

        }

        private static void Framework_Update(Dalamud.Game.Framework framework)
        {
            if (OwnFaceLocked && Service.ClientState.LocalPlayer is not null)
            {
                SendPosToServer(Face, Service.ClientState.LocalPlayer.Position);
            }
        }

        public static void Dispose()
        {
            Service.Framework.Update -= Framework_Update;
            Marshal.FreeHGlobal(PosPackgeBuffer);
            SetObjectFacingOnCastHook?.Dispose();
            //AdjustObjFacingHook?.Dispose();
            ProcessZonePacketUpHook.Dispose();
            Enabled = false;
        }

    }
}
