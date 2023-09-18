using Dalamud.Logging;
using H.Pipes;
using PiPiPlugin.PluginModule;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelPerfectEx.IPC
{
    internal class LogSender 
    {
        private PipeServer<string> pipeServer;
        public LogSender()
        {


            var pipeName = $"ppex.ipc.ffxiv_{Process.GetCurrentProcess().Id}";
            pipeServer = new PipeServer<string>(pipeName);
            pipeServer.ClientConnected += async (o, args) =>
            {
                try
                {
                    Send(new() { Type = LogSender.LogMessageType.Debug, Message = "欢迎接入PPex" });
                    PluginLog.Debug($"CLient {args.Connection.PipeName} is conneected");
                }
                catch (Exception ex)
                {
                    PluginLog.Error(ex.Message);
                    PluginLog.Error(ex.StackTrace);
                }

            };
            pipeServer.MessageReceived += (o, args) =>
            {
                try
                {
                    MessageHandler(args.Message);
                }
                catch (Exception ex)
                {
                    PluginLog.Error($"传入信息错误\n${ex.Message}\n{args.Message}");
                }
            };
            pipeServer.ClientDisconnected += (o, args) =>
            {
                PluginLog.Debug($"CLient {args.Connection.PipeName} is disconneected");

            };
            pipeServer.ExceptionOccurred += (o, args) =>
            {
                PluginLog.Error(args.Exception.Message);
                PluginLog.Error(args.Exception.StackTrace);
            };


            pipeServer.StartAsync();
        }
        public void Dispose()
        {
            pipeServer.DisposeAsync();
        }

        public void Send(LogMessage message)
        {
            pipeServer.WriteAsync($"{(int)message.Type}#{message.Message}");
        }

        public void MessageHandler(string message)
        {
            var args = message.Split("##");
            if (args.Length != 2) { throw new("message has err length"); }
            NetHandler.CommandHandler(args[0], args[1]);
        }

        public enum LogMessageType
        {
            ChatLog = 0,
            Territory = 1,
            ChangePrimaryPlayer = 2,
            AddCombatant = 3,
            RemoveCombatant = 4,
            PartyList = 11,
            PlayerStats = 12,
            StartsCasting = 20,
            ActionEffect = 21,
            AOEActionEffect = 22,
            CancelAction = 23,
            DoTHoT = 24,
            Death = 25,
            StatusAdd = 26,
            TargetIcon = 27,
            WaymarkMarker = 28,
            SignMarker = 29,
            StatusRemove = 30,
            Gauge = 0x1F,
            World = 0x20,
            Director = 33,
            NameToggle = 34,
            Tether = 35,
            LimitBreak = 36,
            EffectResult = 37,
            StatusList = 38,
            UpdateHp = 39,
            ChangeMap = 40,
            SystemLogMessage = 41,
            StatusList3 = 42,
            Settings = 249,
            Process = 250,
            Debug = 251,
            PacketDump = 252,
            Version = 253,
            Error = 254
        }
        public class LogMessage
        {
            public LogMessageType Type { get; set; }
            public string Message { get; set; }
        }
    }
}
