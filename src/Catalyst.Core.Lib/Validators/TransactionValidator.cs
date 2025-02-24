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
using Catalyst.Abstractions.Cryptography;
using Catalyst.Abstractions.Validators;
using Catalyst.Protocol.Network;
using Catalyst.Protocol.Wire;
using Google.Protobuf;
using Serilog;

namespace Catalyst.Core.Lib.Validators
{
    public sealed class TransactionValidator : ITransactionValidator
    {
        private readonly ILogger _logger;
        private readonly ICryptoContext _cryptoContext;

        public TransactionValidator(ILogger logger,
            ICryptoContext cryptoContext)
        {
            _cryptoContext = cryptoContext;
            _logger = logger;
        }

        public bool ValidateTransaction(TransactionBroadcast transactionBroadcast, NetworkType networkType)
        {
            return ValidateTransactionFields(transactionBroadcast)
             && CheckContractInputFields(transactionBroadcast)
             && CheckCfEntries(transactionBroadcast)
             && CheckStEntries(transactionBroadcast)
             && ValidateTransactionSignature(transactionBroadcast, networkType);
        }

        private bool CheckContractInputFields(TransactionBroadcast transactionBroadcast)
        {
            // @TODO DO SOMETHING
            return true;
        }

        private bool ValidateTransactionFields(TransactionBroadcast transactionBroadcast)
        {
            // @TODO DO SOMETHING
            return true;
        }

        private bool ValidateTransactionSignature(TransactionBroadcast transactionBroadcast, NetworkType networkType)
        {
            if (transactionBroadcast.Signature.RawBytes == ByteString.Empty)
            {
                _logger.Error("Transaction signature is null");
                return false;
            }

            var transactionSignature = _cryptoContext.GetSignatureFromBytes(transactionBroadcast.Signature.RawBytes.ToByteArray(),
                transactionBroadcast.PublicEntries.First().Base.SenderPublicKey.ToByteArray());
            var transactionWithoutSig = transactionBroadcast.Clone();
            transactionWithoutSig.Signature = null;

            if (!_cryptoContext.Verify(transactionSignature, transactionWithoutSig.ToByteArray(), transactionBroadcast.Signature.SigningContext.ToByteArray()))
            {
                return true;
            }
            
            _logger.Information(
                "Transaction Signature {signature} invalid.",
                transactionSignature);
            return false;
        }

        private bool CheckCfEntries(TransactionBroadcast transactionBroadcast)
        {
            // @TODO DO SOMETHING
            return true;
        }

        private bool CheckStEntries(TransactionBroadcast transactionBroadcast)
        {
            // @TODO DO SOMETHING
            return true;
        }

        private bool CheckKeySize(ByteString publicKey)
        {
            return publicKey.Length == _cryptoContext.PublicKeyLength;
        }
    }
}
