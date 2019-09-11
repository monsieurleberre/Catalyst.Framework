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
using Catalyst.Protocol.Extensions;
using Catalyst.Protocol.Rpc.Node;
using Catalyst.Protocol.Transaction;
using Catalyst.Protocol.Wire;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

namespace Catalyst.Simulator.Helpers
{
    public static class TransactionHelper
    {
        public static BroadcastRawTransactionRequest GenerateTransaction(uint amount, int fee)
        {
            var guid = Guid.NewGuid();
            var broadcastRawTransactionRequest = new BroadcastRawTransactionRequest();
            var transaction = new TransactionBroadcast
            {
                Timestamp = Timestamp.FromDateTime(DateTime.UtcNow),
                PublicEntries =
                {
                    new PublicEntry
                    {
                        Base = new BaseEntry
                        {
                            SenderPublicKey = ByteString.FromBase64("VkC84TBQOVjrcX81NYV5swPVrE4RN+nKGzIjxNT2AY0="),
                            TransactionFees = (ulong) fee
                        },
                        Amount = amount
                    }
                }
            };

            broadcastRawTransactionRequest.Transaction = transaction;

            return broadcastRawTransactionRequest;
        }
    }
}
