using System;

namespace Validosik.Core.Network.Dto
{
    /// <summary>
    /// DTO contract: instance-side serialization only.
    /// </summary>
    public interface INetworkDto
    {
        /// <summary>Total byte size if serialized now.</summary>
        int GetByteCount();

        /// <summary>Try to write into caller-provided buffer. False if buffer too small.</summary>
        bool TryWrite(Span<byte> destination, out int written);
    }
}