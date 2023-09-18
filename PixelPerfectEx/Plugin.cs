
using Dalamud.Game.Command;
using Dalamud.Interface;
using Dalamud.Plugin;
using Dalamud.Interface.Internal.Notifications;
using Dalamud.Logging;
using ImGuiNET;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PixelPerfectEx.Drawing;
using PixelPerfectEx.Drawing.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text.Json.Nodes;
using System.Threading;
using static PixelPerfectEx.Drawing.IDrawData;
using Lumina.Excel.GeneratedSheets;
using System.Drawing;
using PiPiPlugin.PluginModule;
using PixelPerfectEx.IPC;
using System.Collections.Immutable;

namespace PixelPerfectEx
{
    public sealed partial class Plugin: IDalamudPlugin
    {
        public string Name => "Pixel Perfect Ex";
        private const int CurrentConfigVersion = 1;
        string Command = "/ppex";

        Vector2 DisWindowSize = new Vector2(0,0);
        Vector2 DisWindowPos = new Vector2();

        private ImFontPtr _font;
        private bool _fontLoaded = false;

        public List<AOEInfo> AoeInfos = new();

        public Plugin(DalamudPluginInterface pluginInterface)
        {
            pluginInterface.Create<Service>();
            Service.Plugin = this;
            Service.Configuration = Configuration.Load(pluginInterface.ConfigDirectory);
            if (Service.Configuration.Version < CurrentConfigVersion)
            {
                Service.Configuration.Upgrade();
                Service.Configuration.Save();
            }
            Service.Address = new PluginAddressResolver();
            Service.Address.Setup();

            Service.CommandManager.AddHandler(Command, new CommandInfo(ShowConfig));


            Service.Interface.UiBuilder.Draw += BuildUI;
            Service.Interface.UiBuilder.OpenConfigUi += UiBuilder_OnOpenConfigUi;

            Service.Interface.UiBuilder.BuildFonts += BuildFont;


            IPluginModule.LoadAll();

            ServerStart();
        }
        public void Dispose()
        {
            ServerStop();
            IPluginModule.DisposeAll();
            Service.LogSender.Dispose();
            Service.Interface.UiBuilder.BuildFonts -= BuildFont;
            Service.Interface.UiBuilder.RebuildFonts();
            Service.Interface.UiBuilder.Draw -= BuildUI;
            Service.Interface.UiBuilder.OpenConfigUi -= UiBuilder_OnOpenConfigUi;
            Service.CommandManager.RemoveHandler(Command);

            
        }

        static unsafe int DealChar(ImGuiInputTextCallbackData* data)
        {

            if ((*data).EventChar == '0' | (*data).EventChar == '.')
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }

        AOEInfo.PostionTypeEnum PostionType = AOEInfo.PostionTypeEnum.ActorId;
        AOEInfo.PostionTypeEnum PostionType2 = AOEInfo.PostionTypeEnum.ActorId;
        AOEInfo.TrackEnum trackType = AOEInfo.TrackEnum.FixedAngle;
        string actorId = "0xE0000000";
        string actorName = "丝瓜卡夫卡";
        string actorId2 = "0xE0000000";
        string actorName2 = "丝瓜卡夫卡";
        string trackId = "0xE0000000";
        string trackName = "丝瓜卡夫卡";
        string posX = "0.0";
        string posY = "0.0";
        string posZ = "0.0";
        string posX2 = "0.0";
        string posY2 = "0.0";
        string posZ2 = "0.0";
        string outerSize = "5.0";
        string innerSize = "3.0";
        string rotation = "0.0";
        string sectorAngle = "90.0";
        string length = "8.0";
        string width = "3.0";
        string thikness = "5.0";
        string delay = "0.0";
        string during = "10.0";
        Vector4 color = new(1, 0, 0, 0.3f);
        Vector4 correctColor = new(0, 1, 0, 0.3f);


