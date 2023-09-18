using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Party;
using Dalamud.Interface;
using Dalamud.Logging;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using Newtonsoft.Json;
using PiPiPlugin.PluginModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PixelPerfectEx.IPC
{
    internal class PartySort: IPluginModule
    {
        public static bool Enabled { get; private set; }



        private static List<Lumina.Excel.GeneratedSheets.ClassJob> ClassJobSorted;
        private static Lumina.Excel.GeneratedSheets.ClassJob _selectClassJob;
        private static IntPtr replayPartylist;
        public static void Init()
        {
            var pluginConfigPath = new FileInfo(Path.Combine(Service.Interface.ConfigDirectory.FullName, $"PartyClassJobSort.json"));

            var cidList = new List<uint>();
            if (pluginConfigPath.Exists)
            {
                var data = File.ReadAllText(pluginConfigPath.FullName);
                cidList = JsonConvert.DeserializeObject<List<uint>>(data);
            }

            var classJobs = Service.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.ClassJob>();
            if (cidList.Count==0)
            {
                cidList = classJobs.Where(cl => cl.JobIndex > 0).Select(cl => cl.RowId).ToList();
            }

            ClassJobSorted = new();
            cidList.ForEach(cid =>
            {
                ClassJobSorted.Add(classJobs.GetRow(cid));
            });

            replayPartylist = Service.Scanner.GetStaticAddressFromSig("48 8D 0D ?? ?? ?? ?? 48 83 C4 28 E9 ?? ?? ?? ?? 8B 05 ?? ?? ?? ?? 89 05 ?? ?? ?? ?? C3 CC CC CC 8B 05 ?? ?? ?? ?? 89 05 ?? ?? ?? ?? C3 CC CC CC 8B 05 ?? ?? ?? ?? 89 05 ?? ?? ?? ?? C3 CC CC CC 8B 05 ?? ?? ?? ??");
            PluginLog.Debug($"PartySort_ReplayPartylistIntptr:{replayPartylist:X8}|{replayPartylist.ToInt64()-Service.Address.BaseAddress.ToInt64():X8}");
        }
        public static void Dispose()
        {
            SaveConfig();
        }
        private static void SaveConfig()
        {
            var saveFolder = new DirectoryInfo(Service.Interface.GetPluginConfigDirectory());
            if (!saveFolder.Exists)
                Directory.CreateDirectory(saveFolder.FullName);

            var file = new FileInfo(Path.Combine(saveFolder.FullName, $"PartyClassJobSort.json"));
            
            try
            {
                var cidList= ClassJobSorted.Select(cl=>cl.RowId).ToList();
                var data1 = JsonConvert.SerializeObject(cidList, Formatting.Indented);
                File.WriteAllText(file.FullName, data1);
            }
            catch (Exception e)
            {
                PluginLog.Error($"Could not write to save file {file.FullName}:\n{e}{e.StackTrace}");
            }
        }
        public static void SendSortedPartyListToAct()
        {
            
            Service.LogSender.Send(new LogSender.LogMessage()
            {
                Message = $"PartyList:{JsonConvert.SerializeObject(GetSortedObjectIdList())}:End",
                Type = LogSender.LogMessageType.Debug
            });
        }
        public static List<uint> GetSortedObjectIdList()
        {
            if (Service.Condition[ConditionFlag.DutyRecorderPlayback])
            {
                List<BattleChara> pList= new();
                for (int i = 0; i < 8; i++)
                {
                    var id = Marshal.PtrToStructure<uint>(replayPartylist + 0x1A8 + 0x230 * i);
                    PluginLog.Log($"{id:X}");
                }
                for (int i = 0; i < 8; i++)
                {
                    var id= Marshal.PtrToStructure<uint>(replayPartylist + 0x1A8 + 0x230 * i);
                    pList.Add(Service.GameObjects.SearchById(id) as BattleChara);
                }
                var cid = ClassJobSorted.Select(cl => cl.RowId).ToList();
                pList.Sort((o1, o2) =>
                {
                    if (cid.IndexOf(o1.ClassJob.Id) < cid.IndexOf(o2.ClassJob.Id))
                        return -1;
                    else
                        return 1;
                });
                return pList.Select(o => o.ObjectId).ToList();
            }
            else
            {
                var partyList = Service.PartieList.ToList();
                var cid = ClassJobSorted.Select(cl => cl.RowId).ToList();
                partyList.Sort((o1, o2) =>
                {
                    if (cid.IndexOf(o1.ClassJob.Id) < cid.IndexOf(o2.ClassJob.Id))
                        return -1;
                    else
                        return 1;
                });
                return partyList.Select(o => o.ObjectId).ToList();
            }
            
            
        }
        public static List<string> GetSortedObjectNameList()
        {
            if (Service.Condition[ConditionFlag.DutyRecorderPlayback])
            {
                List<BattleChara> pList = new();
                for (int i = 0; i < 8; i++)
                {
                    var id = Marshal.PtrToStructure<uint>(replayPartylist + 0x1A8 + 0x230 * i);
                    pList.Add(Service.GameObjects.SearchById(id) as BattleChara);
                }
                var cid = ClassJobSorted.Select(cl => cl.RowId).ToList();
                pList.Sort((o1, o2) =>
                {
                    if (cid.IndexOf(o1.ClassJob.Id) < cid.IndexOf(o2.ClassJob.Id))
                        return -1;
                    else
                        return 1;
                });
                return pList.Select(o => o.Name.ToString()).ToList();
            }
            else
            {


                var partyList = Service.PartieList.ToList();
                var cid = ClassJobSorted.Select(cl => cl.RowId).ToList();
                partyList.Sort((o1, o2) =>
                {
                    if (cid.IndexOf(o1.ClassJob.Id) < cid.IndexOf(o2.ClassJob.Id))
                        return -1;
                    else
                        return 1;
                });
                return partyList.Select(o => o.Name.ToString()).ToList();
            }
        }
        public static void DrawSetting()
        {
            ImGui.BeginGroup();


            if (ImGui.BeginChild(
                "##PPex PartySort Selector",
                new Vector2(150 * ImGuiHelpers.GlobalScale, -ImGui.GetFrameHeight() - 1),
                true))
            {
                
                if (!ClassJobSorted.Contains(_selectClassJob))
                    _selectClassJob = null;
                ClassJobSorted.ForEach(classjob => 
                {
                    if (ImGui.Selectable($"{classjob.Name}##PPex partysort Selecable_{classjob.RowId}", _selectClassJob == classjob))
                    {
                        _selectClassJob = classjob;
                    }
                });
                ImGui.EndChild();
            }
            ImGui.SameLine();
            if (ImGui.BeginChild(
                "##PPex PartySort PreView",
                new Vector2(150 * ImGuiHelpers.GlobalScale, -ImGui.GetFrameHeight() - 1),
                true))
            {
                var nList = GetSortedObjectNameList();
                nList.ForEach(name =>
                {
                    ImGui.Selectable($"{name}##PPex partysort preview_{name}", false);
                });
                ImGui.EndChild();
            }


            ImGui.PushFont(UiBuilder.IconFont);
            if (ImGui.Button($"{FontAwesomeIcon.ArrowAltCircleUp.ToIconString()}##Move up a Conditions Button") && _selectClassJob is not null)
            {
                var index = ClassJobSorted.IndexOf(_selectClassJob);
                if (index > 0)
                {
                    (ClassJobSorted[index], ClassJobSorted[index - 1]) = (ClassJobSorted[index - 1], ClassJobSorted[index]);
                    SaveConfig();
                }
            }
            
            ToolTip("Move up select Condition");
            ImGui.SameLine();

            if (ImGui.Button($"{FontAwesomeIcon.ArrowAltCircleDown.ToIconString()}##Move down a Conditions Button") && _selectClassJob is not null)
            {
                var index = ClassJobSorted.IndexOf(_selectClassJob);
                if (index < ClassJobSorted.Count - 1)
                {
                    (ClassJobSorted[index], ClassJobSorted[index + 1]) = (ClassJobSorted[index + 1], ClassJobSorted[index]);
                    SaveConfig();
                }
            }
            ToolTip("Move down select Condition");
            ImGui.PopFont();
            ImGui.EndGroup();
        }
        internal static bool ToolTip(string text)
        {
            if (ImGui.IsItemHovered() && !String.IsNullOrEmpty(text))
            {
                ImGui.PushFont(UiBuilder.DefaultFont);
                ImGui.SetTooltip(text);
                ImGui.PopFont();
                return true;
            }
            return false;

        }
        
    }
}
