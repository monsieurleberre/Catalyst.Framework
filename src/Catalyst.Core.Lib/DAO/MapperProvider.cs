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

using System.Collections.Generic;
using Autofac;
using AutoMapper;
using Catalyst.Abstractions.DAO;

namespace Catalyst.Core.Lib.DAO
{
    public class MapperProvider : IStartable
    {
        private readonly IEnumerable<IMapperInitializer> _mapperConfigurations;
        public static IMapper MasterMapper { get; set; }

        public MapperProvider(IEnumerable<IMapperInitializer> mapperConfigurations)
        {
            _mapperConfigurations = mapperConfigurations;
        }

        public void Start()
        {
            var config = new MapperConfiguration(cfg =>
            {
                foreach (var init in _mapperConfigurations)
                {
                    init.InitMappers(cfg);
                }
            });

            MasterMapper = config.CreateMapper();
        }
    }
}
