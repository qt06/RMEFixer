using System.Runtime.Versioning;

namespace RMEFixer;

[SupportedOSPlatform("windows")]
public sealed class AudioDeviceInfo
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public bool IsDefault => Id == -1;

    public override string ToString() => Name;
}
