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
using Catalyst.Protocol.DfsMarketplace;
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
        private readonly ConcurrentDictionary<string, string> _challengeAnswers;
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
            _challengeAnswers = new ConcurrentDictionary<string, string>();
            _deltaHashProvider = deltaHashProvider;
            _multihashAlgorithm = multihashAlgorithm;
            _dfs = dfs;
            _peerIdentifier = peerIdentifier;
        }

        public async Task<BlockChallengeRequest> Send(IPeerIdentifier recipientPeerIdentifier, string fileCid)
        {
            var latestDeltaHash = _deltaHashProvider.GetLatestDeltaHash(DateTime.UtcNow);
            var blockCids = await _dfs.GetFileBlockCids(fileCid);
            
            if (!blockCids.Any())
            {
                _logger.Error("No blocks to check for " + fileCid);
                return null;
            }

            var challenge = new BlockChallengeRequest
            {
                MainFileCid = fileCid,
                ChallengeSalt = latestDeltaHash
            };

            var challengeHash = challenge.ToByteArray().ComputeMultihash(_multihashAlgorithm).ToString();
            var answer = await Answer(_peerIdentifier, challenge, blockCids);

            if (_challengeAnswers.ContainsKey(challengeHash))
            {
                _logger.Information("Awaiting challenge: " + challenge.MainFileCid);
                return null;
            }

            _challengeAnswers.GetOrAdd(challengeHash, answer);
            
            // TODO: Send Challenge
            
            return challenge;
        }

        public bool Verify(IPeerIdentifier from, BlockChallengeResponse challenge)
        {
            var challengeHash = challenge.BlockChallengeRequestHash;
            if (!_challengeAnswers.ContainsKey(challengeHash))
            {
                _logger.Error("Challenge response does not exist in list.");
                return false;
            }

            _challengeAnswers.TryGetValue(challengeHash, out var expectedAnswer);

            if (string.IsNullOrWhiteSpace(expectedAnswer))
            {
                return false;
            }

            if (!expectedAnswer.Equals(challenge.Answer))
            {
                _logger.Error($"Peer {from} provided wrong answer to {challengeHash} storage challenge");
                return false;
            }

            _challengeAnswers.TryRemove(challengeHash, out _);

            // TODO: Broadcast validation result for % of file

            return true;
        }

        public async Task<string> Answer(IPeerIdentifier challengerPeerIdentifier, BlockChallengeRequest challenge, string[] blockCids = null)
        {
            Multihash answer;
            blockCids = blockCids ?? await _dfs.GetFileBlockCids(challenge.MainFileCid);
            var blocksToCheck = GetBlockCidsToCheck(blockCids, challenge.ChallengeSalt);
            using (var ms = new MemoryStream())
            {
                foreach (var challengeBlockChallengeCid in blocksToCheck)
                {
                    using (var blockStream = await _dfs.GetBlockAsync(challengeBlockChallengeCid))
                    {
                        var salt = System.Text.Encoding.UTF8.GetBytes(challenge.ChallengeSalt);
                        var peerSalt = challengerPeerIdentifier.PeerId.ToByteArray();

                        blockStream.CopyTo(ms);
                        ms.Write(salt);
                        ms.Write(peerSalt);
                    }
                }

                answer = ms.GetBuffer().ComputeMultihash(_multihashAlgorithm);
            }

            return answer?.ToString();
        }

        public async Task IncomingChallenge(IPeerIdentifier senderPeerIdentifier, BlockChallengeRequest challenge)
        {
            var answer = await Answer(senderPeerIdentifier, challenge);
            var blockChallengeRequestHash = challenge.ToByteArray().ComputeMultihash(_multihashAlgorithm);

            var challengeAnswerResponse = new BlockChallengeResponse
            {
                Answer = answer,
                BlockChallengeRequestHash = blockChallengeRequestHash
            };

            // TODO: Send broadcast answer
        }

        private string[] GetBlockCidsToCheck(string[] blockCids, string latestDeltaHash)
        {
            var minimumBlocksToCheck = Math.Max(1, blockCids.Length * (BlockTestPercentage / 100));
            var blocksToCheck = blockCids
               .RandomizeWithSeed(latestDeltaHash, minimumBlocksToCheck)
               .ToArray();
            return blocksToCheck;
        }
    }
}
