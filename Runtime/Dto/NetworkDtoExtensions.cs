using System;

namespace Validosik.Core.Network.Dto
{
    public static class NetworkDtoExtensions
    {
        /// <summary>Allocate and serialize.</summary>
        public static byte[] ToBytes(this INetworkDto dto)
        {
            var size = dto.GetByteCount();
            var buf = new byte[size];
            if (!dto.TryWrite(buf, out var written) || written != size)
            {
                throw new InvalidOperationException("Failed to serialize DTO.");
            }

            return buf;
        }
    }
}