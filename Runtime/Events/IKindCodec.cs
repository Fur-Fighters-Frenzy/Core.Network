namespace Validosik.Core.Events
{
    public interface IKindCodec<TKind> where TKind : struct
    {
        byte ToByte(TKind k);
        TKind FromByte(byte b);
    }
}