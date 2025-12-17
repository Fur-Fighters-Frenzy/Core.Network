namespace Validosik.Core.Network.Transport
{
    public enum ChannelKind : byte
    {
        ReliableOrdered = 0,  // FishNet: Reliable;  LiteNetLib: ReliableOrdered
        Unreliable      = 1,  // FishNet: Unreliable; LiteNetLib: Unreliable
        Sequenced       = 2   // FishNet: Unreliable?*; LiteNetLib: Sequenced
    }
}