﻿using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Dalamud.Interface;
using Dalamud.Interface.Internal.Notifications;
using ImGuiNET;
using Newtonsoft.Json;
using XIVAuras.Auras;
using XIVAuras.Helpers;

namespace XIVAuras.Config
{
    public class AuraListConfig : IConfigPage
    {
        [JsonIgnore] private AuraType _selectedType = AuraType.Group;
        [JsonIgnore] private string _input = string.Empty;
        [JsonIgnore] private string[] _options = Enum.GetNames(typeof(AuraType));

        public string Name => "Auras";

        public List<IAuraListItem> Auras { get; init; }

        public AuraListConfig()
        {
            this.Auras = new List<IAuraListItem>();
        }

        public void DrawConfig()
        {
            this.DrawCreateMenu();
            this.DrawAuraTable();
        }

        private void DrawCreateMenu()
        {
            if (ImGui.BeginChild("##Buttons", new Vector2(484, 40), true))
            {
                ImGui.PushItemWidth(200);
                ImGui.InputTextWithHint("##Input", "Aura Name/Import String", ref _input, 10000);
                ImGui.PopItemWidth();

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.Combo("##Type", ref Unsafe.As<AuraType, int>(ref _selectedType), _options, _options.Length);

                ImGui.SameLine();
                DrawHelpers.DrawButton("Create", FontAwesomeIcon.Plus, () => CreateAura(_selectedType, _input), "Create new Aura or Group");

                ImGui.SameLine();
                DrawHelpers.DrawButton("Import", FontAwesomeIcon.Download, () => ImportAura(_input), "Import new Aura or Group");
                ImGui.PopItemWidth();

                ImGui.EndChild();
            }
        }

        private void DrawAuraTable()
        {
            ImGuiTableFlags flags =
                ImGuiTableFlags.RowBg |
                ImGuiTableFlags.Borders |
                ImGuiTableFlags.BordersOuter |
                ImGuiTableFlags.BordersInner |
                ImGuiTableFlags.ScrollY |
                ImGuiTableFlags.SizingFixedSame |
                ImGuiTableFlags.NoSavedSettings;

            if (ImGui.BeginTable("##Auras_Table", 3, flags, new Vector2(484, 389)))
            {
                ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch, 65, 0);
                ImGui.TableSetupColumn("Type", ImGuiTableColumnFlags.WidthStretch, 15, 1);
                ImGui.TableSetupColumn("Actions", ImGuiTableColumnFlags.WidthStretch, 20, 2);

                ImGui.TableSetupScrollFreeze(0, 1);
                ImGui.TableHeadersRow();

                for (int i = 0; i < this.Auras.Count; i++)
                {
                    IAuraListItem aura = this.Auras[i];

                    ImGui.PushID(i.ToString());
                    ImGui.TableNextRow(ImGuiTableRowFlags.None, 28);

                    if (ImGui.TableSetColumnIndex(0))
                    {
                        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 3f);
                        ImGui.Text(aura.Name);
                    }

                    if (ImGui.TableSetColumnIndex(1))
                    {
                        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 3f);
                        ImGui.Text(aura.Type.ToString());
                    }

                    if (ImGui.TableSetColumnIndex(2))
                    {
                        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 1f);
                        DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.Pen, () => EditAura(aura), "Edit");

                        ImGui.SameLine();
                        DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.Upload, () => ExportAura(aura), "Export");

                        ImGui.SameLine();
                        DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.Trash, () => DeleteAura(aura), "Delete");
                    }
                }

                ImGui.EndTable();
            }
        }

        private void CreateAura(AuraType type, string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                IAuraListItem? newAura = type switch
                {
                    AuraType.Group => new AuraGroup(name),
                    AuraType.Icon => new AuraIcon(name),
                    AuraType.Bar => new AuraBar(name),
                    _ => null
                };

                if (newAura is not null)
                {
                    this.Auras.Add(newAura);
                    this.EditAura(newAura);
                }
            }

            this._input = string.Empty;
        }

        private void EditAura(IAuraListItem aura)
        {
            Singletons.Get<PluginManager>().Edit(aura);
        }

        private void DeleteAura(IAuraListItem aura)
        {
            this.Auras.Remove(aura);
        }

        private void ImportAura(string importString)
        {
            if (!string.IsNullOrEmpty(importString))
            {
                IAuraListItem? newAura = ConfigHelpers.GetAuraFromImportString(importString);

                if (newAura is not null)
                {
                    this.Auras.Add(newAura);
                }
                else
                {
                    DrawHelpers.DrawNotification("Failed to Import Aura!", NotificationType.Error);
                }
            }

            this._input = string.Empty;
        }

        private void ExportAura(IAuraListItem aura)
        {
            string? exportString = ConfigHelpers.GetAuraExportString(aura);

            if (exportString is not null)
            {
                ImGui.SetClipboardText(exportString);
                DrawHelpers.DrawNotification("Export string copied to clipboard.");
            }
            else
            {
                DrawHelpers.DrawNotification("Failed to Export Aura!", NotificationType.Error);
            }
        }
    }
}
