namespace Validosik.Core.Network.Transport.Interfaces
{
    public interface INetTickableServer
    {
        ushort Tick { get; }

        void NextTick();
    }
}