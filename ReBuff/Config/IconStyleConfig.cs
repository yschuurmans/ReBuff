﻿using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using Newtonsoft.Json;
using ReBuff.Helpers;
using ReBuff.Widgets;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ReBuff.Config
{
    public class IconStyleConfig : IConfigPage
    {
        [JsonIgnore]
        public string Name => "Icon";

        [JsonIgnore] private string _labelInput = string.Empty;
        [JsonIgnore] private string _iconSearchInput = string.Empty;
        [JsonIgnore] private List<TriggerData> _iconSearchResults = new List<TriggerData>();
        [JsonIgnore] Vector2 _screenSize = ImGui.GetMainViewport().Size;

        public Vector2 Position = Vector2.Zero;
        public Vector2 IconPosition = new Vector2(5, -1);
        public Vector2 BarPosition = new Vector2(-90, 8);
        public Vector2 IconSize = new Vector2(30, 42);
        public Vector2 Size = new Vector2(40, 40);
        public Vector2 BarSize = new Vector2(150, 26);
        public bool ShowBorder = true;
        public int BorderThickness = 1;
        public ConfigColor BorderColor = new ConfigColor(0, 0, 0, 1);
        public bool ShowProgressSwipe = false;
        public float ProgressSwipeOpacity = 0.6f;
        public bool InvertSwipe = false;
        public bool ShowSwipeLines = false;
        public ConfigColor ProgressLineColor = new ConfigColor(1, 1, 1, 1);
        public int ProgressLineThickness = 2;
        public bool GcdSwipe = false;
        public bool GcdSwipeOnly = false;

        public bool DesaturateIcon = false;
        public float Opacity = 1f;

        public int IconOption = 0;
        public uint CustomIcon = 0;
        public bool CropIcon = false;

        public bool Glow = false;
        public int GlowThickness = 2;
        public int GlowSegments = 8;
        public float GlowSpeed = 1f;
        public ConfigColor GlowColor = new ConfigColor(230f / 255f, 150f / 255f, 0f / 255f, 1f);
        public ConfigColor GlowColor2 = new ConfigColor(0f / 255f, 0f / 255f, 0f / 255f, 0f);
        
        public ConfigColor IconColor = new ConfigColor(1, 0, 0, 1);
        public ConfigColor IconColor2 = new ConfigColor(1, 0, 0, 1);
        public ConfigColor IconColor3 = new ConfigColor(1, 0, 0, 1);
        public ConfigColor IconColor4 = new ConfigColor(1, 0, 0, 1);

        public float Unit = 100;
        public float MaxUnit = 100;
        public float MaxThreshold = 100;
        public float Threshold = 100;
        public int UnitOption = 6;

        public IConfigPage GetDefault() => new IconStyleConfig();

        public void DrawConfig(IConfigurable parent, Vector2 size, float padX, float padY)
        {
            if (ImGui.BeginChild("##IconStyleConfig", new Vector2(size.X, size.Y), true))
            {
                float height = 50;
                if ((this.IconOption == 1 || this.IconOption == 4) && this.CustomIcon > 0)
                {
                    Vector2 iconPos = ImGui.GetWindowPos() + new Vector2(padX, padX);
                    Vector2 iconSize = new Vector2(height, height);
                    this.DrawIconPreview(iconPos, iconSize, this.CustomIcon, this.CropIcon, this.DesaturateIcon, false);
                    ImGui.GetWindowDrawList().AddRect(
                        iconPos,
                        iconPos + iconSize,
                        ImGui.ColorConvertFloat4ToU32(ImGui.GetStyle().Colors[(int)ImGuiCol.Border]));

                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + height + padX);
                }

                ImGui.RadioButton("Automatic Icon", ref this.IconOption, 0);
                ImGui.SameLine();
                ImGui.RadioButton("Custom Icon", ref this.IconOption, 1);
                ImGui.SameLine();
                ImGui.RadioButton("No Icon", ref this.IconOption, 2);
                ImGui.SameLine();
                ImGui.RadioButton("Solid Color", ref this.IconOption, 3);
                ImGui.SameLine();
                ImGui.RadioButton("Indicator", ref this.IconOption, 6);

                if (this.IconOption == 1)
                {
                    float width = ImGui.CalcItemWidth();
                    if (this.CustomIcon > 0)
                    {
                        width -= height + padX;
                        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + height + padX);
                    }

                    ImGui.PushItemWidth(width);
                    if (ImGui.InputTextWithHint("Search", "Search Icons by Name or ID", ref _iconSearchInput, 32, ImGuiInputTextFlags.EnterReturnsTrue))
                    {
                        _iconSearchResults.Clear();
                        if (ushort.TryParse(_iconSearchInput, out ushort iconId))
                        {
                            _iconSearchResults.Add(new TriggerData("", 0, iconId));
                        }
                        else if (!string.IsNullOrEmpty(_iconSearchInput))
                        {
                            _iconSearchResults.AddRange(ActionHelpers.FindActionEntries(_iconSearchInput));
                            _iconSearchResults.AddRange(StatusHelpers.FindStatusEntries(_iconSearchInput));
                            _iconSearchResults.AddRange(ActionHelpers.FindItemEntries(_iconSearchInput));
                        }
                    }
                    ImGui.PopItemWidth();

                    if (_iconSearchResults.Any() && ImGui.BeginChild("##IconPicker", new Vector2(size.X - padX * 2, 60), true))
                    {
                        List<uint> icons = _iconSearchResults.Select(t => t.Icon).Distinct().ToList();
                        for (int i = 0; i < icons.Count; i++)
                        {
                            Vector2 iconPos = ImGui.GetWindowPos().AddX(10) + new Vector2(i * (40 + padX), padY);
                            Vector2 iconSize = new Vector2(40, 40);
                            this.DrawIconPreview(iconPos, iconSize, icons[i], this.CropIcon, false, true);

                            if (ImGui.IsMouseHoveringRect(iconPos, iconPos + iconSize))
                            {
                                if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                                {
                                    this.CustomIcon = icons[i];
                                    _iconSearchResults.Clear();
                                    _iconSearchInput = string.Empty;
                                }
                            }
                        }

                        ImGui.EndChild();
                    }
                }

                if (this.IconOption != 2)
                {
                    if (this.IconOption < 2)
                    {
                        ImGui.Checkbox("Crop Icon", ref this.CropIcon);
                    }
                    else if (this.IconOption == 3)
                    {
                        Vector4 vector = this.IconColor.Vector;
                        ImGui.ColorEdit4("Icon Color", ref vector, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar);
                        this.IconColor.Vector = vector;
                    }
                    else if (this.IconOption == 6)
                    {
                        Vector4 vector = this.IconColor.Vector;
                        ImGui.ColorEdit4("Active Color", ref vector, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar);
                        this.IconColor.Vector = vector;
                        if (UnitOption != 6)
                        {
                            Vector4 vector2 = this.IconColor2.Vector;
                            ImGui.ColorEdit4("Inactive Color", ref vector2, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar);
                            this.IconColor2.Vector = vector2;
                        }
                    }

                    DrawHelpers.DrawSpacing(1);
                    if (this.IconOption != 6 && this.IconOption != 4)
                    {
                        ImGui.DragFloat2("Position", ref this.Position, 1, -_screenSize.X / 2, _screenSize.X / 2);
                        ImGui.DragFloat2("Size", ref this.Size, 1, 0, _screenSize.Y);
                    }

                    if (this.IconOption == 6)
                    {
                        ImGui.DragFloat2("Position", ref this.Position, 1, -_screenSize.X / 2, _screenSize.X / 2);
                        ImGui.DragFloat("Radius", ref this.Size.X, 1, 0, _screenSize.Y);
                        /* Figure out how to make this work later
                        // determine which units we're working with from data
                        DrawHelpers.DrawSpacing(1);
                        ImGui.RadioButton("Static", ref this.UnitOption, 6);
                        ImGui.SameLine();
                        ImGui.RadioButton("HP", ref this.UnitOption, 0);
                        ImGui.SameLine();
                        ImGui.RadioButton("MP", ref this.UnitOption, 1);
                        ImGui.SameLine();
                        ImGui.RadioButton("GP", ref this.UnitOption, 2);
                        ImGui.SameLine();
                        ImGui.RadioButton("CP", ref this.UnitOption, 3);
                        ImGui.SameLine();
                        ImGui.RadioButton("Stacks", ref this.UnitOption, 4);
                        ImGui.SameLine();
                        ImGui.RadioButton("Time", ref this.UnitOption, 5);
                        */
                        switch (this.UnitOption)
                        {
                            // poll UnitOption to determine what kind of dynamic bar we're making
                            // currently limited to static max units. Considering options for cooldowns
                            case 0:
                                MaxThreshold = 100;
                                break;
                            case 1:
                                MaxThreshold = 10000;
                                break;
                            case 2:
                                MaxThreshold = 100;
                                break;
                            case 3:
                                MaxThreshold = 100;
                                break;
                            case 4:
                                MaxThreshold = 16;
                                break;
                            case 5:
                                MaxThreshold = 600;
                                break;
                            case 6:
                                Threshold = 0;
                                MaxThreshold = 0;
                                break;
                        }

                        if (this.UnitOption != 6)
                        ImGui.DragFloat("Threshold", ref this.Threshold, 1, 0, this.MaxThreshold);
                    }

                    if (this.IconOption != 6)
                    {
                        ImGui.Checkbox("Glow", ref this.Glow);
                        if (this.Glow)
                        {
                            DrawHelpers.DrawNestIndicator(1);
                            ImGui.DragInt("Thickness##Glow", ref this.GlowThickness, 1, 1, 16);

                            DrawHelpers.DrawNestIndicator(1);
                            ImGui.DragInt("Glow Segments##Glow", ref this.GlowSegments, 1, 2, 16);

                            DrawHelpers.DrawNestIndicator(1);
                            ImGui.DragFloat("Animation Speed##Glow", ref this.GlowSpeed, 0.05f, 0, 2f);

                            DrawHelpers.DrawNestIndicator(1);
                            Vector4 vector = this.GlowColor.Vector;
                            ImGui.ColorEdit4("Glow Color##Glow", ref vector, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar);
                            this.GlowColor.Vector = vector;

                            DrawHelpers.DrawNestIndicator(1);
                            vector = this.GlowColor2.Vector;
                            ImGui.ColorEdit4("Glow Color 2##Glow", ref vector, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar);
                            this.GlowColor2.Vector = vector;
                        }

                    }
                    
                    DrawHelpers.DrawSpacing(1);
                    ImGui.Checkbox("Show Border", ref this.ShowBorder);
                    if (this.ShowBorder)
                    {
                        DrawHelpers.DrawNestIndicator(1);
                        ImGui.DragInt("Border Thickness", ref this.BorderThickness, 1, 1, 100);

                        DrawHelpers.DrawNestIndicator(1);
                        Vector4 vector = this.BorderColor.Vector;
                        ImGui.ColorEdit4("Border Color", ref vector, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar);
                        this.BorderColor.Vector = vector;
                    }



                    if (this.IconOption != 6)
                    {
                        ImGui.Checkbox("Show Progress Swipe", ref this.ShowProgressSwipe);
                        if (this.ShowProgressSwipe)
                        {
                            DrawHelpers.DrawNestIndicator(1);
                            ImGui.DragFloat("Swipe Opacity", ref this.ProgressSwipeOpacity, .01f, 0, 1);
                            DrawHelpers.DrawNestIndicator(1);
                            ImGui.Checkbox("Invert Swipe", ref this.InvertSwipe);
                            DrawHelpers.DrawNestIndicator(1);
                            ImGui.Checkbox("Show GCD Swipe When Inactive", ref this.GcdSwipe);
                            if (this.GcdSwipe)
                            {
                                DrawHelpers.DrawNestIndicator(2);
                                ImGui.Checkbox("Only show GCD swipe", ref this.GcdSwipeOnly);
                            }

                            DrawHelpers.DrawNestIndicator(1);
                            ImGui.Checkbox("Show Swipe Lines", ref this.ShowSwipeLines);
                            if (this.ShowSwipeLines)
                            {
                                DrawHelpers.DrawNestIndicator(2);
                                Vector4 vector = this.ProgressLineColor.Vector;
                                ImGui.ColorEdit4("Line Color", ref vector, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar);
                                this.ProgressLineColor.Vector = vector;
                                DrawHelpers.DrawNestIndicator(2);
                                ImGui.DragInt("Thickness", ref this.ProgressLineThickness, 1, 1, 5);
                            }
                        }
                    }
                }
            }

            ImGui.EndChild();
        }

        private void DrawIconPreview(Vector2 iconPos, Vector2 iconSize, uint icon, bool crop, bool desaturate, bool text)
        {
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            DrawHelpers.DrawIcon(icon, iconPos, iconSize, crop, 0, desaturate, 1f, drawList);
            if (text)
            {
                string iconText = icon.ToString();
                Vector2 iconTextPos = iconPos + new Vector2(20 - ImGui.CalcTextSize(iconText).X / 2, 38);
                drawList.AddText(iconTextPos, 0xFFFFFFFF, iconText);
            }
        }
    }
}