namespace Validosik.Core.Network.Events.Bucket
{
    public interface IEventDto<TKind> : INetworkDtoBroadcast where TKind : struct
    {
        TKind Kind { get; }
    }
}