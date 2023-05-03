using System.Collections.Generic;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Internal.Notifications;
using ImGuiNET;
using Newtonsoft.Json;
using ReBuff.Widgets;
using ReBuff.Helpers;

namespace ReBuff.Config
{
    public class LabelListConfig : IConfigPage
    {
        [JsonIgnore]
        public string Name => "Labels";

        [JsonIgnore]
        private string _labelInput = string.Empty;

        public List<WidgetLabel> WidgetLabels { get; init; }

        public LabelListConfig()
        {
            this.WidgetLabels = new List<WidgetLabel>();
        }

        public LabelListConfig(params WidgetLabel[] labels)
        {
            this.WidgetLabels = new List<WidgetLabel>(labels);
        }

        public IConfigPage GetDefault()
        {
            WidgetLabel valueLabel = new WidgetLabel("Value", "[value:t]");
            valueLabel.LabelStyleConfig.FontKey = FontsManager.DefaultBigFontKey;
            valueLabel.LabelStyleConfig.FontID = Singletons.Get<FontsManager>().GetFontIndex(FontsManager.DefaultBigFontKey);
            valueLabel.StyleConditions.Conditions.Add(new StyleCondition<LabelStyleConfig>()
            {
                Source = TriggerDataSource.Value,
                Op = TriggerDataOp.Equals,
                Value = 0
            });

            WidgetLabel stacksLabel = new WidgetLabel("Stacks", "[stacks]");
            stacksLabel.LabelStyleConfig.FontKey = FontsManager.DefaultMediumFontKey;
            stacksLabel.LabelStyleConfig.FontID = Singletons.Get<FontsManager>().GetFontIndex(FontsManager.DefaultMediumFontKey);
            stacksLabel.LabelStyleConfig.Position = new Vector2(-1, 0);
            stacksLabel.LabelStyleConfig.ParentAnchor = DrawAnchor.BottomRight;
            stacksLabel.LabelStyleConfig.TextAlign = DrawAnchor.BottomRight;
            stacksLabel.LabelStyleConfig.TextColor = new ConfigColor(0, 0, 0, 1);
            stacksLabel.LabelStyleConfig.OutlineColor = new ConfigColor(1, 1, 1, 1);
            stacksLabel.StyleConditions.Conditions.Add(new StyleCondition<LabelStyleConfig>()
            {
                Source = TriggerDataSource.MaxStacks,
                Op = TriggerDataOp.LessThanEq,
                Value = 1
            });

            return new LabelListConfig(valueLabel, stacksLabel);
        }

        public void DrawConfig(IConfigurable parent, Vector2 size, float padX, float padY)
        {
            this.DrawLabelTable(size, padX);
        }

        private void DrawLabelTable(Vector2 size, float padX)
        {
            ImGuiTableFlags tableFlags =
                ImGuiTableFlags.RowBg |
                ImGuiTableFlags.Borders |
                ImGuiTableFlags.BordersOuter |
                ImGuiTableFlags.BordersInner |
                ImGuiTableFlags.ScrollY |
                ImGuiTableFlags.NoSavedSettings;

            if (ImGui.BeginTable("##Label_Table", 2, tableFlags, size))
            {
                Vector2 buttonSize = new Vector2(30, 0);
                float actionsWidth = buttonSize.X * 3 + padX * 2;

                ImGui.TableSetupColumn("Label Name", ImGuiTableColumnFlags.WidthStretch, 0, 0);
                ImGui.TableSetupColumn("Actions", ImGuiTableColumnFlags.WidthFixed, actionsWidth, 1);

                ImGui.TableSetupScrollFreeze(0, 1);
                ImGui.TableHeadersRow();

                int i = 0;
                for (; i < this.WidgetLabels.Count; i++)
                {
                    ImGui.PushID(i.ToString());
                    ImGui.TableNextRow(ImGuiTableRowFlags.None, 28);

                    WidgetLabel label = this.WidgetLabels[i];

                    if (ImGui.TableSetColumnIndex(0))
                    {
                        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 3f);
                        ImGui.Text(label.Name);
                    }

                    if (ImGui.TableSetColumnIndex(1))
                    {
                        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 1f);
                        DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.Pen, () => EditLabel(label), "Edit", buttonSize);

                        ImGui.SameLine();
                        DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.Upload, () => ExportLabel(label), "Export", buttonSize);

                        ImGui.SameLine();
                        DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.Trash, () => DeleteLabel(label), "Delete", buttonSize);
                    }
                }

                ImGui.PushID((i + 1).ToString());
                ImGui.TableNextRow(ImGuiTableRowFlags.None, 28);
                if (ImGui.TableSetColumnIndex(0))
                {
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 1f);
                    ImGui.PushItemWidth(ImGui.GetColumnWidth());
                    ImGui.InputTextWithHint("##LabelInput", "New Label Name", ref _labelInput, 10000);
                    ImGui.PopItemWidth();
                }

                if (ImGui.TableSetColumnIndex(1))
                {
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 1f);
                    DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.Plus, () => AddLabel(_labelInput), "Create Label", buttonSize);

                    ImGui.SameLine();
                    DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.Download, () => ImportLabel(), "Import Label", buttonSize);
                }

                ImGui.EndTable();
            }
        }

        private void AddLabel(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                this.WidgetLabels.Add(new WidgetLabel(name));
            }

            _labelInput = string.Empty;
        }

        private void ImportLabel()
        {
            string importString = string.Empty;
            try
            {
                importString = ImGui.GetClipboardText();
            }
            catch
            {
                DrawHelpers.DrawNotification("Failed to read from clipboard!", NotificationType.Error);
                return;
            }
            
            WidgetListItem? newWidget = ConfigHelpers.GetFromImportString<WidgetListItem>(importString);

            if (newWidget is WidgetLabel label)
            {
                this.WidgetLabels.Add(label);
            }
            else
            {
                DrawHelpers.DrawNotification("Failed to Import Widget!", NotificationType.Error);
            }

            _labelInput = string.Empty;
        }

        private void EditLabel(WidgetLabel label)
        {
            Singletons.Get<PluginManager>().Edit(label);
        }

        private void ExportLabel(WidgetLabel label)
        {
            ConfigHelpers.ExportToClipboard<WidgetLabel>(label);
        }

        private void DeleteLabel(WidgetLabel label)
        {
            this.WidgetLabels.Remove(label);
        }
    }
}