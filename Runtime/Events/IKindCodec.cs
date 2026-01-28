using System;

namespace Validosik.Core.Network.Events
{
    public interface IKindCodec<TKind> where TKind : unmanaged, Enum
    {
        ushort ToByte(TKind k);
        TKind FromByte(ushort b);
    }
}