using System;

namespace Validosik.Core.Network.Events
{
    public interface IEnvelopeFactory
    {
        byte[] Make(ReadOnlySpan<byte> payload);
    }
}