using System;
using Validosik.Core.Network.Dto;

namespace Validosik.Core.Network.Events.Bucket
{
    public interface IEventDto<TKind> : INetworkDto
        where TKind : unmanaged, Enum
    {
        TKind Kind { get; }
    }
}