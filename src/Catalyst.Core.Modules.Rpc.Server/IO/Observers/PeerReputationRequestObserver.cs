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

using System.Linq;
using Catalyst.Abstractions.IO.Messaging.Correlation;
using Catalyst.Abstractions.IO.Observers;
using Catalyst.Abstractions.P2P;
using Catalyst.Core.Lib.IO.Observers;
using Catalyst.Core.Lib.P2P.Repository;
using Catalyst.Core.Lib.Util;
using Catalyst.Protocol.Peer;
using Catalyst.Protocol.Rpc.Node;
using Dawn;
using DotNetty.Transport.Channels;
using Serilog;

namespace Catalyst.Core.Modules.Rpc.Server.IO.Observers
{
    public sealed class PeerReputationRequestObserver
        : RequestObserverBase<GetPeerReputationRequest, GetPeerReputationResponse>,
            IRpcRequestObserver
    {
        /// <summary>
        /// The PeerReputationRequestHandler 
        /// </summary>
        private readonly IPeerRepository _peerRepository;

        public PeerReputationRequestObserver(IPeerSettings peerSettings,
            ILogger logger,
            IPeerRepository peerRepository)
            : base(logger, peerSettings)
        {
            _peerRepository = peerRepository;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="getPeerReputationRequest"></param>
        /// <param name="channelHandlerContext"></param>
        /// <param name="senderPeerId"></param>
        /// <param name="correlationId"></param>
        /// <returns></returns>
        protected override GetPeerReputationResponse HandleRequest(GetPeerReputationRequest getPeerReputationRequest,
            IChannelHandlerContext channelHandlerContext,
            PeerId senderPeerId,
            ICorrelationId correlationId)
        {
            Guard.Argument(getPeerReputationRequest, nameof(getPeerReputationRequest)).NotNull();
            Guard.Argument(channelHandlerContext, nameof(channelHandlerContext)).NotNull();
            Guard.Argument(senderPeerId, nameof(senderPeerId)).NotNull();
            Logger.Debug("received message of type PeerReputationRequest");

            return new GetPeerReputationResponse
            {
                Reputation = _peerRepository.GetAll().Where(m => m.PeerId.Ip == getPeerReputationRequest.Ip
                     && m.PeerId.PublicKey.KeyToString() == getPeerReputationRequest.PublicKey.KeyToString())
                   .Select(x => x.Reputation).DefaultIfEmpty(int.MinValue).First()
            };
        }
    }
}


