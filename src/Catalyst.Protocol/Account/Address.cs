//#region LICENSE

///**
//* Copyright (c) 2019 Catalyst Network
//*
//* This file is part of Catalyst.Node <https://github.com/catalyst-network/Catalyst.Node>
//*
//* Catalyst.Node is free software: you can redistribute it and/or modify
//* it under the terms of the GNU General Public License as published by
//* the Free Software Foundation, either version 2 of the License, or
//* (at your option) any later version.
//*
//* Catalyst.Node is distributed in the hope that it will be useful,
//* but WITHOUT ANY WARRANTY; without even the implied warranty of
//* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//* GNU General Public License for more details.
//*
//* You should have received a copy of the GNU General Public License
//* along with Catalyst.Node. If not, see <https://www.gnu.org/licenses/>.
//*/

//#endregion

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Catalyst.Common.Extensions;
//using Catalyst.Cryptography.BulletProofs.Wrapper.Interfaces;
//using Catalyst.Protocol.Shared;
//using Dawn;
//using Multiformats.Hash.Algorithms;

using System;
using System.Collections.Generic;
using System.Linq;
using Catalyst.Common.Extensions;
using Catalyst.Cryptography.BulletProofs.Wrapper.Interfaces;
using Catalyst.Protocol.Cryptography;
using Catalyst.Protocol.Extensions;
using Catalyst.Protocol.Network;
using Dawn;
using Multiformats.Hash;
using Multiformats.Hash.Algorithms;
using Org.BouncyCastle.Crypto.Agreement.Kdf;

namespace Catalyst.Protocol.Account  
{
    /// <inheritdoc />
    public partial class Address
    {
        public static readonly int ByteLength = 20;
        public static readonly BLAKE2B_152 Blake2B152 = new BLAKE2B_152();

        private byte[] _rawBytes;

        public Address(IPublicKey publicKey,
            NetworkType network,
            AccountType accountType)
        {
            NetworkType = network;
            AccountType = accountType;
            PublicKeyHash = publicKey.Bytes
               .ComputeRawHash(Blake2B152)
               .ToByteString();
        }

        public bool IsSmartContract => AccountType == AccountType.SmartContractAccount;
        public bool IsPublicAccount => AccountType == AccountType.PublicAccount;
        public bool IsConfidentialAccount => AccountType == AccountType.ConfidentialAccount;

        public byte[] RawBytes =>
            _rawBytes ?? (_rawBytes = new[] {(byte) ((byte) NetworkType | (byte) AccountType)}
               .Concat(PublicKeyHash.ToByteArray())
               .ToArray());

        public string AsBase32Crockford => RawBytes.AsBase32Address();
    }
}
