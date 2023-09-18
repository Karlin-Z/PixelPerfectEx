using ImGuiNET;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static PixelPerfectEx.Drawing.IDrawData;

namespace PixelPerfectEx.Drawing.Types
{
    internal class Donut : DrawBase
    {
        public float Radius { get; set; }
        public float InnerRadius { get; set; }
        public Donut(JObject jo) : base(jo)
        {
            Radius = jo.TryGetValue(nameof(Radius), out var _ra) ? _ra.ToObject<float>() : 0;
            InnerRadius = jo.TryGetValue(nameof(InnerRadius), out var _ira) ? _ira.ToObject<float>() : 0;
        }

        public override void Draw(ImDrawListPtr drawList)
        {
            GetDrawCentre(Centre).ToList().ForEach(c =>
            {
                DrawFunc.DrawDonut.Draw(drawList, c, Radius, InnerRadius, Color);
            });
        }
        private Vector3[] GetDrawCentre(Vector3 centre)
        {
            List<Vector3> drawList = new();
            var olist = Service.GameObjects.Where((o) => o.ObjectKind == Dalamud.Game.ClientState.Objects.Enums.ObjectKind.Player).ToList();
            switch (TrackType)
            {
                case TrackTypeEnum.Nearest:
                    olist.Sort((ob1, ob2) =>
                    {
                        Vector2 c = new(centre.X, centre.Z);
                        Vector2 o1 = new(ob1.Position.X, ob1.Position.Z);
                        Vector2 o2 = new(ob2.Position.X, ob2.Position.Z);
                        if (Vector2.Distance(c, o1) > Vector2.Distance(c, o2))
                            return 1;
                        return -1;
                    });
                    for (int i = 0; i < (uint)TrackValue; i++)
                    {
                        drawList.Add(olist[i].Position);
                    }
                    break;
                case TrackTypeEnum.Farest:
                    olist.Sort((ob1, ob2) =>
                    {
                        Vector2 c = new(centre.X, centre.Z);
                        Vector2 o1 = new(ob1.Position.X, ob1.Position.Z);
                        Vector2 o2 = new(ob2.Position.X, ob2.Position.Z);
                        if (Vector2.Distance(c, o1) > Vector2.Distance(c, o2))
                            return -1;
                        return 1;
                    });
                    for (int i = 0; i < (uint)TrackValue; i++)
                    {
                        drawList.Add(olist[i].Position);
                    }
                    break;
                default:
                    drawList.Add(centre);
                    break;
            }
            return drawList.ToArray();
        }

        private static string exName = "Donut Example";
        private static CentreTypeEnum exCentreType = CentreTypeEnum.ActorId;
        private static string exActorId = "0xE0000000";
        private static string exActorName = "丝瓜卡夫卡";
        private static string exPosX = "0", exPosY = "0", exPosZ = "0.0";
        private static TrackTypeEnum exTrackType = TrackTypeEnum.None;
        private static string exTrackValue = "0";
        private static string exRadius = "10.0";
        private static string exInnerRadius = "5.0";
        private static string exDelay = "0", exDuring = "5";
        private static Vector4 exColor = new(1, 0, 0, 0.25f);

        

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

            strB.Append($"\"{nameof(AoeType)}\":\"{AoeTypeEnum.Donut}\",");

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

            #region Track
            ImGui.Text("Track Type:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(200);
            if (ImGui.BeginCombo("##trackTypeType", $"{exTrackType}"))
            {
                foreach (TrackTypeEnum item in Enum.GetValues(typeof(TrackTypeEnum)))
                {
                    if (ImGui.Selectable($"{item}"))
                        exTrackType = item;
                }
                ImGui.EndCombo();
            }
            switch (exTrackType)
            {
                case TrackTypeEnum.None:
                case TrackTypeEnum.IdTrack:
                case TrackTypeEnum.NameTrack:
                    break;
                default:
                    strB.Append($"\"TrackType\":\"{exTrackType}\",");
                    ImGui.SameLine();
                    ImGui.Text("TrackValue:");
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(100);
                    ImGui.InputText("##TrackValue", ref exTrackValue, 10);
                    strB.Append($"\"TrackValue\":{exTrackValue},");
                    break;
            }


            #endregion

            ImGui.Text($"{nameof(Radius)}:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100);
            ImGui.InputText($"##{nameof(Radius)}", ref exRadius, 20);

            ImGui.SameLine();
            ImGui.Text($"{nameof(InnerRadius)}:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100);
            ImGui.InputText($"##{nameof(InnerRadius)}", ref exInnerRadius, 20);


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


            ImGui.SameLine();
            ImGui.Text($"{nameof(Color)}:");
            ImGui.SameLine();
            ImGui.ColorEdit4($"##{nameof(Color)}Picker", ref exColor, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.AlphaBar | ImGuiColorEditFlags.AlphaPreview);

            strB.Append($"\"{nameof(Radius)}\":{exRadius},\"{nameof(InnerRadius)}\":{exInnerRadius},\"{nameof(Color)}\":{ImGui.ColorConvertFloat4ToU32(exColor)},\"{nameof(Delay)}\":{exDelay},\"{nameof(During)}\":{exDuring}}}");

            var jsonStr = strB.ToString();
            ImGui.Text("Dount Json:");
            ImGui.SameLine();
            if (ImGui.Button($"Test##Dount Test"))
            {
                NetHandler.CommandHandler("Add", jsonStr);
            }
            ImGui.SameLine();
            ImGui.InputText("##roundJson", ref jsonStr, 300, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.ReadOnly);

            ImGui.Unindent();
        }
    }
}
