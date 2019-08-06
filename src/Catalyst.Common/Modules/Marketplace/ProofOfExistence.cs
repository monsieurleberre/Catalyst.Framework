#region LICENSE

/**
* Copyright (c) 2019 Catalyst Network
*
* This file is part of Catalyst.Node <https://github.com/catalyst-network/Catalyst.Node>
*
* Catalyst.Node is free software: you can redistribute it and/or modify
* it under the terms of the GNU General Public License as published by
* the Free Software Foundation, either version 2 of the License, or
* (at your option) any later version.
*
* Catalyst.Node is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with Catalyst.Node. If not, see <https://www.gnu.org/licenses/>.
*/

#endregion

using System;
using System.IO;
using System.Threading.Tasks;
using Catalyst.Common.Extensions;
using Catalyst.Common.Interfaces.Modules.Consensus.Deltas;
using Catalyst.Common.Interfaces.Modules.Dfs;
using Catalyst.Common.Interfaces.Modules.Marketplace;
using Catalyst.Common.Interfaces.P2P;
using Google.Protobuf;
using Multiformats.Hash;
using Multiformats.Hash.Algorithms;

namespace Catalyst.Common.Modules.Marketplace
{
    public class ProofOfExistence : IProofOfExistence
    {
        private readonly IDfs _dfs;
        private readonly IDeltaHashProvider _deltaHashProvider;
        private readonly IMultihashAlgorithm _multihashAlgorithm;
        private int BlockTestPercentage => 20;
        
        public ProofOfExistence(IDfs dfs, IDeltaHashProvider deltaHashProvider, IMultihashAlgorithm multihashAlgorithm)
        {
            _deltaHashProvider = deltaHashProvider;
            _multihashAlgorithm = multihashAlgorithm;
            _dfs = dfs;
        }

        public async Task Send(IPeerIdentifier recipientPeerIdentifier, string fileCid)
        {
            var latestDeltaHash = _deltaHashProvider.GetLatestDeltaHash(DateTime.UtcNow);
            var blockCids = await _dfs.GetFileBlockCids(fileCid);
            var minimumBlocksToCheck = Math.Max(1, blockCids.Length * (BlockTestPercentage / 100));
            
            var challenge = new BlockChallenge
            {
                MainFileCid = fileCid,
                ChallengeSalt = latestDeltaHash,
            };
        }

        public void Verify(IBlockChallengeResponse challengeResponse)
        {

        }

        private async Task<string> Answer(IPeerIdentifier senderPeerIdentifier, IBlockChallenge challenge)
        {
            Multihash answer;
            using (var ms = new MemoryStream())
            {
                foreach (var challengeBlockChallengeCid in challenge.BlockChallengeCids)
                {
                    using (var blockStream = await _dfs.GetBlockAsync(challengeBlockChallengeCid))
                    {
                        var salt = System.Text.Encoding.UTF8.GetBytes(challenge.ChallengeSalt);
                        var peerSalt = senderPeerIdentifier.PeerId.ToByteArray();

                        blockStream.CopyTo(blockStream);
                        ms.Write(salt);
                        ms.Write(peerSalt);
                    }
                }

                answer = ms.GetBuffer().ComputeMultihash(_multihashAlgorithm);
            }

            return answer?.ToString();
        }

        public async Task IncomingChallenge(IPeerIdentifier senderPeerIdentifier, IBlockChallenge challenge)
        {
           
        }
    }
}
