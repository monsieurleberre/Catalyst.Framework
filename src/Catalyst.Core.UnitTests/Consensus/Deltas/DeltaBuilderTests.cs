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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Catalyst.Abstractions.Consensus;
using Catalyst.Abstractions.Consensus.Deltas;
using Catalyst.Abstractions.Cryptography;
using Catalyst.Abstractions.P2P;
using Catalyst.Common.Extensions;
using Catalyst.Common.Utils;
using Catalyst.Core.Consensus;
using Catalyst.Core.Consensus.Deltas;
using Catalyst.Core.Cryptography;
using Catalyst.Core.Extensions;
using Catalyst.Protocol.Deltas;
using Catalyst.Protocol.Extensions;
using Catalyst.Protocol.Peer;
using Catalyst.Protocol.Transaction;
using Catalyst.Protocol.Wire;
using Catalyst.TestUtils;
using FluentAssertions;
using Google.Protobuf;
using Multiformats.Hash.Algorithms;
using Nethereum.Hex.HexConvertors.Extensions;
using NSubstitute;
using Serilog;
using Xunit;

namespace Catalyst.Core.UnitTests.Consensus.Deltas
{
    public sealed class DeltaBuilderTests
    {
        private readonly IDeterministicRandomFactory _randomFactory;
        private readonly IMultihashAlgorithm _hashAlgorithm;
        private readonly Random _random;
        private readonly PeerId _producerId;
        private readonly byte[] _previousDeltaHash;
        private readonly CoinbaseEntry _zeroCoinbaseEntry;
        private readonly IDeltaCache _cache;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILogger _logger;

        public DeltaBuilderTests()
        {
            _random = new Random();

            _hashAlgorithm = Substitute.For<IMultihashAlgorithm>();
            _hashAlgorithm.ComputeHash(Arg.Any<byte[]>()).Returns(ci => ((byte[]) ci[0]).Reverse().ToArray());

            _randomFactory = Substitute.For<IDeterministicRandomFactory>();
            _randomFactory.GetDeterministicRandomFromSeed(Arg.Any<byte[]>())
               .Returns(ci => new IsaacRandom(((byte[]) ci[0]).ToHex()));

            _producerId = PeerIdHelper.GetPeerId("producer");

            _previousDeltaHash = Encoding.UTF8.GetBytes("previousDelta");
            _zeroCoinbaseEntry = new CoinbaseEntry {Amount = 0, ReceiverPublicKey = _producerId.PublicKey};

            _logger = Substitute.For<ILogger>();

            _cache = Substitute.For<IDeltaCache>();

            _dateTimeProvider = new DateTimeProvider();
        }

        [Fact]
        public void BuildDeltaEmptyPoolContent()
        {
            var transactionRetriever = Substitute.For<IDeltaTransactionRetriever>();
            transactionRetriever.GetMempoolTransactionsByPriority().Returns(new List<TransactionBroadcast>());
            
            var deltaBuilder = new DeltaBuilder(transactionRetriever, _randomFactory, _hashAlgorithm, _producerId, _cache, _dateTimeProvider, _logger);

            var candidate = deltaBuilder.BuildCandidateDelta(_previousDeltaHash);

            ValidateDeltaCandidate(candidate, _zeroCoinbaseEntry.ToByteArray());

            _cache.Received(1).AddLocalDelta(Arg.Is(candidate), Arg.Any<Delta>());
        }

        [Fact]
        public void BuildDeltaInvalidTransactionsBasedOnLockTime()
        {
            var random = new Random(12);

            var invalidTransactionList = Enumerable.Range(0, 20).Select(i =>
            {
                var transaction = TransactionHelper.GetTransaction(
                    transactionFees: 954,
                    timeStamp: 157);
                return transaction;
            }).ToList();

            var transactionRetriever = Substitute.For<IDeltaTransactionRetriever>();
            transactionRetriever.GetMempoolTransactionsByPriority().Returns(invalidTransactionList);

            var deltaBuilder = new DeltaBuilder(transactionRetriever, _randomFactory, _hashAlgorithm, _producerId, _cache, _dateTimeProvider, _logger);
            var candidate = deltaBuilder.BuildCandidateDelta(_previousDeltaHash);

            ValidateDeltaCandidate(candidate, _zeroCoinbaseEntry.ToByteArray());

            _cache.Received(1).AddLocalDelta(Arg.Is(candidate), Arg.Any<Delta>());
        }

        [Fact]
        public void BuildDeltaCheckForAccuracy()
        {
            var transactions = Enumerable.Range(0, 20).Select(i =>
            {
                var transaction = TransactionHelper.GetTransaction(
                    standardAmount: (uint) i,
                    standardPubKey: i.ToString(),
                    transactionFees: (ulong) _random.Next(),
                    timeStamp: _random.Next());
                return transaction;
            }).ToList();

            var transactionRetriever = Substitute.For<IDeltaTransactionRetriever>();
            transactionRetriever.GetMempoolTransactionsByPriority().Returns(transactions);

            var selectedTransactions = transactions.Where(t => t.IsPublicTransaction).ToArray();

            var expectedCoinBase = new CoinbaseEntry
            {
                Amount = selectedTransactions.Sum(t => t.SummedEntryFees),
                ReceiverPublicKey = _producerId.PublicKey
            };

            var salt = BitConverter.GetBytes(
                _randomFactory.GetDeterministicRandomFromSeed(_previousDeltaHash).NextInt());

            var rawAndSaltedEntriesBySignature = selectedTransactions.SelectMany(
                t => t.PublicEntries.Select(e => new
                {
                    RawEntry = e,
                    SaltedAndHashedEntry = e.ToByteArray().Concat(salt).ComputeRawHash(_hashAlgorithm)
                }));

            var shuffledEntriesBytes = rawAndSaltedEntriesBySignature
               .OrderBy(v => v.SaltedAndHashedEntry, ByteUtil.ByteListComparer.Default)
               .SelectMany(v => v.RawEntry.ToByteArray())
               .ToArray();

            var signaturesInOrder = selectedTransactions
               .Select(p => p.ToByteArray())
               .OrderBy(s => s, ByteUtil.ByteListComparer.Default)
               .SelectMany(b => b)
               .ToArray();

            var expectedBytesToHash = shuffledEntriesBytes.Concat(signaturesInOrder)
               .Concat(expectedCoinBase.ToByteArray()).ToArray();

            var deltaBuilder = new DeltaBuilder(transactionRetriever, _randomFactory, _hashAlgorithm, _producerId, _cache, _dateTimeProvider, _logger);
            var candidate = deltaBuilder.BuildCandidateDelta(_previousDeltaHash);

            ValidateDeltaCandidate(candidate, expectedBytesToHash);

            _cache.Received(1).AddLocalDelta(Arg.Is(candidate), Arg.Any<Delta>());
        }

        private void ValidateDeltaCandidate(CandidateDeltaBroadcast candidate, byte[] expectedBytesToHash)
        {
            candidate.Should().NotBeNull();
            candidate.ProducerId.Should().Be(_producerId);
            candidate.PreviousDeltaDfsHash.ToByteArray().SequenceEqual(_previousDeltaHash).Should().BeTrue();

            var expectedHash = expectedBytesToHash.ComputeMultihash(_hashAlgorithm);
            candidate.Hash.ToByteArray().SequenceEqual(expectedHash.ToBytes()).Should().BeTrue();
        }
    }
}
