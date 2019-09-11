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

using Catalyst.Abstractions.KeySigner;
using Catalyst.Abstractions.Keystore;
using Catalyst.Common.Utils;
using Catalyst.Core.IO.Handlers;
using Catalyst.Core.Keystore;
using Catalyst.Cryptography.BulletProofs.Wrapper;
using Catalyst.Cryptography.BulletProofs.Wrapper.Interfaces;
using Catalyst.Protocol.Cryptography;
using Catalyst.Protocol.Extensions;
using Catalyst.Protocol.Network;
using Catalyst.Protocol.Wire;
using Catalyst.TestUtils;
using DotNetty.Transport.Channels;
using Google.Protobuf;
using NSubstitute;
using Xunit;

namespace Catalyst.Core.UnitTests.IO.Handlers
{
    public sealed class ProtocolMessageVerifyHandlerTests
    {
        private readonly IChannelHandlerContext _fakeContext;
        private readonly ProtocolMessage _protocolMessage;
        private readonly IKeySigner _keySigner;
        private readonly ISigningContextProvider _signingContextProvider;

        public ProtocolMessageVerifyHandlerTests()
        {
            _fakeContext = Substitute.For<IChannelHandlerContext>();
            _keySigner = Substitute.For<IKeySigner>();
            _signingContextProvider = new SigningContextProvider(NetworkType.Devnet, SignatureType.ProtocolPeer);

            var signatureBytes = ByteUtil.GenerateRandomByteArray(FFI.SignatureLength);
            var publicKeyBytes = ByteUtil.GenerateRandomByteArray(FFI.PublicKeyLength);


            _protocolMessage = new ProtocolMessage
            {
                Signature = signatureBytes.AsProtoSignature(_signingContextProvider.SigningContext),
                Value = new ProtocolMessage
                {
                    PeerId = PeerIdentifierHelper.GetPeerIdentifier(publicKeyBytes.ToString()).PeerId
                }.ToByteString()
            };
        }

        [Fact]
        private void CanFireNextPipelineOnValidSignature()
        {
            _keySigner.Verify(Arg.Any<ISignature>(), Arg.Any<byte[]>(), default)
               .ReturnsForAnyArgs(true);

            var signatureHandler = new ProtocolMessageVerifyHandler(_keySigner, _signingContextProvider);

            signatureHandler.ChannelRead(_fakeContext, _protocolMessage);

            _fakeContext.ReceivedWithAnyArgs().FireChannelRead(_protocolMessage).Received(1);
        }
        
        [Fact]
        private void CanFireNextPipelineOnInvalidSignature()
        {
            _keySigner.Verify(Arg.Any<ISignature>(), Arg.Any<byte[]>(), default)
               .ReturnsForAnyArgs(false);

            var signatureHandler = new ProtocolMessageVerifyHandler(_keySigner, _signingContextProvider);

            signatureHandler.ChannelRead(_fakeContext, _protocolMessage);
            
            _fakeContext.DidNotReceiveWithAnyArgs().FireChannelRead(_protocolMessage).Received(0);
        }
    }
}
