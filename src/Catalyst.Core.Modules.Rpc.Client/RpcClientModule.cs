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

using Autofac;
using Catalyst.Abstractions.IO.Observers;
using Catalyst.Abstractions.Rpc;
using Catalyst.Core.Modules.Rpc.Client.IO.Observers;

namespace Catalyst.Core.Modules.Rpc.Client
{
    public class RpcClientModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<RpcClient>().As<IRpcClient>();
            builder.RegisterType<RpcClientFactory>().As<IRpcClientFactory>();
            builder.RegisterType<RpcClientSettings>().As<IRpcClientConfig>();

            builder.RegisterType<AddFileToDfsResponseObserver>().As<IRpcResponseObserver>();
            builder.RegisterType<BroadcastRawTransactionResponseObserver>().As<IRpcResponseObserver>();
            builder.RegisterType<ChangeDataFolderResponseObserver>().As<IRpcResponseObserver>();
            builder.RegisterType<GetDeltaResponseObserver>().As<IRpcResponseObserver>();
            builder.RegisterType<GetInfoResponseObserver>().As<IRpcResponseObserver>();
            builder.RegisterType<GetMempoolResponseObserver>().As<IRpcResponseObserver>();
            builder.RegisterType<GetPeerInfoResponseObserver>().As<IRpcResponseObserver>();
            builder.RegisterType<GetVersionResponseObserver>().As<IRpcResponseObserver>();
            builder.RegisterType<PeerBlackListingResponseObserver>().As<IRpcResponseObserver>();
            builder.RegisterType<PeerCountResponseObserver>().As<IRpcResponseObserver>();
            builder.RegisterType<PeerListResponseObserver>().As<IRpcResponseObserver>();
            builder.RegisterType<PeerReputationResponseObserver>().As<IRpcResponseObserver>();
            builder.RegisterType<RemovePeerResponseObserver>().As<IRpcResponseObserver>();
            builder.RegisterType<SignMessageResponseObserver>().As<IRpcResponseObserver>();
            builder.RegisterType<TransferFileBytesResponseObserver>().As<IRpcResponseObserver>();
            builder.RegisterType<VerifyMessageResponseObserver>().As<IRpcResponseObserver>();
        }
    }
}
