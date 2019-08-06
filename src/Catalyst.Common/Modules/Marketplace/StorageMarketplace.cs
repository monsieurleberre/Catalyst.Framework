using Catalyst.Common.Interfaces.Modules.Marketplace;
using Catalyst.Common.Interfaces.P2P;
using Catalyst.Common.Interfaces.Repository;
using Catalyst.Common.P2P;

namespace Catalyst.Common.Modules.Marketplace
{
    public sealed class StorageMarketplace : IStorageMarketplace
    {
        private readonly IStorageNodeRepository _repository;

        public StorageMarketplace(IStorageNodeRepository repository)
        {
            _repository = repository;
        }

        public void AddPeer(IPeerIdentifier peerIdentifier)
        {
            _repository.Add(new Peer()
            {
                PeerIdentifier = peerIdentifier
            });
        }
    }
}
