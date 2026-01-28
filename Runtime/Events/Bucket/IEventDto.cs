namespace Validosik.Core.Network.Events.Bucket
{
    public interface IEventDto<TKind>
        where TKind : unmanaged, Enum
    {
        TKind Kind { get; }
    }
}