﻿#region LICENSE

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
using System.Linq;
using System.Reactive.Linq;
using Catalyst.Abstractions.Consensus.Deltas;
using Catalyst.Abstractions.Cryptography;
using Catalyst.Abstractions.Hashing;
using Catalyst.Abstractions.Kvm;
using Catalyst.Abstractions.Mempool;
using Catalyst.Core.Lib.DAO;
using Catalyst.Core.Lib.Extensions;
using Catalyst.Core.Modules.Cryptography.BulletProofs;
using Catalyst.Core.Modules.Hashing;
using Catalyst.Core.Modules.Kvm;
using Catalyst.Core.Modules.Ledger.Repository;
using Catalyst.Protocol.Deltas;
using Catalyst.Protocol.Transaction;
using FluentAssertions;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using LibP2P;
using Microsoft.Reactive.Testing;
using Nethermind.Core;
using Nethermind.Core.Extensions;
using Nethermind.Core.Specs;
using Nethermind.Dirichlet.Numerics;
using Nethermind.Evm;
using Nethermind.Logging;
using Nethermind.Store;
using NSubstitute;
using TheDotNetLeague.MultiFormats.MultiHash;
using Xunit;
using ILogger = Serilog.ILogger;

namespace Catalyst.Core.Modules.Ledger.Tests.IntegrationTests
{
    public class LedgerKvmTests
    {
        private Ledger _ledger;
        private readonly IKvm _kvm;
        private readonly ILogger _logger;
        private readonly MultiHash _genesisHash;
        private readonly IHashProvider _hashProvider;
        private readonly ISpecProvider _specProvider;
        private readonly TestScheduler _testScheduler;
        private readonly StateProvider _stateProvider;
        private readonly ICryptoContext _cryptoContext;
        private readonly IAccountRepository _fakeRepository;
        private readonly IDeltaHashProvider _deltaHashProvider;
        private readonly ILedgerSynchroniser _ledgerSynchroniser;
        private readonly IMempool<TransactionBroadcastDao> _mempool;
        private readonly IContractEntryExecutor _contractEntryExecutor;
        
        public LedgerKvmTests()
        {
            _testScheduler = new TestScheduler();
            _cryptoContext = new FfiWrapper();
            _fakeRepository = Substitute.For<IAccountRepository>();
            _hashProvider = new HashProvider(HashingAlgorithm.GetAlgorithmMetadata("blake2b-256"));
            _genesisHash = _hashProvider.ComputeUtf8MultiHash("genesis");

            _logger = Substitute.For<ILogger>();
            _mempool = Substitute.For<IMempool<TransactionBroadcastDao>>();
            _deltaHashProvider = Substitute.For<IDeltaHashProvider>();
            _ledgerSynchroniser = Substitute.For<ILedgerSynchroniser>();

            _ledgerSynchroniser.DeltaCache.GenesisHash.Returns(_genesisHash);

            IDb stateDbDevice = new MemDb();
            IDb codeDbDevice = new MemDb();

            ISnapshotableDb stateDb = new StateDb(stateDbDevice);
            ISnapshotableDb codeDb = new StateDb(codeDbDevice);

            _stateProvider = new StateProvider(stateDb, codeDb, LimboLogs.Instance);
            IStorageProvider storageProvider = new StorageProvider(stateDb, _stateProvider, LimboLogs.Instance);

            IStateUpdateHashProvider stateUpdateHashProvider = new StateUpdateHashProvider();
            _specProvider = new CatalystSpecProvider();

            _kvm = new KatVirtualMachine(_stateProvider, storageProvider, stateUpdateHashProvider, _specProvider, LimboLogs.Instance);
            _contractEntryExecutor = new ContractEntryExecutor(_specProvider, _stateProvider, storageProvider, _kvm, _logger);
        }
        
        private static ContractEntry PrepareContractEntry(IPublicKey recipient, IPublicKey sender, UInt256 amount, string dataHex = "0x", ulong nonce = 0)
        {
            return new ContractEntry
            {
                Base = new BaseEntry
                {
                    ReceiverPublicKey = recipient == null ? ByteString.Empty : ByteString.CopyFrom(recipient.Bytes),
                    SenderPublicKey = ByteString.CopyFrom(sender.Bytes),
                    Nonce = nonce
                },
                Amount = amount.ToUint256ByteString(),
                Data = ByteString.CopyFrom(Bytes.FromHexString(dataHex))
            };
        }

