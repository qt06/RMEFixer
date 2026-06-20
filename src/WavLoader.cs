using System.Runtime.Versioning;

namespace RMEFixer;

public readonly record struct WavFormat(int SampleRate, int Channels, short BitsPerSample);

[SupportedOSPlatform("windows")]
public static class WavLoader
{
    public static byte[] Load(string path, out WavFormat format)
    {
        using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
        using var reader = new BinaryReader(fs);

        var riff = new string(reader.ReadChars(4));
        if (riff != "RIFF")
            throw new InvalidDataException("Not a RIFF file");

        reader.ReadInt32();

        var wave = new string(reader.ReadChars(4));
        if (wave != "WAVE")
            throw new InvalidDataException("Not a WAVE file");

        int sampleRate = 0, channels = 0, bitsPerSample = 0;
        byte[]? pcmData = null;

        while (reader.BaseStream.Position < reader.BaseStream.Length)
        {
            var chunkId = new string(reader.ReadChars(4));
            var chunkSize = reader.ReadInt32();

            switch (chunkId)
            {
                case "fmt ":
                    var formatTag = reader.ReadInt16();
                    if (formatTag != 1)
                        throw new InvalidDataException("Only PCM format is supported");

                    channels = reader.ReadInt16();
                    sampleRate = reader.ReadInt32();
                    reader.ReadInt32();
                    reader.ReadInt16();
                    bitsPerSample = reader.ReadInt16();

                    var remainingFmt = chunkSize - 16;
                    if (remainingFmt > 0)
                        reader.ReadBytes(remainingFmt);
                    break;

                case "data":
                    pcmData = reader.ReadBytes(chunkSize);
                    break;

                default:
                    reader.ReadBytes(chunkSize);
                    break;
            }

            if (chunkSize % 2 != 0)
                reader.ReadByte();
        }

        if (pcmData is null)
            throw new InvalidDataException("No data chunk found");

        format = new WavFormat(sampleRate, channels, (short)bitsPerSample);
        return pcmData;
    }
}
