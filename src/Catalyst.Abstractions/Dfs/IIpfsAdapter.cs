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
using LibP2P;
using TheDotNetLeague.Ipfs.Abstractions;
using TheDotNetLeague.Ipfs.Core.Lib;

namespace Catalyst.Abstractions.Dfs
{
    public interface IIpfsAdapter : ICoreApi, IDisposable
    {
        IBitswapApi Bitswap { get; }
        IBlockApi Block { get; }
        IBlockRepositoryApi BlockRepository { get; }
        IBootstrapApi Bootstrap { get; }
        IConfigApi Config { get; }
        IpfsEngineOptions Options { get; }
        IDagApi Dag { get; }
        IDhtApi Dht { get; }
        IDnsApi Dns { get; }
        IFileSystemApi FileSystem { get; }
        IGenericApi Generic { get; }
        IKeyApi Key { get; }
        INameApi Name { get; }
        IObjectApi Object { get; }
        IPinApi Pin { get; }
        IPubSubApi PubSub { get; }
        IStatsApi Stats { get; }
        ISwarmApi Swarm { get; }
        void Dispose();
    }
}
