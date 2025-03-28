using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using System.Diagnostics;
using System.IO;
using ECommons.DalamudServices;
using ECommons.ImGuiMethods;

namespace BossMod;

public sealed class AboutTab(DirectoryInfo? replayDir)
{
    private readonly Color SectionBgColor = Color.FromComponents(38, 38, 38);
    private readonly Color BorderColor = Color.FromComponents(178, 178, 178, 204);
    private readonly Color DiscordColor = Color.FromComponents(88, 101, 242);

    private string _lastErrorMessage = "";

    private void DrawNote(string note, string note2 = "")
    {
        using var color = ImRaii.PushColor(ImGuiCol.Text, 0xFF00FFFF);
        using var wrap = ImRaii.TextWrapPos(0);

        ImGuiEx.TextCentered(note);
        if (note2 != "")
            ImGuiEx.TextCentered(note2);
        ImGui.Separator();
    }
    private void DrawWarning(string warning, string warning2 = "")
    {
        using var color = ImRaii.PushColor(ImGuiCol.Text, 0xff0000ff); //red
        using var wrap = ImRaii.TextWrapPos(0);
        ImGuiEx.TextCentered(warning);
        if (warning2 != "")
            ImGuiEx.TextCentered(warning2);
        ImGui.Separator();
    }
    private void DrawSection(string summary, string pointsummary, string[] bulletPoints)
    {
        using var colorBackground = ImRaii.PushColor(ImGuiCol.ChildBg, SectionBgColor.ABGR);
        using var colorBorder = ImRaii.PushColor(ImGuiCol.Border, BorderColor.ABGR);
        using var section = ImRaii.Child(summary, new(0, 145), false, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.AlwaysUseWindowPadding);

        if (!section)
            return;

        ImGuiEx.TextCentered(summary);
        ImGui.Separator();

        ImGuiEx.TextWrapped(pointsummary);

        foreach (var point in bulletPoints)
        {
            ImGui.Bullet();
            ImGui.SameLine();
            ImGuiEx.TextWrapped(point);
        }

        ImGui.Spacing();
        ImGui.Spacing();
    }
    private void DrawButtons()
    {
        var windowWidth = ImGui.GetWindowWidth();
        var buttonWidth = 180; // Width of each button
        var spacing = 10f; // Space between buttons
        var totalButtonWidth = buttonWidth * 4 + spacing * 3;
        var offsetX = (windowWidth - totalButtonWidth) * 0.5f;
        ImGui.SetCursorPosX(offsetX);

        using (ImRaii.PushColor(ImGuiCol.Button, DiscordColor.ABGR))
        {
            if (ImGuiEx.Button("Puni.sh Discord", new Vector2(buttonWidth, 0)))
                _lastErrorMessage = OpenLink("https://discord.gg/Zzrcc8kmvy");
        }
        ImGui.SameLine();

        if (ImGui.Button("Boss Mod Repository", new Vector2(buttonWidth, 0)))
            _lastErrorMessage = OpenLink("https://github.com/awgil/ffxiv_bossmod");
        ImGui.SameLine();

        if (ImGui.Button("Boss Mod Wiki Tutorials", new Vector2(buttonWidth, 0)))
            _lastErrorMessage = OpenLink("https://github.com/awgil/ffxiv_bossmod/wiki");
        ImGui.SameLine();

        if (ImGui.Button("Open Replay Folder", new Vector2(buttonWidth, 0)) && replayDir != null)
            _lastErrorMessage = OpenDirectory(replayDir);

        // Center the error message
        if (!string.IsNullOrEmpty(_lastErrorMessage))
        {
            // Set cursor position to center the error message
            ImGui.SetCursorPosX((windowWidth - ImGui.CalcTextSize(_lastErrorMessage).X) * 0.5f);

            using var color = ImRaii.PushColor(ImGuiCol.Text, 0xff0000ff);
            ImGui.TextUnformatted(_lastErrorMessage);
        }
    }
    public void Draw()
    {
        #region Overview
        using var wrap = ImRaii.TextWrapPos(0);
        var overviewPath = Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName!, @"Images\OVERVIEW.png");
        if (ThreadLoadImageHandler.TryGetTextureWrap(overviewPath, out var overview))
        {
            ImGuiEx.LineCentered("###Overview", () =>
            {
                ImGui.Image(overview.ImGuiHandle, new Vector2(300f.Scale(), 80f.Scale()));
            });
        }
        DrawSection("The most extensive all-in-one tool in FFXIV.", "BossMod (vbm) offers a wide variety of advanced features, including:",
            ["Radar – A comprehensive mini-map that provides real-time visualization of mechanics and safe zones.",
            "Autorotation – Fully automated combat rotations, developed by our dedicated contributors.",
            "Cooldown Planner – A detailed scheduling tool for optimal ability usage in supported boss encounters.",
            "AI – Integrated Artificial Intelligence designed to support dynamic combat strategies. The possibilities are endless."]
        );
        ImGui.Separator();
        ImGui.Spacing();
        #endregion

