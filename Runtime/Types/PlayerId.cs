using System;

namespace Validosik.Core.Network.Types
{
    public readonly struct PlayerId : IEquatable<PlayerId>
    {
        public readonly byte Value;
        public static readonly PlayerId None = new(255);
        public PlayerId(byte value) => Value = value;
        public override string ToString() => Value.ToString();
        public bool Equals(PlayerId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is PlayerId o && Equals(o);
        public override int GetHashCode() => Value;
        public static implicit operator byte(PlayerId v) => v.Value;
        public static implicit operator PlayerId(byte v) => new PlayerId(v);
    }
}