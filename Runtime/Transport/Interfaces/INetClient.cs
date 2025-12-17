using System;

namespace Validosik.Core.Network.Transport.Interfaces
{
    public interface INetClient
    {
        void Send(ReadOnlySpan<byte> raw, ChannelKind ch = ChannelKind.ReliableOrdered);

        event Action<ReadOnlyMemory<byte>, ChannelKind> OnServerMessage;

        event Action OnConnectedAsClient;
    }
}