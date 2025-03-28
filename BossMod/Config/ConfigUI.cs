using BossMod.Autorotation;
using Dalamud.Interface.Utility.Raii;
using ECommons;
using ECommons.DalamudServices;
using ECommons.ImGuiMethods;
using ImGuiNET;
using System.IO;
using System.Reflection;

namespace BossMod;

public sealed class ConfigUI : IDisposable
{
    private class UINode(ConfigNode node)
    {
        public ConfigNode Node = node;
        public string Name = "";
        public int Order;
        public UINode? Parent;
        public List<UINode> Children = [];
    }

    private readonly List<UINode> _roots = [];
    private readonly UITree _tree = new();
    private readonly UITabs _tabs = new();
    private readonly AboutTab _about;
    private readonly ModuleViewer _mv;
    private readonly ConfigRoot _root;
    private readonly WorldState _ws;
    private readonly UIPresetDatabaseEditor? _presets;
    public OpenWindow CurrrentOpenWindow { get; set; }

    public ConfigUI(ConfigRoot config, WorldState ws, DirectoryInfo? replayDir, RotationDatabase? rotationDB)
    {
        _root = config;
        _ws = ws;
        _about = new(replayDir);
        _mv = new(rotationDB?.Plans, ws);
        _presets = rotationDB != null ? new(rotationDB.Presets) : null;

        _tabs.Add("About", _about.Draw);
        _tabs.Add("Settings", DrawSettings);
        _tabs.Add("Supported Bosses", () => _mv.Draw(_tree, _ws));
        _tabs.Add("Autorotation Presets", () => _presets?.Draw());

        Dictionary<Type, UINode> nodes = [];
        foreach (var n in config.Nodes)
        {
            nodes[n.GetType()] = new(n);
        }

        foreach (var (t, n) in nodes)
        {
            var props = t.GetCustomAttribute<ConfigDisplayAttribute>();
            n.Name = props?.Name ?? GenerateNodeName(t);
            n.Order = props?.Order ?? 0;
            n.Parent = props?.Parent != null ? nodes.GetValueOrDefault(props.Parent) : null;

            var parentNodes = n.Parent?.Children ?? _roots;
            parentNodes.Add(n);
        }

        SortByOrder(_roots);
    }

    public void Dispose()
    {
        _mv.Dispose();
    }

    public void ShowTab(string name) => _tabs.Select(name);

