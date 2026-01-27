using System;

namespace Validosik.Core.Events
{
    public interface IEnvelopeFactory
    {
        byte[] Make(ReadOnlySpan<byte> payload);
    }
}