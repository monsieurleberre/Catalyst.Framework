using System.Collections.Generic;
using Catalyst.Common.Interfaces.P2P;
using Ipfs;

namespace Catalyst.Common.Modules.Marketplace
{
    public interface IBlockChallengeResponse
    {
        Cid MainFileCid { get; set; }
        Dictionary<Cid, string> ChallengeResponses { get; set; }
        IPeerIdentifier SenderPeerIdentifier { get; set; }
    }
}