    public void Draw()
    {
        _tabs.Draw();
    }
    public void DrawSettings()
    {
        var region = ImGui.GetContentRegionAvail();
        var itemSpacing = ImGui.GetStyle().ItemSpacing;

        var topLeftSideHeight = region.Y;

        ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, new Vector2(5f.Scale(), 0));
        try
        {
            using var table = ImRaii.Table($"BossModTableContainer", 2, ImGuiTableFlags.Resizable);
            if (!table)
                return;

            ImGui.TableSetupColumn("##LeftColumn", ImGuiTableColumnFlags.WidthFixed, ImGui.GetWindowWidth() / 2);

            ImGui.TableNextColumn();

            var regionSize = ImGui.GetContentRegionAvail();

            ImGui.PushStyleVar(ImGuiStyleVar.SelectableTextAlign, new Vector2(0.5f, 0.5f));
            using (var leftChild = ImRaii.Child($"###BossModLeftSide", regionSize with { Y = topLeftSideHeight }, true, ImGuiWindowFlags.NoDecoration))
            {
                var iconPath = Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName!, @"Images\ICON1.png");

                // Move the cursor a bit to the right before drawing the first icon
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() - 15f);  // Move 10 units to the right
                if (ThreadLoadImageHandler.TryGetTextureWrap(iconPath, out var icon))
                {
                    ImGuiEx.LineCentered("###ICON", () =>
                    {
                        ImGui.Image(icon.ImGuiHandle, new Vector2(100f.Scale(), 100f.Scale()));
                    });
                }
                ImGui.Spacing();
                if (ImGui.Selectable("System", CurrrentOpenWindow == OpenWindow.System))
                {
                    CurrrentOpenWindow = OpenWindow.System;
                }
                if (ImGui.Selectable("Party Roles", CurrrentOpenWindow == OpenWindow.PartyRoles))
                {
                    CurrrentOpenWindow = OpenWindow.PartyRoles;
                }
                if (ImGui.Selectable("Encounters", CurrrentOpenWindow == OpenWindow.Encounters))
                {
                    CurrrentOpenWindow = OpenWindow.Encounters;
                }
                if (ImGui.Selectable("Action Tweaks", CurrrentOpenWindow == OpenWindow.ActionTweaks))
                {
                    CurrrentOpenWindow = OpenWindow.ActionTweaks;
                }
                if (ImGui.Selectable("Autorotation", CurrrentOpenWindow == OpenWindow.Autorotation))
                {
                    CurrrentOpenWindow = OpenWindow.Autorotation;
                }
                if (ImGui.Selectable("Automation", CurrrentOpenWindow == OpenWindow.Automation))
                {
                    CurrrentOpenWindow = OpenWindow.Automation;
                }
                if (ImGui.Selectable("AI", CurrrentOpenWindow == OpenWindow.AI))
                {
                    CurrrentOpenWindow = OpenWindow.AI;
                }
                if (ImGui.Selectable("Replays", CurrrentOpenWindow == OpenWindow.Replays))
                {
                    CurrrentOpenWindow = OpenWindow.Replays;
                }
                if (ImGui.Selectable("Obstacle Maps", CurrrentOpenWindow == OpenWindow.ObstacleMaps))
                {
                    CurrrentOpenWindow = OpenWindow.ObstacleMaps;
                }
                if (ImGui.Selectable("Color Scheme", CurrrentOpenWindow == OpenWindow.ColorScheme))
                {
                    CurrrentOpenWindow = OpenWindow.ColorScheme;
                }
                ImGui.Spacing();
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() - 15f);
                if (ThreadLoadImageHandler.TryGetTextureWrap(iconPath, out var icon1))
                {
                    ImGuiEx.LineCentered("###ICON1", () =>
                    {
                        ImGui.Image(icon1.ImGuiHandle, new Vector2(100f.Scale(), 100f.Scale()));
                    });
                }
            }

            ImGui.PopStyleVar();
            ImGui.TableNextColumn();
            using var rightChild = ImRaii.Child($"###BossModRightSide", Vector2.Zero, true);
            ImGui.Spacing();

            switch (CurrrentOpenWindow)
            {
                case OpenWindow.System:
                    DrawSystem();
                    break;
                case OpenWindow.PartyRoles:
                    DrawPartyRoles();
                    break;
                case OpenWindow.Encounters:
                    DrawEncounters();
                    break;
                case OpenWindow.ActionTweaks:
                    DrawActionTweaks();
                    break;
                case OpenWindow.Autorotation:
                    DrawAutorotation();
                    break;
                case OpenWindow.Automation:
                    DrawAutomation();
                    break;
                case OpenWindow.AI:
                    DrawAI();
                    break;
                case OpenWindow.Replays:
                    DrawReplays();
                    break;
                case OpenWindow.ObstacleMaps:
                    DrawObstacleMaps();
                    break;
                case OpenWindow.ColorScheme:
                    DrawColorScheme();
                    break;
                case OpenWindow.None:
                    DrawNoSelection();
                    break;
                default:
                    break;
            }
                    ;
        }
        catch (Exception ex)
        {
            ex.Log();
        }
        ImGui.PopStyleVar();
    }
    private void DrawNoSelection()
    {
        var settingsPath = Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName!, @"Images\SETTINGS.png");
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() - 15f);  // Move 10 units to the right
        if (ThreadLoadImageHandler.TryGetTextureWrap(settingsPath, out var settings))
        {
            ImGuiEx.LineCentered("###Settings", () =>
            {
                ImGui.Image(settings.ImGuiHandle, new Vector2(300f.Scale(), 80f.Scale()));
            });
        }

        ImGuiEx.TextCentered("This is the Settings menu.");
        ImGui.Spacing();
        ImGuiEx.TextCentered("Here you can configure various options within the plugin.");
        ImGui.Spacing();
        ImGuiEx.TextCentered("Make sure to read tooltips for guidance when navigating!");
        ImGui.Spacing();
        ImGui.Separator();
    }
    private void DrawColorScheme()
    {
        var colorSchemeNode = _roots.FirstOrDefault(n => n.Name == "Color Scheme");
        if (colorSchemeNode != null)
        {
            DrawNode(colorSchemeNode.Node, _root, _tree, _ws);
            DrawNodes(colorSchemeNode.Children);
        }
        else
        {
            ImGui.Text("Color Scheme node not found.");
        }
    }

    private void DrawSystem()
    {
        var systemNode = _roots.FirstOrDefault(n => n.Name == "Boss Modules and Radar");
        if (systemNode != null)
        {
            DrawNode(systemNode.Node, _root, _tree, _ws);
            DrawNodes(systemNode.Children);
        }
        else
        {
            ImGui.Text("System node not found.");
        }
    }
    private void DrawPartyRoles()
    {
        var partyRolesNode = _roots.FirstOrDefault(n => n.Name == "Party Roles");
        if (partyRolesNode != null)
        {
            DrawNode(partyRolesNode.Node, _root, _tree, _ws);
        }
        else
        {
            ImGui.Text("Party Roles node not found.");
        }
    }
    private void DrawEncounters()
    {
        var encountersNode = _roots.FirstOrDefault(n => n.Name == "Encounter-Specific Options");
        if (encountersNode != null)
        {
            DrawNode(encountersNode.Node, _root, _tree, _ws);
            DrawNodes(encountersNode.Children);
        }
        else
        {
            ImGui.Text("Encounters node not found.");
        }
    }

    public void DrawActionTweaks()
    {
        var actionTweaksNode = _roots.FirstOrDefault(n => n.Name == "Action Tweaks");
        if (actionTweaksNode != null)
        {
            DrawNode(actionTweaksNode.Node, _root, _tree, _ws);
            DrawNodes(actionTweaksNode.Children);
        }
        else
        {
            ImGui.Text("Action Tweaks node not found.");
        }
    }
    private void DrawAutorotation()
    {
        var autorotationNode = _roots.FirstOrDefault(n => n.Name == "Autorotation");
        if (autorotationNode != null)
        {
            DrawNode(autorotationNode.Node, _root, _tree, _ws);
            DrawNodes(autorotationNode.Children);
        }
        else
        {
            ImGui.Text("Autorotation node not found.");
        }
    }
    private void DrawAutomation()
    {
        var automationNode = _roots.FirstOrDefault(n => n.Name == "Full duty automation");
        if (automationNode != null)
        {
            DrawNode(automationNode.Node, _root, _tree, _ws);
            DrawNodes(automationNode.Children);
        }
        else
        {
            ImGui.Text("Automation node not found.");
        }
    }
    private void DrawAI()
    {
        var aiNode = _roots.FirstOrDefault(n => n.Name == "AI Configuration");
        if (aiNode != null)
        {
            DrawNode(aiNode.Node, _root, _tree, _ws);
            DrawNodes(aiNode.Children);
        }
    }
    private void DrawReplays()
    {
        var replaysNode = _roots.FirstOrDefault(n => n.Name == "Replays");
        if (replaysNode != null)
        {
            DrawNode(replaysNode.Node, _root, _tree, _ws);
            DrawNodes(replaysNode.Children);
        }
        else
        {
            ImGui.Text("Replays node not found.");
        }
    }
    private void DrawObstacleMaps()
    {
        var obstacleMapsNode = _roots.FirstOrDefault(n => n.Name == "Obstacle map development");

        if (obstacleMapsNode != null)
        {
            DrawNode(obstacleMapsNode.Node, _root, _tree, _ws);
            DrawNodes(obstacleMapsNode.Children);
        }
        else
        {
            ImGui.Text("Obstacle map development node not found.");
        }
    }

    /*
    private void DrawNodes(List<UINode> nodes)
    {
        foreach (var n in _tree.Nodes(nodes, n => new(n.Name)))
        {
            using var child = ImRaii.Child(n.Name, new Vector2(0, 0), true,
                ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.AlwaysUseWindowPadding);
            DrawNode(n.Node, _root, _tree, _ws);
            DrawNodes(n.Children);
        }
    }
    */
    public static void DrawNode(ConfigNode node, ConfigRoot root, UITree tree, WorldState ws)
    {
        // Draw properties of the node (fields with PropertyDisplayAttribute)
        foreach (var field in node.GetType().GetFields())
        {
            var props = field.GetCustomAttribute<PropertyDisplayAttribute>();
            if (props == null)
                continue;

            var value = field.GetValue(node);
            if (DrawProperty(props.Label, props.Tooltip, node, field, value, root, tree, ws))
            {
                node.Modified.Fire();
            }

            if (props.Separator)
            {
                ImGui.Separator();
            }
        }

        // Draw custom content for the node (if any)
        node.DrawCustom(tree, ws);
    }

    private static string GenerateNodeName(Type t) => t.Name.EndsWith("Config", StringComparison.Ordinal) ? t.Name[..^"Config".Length] : t.Name;

    private void SortByOrder(List<UINode> nodes)
    {
        nodes.SortBy(e => e.Order);
        foreach (var n in nodes)
            SortByOrder(n.Children);
    }

    private void DrawNodes(List<UINode> nodes)
    {
        foreach (var n in _tree.Nodes(nodes, n => new(n.Name)))
        {
            DrawNode(n.Node, _root, _tree, _ws);
            DrawNodes(n.Children);
        }
    }

    private static void DrawHelp(string tooltip)
    {
        // draw tooltip marker with proper alignment
        ImGui.AlignTextToFramePadding();
        if (tooltip.Length > 0)
        {
            UIMisc.HelpMarker(tooltip);
        }
        else
        {
            using var invisible = ImRaii.PushColor(ImGuiCol.Text, 0x00000000);
            UIMisc.IconText(Dalamud.Interface.FontAwesomeIcon.InfoCircle, "(?)");
        }
        ImGui.SameLine();
    }

    private static bool DrawProperty(string label, string tooltip, ConfigNode node, FieldInfo member, object? value, ConfigRoot root, UITree tree, WorldState ws) => value switch
    {
        bool v => DrawProperty(label, tooltip, node, member, v),
        Enum v => DrawProperty(label, tooltip, node, member, v),
        float v => DrawProperty(label, tooltip, node, member, v),
        int v => DrawProperty(label, tooltip, node, member, v),
        string v => DrawProperty(label, tooltip, node, member, v),
        Color v => DrawProperty(label, tooltip, node, member, v),
        Color[] v => DrawProperty(label, tooltip, node, member, v),
        GroupAssignment v => DrawProperty(label, tooltip, node, member, v, root, tree, ws),
        _ => false
    };

    private static bool DrawProperty(string label, string tooltip, ConfigNode node, FieldInfo member, bool v)
    {
        DrawHelp(tooltip);
        var combo = member.GetCustomAttribute<PropertyComboAttribute>();
        if (combo != null)
        {
            if (UICombo.Bool(label, combo.Values, ref v))
            {
                member.SetValue(node, v);
                return true;
            }
        }
        else
        {
            if (ImGui.Checkbox(label, ref v))
            {
                member.SetValue(node, v);
                return true;
            }
        }
        return false;
    }

    private static bool DrawProperty(string label, string tooltip, ConfigNode node, FieldInfo member, Enum v)
    {
        DrawHelp(tooltip);
        if (UICombo.Enum(label, ref v))
        {
            member.SetValue(node, v);
            return true;
        }
        return false;
    }

    private static bool DrawProperty(string label, string tooltip, ConfigNode node, FieldInfo member, float v)
    {
        DrawHelp(tooltip);
        var slider = member.GetCustomAttribute<PropertySliderAttribute>();
        if (slider != null)
        {
            var flags = ImGuiSliderFlags.None;
            if (slider.Logarithmic)
                flags |= ImGuiSliderFlags.Logarithmic;
            ImGui.SetNextItemWidth(MathF.Min(ImGui.GetWindowWidth() * 0.30f, 175));
            if (ImGui.DragFloat(label, ref v, slider.Speed, slider.Min, slider.Max, "%.3f", flags))
            {
                member.SetValue(node, v);
                return true;
            }
        }
        else
        {
            if (ImGui.InputFloat(label, ref v))
            {
                member.SetValue(node, v);
                return true;
            }
        }
        return false;
    }

    private static bool DrawProperty(string label, string tooltip, ConfigNode node, FieldInfo member, int v)
    {
        DrawHelp(tooltip);
        var slider = member.GetCustomAttribute<PropertySliderAttribute>();
        if (slider != null)
        {
            var flags = ImGuiSliderFlags.None;
            if (slider.Logarithmic)
                flags |= ImGuiSliderFlags.Logarithmic;
            ImGui.SetNextItemWidth(MathF.Min(ImGui.GetWindowWidth() * 0.30f, 175));
            if (ImGui.DragInt(label, ref v, slider.Speed, (int)slider.Min, (int)slider.Max, "%d", flags))
            {
                member.SetValue(node, v);
                return true;
            }
        }
        else
        {
            if (ImGui.InputInt(label, ref v))
            {
                member.SetValue(node, v);
                return true;
            }
        }
        return false;
    }

    private static bool DrawProperty(string label, string tooltip, ConfigNode node, FieldInfo member, string v)
    {
        DrawHelp(tooltip);
        if (ImGui.InputText(label, ref v, 256))
        {
            member.SetValue(node, v);
            return true;
        }
        return false;
    }

    private static bool DrawProperty(string label, string tooltip, ConfigNode node, FieldInfo member, Color v)
    {
        DrawHelp(tooltip);
        var col = v.ToFloat4();
        if (ImGui.ColorEdit4(label, ref col, ImGuiColorEditFlags.PickerHueWheel))
        {
            member.SetValue(node, Color.FromFloat4(col));
            return true;
        }
        return false;
    }

    private static bool DrawProperty(string label, string tooltip, ConfigNode node, FieldInfo member, Color[] v)
    {
        var modified = false;
        for (int i = 0; i < v.Length; ++i)
        {
            DrawHelp(tooltip);
            var col = v[i].ToFloat4();
            if (ImGui.ColorEdit4($"{label} {i}", ref col, ImGuiColorEditFlags.PickerHueWheel))
            {
                v[i] = Color.FromFloat4(col);
                member.SetValue(node, v);
                modified = true;
            }
        }
        return modified;
    }

    private static bool DrawProperty(string label, string tooltip, ConfigNode node, FieldInfo member, GroupAssignment v, ConfigRoot root, UITree tree, WorldState ws)
    {
        var group = member.GetCustomAttribute<GroupDetailsAttribute>();
        if (group == null)
            return false;

        DrawHelp(tooltip);
        var modified = false;
        foreach (var tn in tree.Node(label, false, v.Validate() ? 0xffffffff : 0xff00ffff, () => DrawPropertyContextMenu(node, member, v)))
        {
            using var indent = ImRaii.PushIndent();
            using var table = ImRaii.Table("table", group.Names.Length + 2, ImGuiTableFlags.SizingFixedFit);
            if (!table)
                continue;

            foreach (var n in group.Names)
                ImGui.TableSetupColumn(n);
            ImGui.TableSetupColumn("----");
            ImGui.TableSetupColumn("Name");
            ImGui.TableHeadersRow();

            var assignments = root.Get<PartyRolesConfig>().SlotsPerAssignment(ws.Party);
            for (int i = 0; i < (int)PartyRolesConfig.Assignment.Unassigned; ++i)
            {
                var r = (PartyRolesConfig.Assignment)i;
                ImGui.TableNextRow();
                for (int c = 0; c < group.Names.Length; ++c)
                {
                    ImGui.TableNextColumn();
                    if (ImGui.RadioButton($"###{r}:{c}", v[r] == c))
                    {
                        v[r] = c;
                        modified = true;
                    }
                }
                ImGui.TableNextColumn();
                if (ImGui.RadioButton($"###{r}:---", v[r] < 0 || v[r] >= group.Names.Length))
                {
                    v[r] = -1;
                    modified = true;
                }

                string name = r.ToString();
                if (assignments.Length > 0)
                    name += $" ({ws.Party[assignments[i]]?.Name})";
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(name);
            }
        }
        return modified;
    }

    private static void DrawPropertyContextMenu(ConfigNode node, FieldInfo member, GroupAssignment v)
    {
        foreach (var preset in member.GetCustomAttributes<GroupPresetAttribute>())
        {
            if (ImGui.MenuItem(preset.Name))
            {
                for (int i = 0; i < preset.Preset.Length; ++i)
                    v.Assignments[i] = preset.Preset[i];
                node.Modified.Fire();
            }
        }
    }
    public enum OpenWindow
    {
        None = 0,
        System = 1,
        PartyRoles = 2,
        Encounters = 3,
        ActionTweaks = 4,
        Autorotation = 5,
        Automation = 6,
        AI = 7,
        Replays = 8,
        ObstacleMaps = 9,
        ColorScheme = 10
    }
}
