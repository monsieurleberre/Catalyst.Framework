using System.Collections.Generic;
using Ipfs;

namespace Catalyst.Common.Modules.Marketplace
{
    public interface IBlockChallengeResponse
    {
        Cid MainFileCid { get; set; }

        Dictionary<Cid, string> ChallengeResponses { get; set; }
    }
}