        bool drawDebugGraphics = false;
        private void BuildConfigWindow()
        {
            bool configVisible = Service.Configuration.ConfigVisible;
            if (configVisible)
            {
                bool changed = false;
                ImGui.SetNextWindowSize(new Vector2(300, 500), ImGuiCond.FirstUseEver);
                if (ImGui.Begin("Pixel Perfect Ex Config", ref configVisible))
                {
                   if (ImGui.BeginTabBar("Setting Tab"))
                   {
                        if (ImGui.BeginTabItem("Position Assist"))
                        {
                            ImGui.PushID("Position Assist Options");
                            if (ImGui.CollapsingHeader("Owner Position"))
                            {
                                ImGui.Indent();
                                changed |= ImGui.Checkbox("Hitbox##Owner Hitbox", ref Service.Configuration.OwnerHitboxVisible);
                                changed |= ImGui.SliderFloat("Hitbox Size##Owner Hitbox Size", ref Service.Configuration.OwnerHitboxSize, 2f, 10f, "%.0f", ImGuiSliderFlags.Logarithmic);
                                ImGui.SameLine();
                                changed |= ImGui.ColorEdit4("Hitbox Colour##Owner Hitbox Colour", ref Service.Configuration.OwnerHitboxColour, ImGuiColorEditFlags.NoInputs);
                                changed |= ImGui.Checkbox("Hitbox Outer Ring##Owner Hitbox Outer Ring", ref Service.Configuration.OwnerHitboxOuterRingVisible);
                                ImGui.SameLine();
                                changed |= ImGui.ColorEdit4("Outer Ring Colour##Owner Outer Ring Colour", ref Service.Configuration.OwnerHitboxOuterRingColour, ImGuiColorEditFlags.NoInputs);

                                ImGui.NewLine();
                                changed |= ImGui.Checkbox("Ring##Owner Ring", ref Service.Configuration.OwnerRingVisible);
                                changed |= ImGui.SliderFloat("Ring Size##Owner Ring Size", ref Service.Configuration.OwnerRingSize, -10f, 50f, "%.0f", ImGuiSliderFlags.Logarithmic);
                                changed |= ImGui.SliderFloat("Ring Thickness##Owner Ring Thickness", ref Service.Configuration.OwnerRingThickness, 0f, 10f, "%.0f");
                                ImGui.SameLine();
                                changed |= ImGui.ColorEdit4("Ring Colour##Owner Ring Colour", ref Service.Configuration.OwnerRingColour, ImGuiColorEditFlags.NoInputs);

                                ImGui.NewLine();
                                changed |= ImGui.Checkbox("Combat Only##Owner Combat Only", ref Service.Configuration.OwnerHitboxCombatOnly);
                                changed |= ImGui.Checkbox("Instance Only##Owner Instance Only", ref Service.Configuration.OwnerHitboxInstanceOnly);

                                ImGui.Unindent();
                            }
                            if (ImGui.CollapsingHeader("Target Position"))
                            {
                                ImGui.Indent();

                                changed |= ImGui.Checkbox("Hitbox", ref Service.Configuration.TargetHitboxVisible);
                                changed |= ImGui.SliderFloat("Hitbox Size", ref Service.Configuration.TargetHitboxSize, 2f, 10f, "%.0f", ImGuiSliderFlags.Logarithmic);
                                ImGui.SameLine();
                                changed |= ImGui.ColorEdit4("Hitbox Colour", ref Service.Configuration.TargetHitboxColour, ImGuiColorEditFlags.NoInputs);
                                changed |= ImGui.Checkbox("Hitbox Outer Ring", ref Service.Configuration.TargetHitboxOuterRingVisible);
                                ImGui.SameLine();
                                changed |= ImGui.ColorEdit4("Outer Ring Colour", ref Service.Configuration.TargetHitboxOuterRingColour, ImGuiColorEditFlags.NoInputs);

                                ImGui.NewLine();
                                changed |= ImGui.Checkbox("FaceMark", ref Service.Configuration.TargetBodyPosVisible);
                                changed |= ImGui.SliderFloat("FaceMark Size", ref Service.Configuration.TargetBodyPosSize, 1.0f, 10.0f, "%.1f", ImGuiSliderFlags.Logarithmic);
                                changed |= ImGui.SliderFloat("FaceMark PosOffset", ref Service.Configuration.TargetBodyPosOffset, -5.0f, 5.0f, "%.2f", ImGuiSliderFlags.Logarithmic);
                                changed |= ImGui.SliderFloat("FaceMark Thickness", ref Service.Configuration.TargetBodyPosThickness, 1f, 20f, "%.0f", ImGuiSliderFlags.Logarithmic);
                                ImGui.SameLine();
                                changed |= ImGui.ColorEdit4("FaceMark Colour", ref Service.Configuration.TargetBodyPosColour, ImGuiColorEditFlags.NoInputs);

                                ImGui.NewLine();
                                changed |= ImGui.Checkbox("Ring", ref Service.Configuration.TargetRingVisible);
                                changed |= ImGui.SliderFloat("Ring Size", ref Service.Configuration.TargetRingSize, -10f, 50f, "%.0f", ImGuiSliderFlags.Logarithmic);
                                changed |= ImGui.SliderFloat("Ring Thickness", ref Service.Configuration.TargetRingThickness, 0f, 10f, "%.0f");
                                ImGui.SameLine();
                                changed |= ImGui.ColorEdit4("Ring Colour", ref Service.Configuration.TargetRingColour, ImGuiColorEditFlags.NoInputs);

                                ImGui.NewLine();
                                changed |= ImGui.Checkbox("Ring2", ref Service.Configuration.TargetRing2Visible);
                                changed |= ImGui.SliderFloat("Ring2 Size", ref Service.Configuration.TargetRing2Size, -10f, 50f, "%.0f", ImGuiSliderFlags.Logarithmic);
                                changed |= ImGui.SliderFloat("Ring2 Thickness", ref Service.Configuration.TargetRing2Thickness, 0f, 10f, "%.0f");
                                ImGui.SameLine();
                                changed |= ImGui.ColorEdit4("Ring2 Colour", ref Service.Configuration.TargetRing2Colour, ImGuiColorEditFlags.NoInputs);

                                ImGui.NewLine();

                                changed |= ImGui.Checkbox("RelativePosition Line", ref Service.Configuration.RelativePositionLineVisible);
                                changed |= ImGui.SliderFloat("RelativePositionLine Length", ref Service.Configuration.RelativePositionLineLength, -10f, 20f, "%.0f");
                                ImGui.SameLine();
                                changed |= ImGui.ColorEdit4("RelativePositionLine Colour", ref Service.Configuration.RelativePositionLineColour, ImGuiColorEditFlags.NoInputs);


                                changed |= ImGui.Checkbox("Target-Player Line", ref Service.Configuration.TargetPlayerLineVisible);
                                ImGui.SameLine();
                                changed |= ImGui.ColorEdit4("Target-Player Line Colour", ref Service.Configuration.TargetPlayerLineColour, ImGuiColorEditFlags.NoInputs);


                                ImGui.NewLine();
                                changed |= ImGui.Checkbox("Combat Only", ref Service.Configuration.TargetHitboxCombatOnly);
                                changed |= ImGui.Checkbox("Instance Only", ref Service.Configuration.TargetHitboxInstanceOnly);


                                ImGui.Unindent();
                            }

                            if (ImGui.CollapsingHeader("Distance Show"))
                            {
                                ImGui.Indent();

                                changed |= ImGui.Checkbox("Distace Show##Dis Show", ref Service.Configuration.DistanceShowVisible);
                                changed |= ImGui.Checkbox("Follow Player##Dis Follow", ref Service.Configuration.DistanceShowFollowPlayer);
                                unsafe
                                {
                                    changed |= ImGui.InputText("Formate##DisFormat", ref Service.Configuration.DistanceShowFormat, 10, ImGuiInputTextFlags.CallbackCharFilter, new ImGuiInputTextCallback(DealChar));
                                }
                                changed |= ImGui.ColorEdit4("Colour#DisShow color", ref Service.Configuration.DistanceShowColour, ImGuiColorEditFlags.NoInputs);
                                changed |= ImGui.SliderFloat("BgAlpha##DisShow BgAlpha", ref Service.Configuration.DistanceBgAlpha, 0, 1);
                                ImGui.SameLine();
                                changed |= ImGui.SliderFloat("Large##DisShow Large", ref Service.Configuration.DistanceShowLarge, 0, 2);
                                changed |= ImGui.DragFloat2("Window Offset##DisWindow Offset", ref Service.Configuration.DistanceShowOffset);
                                changed |= ImGui.Checkbox("Combat Only##Dis Combat Only", ref Service.Configuration.DistanceShowCombatOnly);
                                changed |= ImGui.Checkbox("Instance Only##Dis Combat Only", ref Service.Configuration.DistanceShowInstanceOnly);

                                ImGui.Unindent();
                            }


                            ImGui.PopID();
                            ImGui.EndTabItem();
                        }

                        if (ImGui .BeginTabItem("AOE Assist"))
                        {
                            ImGui.InputInt("Listen Port(Restart to Apply)", ref Service.Configuration.HttpPort);
                            ImGui.Text($"Send Json to http://127.0.0.1:{Service.Configuration.HttpPort}/DrawAoe to draw aoe");
                            ImGui.Text("Use Debug Mode to edit Json.");
                            ImGui.Text("You can also combine multiple AOE messages as one send by using [].");
                            changed |= ImGui.Checkbox("Draw AOE Assist##DrawAoeAssist", ref Service.Configuration.DrawAoeAssist);
                            if(ImGui.Button($"清除绘图队列"))
                            {
                                lock (Service.DrawDatas) { Service.DrawDatas.Clear(); }
                            }
                            if (ImGui.Button($"清除移动队列"))
                            {
                                AutoMoveHack.ClearAll();
                            }
                            ImGui.PushID("AOE Assist Options");
                            if (ImGui.CollapsingHeader("Setting##drawAoeSetting"))
                            {
                                ImGui.Indent();
                                ImGui.SetNextItemWidth(200);
                                ImGui.Text("Segment:");
                                ImGui.SameLine();
                                changed |= ImGui.SliderInt("##SegmentSetting", ref Service.Configuration.Segment, 20, 100);

                                
                                changed |= ImGui.Checkbox("Strock##Strock", ref Service.Configuration.Strock);
                                if (Service.Configuration.Strock)
                                {
                                    ImGui.SameLine();
                                    changed |= ImGui.SliderFloat("##Strock Width", ref Service.Configuration.StrockWidth, 0, 20, "%.0f");
                                }
                                

                                ImGui.SetNextItemWidth(200);
                                ImGui.Text("Draw Size:");
                                ImGui.SameLine();
                                changed |= ImGui.SliderInt("x of screen size##Draw Size", ref Service.Configuration.DrawSize, 1, 10);
                                ImGui.SetNextItemWidth(200);
                                ImGui.Text("Default Thickness:");
                                ImGui.SameLine();
                                changed |= ImGui.SliderFloat("##Default Thickness", ref Service.Configuration.DefaultThickness, 1, 20,"%.0f");



                                changed |= ImGui.Checkbox("DrawDebug Mode##DrawDebug Enable", ref Service.Configuration.DrawDebug);
                                if (Service.Configuration.DrawDebug)
                                {
                                    ImGui.Indent();
                                    changed |= ImGui.Checkbox("Show receive json##ShowReceive", ref Service.Configuration.ShowReceive);
                                    ImGui.Checkbox("Draw Debug Graphics##DrawDebugGraphics", ref drawDebugGraphics);
                                    if (drawDebugGraphics)
                                    {
                                        long targetAddr = 0;
                                        unsafe
                                        {
                                            targetAddr = *(long*)(Service.Address.PlayerTargetPtr);
                                        }
                                        if (targetAddr != 0)
                                        {
                                            Vector3 targetPostion;
                                            unsafe
                                            {
                                                var x = *(float*)(targetAddr + 0xA0);
                                                var y = *(float*)(targetAddr + 0xA4);
                                                var z = *(float*)(targetAddr + 0xA8);
                                                targetPostion = new Vector3(x, y, z);
                                            }
                                            var actor = Service.ClientState.LocalPlayer;

                                            GetValibleDrawPoint(targetPostion, actor.Position, out var op);
                                            GetValibleDrawPoint(actor.Position, targetPostion, out var op2);
                                            ImGui.Text($"{op2.X:0000.0},{op2.Y:0000.0}");
                                            ImGui.Text($"{op.X:0000.0},{op.Y:0000.0}");
                                            ImGui.GetBackgroundDrawList().AddLine(op2, op, ImGui.ColorConvertFloat4ToU32(new Vector4(0, 1, 0, 1)));

                                        }
                                    }
                                    ImGui.Unindent();
                                }
                                ImGui.Unindent();
                            }
                            ImGui.PopID();

                            if (ImGui.CollapsingHeader("New Aoe Json Example"))
                            {
                                ImGui.Indent();
                                if (ImGui.CollapsingHeader("New Circle Json Example"))
                                    Circle.DrawSetting();
                                if (ImGui.CollapsingHeader("New Donut Json Example"))
                                    Donut.DrawSetting();
                                if (ImGui.CollapsingHeader("New Rect Json Example"))
                                    Rect.DrawSetting();
                                if (ImGui.CollapsingHeader("New Sector Json Example"))
                                    Sector.DrawSetting();
                                if (ImGui.CollapsingHeader("New Back Json Example"))
                                    Back.DrawSetting();
                                if (ImGui.CollapsingHeader("New Link Json Example"))
                                    Link.DrawSetting();
                                if (ImGui.CollapsingHeader("New Repel Json Example"))
                                    Repel.DrawSetting();
                                ImGui.Unindent();
                                
                            }

                            if (ImGui.Button("test"))
                            {
                                string str = "{\"Name\":\"Donut Example\",\"AoeType\":\"Donut\",\"CentreType\":\"PostionValue\",\"CentreValue\":{\"X\":1,\"Y\":2,\"Z\":3},\"TrackType\":\"Nearest\",\"TrackValue\":0x12,\"Radius\":10.0,\"InnerRadius\":5.0,\"Color\":1073742079,\"Delay\":0,\"During\":5}";
                                var js= JObject.Parse(str);
                                var v1 = js.TryGetValue("TrackValue", out var stringComparison);
                                var r = stringComparison.ToObject<uint>();
                                var v2 = js.Value<object>("TrackValue");
                            }
                            





                            ImGui.EndTabItem();
                        }
                        if (ImGui.BeginTabItem("Extra Log"))
                        {
                            if (ImGui.CollapsingHeader("Party sort"))
                            {
                                ImGui.Indent();
                                PartySort.DrawSetting();
                                ImGui.Unindent();
                            }
                            if (ImGui.Button("Test"))
                            {
                                ObjectEffectHack.Test();
                            }
                            if (ImGui.Button("测试设置面向"))
                            {
                                ObjectFacingHack.Test();
                            }

                            ImGui.EndTabItem();
                        }
                        ImGui.EndTabBar();
                   }



                    ImGui.End();



                    if (changed)
                    {
                        Service.Configuration.Save();
                    }

                }

            }
            if (configVisible != Service.Configuration.ConfigVisible)
            {
                Service.Configuration.ConfigVisible = configVisible;
                Service.Configuration.Save();
            }
        }
        private void BuildUI()
        {
            if (!_fontLoaded)
            {
                Service.Interface.UiBuilder.RebuildFonts();
                return;
            }

            BuildConfigWindow();

            DisWindowSize = ImGui.GetIO().DisplaySize;
            


            var actor = Service.ClientState.LocalPlayer;
            if (actor == null) return;
            ImGui.SetNextWindowSize(ImGui.GetMainViewport().Size);
            ImGuiHelpers.ForceNextWindowMainViewport();
            ImGuiHelpers.SetNextWindowPosRelativeMainViewport(Vector2.Zero);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
            try
            {
                if (ImGui.Begin("Display", ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoBackground))
                {
                    ImGui.GetBackgroundDrawList().Flags &= ~ImDrawListFlags.AntiAliasedFill;
                    ImGui.GetWindowDrawList().Flags &= ~ImDrawListFlags.AntiAliasedFill;
                    ImGui.GetBackgroundDrawList().Flags &= ~ImDrawListFlags.AllowVtxOffset;
                    ImGui.GetWindowDrawList().Flags &= ~ImDrawListFlags.AllowVtxOffset;
                    if ((Service.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.InCombat] | !Service.Configuration.TargetHitboxCombatOnly) & (Service.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.BoundByDuty] | !Service.Configuration.TargetHitboxInstanceOnly))
                    {
                        long targetAddr = 0;
                        unsafe
                        {
                            targetAddr = *(long*)(Service.Address.PlayerTargetPtr);
                        }
                        if (targetAddr != 0)
                        {
                            Vector3 targetPostion;
                            unsafe
                            {
                                var x = *(float*)(targetAddr + 0xB0);
                                var y = *(float*)(targetAddr + 0xB4);
                                var z = *(float*)(targetAddr + 0xB8);
                                targetPostion = new Vector3(x, y, z);
                            }

                            //目标hitbox
                            if (Service.Configuration.TargetHitboxVisible)
                            {
                                if (Service.GameGui.WorldToScreen(targetPostion, out var pos))
                                {
                                    ImGui.GetWindowDrawList().AddCircleFilled(pos, Service.Configuration.TargetHitboxSize, ImGui.GetColorU32(Service.Configuration.TargetHitboxColour));
                                    if (Service.Configuration.TargetHitboxOuterRingVisible)
                                    {
                                        ImGui.GetWindowDrawList().AddCircle(pos, Service.Configuration.TargetHitboxSize + 0.2f, ImGui.GetColorU32(Service.Configuration.TargetHitboxOuterRingColour));
                                    }
                                }
                            }

                            //面向指示
                            if (Service.Configuration.TargetBodyPosVisible)
                            {
                                float size = Service.Configuration.TargetBodyPosSize;
                                float thickness = Service.Configuration.TargetBodyPosThickness;
                                float rotation = 0;
                                var col = ImGui.GetColorU32(Service.Configuration.TargetBodyPosColour);

                                unsafe
                                {
                                    rotation = *(float*)((float*)(targetAddr + 0xC0));
                                }
                                var beginPos = new Vector3(targetPostion.X + (Service.Configuration.TargetBodyPosOffset * (float)Math.Sin(0 + rotation)), targetPostion.Y, targetPostion.Z + (Service.Configuration.TargetBodyPosOffset * (float)Math.Cos((0 + rotation))));
                                var posEnd = new Vector3(beginPos.X + (size * (float)Math.Sin(0 + rotation)), beginPos.Y, beginPos.Z + (size * (float)Math.Cos((0 + rotation))));

                                var posEnd2 = new Vector3(posEnd.X + (size * 0.5f * (float)Math.Sin(Math.PI / 8f * 7f + rotation)), posEnd.Y, posEnd.Z + (size * 0.5f * (float)Math.Cos((Math.PI / 8f * 7f + rotation))));
                                var posEnd3 = new Vector3(posEnd.X + (size * 0.5f * (float)Math.Sin(-Math.PI / 8f * 7f + rotation)), posEnd.Y, posEnd.Z + (size * 0.5f * (float)Math.Cos((-Math.PI / 8f * 7f + rotation))));


                                Drawing.DrawFunc.DrawLine.Draw(ImGui.GetWindowDrawList(),beginPos,posEnd, thickness, col);
                                Drawing.DrawFunc.DrawLine.Draw(ImGui.GetWindowDrawList(), posEnd, posEnd2, thickness, col);
                                Drawing.DrawFunc.DrawLine.Draw(ImGui.GetWindowDrawList(), posEnd, posEnd3, thickness, col);
                            }

                            //目标圈
                            if (Service.Configuration.TargetRingVisible)
                            {
                                float radius = Service.Configuration.TargetRingSize;
                                unsafe
                                {
                                    radius += *(float*)(targetAddr + 0xD0);
                                    if (Service.Configuration.TargetRingSize > 0)
                                    {
                                        radius += *(float*)(actor.Address.ToInt64() + 0xD0);
                                    }

                                }
                                Drawing.DrawFunc.DrawRing.Draw(ImGui.GetWindowDrawList(), targetPostion, radius, Service.Configuration.TargetRingThickness, ImGui.GetColorU32(Service.Configuration.TargetRingColour));
                            }
                            //目标圈2
                            if (Service.Configuration.TargetRing2Visible)
                            {
                                float radius = Service.Configuration.TargetRing2Size;
                                unsafe
                                {
                                    radius += *(float*)(targetAddr + 0xD0);
                                    if (Service.Configuration.TargetRing2Size > 0)
                                    {
                                        radius += *(float*)(actor.Address.ToInt64() + 0xD0);
                                    }

                                }
                                Drawing.DrawFunc.DrawRing.Draw(ImGui.GetWindowDrawList(), targetPostion, radius, Service.Configuration.TargetRing2Thickness, ImGui.GetColorU32(Service.Configuration.TargetRing2Colour));
                            }



                            //身位分线
                            if (Service.Configuration.RelativePositionLineVisible)
                            {
                                float radius = Service.Configuration.RelativePositionLineLength;
                                float rotartion = 0;
                                float thickness = 2;
                                var col = ImGui.GetColorU32(Service.Configuration.RelativePositionLineColour);
                                unsafe
                                {
                                    radius += *(float*)((float*)(targetAddr + 0xD0));
                                    if (Service.Configuration.TargetRingSize > 0)
                                    {
                                        radius += *(float*)(actor.Address.ToInt64() + 0xD0);
                                    }
                                    rotartion = *(float*)((float*)(targetAddr + 0xC0));
                                }
                                var pos_1 = new Vector3(targetPostion.X + (radius * (float)Math.Sin(rotartion + Math.PI / 4f)), targetPostion.Y, targetPostion.Z + (radius * (float)Math.Cos((rotartion + Math.PI / 4f))));
                                var pos_2 = new Vector3(targetPostion.X + (-radius * (float)Math.Sin(rotartion + Math.PI / 4f)), targetPostion.Y, targetPostion.Z + (-radius * (float)Math.Cos((rotartion + Math.PI / 4f))));

                                var pos_3 = new Vector3(targetPostion.X + (radius * (float)Math.Sin(rotartion + Math.PI / 4f * 3f)), targetPostion.Y, targetPostion.Z + (radius * (float)Math.Cos((rotartion + Math.PI / 4f * 3f))));
                                var pos_4 = new Vector3(targetPostion.X + (-radius * (float)Math.Sin(rotartion + Math.PI / 4f * 3f)), targetPostion.Y, targetPostion.Z + (-radius * (float)Math.Cos((rotartion + Math.PI / 4f * 3f))));
                                Drawing.DrawFunc.DrawLine.Draw(ImGui.GetWindowDrawList(), pos_1, pos_2, thickness, col);
                                Drawing.DrawFunc.DrawLine.Draw(ImGui.GetWindowDrawList(), pos_3, pos_4, thickness, col);
                            }

                            //目标连线
                            if (Service.Configuration.TargetPlayerLineVisible)
                            {
                                Drawing.DrawFunc.DrawLine.Draw(ImGui.GetWindowDrawList(), actor.Position, targetPostion, 2, ImGui.GetColorU32(Service.Configuration.TargetPlayerLineColour));
                            }

                        }

                    }

                    if ((Service.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.InCombat] | !Service.Configuration.OwnerHitboxCombatOnly) & (Service.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.BoundByDuty] | !Service.Configuration.OwnerHitboxInstanceOnly))
                    {
                        //角色碰撞点
                        if (Service.Configuration.OwnerHitboxVisible)
                        {
                            if (Service.GameGui.WorldToScreen(actor.Position, out var pos))
                            {
                                ImGui.GetWindowDrawList().AddCircleFilled(pos, Service.Configuration.OwnerHitboxSize, ImGui.GetColorU32(Service.Configuration.OwnerHitboxColour));
                                if (Service.Configuration.OwnerHitboxOuterRingVisible)
                                {
                                    ImGui.GetWindowDrawList().AddCircle(pos, Service.Configuration.OwnerHitboxSize + 0.2f, ImGui.GetColorU32(Service.Configuration.OwnerHitboxOuterRingColour));
                                }
                            }
                        }

                        if (Service.Configuration.OwnerRingVisible)
                        {
                            float radius = Service.Configuration.OwnerRingSize;
                            unsafe
                            {
                                radius += *(float*)(actor.Address.ToInt64() + 0xD0);
                            }
                            Drawing.DrawFunc.DrawRing.Draw(ImGui.GetWindowDrawList(), actor.Position, radius, Service.Configuration.OwnerRingThickness, ImGui.GetColorU32(Service.Configuration.OwnerRingColour));
                        }

                    }

