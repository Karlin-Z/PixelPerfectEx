using Dalamud.Memory.Exceptions;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Text;
using static FFXIVClientStructs.FFXIV.Client.UI.Misc.ConfigModule;
using static PixelPerfectEx.Drawing.IDrawData;

namespace PixelPerfectEx.Drawing.Types
{
    internal class Link : DrawBase
    {
        public CentreTypeEnum Centre2Type { get; set; }
        public object Centre2Value { get; set; }
        [JsonIgnore]
        public Vector3 Centre2
        {
            get
            {
                switch (Centre2Type)
                {
                    case CentreTypeEnum.ActorId:
                        var id = (uint)Centre2Value;
                        foreach (var ac in Service.GameObjects)
                        {
                            if (ac.ObjectId == id)
                                return ac.Position;
                        }
                        break;
                    case CentreTypeEnum.ActorName:
                        var name = (string)Centre2Value;
                        foreach (var ac in Service.GameObjects)
                        {
                            if (ac.Name.ToString() == name)
                                return ac.Position;
                        }
                        break;
                    case CentreTypeEnum.PostionValue:
                        return (Vector3)Centre2Value;
                    default:
                        break;
                }
                return new(float.NaN, float.NaN, float.NaN);
            }
        }
        public float Thikness { get; set; }
        
        public Link(JObject jo) : base(jo)
        {
            Centre2Type = jo.TryGetValue(nameof(Centre2Type), out var _ra) ? _ra.ToObject<CentreTypeEnum>() : CentreTypeEnum.ActorId;
            jo.TryGetValue(nameof(Centre2Value), out var _cv2);
            switch (Centre2Type)
            {
                case CentreTypeEnum.ActorId:
                    Centre2Value = _cv2.ToObject<uint>();
                    break;
                case CentreTypeEnum.ActorName:
                    Centre2Value = _cv2.ToObject<string>();
                    break;
                case CentreTypeEnum.PostionValue:
                    Centre2Value = _cv2.ToObject<Vector3>();
                    break;
            }
            Thikness = jo.TryGetValue(nameof(Thikness), out var _thik) ? _thik.ToObject<float>() : Service.Configuration.DefaultThickness;
        }

        public override void Draw(ImDrawListPtr drawList)
        {
            DrawFunc.DrawLine.Draw(drawList, Centre, Centre2, Thikness, Color);
        }

        

        private static string exName = "Back Example";
        private static CentreTypeEnum exCentreType=CentreTypeEnum.ActorId;
        private static string exActorId="0xE0000000";
        private static string exActorName = "丝瓜卡夫卡";
        private static string exPosX = "0", exPosY = "0", exPosZ="0.0";
        private static CentreTypeEnum exCentre2Type = CentreTypeEnum.ActorId;
        private static string exActor2Id = "0xE0000000";
        private static string exActor2Name = "丝瓜卡夫卡";
        private static string exPos2X = "0", exPos2Y = "0", exPos2Z = "0.0";

        private static string exThikness="1";
        private static string exSeeAngle="90";
        private static string exDelay = "0", exDuring = "5";
        private static Vector4 exColor=new(0,1,0,1f);




