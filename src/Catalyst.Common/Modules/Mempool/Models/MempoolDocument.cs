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
using System.ComponentModel.DataAnnotations;
using Catalyst.Common.Interfaces.Modules.Mempool;
using Catalyst.Common.Util;
using Catalyst.Protocol.Common;
using Catalyst.Protocol.Transaction;
using Google.Protobuf;
using Newtonsoft.Json;
using ProtoBuf;
using SharpRepository.Repository;

namespace Catalyst.Common.Modules.Mempool.Models
{
    public sealed class MempoolDocument : IMempoolDocument
    {
        [RepositoryPrimaryKey(Order = 1)]
        [JsonProperty("id")]
        [Key]
        public string DocumentId => Transaction?.Signature?.ToByteString()?.ToBase64();

        public TransactionBroadcast Transaction { get; set; }
    }

    [ProtoContract]
    public sealed class MempoolTempDocument : ITempDocument
    {
        [RepositoryPrimaryKey(Order = 1)]
        [JsonProperty("id")]
        [Key]

        //public string DocumentId => BitConverter.GetBytes(new Random().NextDouble()).ToByteString().ToBase64();

        public string DocumentId { get; set; }
        //public int Transaction { get; set; }

        //public string DocumentId => Transaction?.Signature?.ToByteString()?.ToBase64();

        public PeerId Transaction { get; set; }

        //public TransactionBroadcast Transaction { get; set; }

    }
}
