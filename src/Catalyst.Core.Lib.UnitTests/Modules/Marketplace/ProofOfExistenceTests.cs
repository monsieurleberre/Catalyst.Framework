using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Catalyst.Common.Cryptography;
using Catalyst.Common.Interfaces.Modules.Consensus.Deltas;
using Catalyst.Common.Interfaces.Modules.Dfs;
using Catalyst.Common.Interfaces.Modules.Marketplace;
using Catalyst.Common.Interfaces.P2P;
using Catalyst.Common.Util;
using Catalyst.Core.Lib.Modules.Marketplace;
using Catalyst.TestUtils;
using FluentAssertions;
using Multiformats.Hash;
using Multiformats.Hash.Algorithms;
using NSubstitute;
using Serilog;
using Xunit;

namespace Catalyst.Core.Lib.UnitTests.Modules.Marketplace
{
    public sealed class ProofOfExistenceTests : IDisposable
    {
        private readonly IProofOfExistence _proofOfExistence;
        private readonly IPeerIdentifier _peerIdentifier;
        private readonly IDfs _dfs;
        private readonly IDeltaHashProvider _deltaHashProvider;
        private readonly string[] _fakeBlockCids = {"FakeCidFileBlock1", "FakeCidFileBlock2", "FakeCidFileBlock3"};
        private readonly List<Stream> _fakeBlockStreams;

        public ProofOfExistenceTests()
        {
            _fakeBlockStreams = new List<Stream>();

            _peerIdentifier = PeerIdentifierHelper.GetPeerIdentifier("SENDER");
            _dfs = Substitute.For<IDfs>();
            _deltaHashProvider = Substitute.For<IDeltaHashProvider>();

            _deltaHashProvider.GetLatestDeltaHash(Arg.Any<DateTime>())
               .Returns(Multihash.Sum(HashType.ID, ByteUtil.GenerateRandomByteArray(32)));

            _dfs.GetFileBlockCids(Arg.Any<string>()).Returns(_fakeBlockCids);

            foreach (var fakeBlockCid in _fakeBlockCids)
            {
                var fakeBlockBytes = ByteUtil.GenerateRandomByteArray(1024);
                _dfs.GetBlockAsync(fakeBlockCid).Returns(info =>
                {
                    var ms = new MemoryStream(fakeBlockBytes);
                    _fakeBlockStreams.Add(ms);
                    return ms;
                });
            }

            _proofOfExistence = new ProofOfExistence(
                Substitute.For<ILogger>(),
                Substitute.For<IPeerClient>(),
                _peerIdentifier,
                _dfs,
                _deltaHashProvider,
                new BLAKE2B_256());
        }

        [Fact]
        public async Task Can_Pass_PoE_Challenge_With_Correct_Answer()
        {
            var challenge = await _proofOfExistence.Send(_peerIdentifier, "Any");
            var answer = await _proofOfExistence.Answer(_peerIdentifier, challenge);
            _proofOfExistence.Verify(_peerIdentifier, challenge, answer).Should().BeTrue();
        }

        [Fact]
        public async Task Can_Fail_PoE_Challenge_With_Incorrect_Answer()
        {
            var challenge = await _proofOfExistence.Send(_peerIdentifier, "Any");
            _proofOfExistence.Verify(_peerIdentifier, challenge, "A fake answer").Should().BeFalse();
        }

        public void Dispose()
        {
            _fakeBlockStreams.ForEach(stream => stream.Dispose());
            _fakeBlockStreams.Clear();
        }
    }
}