        public static void DrawSetting()
        {
            ImGui.Indent();
            StringBuilder strB = new();
            strB.Append('{');

            ImGui.Text($"{nameof(Name)}:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(350);
            ImGui.InputText($"##{nameof(Name)}", ref exName, 15);
            strB.Append($"\"{nameof(Name)}\":\"{exName}\",");

            strB.Append($"\"{nameof(AoeType)}\":\"{AoeTypeEnum.Link}\",");

            #region Centre Type
            ImGui.Text("Centre Type:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(200);
            if (ImGui.BeginCombo($"##{nameof(CentreType)}", $"{exCentreType}"))
            {
                foreach (CentreTypeEnum item in Enum.GetValues(typeof(CentreTypeEnum)))
                {
                    if (ImGui.Selectable($"{item}"))
                    {
                        exCentreType = item;
                    }
                }
                ImGui.EndCombo();
            }
            strB.Append($"\"{nameof(CentreType)}\":\"{exCentreType}\",");

            strB.Append($"\"{nameof(CentreValue)}\":");
            ImGui.SameLine();
            switch (exCentreType)
            {
                case CentreTypeEnum.ActorId:
                    ImGui.SetNextItemWidth(350);
                    ImGui.InputText("##ActorId", ref exActorId, 40);
                    strB.Append($"{exActorId},");
                    break;
                case CentreTypeEnum.ActorName:
                    ImGui.SetNextItemWidth(350);
                    ImGui.InputText("##ActorName", ref exActorName, 40);
                    strB.Append($"\"{exActorName}\",");
                    break;
                case CentreTypeEnum.PostionValue:
                    ImGui.SameLine();
                    ImGui.Text("X:");
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(100);
                    ImGui.InputText("##posX", ref exPosX, 15);
                    ImGui.SameLine();
                    ImGui.Text("Y:");
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(100);
                    ImGui.InputText("##posY", ref exPosY, 10);
                    ImGui.SameLine();
                    ImGui.Text("Z:");
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(100);
                    ImGui.InputText("##posZ", ref exPosZ, 10);
                    strB.Append($"{{\"X\":{exPosX},\"Y\":{exPosY},\"Z\":{exPosZ}}},");
                    break;
            }
            #endregion

            #region Centre2 Type
            ImGui.Text("Centre2 Type:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(200);
            if (ImGui.BeginCombo($"##{nameof(Centre2Type)}", $"{exCentre2Type}"))
            {
                foreach (CentreTypeEnum item in Enum.GetValues(typeof(CentreTypeEnum)))
                {
                    if (ImGui.Selectable($"{item}"))
                    {
                        exCentre2Type = item;
                    }
                }
                ImGui.EndCombo();
            }
            strB.Append($"\"{nameof(Centre2Type)}\":\"{exCentre2Type}\",");

            strB.Append($"\"{nameof(Centre2Value)}\":");
            ImGui.SameLine();
            switch (exCentre2Type)
            {
                case CentreTypeEnum.ActorId:
                    ImGui.SetNextItemWidth(350);
                    ImGui.InputText("##Actor2Id", ref exActor2Id, 40);
                    strB.Append($"{exActor2Id},");
                    break;
                case CentreTypeEnum.ActorName:
                    ImGui.SetNextItemWidth(350);
                    ImGui.InputText("##Actor2Name", ref exActor2Name, 40);
                    strB.Append($"\"{exActor2Name}\",");
                    break;
                case CentreTypeEnum.PostionValue:
                    ImGui.SameLine();
                    ImGui.Text("X:");
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(100);
                    ImGui.InputText("##pos2X", ref exPos2X, 15);
                    ImGui.SameLine();
                    ImGui.Text("Y:");
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(100);
                    ImGui.InputText("##pos2Y", ref exPos2Y, 10);
                    ImGui.SameLine();
                    ImGui.Text("Z:");
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(100);
                    ImGui.InputText("##pos2Z", ref exPos2Z, 10);
                    strB.Append($"{{\"X\":{exPos2X},\"Y\":{exPos2Y},\"Z\":{exPos2Z}}},");
                    break;
            }
            #endregion


            
            
            ImGui.Text($"{nameof(Thikness)}:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100);
            ImGui.InputText($"##{nameof(Thikness)}", ref exThikness, 20);

            ImGui.SameLine();
            ImGui.Text($"{nameof(Delay)}:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100);
            ImGui.InputText($"##{nameof(Delay)}", ref exDelay, 20);


            ImGui.SameLine();
            ImGui.Text($"{nameof(During)}:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100);
            ImGui.InputText($"##{nameof(During)}", ref exDuring, 20);


            ImGui.Text($"{nameof(Color)}:");
            ImGui.SameLine();
            ImGui.ColorEdit4($"##{nameof(Color)}Picker", ref exColor, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.AlphaBar | ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.Float);

            
            strB .Append($"\"{nameof(Thikness)}\":{exThikness},\"{nameof(Color)}\":{ImGui.ColorConvertFloat4ToU32(exColor)},\"{nameof(Delay)}\":{exDelay},\"{nameof(During)}\":{exDuring}}}") ;

            var jsonStr = strB.ToString();
            ImGui.Text($"{nameof(Link)} Json:");
            ImGui.SameLine();
            if (ImGui.Button($"Test##{nameof(Link)} Test"))
            {
                NetHandler.CommandHandler("Add", jsonStr);
            }
            ImGui.SameLine();
            ImGui.InputText($"##{nameof(Back)} Json", ref jsonStr, 300, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.ReadOnly);

            ImGui.Unindent();
        }
       
    }
}
