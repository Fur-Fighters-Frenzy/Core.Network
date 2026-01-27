using System.Collections.Generic;

namespace App.Network.Core.Mapping
{
    internal abstract class BytePlayerRegistry<T>
    {
        internal const byte None = 255;

        private static readonly EqualityComparer<T> Eq = EqualityComparer<T>.Default;

        // pid -> T
        private readonly T[] _byPid = new T[255];

        // T -> pid
        private readonly Dictionary<T, byte> _byConnection = new();

        public IReadOnlyDictionary<T, byte> Values => _byConnection;

        /// Find free pid
        private bool TryAcquirePid(out byte pid)
        {
            for (byte i = 0; i < _byPid.Length; ++i)
            {
                if (!Eq.Equals(_byPid[i], default))
                {
                    continue;
                }

                pid = i;
                return true;
            }

            pid = None;
            return false;
        }

        public bool TryGetPid(T value, out byte pid) =>
            _byConnection.TryGetValue(value, out pid);

        public bool TryGetValue(byte pid, out T value)
        {
            if (pid < _byPid.Length)
            {
                value = _byPid[pid];
                return true;
            }

            value = default;
            return false;
        }

        public bool Allocate(T value, out byte pid)
        {
            if (value is null)
            {
                pid = None;
                return false;
            }

            if (_byConnection.TryGetValue(value, out pid))
            {
                return true; // already exists
            }

            if (!TryAcquirePid(out pid))
            {
                return false;
            }

            _byPid[pid] = value;
            _byConnection[value] = pid;
            return true;
        }

        public byte ReleaseByValue(T value)
        {
            if (value is null)
            {
                return None;
            }

            if (!_byConnection.TryGetValue(value, out var pid))
            {
                return None;
            }

            _byConnection.Remove(value);
            if (pid < _byPid.Length && _byPid[pid].Equals(value))
            {
                _byPid[pid] = default;
            }

            return pid;
        }

        public void ReleaseByPid(byte pid)
        {
            if (!TryGetValue(pid, out var value)
                || Eq.Equals(value, default))
            {
                return;
            }

            _byConnection.Remove(value);
            _byPid[pid] = default;
        }
    }
}