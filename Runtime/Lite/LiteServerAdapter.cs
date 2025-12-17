#if LITENETLIB_ENABLED
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Validosik.Core.Network.Transport;
using Validosik.Core.Network.Transport.Interfaces;
using Validosik.Core.Network.Types;
using LiteNetLib;

namespace Validosik.Core.Network.Lite
{
    public sealed class LiteServerAdapter : INetServer, INetEventListener, IDisposable
    {
        public event Action<PlayerId, ReadOnlyMemory<byte>, ChannelKind> OnClientMessage;
        public event Action<PlayerId> OnClientDisconnected;
        public event Action<PlayerId> OnClientConnected;

        private readonly NetManager                    _server;
        private readonly Dictionary<PlayerId, NetPeer> _peers = new();
        private readonly LiteChannelMap                _map;
        private readonly string                        _connectionKey;
        private          bool                          _disposed;

        public LiteServerAdapter(int port, LiteChannelMap map = null, string connectionKey = "proto")
        {
            _map = map ?? new LiteChannelMap();
            _server = new NetManager(this)
            {
                AutoRecycle = true,
                IPv6Enabled = false
            };

            if (!_server.Start(port))
            {
                throw new InvalidOperationException($"LiteNetLib server failed to start on port {port}.");
            }

            _connectionKey = connectionKey;
        }

        public void Send(PlayerId to, ReadOnlySpan<byte> raw, ChannelKind ch = ChannelKind.ReliableOrdered)
        {
            if (_disposed)
            {
                return;
            }

            if (!_peers.TryGetValue(to, out var peer))
            {
                return;
            }

            var (delivery, channel) = _map.ToLite(ch);
            peer.Send(raw.ToArray(), channel, delivery);
        }

        public void Broadcast(ReadOnlySpan<byte> raw, ChannelKind ch = ChannelKind.ReliableOrdered)
        {
            if (_disposed)
            {
                return;
            }

            var (delivery, channel) = _map.ToLite(ch);
            foreach (var peer in _peers.Values)
            {
                peer.Send(raw.ToArray(), channel, delivery);
            }
        }

        public void BroadcastExcept(PlayerId[] except, ReadOnlySpan<byte> raw,
            ChannelKind ch = ChannelKind.ReliableOrdered)
        {
            if (_disposed)
            {
                return;
            }

            if (except == null || except.Length == 0)
            {
                Broadcast(raw, ch);
                return;
            }

            var skip = new HashSet<byte>();
            foreach (var pid in except)
            {
                skip.Add(pid.Value);
            }

            var (delivery, channel) = _map.ToLite(ch);
            var bytes = raw.ToArray();

            foreach (var (pid, peer) in _peers)
            {
                if (skip.Contains(pid))
                {
                    continue;
                }

                peer.Send(bytes, channel, delivery);
            }
        }

        public void Poll()
        {
            if (_disposed)
            {
                return;
            }

            _server.PollEvents();
        }

        public void Disconnect(PlayerId pid)
        {
            if (_peers.TryGetValue(pid, out var peer))
            {
                peer.Disconnect();
            }
        }


        // --- INetEventListener ---
        public void OnPeerConnected(NetPeer peer)
        {
            // TODO: create peer to PlayerId mapping
            var pid = (byte)peer.Id;
            _peers[pid] = peer;
            OnClientConnected?.Invoke(pid);
        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            var pid = (byte)peer.Id;
            _peers.Remove(pid);
            OnClientDisconnected?.Invoke(pid);
        }

        public void OnNetworkError(System.Net.IPEndPoint endPoint, SocketError socketError)
        {
            /* no-op */
        }

        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber,
            DeliveryMethod deliveryMethod)
        {
            var data = reader.GetRemainingBytes();
            reader.Recycle();

            var pid = new PlayerId((byte)peer.Id);
            var kind = _map.FromLite(deliveryMethod, channelNumber);

            OnClientMessage?.Invoke(pid, data, kind);
        }

        public void OnNetworkReceiveUnconnected(System.Net.IPEndPoint remoteEndPoint, NetPacketReader reader,
            UnconnectedMessageType messageType)
        {
            reader.Recycle();
        }

        public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
            /* client side not used */
        }

        public void OnConnectionRequest(ConnectionRequest request) =>
            request.AcceptIfKey(_connectionKey);

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _server.Stop();
        }
    }
}
#endif