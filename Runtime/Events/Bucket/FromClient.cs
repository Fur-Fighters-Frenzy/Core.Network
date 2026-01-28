using Validosik.Core.Network.Types;
using Validosik.Core.Buckets;

namespace Validosik.Core.Network.Events.Bucket
{
    public readonly struct FromClient<TDto> : IBroadcast
        where TDto : INetworkDtoBroadcast
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