namespace Validosik.Core.Network.Events
{
    public interface IKindCodec<TKind> where TKind : struct
    {
        ushort ToByte(TKind k);
        TKind FromByte(ushort b);
    }
}