                    if (Service.Configuration.DrawDebug)
                    {
                        long targetAddr = 0;
                        unsafe
                        {
                            targetAddr = *(long*)(Service.Address.PlayerTargetPtr);
                        }

                        //DrawWorldSector(actor.Position, actor.Rotation, a1, 6, 100, ImGui.ColorConvertFloat4ToU32(new Vector4(1, 0, 0, 0.1f)));
                        if (actor.TargetObject != null)
                        {
                            //DrawWordQuad(actor.TargetObject.Position, actor.TargetObject.Rotation, a3, a4, ImGui.ColorConvertFloat4ToU32(new Vector4(0, 1, 0, 0.25f)));
                            //DrawWorldRing(actor.TargetObject.Position, 5, 20, 100, ImGui.ColorConvertFloat4ToU32(new Vector4(0, 1, 0, 0.25f)));
                        }

                    }
                    if (Service.Configuration.DrawAoeAssist)
                    {

                        lock (Service.DrawDatas) { Service.DrawDatas.RemoveWhere(dd => dd.IsExpired); }

                        //DrawDatas.ForEach(dd => dd.Draw(ImGui.GetWindowDrawList()));
                        foreach (var dd in Service.DrawDatas.ToImmutableArray())
                        {
                            if (dd.IsVisible)
                            { 
                                dd.Draw(ImGui.GetWindowDrawList()); 
                            } 
                        }

                        DealOldDraw();
                    }
                }

                #region 距离指示器
                if (Service.GameGui.WorldToScreen(actor.Position, out var pos1))
                {
                    if ((Service.Configuration.DistanceShowVisible) & (Service.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.InCombat] | !Service.Configuration.DistanceShowCombatOnly) & (Service.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.BoundByDuty] | !Service.Configuration.DistanceShowInstanceOnly))

                    {
                        ImGuiWindowFlags windowFlag = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.AlwaysAutoResize;
                        //ImGui.SetNextWindowSize(DisWindowSize, ImGuiCond.Always);
                        ImGui.SetNextWindowPos(DisWindowPos, ImGuiCond.Always);
                        ImGui.SetNextWindowBgAlpha(Service.Configuration.DistanceBgAlpha);
                        if (_font.IsLoaded()) ImGui.PushFont(_font);
                        if (ImGui.Begin("DisShow", ref Service.Configuration.DistanceShowVisible, windowFlag))
                        {
                            float targetDis = Marshal.PtrToStructure<float>(Service.Address.PlayerTargetDisPtr);
                            ImGui.SetWindowFontScale(Service.Configuration.DistanceShowLarge);
                            string disStr = targetDis.ToString(Service.Configuration.DistanceShowFormat);
                            var strSize = ImGui.CalcTextSize(disStr);


                            Vector2 pos2 = pos1;
                            if (!Service.Configuration.DistanceShowFollowPlayer)
                            {
                                pos2 = new Vector2(ImGui.GetIO().DisplaySize.X * 0.5f, ImGui.GetIO().DisplaySize.Y * 0.5f);

                            }
                            DisWindowPos.X = pos2.X - ImGui.GetWindowWidth() * 0.5f + Service.Configuration.DistanceShowOffset.X;
                            DisWindowPos.Y = pos2.Y - ImGui.GetWindowHeight() * 0.5f + Service.Configuration.DistanceShowOffset.Y;



                            ImGui.PushStyleColor(ImGuiCol.Text, ImGui.GetColorU32(Service.Configuration.DistanceShowColour));


                            ImGui.SetWindowFontScale(Service.Configuration.DistanceShowLarge);
                            ImGui.SetCursorPosY((ImGui.GetWindowHeight() - strSize.Y) * 0.5f);
                            ImGui.SetCursorPosX((ImGui.GetWindowWidth() - strSize.X) * 0.5f);
                            ImGui.Text(disStr);
                            ImGui.End();

                            ImGui.PopStyleColor();
                        }

                        if (_font.IsLoaded()) ImGui.PopFont();

                    }
                }
                #endregion
            }
            catch (Exception ex)
            {

                Dalamud.Logging.PluginLog.Error($"{ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                ImGui.End();
                ImGui.PopStyleVar();
            }


        }
        
        private void DealOldDraw()
        {
            for (int i = AoeInfos.Count - 1; i >= 0; i--)
            {
                if (AoeInfos[i].EndTime < DateTime.Now) AoeInfos.Remove(AoeInfos[i]);
            }
            lock (AoeInfos)
            {
                var actor = Service.ClientState.LocalPlayer;
                foreach (var aoeInfo in AoeInfos)
                {
                    if (aoeInfo.StartTime > DateTime.Now | aoeInfo.EndTime < DateTime.Now)
                        continue;


                    Vector3 postion = new Vector3(float.NaN, float.NaN, float.NaN);

                    float rotation = 0;
                    if (aoeInfo.PostionType == AOEInfo.PostionTypeEnum.PostionValue)
                    {
                        postion = aoeInfo.Postion;
                    }
                    if (aoeInfo.PostionType == AOEInfo.PostionTypeEnum.ActorName)
                    {
                        foreach (var ac in Service.GameObjects)
                        {
                            if (ac.Name.ToString() == aoeInfo.ActorName)
                            {
                                postion = ac.Position;
                                rotation = ac.Rotation;
                                break;
                            }
                        }
                    }
                    if (aoeInfo.PostionType == AOEInfo.PostionTypeEnum.ActorId)
                    {
                        foreach (var ac in Service.GameObjects)
                        {
                            if (ac.ObjectId == aoeInfo.ActorId)
                            {
                                postion = ac.Position;
                                rotation = ac.Rotation;
                                break;
                            }
                        }
                    }
                    if (aoeInfo.AoeType == AOEInfo.AoeTypeEnum.TP)
                    {
                        postion = actor.Position;
                        rotation = actor.Rotation;
                    }

                    if (float.IsNaN(postion.X)) continue;

                    rotation += (aoeInfo.Rotation / 180f) * -MathF.PI;
                    if (aoeInfo.Thickness <= 0)
                    {
                        aoeInfo.Thickness = Service.Configuration.DefaultThickness;
                    }

                    if (aoeInfo.TrackMode == AOEInfo.TrackEnum.NameTrack)
                    {
                        foreach (var ac in Service.GameObjects)
                        {
                            if (ac.Name.ToString() == aoeInfo.TrackName)
                            {
                                rotation = MathF.Atan2(ac.Position.X - postion.X, ac.Position.Z - postion.Z);
                                break;
                            }
                        }
                    }
                    if (aoeInfo.TrackMode == AOEInfo.TrackEnum.IdTrack)
                    {
                        foreach (var ac in Service.GameObjects)
                        {
                            if (ac.ObjectId == aoeInfo.TrackId)
                            {
                                rotation = MathF.Atan2(ac.Position.X - postion.X, ac.Position.Z - postion.Z);
                                break;
                            }
                        }
                    }

                    if (aoeInfo.TrackMode == AOEInfo.TrackEnum.Nearest)
                    {

                    }

                    if (aoeInfo.AoeType == AOEInfo.AoeTypeEnum.Round)
                    {

                        Drawing.DrawFunc.DrawCircle.Draw(ImGui.GetWindowDrawList(), postion, aoeInfo.OuterRadius, aoeInfo.Color);
                    }
                    if (aoeInfo.AoeType == AOEInfo.AoeTypeEnum.Ring)
                    {
                        Drawing.DrawFunc.DrawDonut.Draw(ImGui.GetWindowDrawList(), postion, aoeInfo.OuterRadius, aoeInfo.InnerRadius, aoeInfo.Color);
                    }
                    if (aoeInfo.AoeType == AOEInfo.AoeTypeEnum.Rect)
                    {
                        Drawing.DrawFunc.DrawRect.Draw(ImGui.GetWindowDrawList(), postion, aoeInfo.Length, aoeInfo.Width, rotation, aoeInfo.Color);
                    }
                    if (aoeInfo.AoeType == AOEInfo.AoeTypeEnum.Sector)
                    {
                        Drawing.DrawFunc.DrawSector.Draw(ImGui.GetWindowDrawList(), postion, aoeInfo.OuterRadius, aoeInfo.SectorAngle, rotation, aoeInfo.Color);
                    }
                    if (aoeInfo.AoeType == AOEInfo.AoeTypeEnum.Repel)
                    {
                        var end = actor.Position + Vector3.Normalize(actor.Position - postion) * aoeInfo.Length;
                        Drawing.DrawFunc.DrawLine.Draw(ImGui.GetWindowDrawList(), actor.Position, end, aoeInfo.Thickness, aoeInfo.Color);
                    }
                    if (aoeInfo.AoeType == AOEInfo.AoeTypeEnum.Back)
                    {
                        var actorVec = new Vector2(MathF.Sin(actor.Rotation), MathF.Cos(actor.Rotation));
                        var tToa = Vector2.Normalize(new Vector2(actor.Position.X - postion.X, actor.Position.Z - postion.Z));
                        var ag = MathF.Acos(Vector2.Dot(actorVec, tToa));
                        var seeAngle = aoeInfo.SectorAngle / 180 * MathF.PI / 2;
                        var col = ag > MathF.PI - seeAngle ? aoeInfo.Color : aoeInfo.CorrectColor;
                        Drawing.DrawFunc.DrawLine.Draw(ImGui.GetWindowDrawList(), actor.Position, postion, aoeInfo.Thickness, col);
                    }
                    if (aoeInfo.AoeType == AOEInfo.AoeTypeEnum.TP)
                    {

                        var endPos = new Vector3(MathF.Sin(rotation), 0, MathF.Cos(rotation)) * aoeInfo.Length + actor.Position;
                        Drawing.DrawFunc.DrawLine.Draw(ImGui.GetWindowDrawList(), actor.Position, endPos, aoeInfo.Thickness, aoeInfo.Color);
                    }
                    if (aoeInfo.AoeType == AOEInfo.AoeTypeEnum.Link)
                    {
                        Vector3 postion2 = new Vector3(float.NaN, float.NaN, float.NaN);
                        if (aoeInfo.PostionType2 == AOEInfo.PostionTypeEnum.PostionValue)
                        {
                            postion2 = aoeInfo.Postion2;
                        }
                        if (aoeInfo.PostionType2 == AOEInfo.PostionTypeEnum.ActorName)
                        {
                            foreach (var ac in Service.GameObjects)
                            {
                                if (ac.Name.ToString() == aoeInfo.ActorName2)
                                {
                                    postion2 = ac.Position;
                                    break;
                                }
                            }
                        }
                        if (aoeInfo.PostionType2 == AOEInfo.PostionTypeEnum.ActorId)
                        {
                            foreach (var ac in Service.GameObjects)
                            {
                                if (ac.ObjectId == aoeInfo.ActorId2)
                                {
                                    postion2 = ac.Position;
                                    break;
                                }
                            }
                        }
                        if (float.IsNaN(postion2.X)) continue;
                        if (aoeInfo.Thickness <= 0)
                        {
                            aoeInfo.Thickness = Service.Configuration.DefaultThickness;
                        }
                        Drawing.DrawFunc.DrawLine.Draw(ImGui.GetWindowDrawList(), postion, postion2, aoeInfo.Thickness, aoeInfo.Color);
                    }

                }
            }
        }
       
        private bool GetValibleDrawPoint(Vector3 pointInWorld, Vector3 center, out Vector2 adjustScrPoint)
        {
            var drawSize = DisWindowSize * (Service.Configuration.DrawSize*2-1);
            var adjustSize = DisWindowSize * (Service.Configuration.DrawSize - 1);

            Vector2 point2D;
            
            for (int i = 0; i < 10; i++)
            {
                if (Service.GameGui.WorldToScreen(pointInWorld, out point2D))
                {
                    adjustScrPoint = point2D;
                    return true;
                }
                else
                {
                    if (point2D.Y>0 & point2D.Y<DisWindowSize.Y)
                    {

                    }
                    var p= point2D + adjustSize;
                    if (p.X < drawSize.X & p.Y < drawSize.Y & p.X > 0 & p.Y > 0)
                    {
                        adjustScrPoint = point2D;
                        return true;
                    }
                }
                pointInWorld = center + ((pointInWorld - center) / 2);
            }

            adjustScrPoint = new(0, 0);
            return false;
        }
        

        float a1, a2, a3, a4;
        int i1=3;

        
        
        


        private void ShowConfig(string command, string arguments)
        {
            Service.Configuration.ConfigVisible = true;
            Service.Configuration.Save();
        }
        private void UiBuilder_OnOpenConfigUi()
        {
            Service.Configuration.ConfigVisible = !Service.Configuration.ConfigVisible;
        }
        private void BuildFont()
        {
            try
            {
                var filePath = Path.Combine(Service.Interface.DalamudAssetDirectory.FullName, "UIRes",
                    "NotoSansCJKsc-Medium.otf");
                if (!File.Exists(filePath)) throw new FileNotFoundException("Font file not found!");
                _font = ImGui.GetIO().Fonts.AddFontFromFileTTF(filePath, 80, null);
            }
            catch (Exception e)
            {

                //PluginLog.LogError(e.Message);
            }

            _fontLoaded = true;
        }


        #region Httpserver
        private HttpServer _httpServer;
        private void ServerStart(object sender = null, EventArgs e = null)
        {
            try
            {
                _httpServer = new HttpServer(Service.Configuration.HttpPort);
                _httpServer.PPexHttpActionDelegate = DoActionByCommand;
                _httpServer.OnException += OnException;


            }
            catch (Exception ex)
            {
                OnException(ex);
            }
        }

        private void ServerStop(object sender = null, EventArgs e = null)
        {
            try
            {
                _httpServer.Stop();
                _httpServer.PPexHttpActionDelegate = null;
                _httpServer.OnException -= OnException;
            }
            catch (Exception ex)
            {
                OnException(ex);
            }


        }
        /// <summary>
        /// 委托给HttpServer类的异常处理
        /// </summary>
        /// <param name="ex"></param>
        private void OnException(Exception ex)
        {
            string errorMessage = $"无法在{_httpServer.Port}端口启动监听\n{ex.Message}";

            //PluginUI.Log(errorMessage);
            Service.ChatGui.Print(errorMessage);
        }

        /// <summary>
        ///     执行指令对应的方法
        /// </summary>
        /// <param name="command"></param>
        /// <param name="payload"></param>
        public void DoActionByCommand(string command, string payload)
        {
            try
            {
                ThreadPool.QueueUserWorkItem((o) =>
                {
                    NetHandler.CommandHandler(command, payload);
                });

            }
            catch (Exception ex)
            {
                Dalamud.Logging.PluginLog.Error($"Command \"{command}\" have a error:");
                Dalamud.Logging.PluginLog.Error($"{ex.Message}\n{ex.StackTrace}");
                
            }
        }

        #endregion
    }
}
