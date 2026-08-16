using System.Buffers.Binary;
using System.Security.Cryptography;

namespace TestR.Domain;

public static class SequentialGuid
{
    public static Guid Create()
    {
        Span<byte> bytes = stackalloc byte[16];
        RandomNumberGenerator.Fill(bytes[6..]);

        var timestampMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        bytes[0] = (byte)(timestampMs >> 40);
        bytes[1] = (byte)(timestampMs >> 32);
        bytes[2] = (byte)(timestampMs >> 24);
        bytes[3] = (byte)(timestampMs >> 16);
        bytes[4] = (byte)(timestampMs >> 8);
        bytes[5] = (byte)timestampMs;

        bytes[6] = (byte)((bytes[6] & 0x0F) | 0x70);
        bytes[8] = (byte)((bytes[8] & 0x3F) | 0x80);

        BinaryPrimitives.WriteUInt32LittleEndian(bytes[..4], BinaryPrimitives.ReadUInt32BigEndian(bytes[..4]));
        BinaryPrimitives.WriteUInt16LittleEndian(bytes[4..6], BinaryPrimitives.ReadUInt16BigEndian(bytes[4..6]));
        BinaryPrimitives.WriteUInt16LittleEndian(bytes[6..8], BinaryPrimitives.ReadUInt16BigEndian(bytes[6..8]));

        return new Guid(bytes);
    }
}
