using Dalamud.Game.ClientState.Objects.Types;
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
    internal class Rect : DrawBase
    {
        

        //矩形 XAxisModifier是宽度
        public float Length { get; set; }
        public float Width { get; set; }
        public float Rotation { get; set; }
        public Rect(JObject jo) : base(jo)
        {
            Length = jo.TryGetValue(nameof(Length), out var _l) ? _l.ToObject<float>() : 0;
            Width = jo.TryGetValue(nameof(Width), out var _w) ? _w.ToObject<float>() : 0;
            Rotation = jo.TryGetValue(nameof(Rotation), out var _r) ? _r.ToObject<float>() : 0;
        }
        public override void Draw(ImDrawListPtr drawList)
        {
            var rotations = GetRotations();
            var centre = Centre;
            foreach (var rot in rotations)
            {
                DrawFunc.DrawRect.Draw(drawList, centre, Length, Width, rot, Color);
            }
        }
        private List<Vector2> GetPiont(Vector3 start, Vector3 goal)
        {
            var points = new List<Vector2>();
            var nor = Vector3.Normalize(goal - start);
            var num = Convert.ToInt32(MathF.Floor(Vector3.Distance(start, goal)));
            for (int i = 0; i <= num; i++)
            {
                var pin = start + nor * i;
                PPexProjection.WorldToScreen(pin, out Vector2 p, out Vector3 p1r);
                // Dont add points that may be projected to weird positions
                if (p1r.Z >= -.1f)
                {
                    points.Add(p);
                }
            }
            PPexProjection.WorldToScreen(goal, out Vector2 pe, out Vector3 p2r);
            // Dont add points that may be projected to weird positions
            if (p2r.Z >= -.1f)
            {
                points.Add(pe);
            }
            return points;
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
                    tObList=Service.GameObjects
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
                    tObList = olist.Take((int)(uint)TrackValue).Select(o=>o.Position).ToList();
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

        private static string exName = "Rect Example";
        private static CentreTypeEnum exCentreType = CentreTypeEnum.ActorId;
        private static string exActorId = "0xE0000000";
        private static string exActorName = "丝瓜卡夫卡";
        private static string exPosX = "0", exPosY = "0", exPosZ = "0.0";
        private static TrackTypeEnum exTrackType = TrackTypeEnum.None;
        private static string exTrackValue = "0";
        private static string exLength = "5.0", exWidth = "5.0", exRotation = "0.0";
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

            strB.Append($"\"{nameof(AoeType)}\":\"{AoeTypeEnum.Rect}\",");

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

            ImGui.Text($"{nameof(Length)}:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100);
            ImGui.InputText($"##{nameof(Length)}", ref exLength, 20);


            ImGui.SameLine();
            ImGui.Text($"{nameof(Width)}:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100);
            ImGui.InputText($"##{nameof(Width)}", ref exWidth, 20);


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

            strB.Append($"\"{nameof(Length)}\":{exLength},\"{nameof(Width)}\":{exWidth},\"{nameof(Rotation)}\":{exRotation},\"{nameof(Color)}\":{ImGui.ColorConvertFloat4ToU32(exColor)},\"{nameof(Delay)}\":{exDelay},\"{nameof(During)}\":{exDuring}}}");

            var jsonStr = strB.ToString();
            ImGui.Text("Rect Json:");
            ImGui.SameLine();
            if (ImGui.Button($"Test##{nameof(Rect)} Test"))
            {
                NetHandler.CommandHandler("Add", jsonStr);
            }
            ImGui.SameLine();
            ImGui.InputText("##roundJson", ref jsonStr, 300, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.ReadOnly);

            ImGui.Unindent();
        }
    }
}
