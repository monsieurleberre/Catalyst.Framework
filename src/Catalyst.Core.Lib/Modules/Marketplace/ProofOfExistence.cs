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
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Catalyst.Common.Extensions;
using Catalyst.Common.Interfaces.Modules.Consensus.Deltas;
using Catalyst.Common.Interfaces.Modules.Dfs;
using Catalyst.Common.Interfaces.Modules.Marketplace;
using Catalyst.Common.Interfaces.P2P;
using Google.Protobuf;
using Multiformats.Hash;
using Multiformats.Hash.Algorithms;
using Serilog;

namespace Catalyst.Core.Lib.Modules.Marketplace
{
    public class ProofOfExistence : IProofOfExistence
    {
        private readonly IDfs _dfs;
        private readonly IDeltaHashProvider _deltaHashProvider;
        private readonly IMultihashAlgorithm _multihashAlgorithm;
        private readonly ConcurrentDictionary<IBlockChallenge, string> _challengeAnswers;
        private readonly IPeerIdentifier _peerIdentifier;
        private readonly IPeerClient _peerClient;
        private readonly ILogger _logger;

        private int BlockTestPercentage => 20;

        public ProofOfExistence(ILogger logger, 
            IPeerClient peerClient, 
            IPeerIdentifier peerIdentifier, 
            IDfs dfs, 
            IDeltaHashProvider deltaHashProvider, 
            IMultihashAlgorithm multihashAlgorithm)
        {
            _logger = logger;
            _peerClient = peerClient;
            _challengeAnswers = new ConcurrentDictionary<IBlockChallenge, string>();
            _deltaHashProvider = deltaHashProvider;
            _multihashAlgorithm = multihashAlgorithm;
            _dfs = dfs;
            _peerIdentifier = peerIdentifier;
        }

        public async Task Send(IPeerIdentifier recipientPeerIdentifier, string fileCid)
        {
            var latestDeltaHash = _deltaHashProvider.GetLatestDeltaHash(DateTime.UtcNow);
            var blockCids = await _dfs.GetFileBlockCids(fileCid);
            var minimumBlocksToCheck = Math.Max(1, blockCids.Length * (BlockTestPercentage / 100));
            var blocksToCheck = blockCids.Shuffle().Take(minimumBlocksToCheck).ToArray();

            var challenge = new BlockChallenge
            {
                MainFileCid = fileCid,
                ChallengeSalt = latestDeltaHash,
                BlockChallengeCids = blocksToCheck
            };
            
            var answer = await Answer(_peerIdentifier, challenge);

            if (_challengeAnswers.ContainsKey(challenge))
            {
                _logger.Information("Awaiting challenge: " + challenge.MainFileCid);
                return;
            }

            _challengeAnswers.GetOrAdd(challenge, answer);

            // TODO: Send Challenge
        }

        public bool Verify(IPeerIdentifier from, IBlockChallenge challenge, string answer)
        {
            if (!_challengeAnswers.ContainsKey(challenge))
            {
                _logger.Error("Challenge response does not exist in list.");
                return false;
            }

            _challengeAnswers.TryGetValue(challenge, out string expectedAnswer);

            if (string.IsNullOrWhiteSpace(expectedAnswer))
            {
                return false;
            }

            if (!expectedAnswer.Equals(answer))
            {
                _logger.Error($"Peer {from} provided wrong answer to {challenge.MainFileCid} storage challenge");
                return false;
            }

            _challengeAnswers.TryRemove(challenge, out _);

            // TODO: Broadcast validation result for % of file

            return true;
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
            var 
        }
    }
}