        private static PublicEntry PreparePublicEntry(IPublicKey recipient, IPublicKey sender, UInt256 amount)
        {
            return new PublicEntry
            {
                Base = new BaseEntry
                {
                    ReceiverPublicKey = ByteString.CopyFrom(recipient.Bytes),
                    SenderPublicKey = ByteString.CopyFrom(sender.Bytes),
                },
                Amount = amount.ToUint256ByteString(),
            };
        }

        private void RunDeltas(Delta delta)
        {
            var hash1 = _hashProvider.ComputeUtf8MultiHash("update");
            var updates = new[] {hash1};
            _ledgerSynchroniser.CacheDeltasBetween(default, default, default)
               .ReturnsForAnyArgs(new Cid[] {hash1, _genesisHash});
            _ledgerSynchroniser.DeltaCache.TryGetOrAddConfirmedDelta(Arg.Any<Cid>(), out Arg.Any<Delta>())
               .Returns(c =>
                {
                    c[1] = delta;
                    return true;
                }, c => false);

            _deltaHashProvider.DeltaHashUpdates.Returns(updates.Select(h => (Cid) h).ToObservable(_testScheduler));

            _ledger = new Ledger(_kvm, _contractEntryExecutor, _stateProvider, _specProvider, _fakeRepository, _deltaHashProvider, _ledgerSynchroniser, _mempool, _logger);
            
            _testScheduler.Start();
        }
        
        [Fact]
        public void Should_Update_State_On_Contract_Entry()
        {
            var recipient = _cryptoContext.GeneratePrivateKey().GetPublicKey();
            var sender = _cryptoContext.GeneratePrivateKey().GetPublicKey();
            _stateProvider.CreateAccount(sender.ToKvmAddress(), 1000);
            _stateProvider.CreateAccount(recipient.ToKvmAddress(), UInt256.Zero);

            Delta delta = new Delta
            {
                TimeStamp = Timestamp.FromDateTime(DateTime.UtcNow),
                ContractEntries =
                {
                    PrepareContractEntry(recipient, sender, 7)
                }
            };

            RunDeltas(delta);
            
            _stateProvider.GetAccount(recipient.ToKvmAddress()).Balance.Should().Be(7);
        }

        [Fact]
        public void Should_Update_State_On_Public_Entry()
        {
            var recipient = _cryptoContext.GeneratePrivateKey().GetPublicKey();
            var sender = _cryptoContext.GeneratePrivateKey().GetPublicKey();
            _stateProvider.CreateAccount(sender.ToKvmAddress(), 1000);
            _stateProvider.CreateAccount(recipient.ToKvmAddress(), UInt256.Zero);

            Delta delta = new Delta
            {
                TimeStamp = Timestamp.FromDateTime(DateTime.UtcNow),
                PublicEntries =
                {
                    PreparePublicEntry(recipient, sender, 3)
                }
            };

            RunDeltas(delta);
            
            Account account = _stateProvider.GetAccount(recipient.ToKvmAddress());
            account.Balance.Should().Be(3);
        }

        [Fact]
        public void Should_Deploy_Code_On_Code_Entry()
        {
            var sender = _cryptoContext.GeneratePrivateKey().GetPublicKey();
            _stateProvider.CreateAccount(sender.ToKvmAddress(), 1000);
            var contractAddress = Address.OfContract(sender.ToKvmAddress(), _stateProvider.GetNonce(sender.ToKvmAddress()));

            // PUSH1 1 PUSH1 0 MSTORE PUSH1 1 PUSH1 31 RETURN STOP
            string initCodeHex = "0x60016000526001601FF300";

            Delta delta = new Delta
            {
                TimeStamp = Timestamp.FromDateTime(DateTime.UtcNow),
                ContractEntries =
                {
                    PrepareContractEntry(null, sender, 5, initCodeHex)
                }
            };
            
            RunDeltas(delta);
            
            Account account = _stateProvider.GetAccount(contractAddress);
            account.Balance.Should().Be(5);
            _stateProvider.GetCode(account.CodeHash).Should().Equal(Bytes.FromHexString("0x01"));
        }
        
