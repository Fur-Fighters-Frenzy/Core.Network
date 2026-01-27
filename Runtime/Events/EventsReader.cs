using System;
using System.Buffers.Binary;

namespace Validosik.Core.Events
{
    /// TLV reader: [u16 count] { [u8 kind][u16 len][blob] }*
    public ref struct EventsReader<TKind, TCodec>
        where TKind : struct
        where TCodec : struct, IKindCodec<TKind>
    {
        private readonly ReadOnlySpan<byte> _span;
        private          int                _off;
        private          ushort             _remain;
        private          TCodec             _codec;

        public EventsReader(ReadOnlySpan<byte> payload)
        {
            _span = payload;
            _off = 0;
            _codec = default;
            _remain = 0;

            if (_span.Length < 2)
            {
                return;
            }

            _remain = BinaryPrimitives.ReadUInt16LittleEndian(_span.Slice(0, 2));
            _off = 2;
        }

        public bool TryRead(out TKind kind, out ReadOnlySpan<byte> blob)
        {
            kind = default;
            blob = default;

            if (_remain == 0) return false;
            if (_off + 3 > _span.Length)
            {
                _remain = 0;
                return false;
            }

            kind = _codec.FromByte(_span[_off++]);
            var len = BinaryPrimitives.ReadUInt16LittleEndian(_span.Slice(_off, 2));
            _off += 2;

            if (_off + len > _span.Length)
            {
                _remain = 0;
                return false;
            }

            blob = _span.Slice(_off, len);
            _off += len;
            --_remain;
            return true;
        }
    }
}