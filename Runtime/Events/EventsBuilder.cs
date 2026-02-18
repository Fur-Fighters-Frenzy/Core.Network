using System;
using System.Buffers;
using System.Buffers.Binary;
using Validosik.Core.Network.Transport.Interfaces;
using Validosik.Core.Network.Types;
using Validosik.Core.Network.Events.Bucket;
using Validosik.Core.Network.Transport;

namespace Validosik.Core.Network.Events
{
    /// TLV buffer: [u16 count] { [u16 kind][u16 len][blob] }*
    public class EventsBuilder<TKind, TCodec, TEnvelope> : IDisposable
        where TKind : unmanaged, Enum
        where TCodec : struct, IKindCodec<TKind>
        where TEnvelope : struct, IEnvelopeFactory
    {
        private const int HeaderReserve = 2;
        private byte[] _buffer;
        private int _written;
        private ushort _count;
        private readonly TCodec _codec;
        private readonly TEnvelope _env;

        private byte[] _envBuffer;
        private int _envWritten;

        public EventsBuilder(int initialCapacity = 256)
        {
            _buffer = ArrayPool<byte>.Shared.Rent(Math.Max(initialCapacity, HeaderReserve));
            _written = HeaderReserve;
            _count = 0;
            _codec = default;
            _env = default;

            _envBuffer = ArrayPool<byte>.Shared.Rent(Math.Max(initialCapacity + 4, 64));
            _envWritten = 0;
        }

        /// <summary>Clears current batch.</summary>
        public EventsBuilder<TKind, TCodec, TEnvelope> Reset()
        {
            _written = HeaderReserve;
            _count = 0;
            return this;
        }

        public EventsBuilder<TKind, TCodec, TEnvelope> AddRaw(TKind kind, ReadOnlySpan<byte> payload)
        {
            Ensure(2 + 2 + payload.Length);

            BinaryPrimitives.WriteUInt16LittleEndian(_buffer.AsSpan(_written, 2), _codec.ToByte(kind));
            _written += 2;

            BinaryPrimitives.WriteUInt16LittleEndian(_buffer.AsSpan(_written, 2), (ushort)payload.Length);
            _written += 2;

            payload.CopyTo(_buffer.AsSpan(_written));
            _written += payload.Length;
            ++_count;

            return this;
        }

        /// <summary>
        /// Safe add: returns false if DTO serialization is invalid.
        /// Useful for untrusted input / defensive code.
        /// </summary>
        public bool TryAddDto<TDto>(in TDto dto) where TDto : struct, IEventDto<TKind>
        {
            var size = dto.GetByteCount();
            switch (size)
            {
                case <= 0:
                    return false;

                case <= 64:
                {
                    Span<byte> tmp = stackalloc byte[size];
                    if (!dto.TryWrite(tmp, out var w) || w != size)
                    {
                        return false;
                    }

                    AddRaw(dto.Kind, tmp);
                    return true;
                }

                default:
                {
                    var buf = ArrayPool<byte>.Shared.Rent(size);
                    try
                    {
                        var span = buf.AsSpan(0, size);
                        if (!dto.TryWrite(span, out var w) || w != size)
                        {
                            return false;
                        }

                        AddRaw(dto.Kind, span);
                        return true;
                    }
                    finally
                    {
                        ArrayPool<byte>.Shared.Return(buf);
                    }
                }
            }
        }

        /// <summary>
        /// Fluent add: throws if DTO serialization is invalid.
        /// This is what you use in your own server logic (MVP style).
        /// </summary>
        public EventsBuilder<TKind, TCodec, TEnvelope> AddDto<TDto>(in TDto dto)
            where TDto : struct, IEventDto<TKind>
        {
            if (!TryAddDto(dto))
            {
                throw new InvalidOperationException(
                    $"Failed to serialize DTO {typeof(TDto).Name}. " +
                    $"Check GetByteCount/TryWrite consistency.");
            }

            return this;
        }

        public ReadOnlySpan<byte> BuildPayload()
        {
            BinaryPrimitives.WriteUInt16LittleEndian(_buffer.AsSpan(0, 2), _count);
            return new ReadOnlySpan<byte>(_buffer, 0, _written);
        }

        private ReadOnlySpan<byte> BuildEnvelopeSpan()
        {
            var payload = BuildPayload();

            var need = _env.GetByteCount(payload.Length);
            EnsureEnvelope(need);

            if (!_env.TryWrite(payload, _envBuffer.AsSpan(0, need), out _envWritten) || _envWritten <= 0)
            {
                throw new InvalidOperationException("Failed to build envelope.");
            }

            return new ReadOnlySpan<byte>(_envBuffer, 0, _envWritten);
        }

        public virtual void Send(INetServer server, PlayerId to, ChannelKind ch = ChannelKind.ReliableOrdered) =>
            server.Send(to, BuildEnvelopeSpan(), ch);

        public virtual void Broadcast(INetServer server, ChannelKind ch = ChannelKind.ReliableOrdered) =>
            server.Broadcast(BuildEnvelopeSpan(), ch);

        public virtual void BroadcastExcept(INetServer server, PlayerId except,
            ChannelKind ch = ChannelKind.ReliableOrdered) =>
            server.BroadcastExcept(except, BuildEnvelopeSpan(), ch);

        public virtual void BroadcastExcept(INetServer server, ChannelKind ch = ChannelKind.ReliableOrdered,
            params PlayerId[] except) =>
            server.BroadcastExcept(except, BuildEnvelopeSpan(), ch);

        public virtual void Send(INetClient client, ChannelKind ch = ChannelKind.ReliableOrdered) =>
            client.Send(BuildEnvelopeSpan(), ch);

        public void Dispose()
        {
            if (_buffer != null)
            {
                ArrayPool<byte>.Shared.Return(_buffer);
                _buffer = null;
            }

            if (_envBuffer != null)
            {
                ArrayPool<byte>.Shared.Return(_envBuffer);
                _envBuffer = null;
            }
        }

        private void Ensure(int more)
        {
            if (_written + more <= _buffer.Length)
            {
                return;
            }

            var newSize = Math.Max(_buffer.Length * 2, _written + more);
            var newBuffer = ArrayPool<byte>.Shared.Rent(newSize);
            Buffer.BlockCopy(_buffer, 0, newBuffer, 0, _written);
            ArrayPool<byte>.Shared.Return(_buffer);
            _buffer = newBuffer;
        }

        private void EnsureEnvelope(int need)
        {
            if (need <= _envBuffer.Length)
            {
                return;
            }

            var newSize = Math.Max(_envBuffer.Length * 2, need);
            var newBuffer = ArrayPool<byte>.Shared.Rent(newSize);
            ArrayPool<byte>.Shared.Return(_envBuffer);
            _envBuffer = newBuffer;
        }
    }
}