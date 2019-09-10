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

using Catalyst.Protocol;
using Catalyst.Protocol.Cryptography;
using Catalyst.Protocol.Transaction;
using Catalyst.Protocol.Wire;
using Google.Protobuf.WellKnownTypes;

namespace Catalyst.TestUtils
{
    public static class TransactionHelper
    {
        public static TransactionBroadcast GetTransaction(uint standardAmount = 123,
            string standardPubKey = "standardPubKey",
            TransactionType transactionType = TransactionType.Public,
            long timeStamp = 12345,
            ulong transactionFees = 2)
        {
            var transaction = new TransactionBroadcast
            {
                PublicEntries =
                {
                    new PublicEntry
                    {
                        Amount = standardAmount,
                        Base = new BaseEntry
                        {
                            Sender = new PublicKey {RawBytes = standardPubKey.ToUtf8ByteString()},
                            TransactionFees = transactionFees
                        }
                    }
                },
                
                Timestamp = new Timestamp
                {
                    Seconds = timeStamp
                }
            };
            return transaction;
        }
    }
}
