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
using Catalyst.Core.Lib.Mempool.Documents;
using SharpRepository.MongoDbRepository;
using SharpRepository.Repository;
using SharpRepository.Repository.Caching;

namespace Catalyst.Modules.Repository.MongoDb
{
    public class MempoolModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c => new MongoDbRepository<MempoolDocument>(
                c.ResolveOptional<ICachingStrategy<MempoolDocument, string>>()
            )).As<IRepository<MempoolDocument, string>>().SingleInstance();
        }  
    }
}
