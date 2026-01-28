using System;
using System.Collections.Generic;
using Validosik.Core.Network.Types;

namespace Validosik.Core.Network.Mapping
{
    internal abstract class PlayerConnectionMapping<T, TConnectionRegistry, TTokenRegistry>
        where TConnectionRegistry : BytePlayerRegistry<T>, new()
        where TTokenRegistry : BytePlayerRegistry<Guid>, new()
    {
        private readonly TConnectionRegistry      _connectionRegistry;
        private readonly BytePlayerRegistry<Guid> _tokenRegistry;

        protected PlayerConnectionMapping()
        {
            _connectionRegistry = new TConnectionRegistry();
            _tokenRegistry = new TTokenRegistry();
        }

        public IReadOnlyDictionary<T, byte> AllConnections => _connectionRegistry.Values;
        public IReadOnlyDictionary<Guid, byte> AllTokens => _tokenRegistry.Values;

        /// If the pid already exists, we'll return it.
        /// If not, we'll allocate it.
        public (PlayerId pid, Guid token) MapConnectionToPlayer(T connection)
        {
            var token = Guid.NewGuid();
            while (_tokenRegistry.TryGetPid(token, out var testPid)
                   && testPid != PlayerId.None)
            {
                token = Guid.NewGuid();
            }

            if (!_connectionRegistry.Allocate(connection, out var pid))
            {
                return (PlayerId.None, Guid.Empty); // There are no free slots (you can report an error)
            }

            if (!_tokenRegistry.Allocate(token, out _))
            {
                return (PlayerId.None, Guid.Empty); // There are no free slots (you can report an error)
            }

            return (new PlayerId(pid), token);
        }

        public PlayerId ReleaseConnection(T connection)
        {
            var pid = _connectionRegistry.ReleaseByValue(connection);
            _tokenRegistry.ReleaseByPid(pid);
            return pid;
        }

        public bool TryGetPid(T connection, out byte pid) =>
            _connectionRegistry.TryGetPid(connection, out pid);

        public bool TryGetConnection(PlayerId pid, out T connection) =>
            _connectionRegistry.TryGetValue(pid.Value, out connection);

        public bool TryGetToken(PlayerId pid, out Guid token) =>
            _tokenRegistry.TryGetValue(pid.Value, out token);
    }
}