        [Fact]
        public void Should_Deploy_ERC20()
        {
            var sender = _cryptoContext.GeneratePrivateKey().GetPublicKey();
            _stateProvider.CreateAccount(sender.ToKvmAddress(), 1000);
            var contractAddress1 = Address.OfContract(sender.ToKvmAddress(), _stateProvider.GetNonce(sender.ToKvmAddress()));
            var contractAddress2 = Address.OfContract(sender.ToKvmAddress(), _stateProvider.GetNonce(sender.ToKvmAddress()) + 2);
            
            string migrationInitHex = "608060405234801561001057600080fd5b50336000806101000a81548173ffffffffffffffffffffffffffffffffffffffff021916908373ffffffffffffffffffffffffffffffffffffffff1602179055506102f8806100606000396000f300608060405260043610610062576000357c0100000000000000000000000000000000000000000000000000000000900463ffffffff1680630900f01014610067578063445df0ac146100aa5780638da5cb5b146100d5578063fdacd5761461012c575b600080fd5b34801561007357600080fd5b506100a8600480360381019080803573ffffffffffffffffffffffffffffffffffffffff169060200190929190505050610159565b005b3480156100b657600080fd5b506100bf610241565b6040518082815260200191505060405180910390f35b3480156100e157600080fd5b506100ea610247565b604051808273ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff16815260200191505060405180910390f35b34801561013857600080fd5b506101576004803603810190808035906020019092919050505061026c565b005b60008060009054906101000a900473ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff163373ffffffffffffffffffffffffffffffffffffffff16141561023d578190508073ffffffffffffffffffffffffffffffffffffffff1663fdacd5766001546040518263ffffffff167c010000000000000000000000000000000000000000000000000000000002815260040180828152602001915050600060405180830381600087803b15801561022457600080fd5b505af1158015610238573d6000803e3d6000fd5b505050505b5050565b60015481565b6000809054906101000a900473ffffffffffffffffffffffffffffffffffffffff1681565b6000809054906101000a900473ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff163373ffffffffffffffffffffffffffffffffffffffff1614156102c957806001819055505b505600a165627a7a723058206bf582fa33b86704b115209928731f910b5f9872d5a4c376fe8d639aa373ad790029";
            string call1Hex = "fdacd5760000000000000000000000000000000000000000000000000000000000000001";
            string initCodeHex = "608060405234801561001057600080fd5b50604051610e30380380610e308339810180604052810190808051906020019092919080518201929190602001805190602001909291908051820192919050505083600160003373ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff168152602001908152602001600020819055508360008190555082600390805190602001906100b29291906100ee565b5081600460006101000a81548160ff021916908360ff16021790555080600590805190602001906100e49291906100ee565b5050505050610193565b828054600181600116156101000203166002900490600052602060002090601f016020900481019282601f1061012f57805160ff191683800117855561015d565b8280016001018555821561015d579182015b8281111561015c578251825591602001919060010190610141565b5b50905061016a919061016e565b5090565b61019091905b8082111561018c576000816000905550600101610174565b5090565b90565b610c8e806101a26000396000f3006080604052600436106100af576000357c0100000000000000000000000000000000000000000000000000000000900463ffffffff16806306fdde03146100b4578063095ea7b31461014457806318160ddd146101a957806323b872dd146101d457806327e235e314610259578063313ce567146102b05780635c658165146102e157806370a082311461035857806395d89b41146103af578063a9059cbb1461043f578063dd62ed3e146104a4575b600080fd5b3480156100c057600080fd5b506100c961051b565b6040518080602001828103825283818151815260200191508051906020019080838360005b838110156101095780820151818401526020810190506100ee565b50505050905090810190601f1680156101365780820380516001836020036101000a031916815260200191505b509250505060405180910390f35b34801561015057600080fd5b5061018f600480360381019080803573ffffffffffffffffffffffffffffffffffffffff169060200190929190803590602001909291905050506105b9565b604051808215151515815260200191505060405180910390f35b3480156101b557600080fd5b506101be6106ab565b6040518082815260200191505060405180910390f35b3480156101e057600080fd5b5061023f600480360381019080803573ffffffffffffffffffffffffffffffffffffffff169060200190929190803573ffffffffffffffffffffffffffffffffffffffff169060200190929190803590602001909291905050506106b1565b604051808215151515815260200191505060405180910390f35b34801561026557600080fd5b5061029a600480360381019080803573ffffffffffffffffffffffffffffffffffffffff16906020019092919050505061094b565b6040518082815260200191505060405180910390f35b3480156102bc57600080fd5b506102c5610963565b604051808260ff1660ff16815260200191505060405180910390f35b3480156102ed57600080fd5b50610342600480360381019080803573ffffffffffffffffffffffffffffffffffffffff169060200190929190803573ffffffffffffffffffffffffffffffffffffffff169060200190929190505050610976565b6040518082815260200191505060405180910390f35b34801561036457600080fd5b50610399600480360381019080803573ffffffffffffffffffffffffffffffffffffffff16906020019092919050505061099b565b6040518082815260200191505060405180910390f35b3480156103bb57600080fd5b506103c46109e4565b6040518080602001828103825283818151815260200191508051906020019080838360005b838110156104045780820151818401526020810190506103e9565b50505050905090810190601f1680156104315780820380516001836020036101000a031916815260200191505b509250505060405180910390f35b34801561044b57600080fd5b5061048a600480360381019080803573ffffffffffffffffffffffffffffffffffffffff16906020019092919080359060200190929190505050610a82565b604051808215151515815260200191505060405180910390f35b3480156104b057600080fd5b50610505600480360381019080803573ffffffffffffffffffffffffffffffffffffffff169060200190929190803573ffffffffffffffffffffffffffffffffffffffff169060200190929190505050610bdb565b6040518082815260200191505060405180910390f35b60038054600181600116156101000203166002900480601f0160208091040260200160405190810160405280929190818152602001828054600181600116156101000203166002900480156105b15780601f10610586576101008083540402835291602001916105b1565b820191906000526020600020905b81548152906001019060200180831161059457829003601f168201915b505050505081565b600081600260003373ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff16815260200190815260200160002060008573ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff168152602001908152602001600020819055508273ffffffffffffffffffffffffffffffffffffffff163373ffffffffffffffffffffffffffffffffffffffff167f8c5be1e5ebec7d5bd14f71427d1e84f3dd0314c0f7b2291e5b200ac8c7c3b925846040518082815260200191505060405180910390a36001905092915050565b60005481565b600080600260008673ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff16815260200190815260200160002060003373ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff16815260200190815260200160002054905082600160008773ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff16815260200190815260200160002054101580156107825750828110155b151561078d57600080fd5b82600160008673ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff1681526020019081526020016000206000828254019250508190555082600160008773ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff168152602001908152602001600020600082825403925050819055507fffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff8110156108da5782600260008773ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff16815260200190815260200160002060003373ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff168152602001908152602001600020600082825403925050819055505b8373ffffffffffffffffffffffffffffffffffffffff168573ffffffffffffffffffffffffffffffffffffffff167fddf252ad1be2c89b69c2b068fc378daa952ba7f163c4a11628f55a4df523b3ef856040518082815260200191505060405180910390a360019150509392505050565b60016020528060005260406000206000915090505481565b600460009054906101000a900460ff1681565b6002602052816000526040600020602052806000526040600020600091509150505481565b6000600160008373ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff168152602001908152602001600020549050919050565b60058054600181600116156101000203166002900480601f016020809104026020016040519081016040528092919081815260200182805460018160011615610100020316600290048015610a7a5780601f10610a4f57610100808354040283529160200191610a7a565b820191906000526020600020905b815481529060010190602001808311610a5d57829003601f168201915b505050505081565b600081600160003373ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff1681526020019081526020016000205410151515610ad257600080fd5b81600160003373ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff1681526020019081526020016000206000828254039250508190555081600160008573ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff168152602001908152602001600020600082825401925050819055508273ffffffffffffffffffffffffffffffffffffffff163373ffffffffffffffffffffffffffffffffffffffff167fddf252ad1be2c89b69c2b068fc378daa952ba7f163c4a11628f55a4df523b3ef846040518082815260200191505060405180910390a36001905092915050565b6000600260008473ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff16815260200190815260200160002060008373ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff168152602001908152602001600020549050929150505600a165627a7a72305820b01e799233fd04eb73bd6896cd148b8926873dc1418dd3e1a44f25a5e59903d2002900000000000000000000000000000000000000000000000000000000000f42400000000000000000000000000000000000000000000000000000000000000080000000000000000000000000000000000000000000000000000000000000000800000000000000000000000000000000000000000000000000000000000000c000000000000000000000000000000000000000000000000000000000000000034b4154000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000034b41540000000000000000000000000000000000000000000000000000000000";
            string call2Hex = "fdacd5760000000000000000000000000000000000000000000000000000000000000001";

            // to // 0x9bced1be5c820ccb4bb52c1f83862d06b6d02c9f
            
            Delta delta = new Delta
            {
                TimeStamp = Timestamp.FromDateTime(DateTime.UtcNow),
                ContractEntries =
                {
                    PrepareContractEntry(null, sender, 0, migrationInitHex),
                    PrepareContractEntry(null, sender, 0, call1Hex, 1),
                    PrepareContractEntry(null, sender, 0, initCodeHex, 2),
                    PrepareContractEntry(null, sender, 0, call2Hex, 3),
                }
            };
            
            RunDeltas(delta);
            
            Account migrationAccount = _stateProvider.GetAccount(contractAddress1);
            migrationAccount.Balance.Should().Be(0);
            _stateProvider.GetCode(migrationAccount.CodeHash).Should().Equal(Bytes.FromHexString("608060405260043610610062576000357c0100000000000000000000000000000000000000000000000000000000900463ffffffff1680630900f01014610067578063445df0ac146100aa5780638da5cb5b146100d5578063fdacd5761461012c575b600080fd5b34801561007357600080fd5b506100a8600480360381019080803573ffffffffffffffffffffffffffffffffffffffff169060200190929190505050610159565b005b3480156100b657600080fd5b506100bf610241565b6040518082815260200191505060405180910390f35b3480156100e157600080fd5b506100ea610247565b604051808273ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff16815260200191505060405180910390f35b34801561013857600080fd5b506101576004803603810190808035906020019092919050505061026c565b005b60008060009054906101000a900473ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff163373ffffffffffffffffffffffffffffffffffffffff16141561023d578190508073ffffffffffffffffffffffffffffffffffffffff1663fdacd5766001546040518263ffffffff167c010000000000000000000000000000000000000000000000000000000002815260040180828152602001915050600060405180830381600087803b15801561022457600080fd5b505af1158015610238573d6000803e3d6000fd5b505050505b5050565b60015481565b6000809054906101000a900473ffffffffffffffffffffffffffffffffffffffff1681565b6000809054906101000a900473ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff163373ffffffffffffffffffffffffffffffffffffffff1614156102c957806001819055505b505600a165627a7a723058206bf582fa33b86704b115209928731f910b5f9872d5a4c376fe8d639aa373ad790029"));
            
            Account erc20Account = _stateProvider.GetAccount(contractAddress2);
            erc20Account.Balance.Should().Be(0);
            _stateProvider.GetCode(erc20Account.CodeHash).Should().Equal(Bytes.FromHexString("6080604052600436106100af576000357c0100000000000000000000000000000000000000000000000000000000900463ffffffff16806306fdde03146100b4578063095ea7b31461014457806318160ddd146101a957806323b872dd146101d457806327e235e314610259578063313ce567146102b05780635c658165146102e157806370a082311461035857806395d89b41146103af578063a9059cbb1461043f578063dd62ed3e146104a4575b600080fd5b3480156100c057600080fd5b506100c961051b565b6040518080602001828103825283818151815260200191508051906020019080838360005b838110156101095780820151818401526020810190506100ee565b50505050905090810190601f1680156101365780820380516001836020036101000a031916815260200191505b509250505060405180910390f35b34801561015057600080fd5b5061018f600480360381019080803573ffffffffffffffffffffffffffffffffffffffff169060200190929190803590602001909291905050506105b9565b604051808215151515815260200191505060405180910390f35b3480156101b557600080fd5b506101be6106ab565b6040518082815260200191505060405180910390f35b3480156101e057600080fd5b5061023f600480360381019080803573ffffffffffffffffffffffffffffffffffffffff169060200190929190803573ffffffffffffffffffffffffffffffffffffffff169060200190929190803590602001909291905050506106b1565b604051808215151515815260200191505060405180910390f35b34801561026557600080fd5b5061029a600480360381019080803573ffffffffffffffffffffffffffffffffffffffff16906020019092919050505061094b565b6040518082815260200191505060405180910390f35b3480156102bc57600080fd5b506102c5610963565b604051808260ff1660ff16815260200191505060405180910390f35b3480156102ed57600080fd5b50610342600480360381019080803573ffffffffffffffffffffffffffffffffffffffff169060200190929190803573ffffffffffffffffffffffffffffffffffffffff169060200190929190505050610976565b6040518082815260200191505060405180910390f35b34801561036457600080fd5b50610399600480360381019080803573ffffffffffffffffffffffffffffffffffffffff16906020019092919050505061099b565b6040518082815260200191505060405180910390f35b3480156103bb57600080fd5b506103c46109e4565b6040518080602001828103825283818151815260200191508051906020019080838360005b838110156104045780820151818401526020810190506103e9565b50505050905090810190601f1680156104315780820380516001836020036101000a031916815260200191505b509250505060405180910390f35b34801561044b57600080fd5b5061048a600480360381019080803573ffffffffffffffffffffffffffffffffffffffff16906020019092919080359060200190929190505050610a82565b604051808215151515815260200191505060405180910390f35b3480156104b057600080fd5b50610505600480360381019080803573ffffffffffffffffffffffffffffffffffffffff169060200190929190803573ffffffffffffffffffffffffffffffffffffffff169060200190929190505050610bdb565b6040518082815260200191505060405180910390f35b60038054600181600116156101000203166002900480601f0160208091040260200160405190810160405280929190818152602001828054600181600116156101000203166002900480156105b15780601f10610586576101008083540402835291602001916105b1565b820191906000526020600020905b81548152906001019060200180831161059457829003601f168201915b505050505081565b600081600260003373ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff16815260200190815260200160002060008573ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff168152602001908152602001600020819055508273ffffffffffffffffffffffffffffffffffffffff163373ffffffffffffffffffffffffffffffffffffffff167f8c5be1e5ebec7d5bd14f71427d1e84f3dd0314c0f7b2291e5b200ac8c7c3b925846040518082815260200191505060405180910390a36001905092915050565b60005481565b600080600260008673ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff16815260200190815260200160002060003373ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff16815260200190815260200160002054905082600160008773ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff16815260200190815260200160002054101580156107825750828110155b151561078d57600080fd5b82600160008673ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff1681526020019081526020016000206000828254019250508190555082600160008773ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff168152602001908152602001600020600082825403925050819055507fffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff8110156108da5782600260008773ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff16815260200190815260200160002060003373ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff168152602001908152602001600020600082825403925050819055505b8373ffffffffffffffffffffffffffffffffffffffff168573ffffffffffffffffffffffffffffffffffffffff167fddf252ad1be2c89b69c2b068fc378daa952ba7f163c4a11628f55a4df523b3ef856040518082815260200191505060405180910390a360019150509392505050565b60016020528060005260406000206000915090505481565b600460009054906101000a900460ff1681565b6002602052816000526040600020602052806000526040600020600091509150505481565b6000600160008373ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff168152602001908152602001600020549050919050565b60058054600181600116156101000203166002900480601f016020809104026020016040519081016040528092919081815260200182805460018160011615610100020316600290048015610a7a5780601f10610a4f57610100808354040283529160200191610a7a565b820191906000526020600020905b815481529060010190602001808311610a5d57829003601f168201915b505050505081565b600081600160003373ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff1681526020019081526020016000205410151515610ad257600080fd5b81600160003373ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff1681526020019081526020016000206000828254039250508190555081600160008573ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff168152602001908152602001600020600082825401925050819055508273ffffffffffffffffffffffffffffffffffffffff163373ffffffffffffffffffffffffffffffffffffffff167fddf252ad1be2c89b69c2b068fc378daa952ba7f163c4a11628f55a4df523b3ef846040518082815260200191505060405180910390a36001905092915050565b6000600260008473ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff16815260200190815260200160002060008373ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff168152602001908152602001600020549050929150505600a165627a7a72305820b01e799233fd04eb73bd6896cd148b8926873dc1418dd3e1a44f25a5e59903d20029"));
        }
    }
}
