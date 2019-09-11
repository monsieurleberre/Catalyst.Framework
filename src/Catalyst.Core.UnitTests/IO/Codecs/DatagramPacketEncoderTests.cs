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

using System.Net;
using Catalyst.Abstractions.P2P;
using Catalyst.Common.Utils;
using Catalyst.Core.Extensions;
using Catalyst.Core.IO.Messaging.Correlation;
using Catalyst.Core.IO.Messaging.Dto;
using Catalyst.Protocol.Cryptography;
using Catalyst.Protocol.Extensions;
using Catalyst.Protocol.IPPN;
using Catalyst.Protocol.Wire;
using Catalyst.TestUtils;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Codecs.Protobuf;
using DotNetty.Transport.Channels.Embedded;
using DotNetty.Transport.Channels.Sockets;
using Google.Protobuf;
using Xunit;

namespace Catalyst.Core.UnitTests.IO.Codecs
{
    public sealed class DatagramPacketEncoderTests
    {
        private readonly EmbeddedChannel _channel;
        private readonly IPeerIdentifier _recipientPid;
        private readonly DatagramPacket _datagramPacket;
        private readonly ProtocolMessage _protocolMessage;
        
        public DatagramPacketEncoderTests()
        {
            _channel = new EmbeddedChannel(
                new DatagramPacketEncoder<IMessage>(new ProtobufEncoder())
            );

            var senderPid = PeerIdentifierHelper.GetPeerIdentifier("sender",
                IPAddress.Loopback,
                10000
            );
            
            _recipientPid = PeerIdentifierHelper.GetPeerIdentifier("sender",
                IPAddress.Loopback,
                20000
            );

            _protocolMessage = new PingRequest()
               .ToProtocolMessage(senderPid.PeerId, CorrelationId.GenerateCorrelationId())
               .ToProtocolMessage(senderPid.PeerId)
               .ToProtocolMessage(senderPid.PeerId,
                    signature: ByteUtil.GenerateRandomByteArray(64).ToByteString()
                       .AsProtoSignature(new SigningContext()));
            
            _datagramPacket = new DatagramPacket(
                Unpooled.WrappedBuffer(_protocolMessage.ToByteArray()),
                senderPid.IpEndPoint,
                _recipientPid.IpEndPoint
            );
        }

        [Fact]
        public void DatagramPacketEncoder_Can_Encode_IMessage_With_ProtobufEncoder()
        {
            Assert.True(_channel.WriteOutbound(new SignedMessageDto(_protocolMessage, _recipientPid)));

            var datagramPacket = _channel.ReadOutbound<DatagramPacket>();
            Assert.NotNull(datagramPacket);

            Assert.Equal(_datagramPacket.Content, datagramPacket.Content);
            Assert.Equal(_datagramPacket.Sender, datagramPacket.Sender);
            Assert.Equal(_datagramPacket.Recipient, datagramPacket.Recipient);
            datagramPacket.Release();
            Assert.False(_channel.Finish());
        }

        [Fact]
        public void DatagramPacketEncoder_Will_Not_Encode_UnmatchedMessageType()
        {
            Assert.True(_channel.WriteOutbound(_protocolMessage));
        
            var ProtocolMessage = _channel.ReadOutbound<ProtocolMessage>();
            Assert.NotNull(ProtocolMessage);
            Assert.Same(_protocolMessage, ProtocolMessage);
            Assert.False(_channel.Finish());
        }
        
        [Fact]
        public void DatagramPacketEncoder_Will_Not_Encode_UnmatchedType()
        {
            const string expected = "junk";
            Assert.True(_channel.WriteOutbound(expected));
        
            var content = _channel.ReadOutbound<string>();
            Assert.Same(expected, content);
            Assert.False(_channel.Finish());
        }
    }
}
