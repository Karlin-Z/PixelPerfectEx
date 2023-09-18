using FFXIVClientStructs.FFXIV.Client.Game.UI;
using ImGuiNET;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static FFXIVClientStructs.FFXIV.Client.UI.Misc.ConfigModule;
using static PixelPerfectEx.Drawing.IDrawData;

namespace PixelPerfectEx.Drawing.Types
{
    internal class Sector : DrawBase
    {

        public float Radius { get; set; }
        public float Angle { get; set; }
        public float Rotation { get; set; }
        public Sector(JObject jo) : base(jo)
        {
            Radius = jo.TryGetValue(nameof(Radius), out var _l) ? _l.ToObject<float>() : 0;
            Angle = jo.TryGetValue(nameof(Angle), out var _w) ? _w.ToObject<float>() : 0;
            Rotation = jo.TryGetValue(nameof(Rotation), out var _r) ? _r.ToObject<float>() : 0;
        }
        public override void Draw(ImDrawListPtr drawList)
        {
            foreach (var rot in GetRotations())
            {
                DrawFunc.DrawSector.Draw(drawList,Centre,Radius,Angle,rot,Color);
            }
        }
        private float[] GetRotations()
        {

            var centre = Centre;


            List<float> rotations = new();
            List<Vector3> tObList = new();
            var olist = Service.GameObjects.Where((o) => o.ObjectKind == Dalamud.Game.ClientState.Objects.Enums.ObjectKind.Player).ToList();
            switch (TrackType)
            {
                case TrackTypeEnum.None:
                    {
                        var rr = Rotation / 180f * -MathF.PI;
                        switch (CentreType)
                        {
                            case CentreTypeEnum.ActorId:
                                var ar = Service.GameObjects.Where(o => o.ObjectId == (uint)CentreValue).Select(o => o.Rotation).ToArray();
                                for (int i = 0; i < ar.Length; i++)
                                {
                                    ar[i] += rr;
                                }
                                return ar;
                            case CentreTypeEnum.ActorName:
                                var nr = Service.GameObjects.Where(o => o.Name.ToString() == (string)CentreValue).Select(o => o.Rotation).ToArray();
                                for (int i = 0; i < nr.Length; i++)
                                {
                                    nr[i] += rr;
                                }
                                return nr;
                            case CentreTypeEnum.PostionValue:
                                return new List<float>()
                                {
                                    rr
                                }.ToArray();
                        }
                    }
                    break;
                case TrackTypeEnum.IdTrack:
                    tObList = Service.GameObjects
                        .Where(o => o.ObjectId == (uint)TrackValue)
                        .Select(o => o.Position)
                        .ToList();
                    break;
                case TrackTypeEnum.NameTrack:
                    tObList = Service.GameObjects
                        .Where(o => o.Name.ToString() == (string)TrackValue)
                        .Select(o => o.Position)
                        .ToList();
                    break;
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
                    tObList = olist.Take((int)(uint)TrackValue).Select(o => o.Position).ToList();
                    break;
                case TrackTypeEnum.Farest:
                    olist.Sort((ob1, ob2) =>
                    {
                        Vector2 c = new(centre.X, centre.Z);
                        Vector2 o1 = new(ob1.Position.X, ob1.Position.Z);
                        Vector2 o2 = new(ob2.Position.X, ob2.Position.Z);
                        if (Vector2.Distance(c, o1) < Vector2.Distance(c, o2))
                            return 1;
                        return -1;
                    });
                    tObList = olist.Take((int)(uint)TrackValue).Select(o => o.Position).ToList();
                    break;
            }
            tObList.ForEach(ob =>
            {
                rotations.Add(MathF.Atan2(ob.X - centre.X, ob.Z - centre.Z));
            });

            //return drawList.ToArray();
            return rotations.ToArray();
        }

        private static string exName = "Sector Example";
        private static CentreTypeEnum exCentreType = CentreTypeEnum.ActorId;
        private static string exActorId = "0xE0000000";
        private static string exActorName = "丝瓜卡夫卡";
        private static string exPosX = "0", exPosY = "0", exPosZ = "0.0";
        private static TrackTypeEnum exTrackType = TrackTypeEnum.None;
        private static string exTrackValue = "0";
        private static string exRadius = "5.0", exAngle = "5.0", exRotation = "0.0";
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

            strB.Append($"\"{nameof(AoeType)}\":\"{AoeTypeEnum.Sector}\",");

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
                    {
                        exTrackType = item;

                    }
                }
                ImGui.EndCombo();
            }
            switch (exTrackType)
            {
                case TrackTypeEnum.None:
                    break;
                case TrackTypeEnum.NameTrack:
                    strB.Append($"\"TrackType\":\"{exTrackType}\",");
                    ImGui.SameLine();
                    ImGui.Text("TrackValue:");
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(100);
                    ImGui.InputText("##TrackValue", ref exTrackValue, 10);
                    strB.Append($"\"TrackValue\":\"{exTrackValue}\",");
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
            ImGui.Text($"{nameof(Angle)}:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100);
            ImGui.InputText($"##{nameof(Angle)}", ref exAngle, 20);


            ImGui.SameLine();
            ImGui.Text($"{nameof(Rotation)}:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100);
            ImGui.InputText($"##{nameof(Rotation)}", ref exRotation, 20);


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

            strB.Append($"\"{nameof(Radius)}\":{exRadius},\"{nameof(Angle)}\":{exAngle},\"{nameof(Rotation)}\":{exRotation},\"{nameof(Color)}\":{ImGui.ColorConvertFloat4ToU32(exColor)},\"{nameof(Delay)}\":{exDelay},\"{nameof(During)}\":{exDuring}}}");

            var jsonStr = strB.ToString();
            ImGui.Text("Sector Json:");
            ImGui.SameLine();
            if (ImGui.Button($"Test##{nameof(Sector)} Test"))
            {
                NetHandler.CommandHandler("Add", jsonStr);
            }
            ImGui.SameLine();
            ImGui.InputText("##roundJson", ref jsonStr, 300, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.ReadOnly);

            ImGui.Unindent();
        }
    }
}
