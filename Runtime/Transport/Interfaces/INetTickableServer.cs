namespace App.Network.Core.Transport.Interfaces
{
    public interface INetTickableServer
    {
        ushort Tick { get; }

        void NextTick();
    }
}