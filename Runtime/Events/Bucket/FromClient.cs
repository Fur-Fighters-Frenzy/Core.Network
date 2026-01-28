using Validosik.Core.Network.Types;

namespace Validosik.Core.Network.Events.Bucket
{
    public readonly struct FromClient<TDto>
        where TDto : INetworkDto
    {
        public readonly PlayerId Sender;
        public readonly TDto     Payload;

        public FromClient(PlayerId sender, TDto payload)
        {
            Sender  = sender;
            Payload = payload;
        }
    }
}