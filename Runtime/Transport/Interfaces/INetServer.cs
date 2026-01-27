using System;
using Validosik.Core.Network.Types;

namespace Validosik.Core.Network.Transport.Interfaces
{
    public interface INetServer
    {
        event Action<PlayerId> OnClientDisconnected;
        event Action<PlayerId> OnClientConnected;
        
        void Send(PlayerId to, ReadOnlySpan<byte> raw, ChannelKind ch = ChannelKind.ReliableOrdered);
        void Broadcast(ReadOnlySpan<byte> raw, ChannelKind ch = ChannelKind.ReliableOrdered);
        void BroadcastExcept(PlayerId except, ReadOnlySpan<byte> raw, ChannelKind ch = ChannelKind.ReliableOrdered);
        void BroadcastExcept(PlayerId[] except, ReadOnlySpan<byte> raw, ChannelKind ch = ChannelKind.ReliableOrdered);
        
        void Poll();
        
        void Disconnect(PlayerId pid);

        event Action<PlayerId, ReadOnlyMemory<byte>, ChannelKind> OnClientMessage;
    }
}