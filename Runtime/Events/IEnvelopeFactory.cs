using System;

namespace Validosik.Core.Network.Events
{
    public interface IEnvelopeFactory
    {
        int GetByteCount(int payloadLen);

        bool TryWrite(ReadOnlySpan<byte> payload, Span<byte> destination, out int written);
    }
}