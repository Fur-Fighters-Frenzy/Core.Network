using System;
using System.Buffers.Binary;
using Validosik.Core.Network.Types;

namespace Validosik.Core.Network.Dto
{
    /// <summary>
    /// Handshake: [u64 matchId][u16 seq][u8 playerId][guid(16 bytes)] => 27 bytes total.
    /// Integers: little-endian. Guid: via Guid.TryWriteBytes / new Guid(ReadOnlySpan&lt;byte&gt;).
    /// </summary>
    public readonly partial struct HandshakeDto : INetworkDto
    {
        public const int    Size        = 27;
        public const ushort MessageType = 0;

        public readonly ulong    MatchId;
        public readonly ushort   Seq;
        public readonly PlayerId PlayerId;
        public readonly Guid     Token;

        public HandshakeDto(ulong matchId, ushort seq, PlayerId playerId, Guid token)
        {
            MatchId = matchId;
            Seq = seq;
            PlayerId = playerId;
            Token = token;
        }

        public int GetByteCount() => Size;

        public bool TryWrite(Span<byte> destination, out int written)
        {
            written = 0;
            if (destination.Length < Size)
            {
                return false;
            }

            BinaryPrimitives.WriteUInt64LittleEndian(destination.Slice(0, 8), MatchId);
            BinaryPrimitives.WriteUInt16LittleEndian(destination.Slice(8, 2), Seq);
            destination[10] = PlayerId.Value;

            if (!Token.TryWriteBytes(destination.Slice(11, 16)))
            {
                return false;
            }

            written = Size;
            return true;
        }

        public static bool TryFromBytes(ReadOnlySpan<byte> source, out HandshakeDto dto)
        {
            dto = default;
            if (source.Length < Size)
            {
                return false;
            }

            var match = BinaryPrimitives.ReadUInt64LittleEndian(source.Slice(0, 8));
            var seq = BinaryPrimitives.ReadUInt16LittleEndian(source.Slice(8, 2));
            var pid = source[10];
            var token = new Guid(source.Slice(11, 16));

            dto = new HandshakeDto(match, seq, new PlayerId(pid), token);
            return true;
        }
    }
}