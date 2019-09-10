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
using Catalyst.Cryptography.BulletProofs.Wrapper.Interfaces;
using Catalyst.Protocol.Interfaces.Validators;
using Catalyst.Protocol.Network;
using Catalyst.Protocol.Wire;
using Google.Protobuf;
using Serilog;

namespace Catalyst.Protocol.Validators
{
    public class TransactionValidator : ITransactionValidator
    {
        private readonly ILogger _logger;
        private readonly IWrapper _cryptoContext;

        public TransactionValidator(ILogger logger,
            IWrapper cryptoContext)
        {
            _cryptoContext = cryptoContext;
            _logger = logger;
        }

        public bool ValidateTransaction(TransactionBroadcast transactionBroadcast, NetworkType network)
        {
            return ValidateTransactionFields(transactionBroadcast)
             && CheckContractInputFields(transactionBroadcast)
             && CheckCfEntries(transactionBroadcast)
             && CheckStEntries(transactionBroadcast)
             && ValidateTransactionSignature(transactionBroadcast, network);
        }

        private bool CheckContractInputFields(TransactionBroadcast transaction)
        {
            throw new NotImplementedException();
        }

        private bool ValidateTransactionFields(TransactionBroadcast transactionBroadcast)
        {
            throw new NotImplementedException();
        }

        private bool ValidateTransactionSignature(TransactionBroadcast transactionBroadcast, NetworkType network)
        {
            throw new NotImplementedException();
        }

        private bool CheckCfEntries(TransactionBroadcast transactionBroadcast)
        {
            throw new NotImplementedException();
        }

        private bool CheckStEntries(TransactionBroadcast transactionBroadcast)
        {
            throw new NotImplementedException();
        }

        private bool CheckKeySize(ByteString publicKey)
        {
            throw new NotImplementedException();
        }
    }
}