        #region Radar
        var radarPath = Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName!, @"Images\RADAR.png");
        if (ThreadLoadImageHandler.TryGetTextureWrap(radarPath, out var radar))
        {
            ImGuiEx.LineCentered("###Radar", () =>
            {
                ImGui.Image(radar.ImGuiHandle, new Vector2(300f.Scale(), 80f.Scale()));
            });
        }

        DrawSection("The most notorious part of the plugin.", "Key features include:",
            ["Display of an on-screen tactical mini-map, showcasing player positions, boss locations, imminent AoE effects, and other critical mechanics.",
            "Elimination of the need for manual memorization of ability names, providing seamless gameplay.",
            "Offer of precise, real-time visual feedback on potential AoE impacts, ensuring quick reactions.",
            "Automatic activation for supported encounters, with a comprehensive list available under the 'Supported Bosses' tab."]);
        DrawWarning("NOTE: Not every fight is supported or has been made yet.");

        ImGui.Spacing();
        #endregion

        #region Autorotation
        var autorotPath = Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName!, @"Images\AUTOROTATION.png");
        if (ThreadLoadImageHandler.TryGetTextureWrap(autorotPath, out var autorot))
        {
            ImGuiEx.LineCentered("###AUTOROTATION", () =>
            {
                ImGui.Image(autorot.ImGuiHandle, new Vector2(300f.Scale(), 80f.Scale()));
            });
        }

        DrawSection("Of course, we couldn't be an all-in-one tool without Autorotation!", "Key features include:",
            ["Automatic execution of highly optimized combat rotations. ",
            "Intricate customization available in the 'Autorotation Presets' tab. ",
            "Accessiblity of rotation maturity details via tooltips.",
            "Comprehensive usage guidelines, provided in the GitHub wiki."]);
        DrawNote("Each Autorotation module is maintained by individual contributors.", "Please reach out to or ping the listed contributor of the Autorotation module you are using when reporting any issues.");
        ImGui.Spacing();
        #endregion

        #region CD Planner
        var cdpPath = Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName!, @"Images\CDPLANNER.png");
        if (ThreadLoadImageHandler.TryGetTextureWrap(cdpPath, out var cdp))
        {
            ImGuiEx.LineCentered("###CDPLANNER", () =>
            {
                ImGui.Image(cdp.ImGuiHandle, new Vector2(300f.Scale(), 80f.Scale()));
            });
        }

        DrawSection("The best way to explicitly plan any ability at any specific time in any fight.", "Key features include:",
            ["Facilitation of strategic cooldown management for supported encounters. ",
            "Precise ability queuing & execution, down to the last millisecond.",
            "Full timeline of supported fight, ensuring the proper timing for best outcome",
            "A full, detailed setup with instructions are available in our GitHub wiki."]);
        DrawWarning("NOTE: Not every fight is supported or has been made yet.");
        ImGui.Spacing();
        #endregion

        #region AI
        var aiPath = Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName!, @"Images\AI.png");
        if (ThreadLoadImageHandler.TryGetTextureWrap(aiPath, out var ai))
        {
            ImGuiEx.LineCentered("###AI", () =>
            {
                ImGui.Image(ai.ImGuiHandle, new Vector2(300f.Scale(), 80f.Scale()));
            });
        }

        DrawSection("Yes, you read that right. AI is also included!", "Key features include:",
            ["Enhancement of gameplay by automating movement during boss fights. ",
            "Automatic navigation of character based on dynamically detected safe zones, as displayed on the radar. ",
            "Integration with external plugins for full-duty automation.," +
            "Supports ALL content, from casual to hardcore."]);
        DrawWarning("NOTE: NOT recommended for use in group-based content.", "Although it is still possible, it is best to refrain from any potential risks.");
        ImGui.Spacing();
        #endregion

        #region Replays

        var replaysPath = Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName!, @"Images\REPLAYS.png");
        if (ThreadLoadImageHandler.TryGetTextureWrap(replaysPath, out var replays))
        {
            ImGuiEx.LineCentered("###REPLAYS", () =>
            {
                ImGui.Image(replays.ImGuiHandle, new Vector2(300f.Scale(), 80f.Scale()));
            });
        }

        DrawSection("Replays provide in-depth analysis for boss module development, troubleshooting, and cooldown planning.", "How to enable & send replays:",
            ["Enable via Settings > Show Replay Management UI (or activate automatic recording).",
            "Type '/vbm r' to open up the Replay menu",
            "You can manually go to your folder containing the Replay files by clicking 'Open Replay Folder'",
            $"Additionally, Replay files are stored in: '{replayDir}'."]);
        DrawNote("NOTE: When uploading/sharing a replay, please remember that you are also sharing your character's information as well, like character name.");
        ImGui.Spacing();
        #endregion

        #region Contributing
        var contributorPath = Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName!, @"Images\CONTRIBUTE.png");
        if (ThreadLoadImageHandler.TryGetTextureWrap(contributorPath, out var contributor))
        {
            ImGuiEx.LineCentered("###CONTRIBUTE", () =>
            {
                ImGui.Image(contributor.ImGuiHandle, new Vector2(300f.Scale(), 80f.Scale()));
            });
        }
        DrawNote("We are always looking for contributors to help improve the plugin's health & experience.", "If you are interested in contributing, these links below will guide you to where & how you can contribute.");
        DrawButtons();

        ImGui.Spacing();
        #endregion
    }

    private string OpenLink(string link)
    {
        try
        {
            Process.Start(new ProcessStartInfo(link) { UseShellExecute = true });
            return "";
        }
        catch (Exception e)
        {
            Service.Log($"Error opening link {link}: {e}");
            return $"Failed to open link '{link}', open it manually in the browser.";
        }
    }

    private string OpenDirectory(DirectoryInfo dir)
    {
        if (!dir.Exists)
            return $"Directory '{dir}' not found.";

        try
        {
            Process.Start(new ProcessStartInfo(dir.FullName) { UseShellExecute = true });
            return "";
        }
        catch (Exception e)
        {
            Service.Log($"Error opening directory {dir}: {e}");
            return $"Failed to open folder '{dir}', open it manually.";
        }
    }
}
