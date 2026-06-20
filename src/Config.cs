using System.Runtime.Versioning;
using System.Text.Json;

namespace RMEFixer;

public readonly record struct HotkeyBinding(uint Modifiers, uint VirtualKey)
{
    public override string ToString()
    {
        var parts = new List<string>(4);
        if ((Modifiers & 0x0001) != 0) parts.Add("Alt");
        if ((Modifiers & 0x0002) != 0) parts.Add("Ctrl");
        if ((Modifiers & 0x0004) != 0) parts.Add("Shift");
        if ((Modifiers & 0x0008) != 0) parts.Add("Win");
        parts.Add(VkToName(VirtualKey));
        return string.Join("+", parts);
    }

    private static string VkToName(uint vk) =>
        vk switch
        {
            >= 0x70 and <= 0x87 => $"F{(vk - 0x70) + 1}",
            >= 0x30 and <= 0x39 => ((char)(vk - 0x30 + '0')).ToString(),
            >= 0x41 and <= 0x5A => ((char)(vk - 0x41 + 'A')).ToString(),
            0x20 => "Space",
            0x0D => "Enter",
            0x09 => "Tab",
            0x1B => "Esc",
            0x08 => "Backspace",
            0x2E => "Delete",
            0x24 => "Home",
            0x23 => "End",
            0x21 => "PageUp",
            0x22 => "PageDown",
            0x25 => "Left",
            0x27 => "Right",
            0x26 => "Up",
            0x28 => "Down",
            0x2D => "Insert",
            0x13 => "Pause",
            0x2C => "PrintScreen",
            _ => $"0x{vk:X}"
        };
}

public static class HotkeyParser
{
    private static readonly Dictionary<string, uint> ModMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Ctrl"] = 0x0002,
        ["Alt"] = 0x0001,
        ["Win"] = 0x0008,
        ["Shift"] = 0x0004,
    };

    private static readonly Dictionary<string, uint> KeyMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Space"] = 0x20, ["Enter"] = 0x0D, ["Tab"] = 0x09, ["Esc"] = 0x1B,
        ["Backspace"] = 0x08, ["Delete"] = 0x2E, ["Insert"] = 0x2D,
        ["Home"] = 0x24, ["End"] = 0x23, ["PageUp"] = 0x21, ["PageDown"] = 0x22,
        ["Left"] = 0x25, ["Right"] = 0x27, ["Up"] = 0x26, ["Down"] = 0x28,
        ["Pause"] = 0x13, ["PrintScreen"] = 0x2C, ["CapsLock"] = 0x14,
        ["NumLock"] = 0x90, ["ScrollLock"] = 0x91,
        ["Divide"] = 0x6F, ["Multiply"] = 0x6A, ["Subtract"] = 0x6D,
        ["Add"] = 0x6B, ["Decimal"] = 0x6E,
    };

    static HotkeyParser()
    {
        for (uint i = 0; i <= 9; i++)
            KeyMap[i.ToString()] = 0x30 + i;
        for (uint i = 0; i < 26; i++)
            KeyMap[((char)('A' + i)).ToString()] = 0x41 + i;
        for (uint i = 1; i <= 24; i++)
            KeyMap[$"F{i}"] = 0x6F + i;
    }

    public static bool TryParse(string input, out HotkeyBinding binding)
    {
        binding = default;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var parts = input.Split('+', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2) return false;

        uint mods = 0;
        for (int i = 0; i < parts.Length - 1; i++)
        {
            if (!ModMap.TryGetValue(parts[i], out var m)) return false;
            mods |= m;
        }

        if (!KeyMap.TryGetValue(parts[^1], out var vk)) return false;

        binding = new HotkeyBinding(mods, vk);
        return true;
    }
}

public static class ConfigManager
{
    private static string ConfigPath => Path.Combine(AppContext.BaseDirectory, "config.json");

    private static readonly HotkeyBinding DefaultHotkey = new(0x0002 | 0x0008 | 0x0001, 0x79); // Ctrl+Win+Alt+F10

    public static HotkeyBinding Load()
    {
        if (!File.Exists(ConfigPath))
        {
            Save(DefaultHotkey);
            return DefaultHotkey;
        }

        try
        {
            var json = File.ReadAllText(ConfigPath);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("hotkey", out var prop) && prop.ValueKind == JsonValueKind.String)
            {
                var str = prop.GetString()!;
                if (HotkeyParser.TryParse(str, out var binding))
                    return binding;

                System.Diagnostics.Debug.WriteLine($"config.json: 无法解析快捷键 \"{str}\"，使用默认值");
            }

            return DefaultHotkey;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"config.json 加载失败: {ex.Message}");
            return DefaultHotkey;
        }
    }

    private static void Save(HotkeyBinding binding)
    {
        try
        {
            var json = $$"""
{
  "hotkey": "{{binding}}"
}
""";
            File.WriteAllText(ConfigPath, json);
        }
        catch { }
    }
}
