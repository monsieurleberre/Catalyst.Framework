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
using System.IO;
using System.Threading.Tasks;
using System.Transactions;
using Autofac;
using Catalyst.Common.Config;
using Catalyst.Common.Interfaces.Modules.Mempool;
using Catalyst.Common.IO.Messaging.Correlation;
using Catalyst.Common.Modules.Mempool.Models;
using Catalyst.Protocol;
using Catalyst.TestUtils;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;
using SharpRepository.EfCoreRepository;
using Catalyst.Common.Repository;
using SharpRepository.Repository;

//using Entity = Microsoft.EntityFrameworkCore.Entity;
//using DbContext = Microsoft.EntityFrameworkCore.DbContext;
//using DbSet = Microsoft.EntityFrameworkCore.DbSet;

namespace Catalyst.Core.Lib.IntegrationTests.P2P
{
    public sealed class PeerRepositoryIntegrationTests : FileSystemBasedTest
    {
        private ContainerProvider _containerProvider;

        private Microsoft.EntityFrameworkCore.DbContext context;

        public void Setup()
        {
            //var options = new DbContextOptionsBuilder<TestObjectContextCore>()
            //   .UseSqlServer(
            //        "Server = databasemachine.traderiser.com\\SQL2012, 49175; Database = AtlasCity; User Id = developer; Password = d3v3lop3rhous3;")
            //   .Options;

            //// Create the schema in the database
            //using (var context = new TestObjectContextCore(options))
            //{
            //    context.Database.EnsureCreated();
            //}

            //// Run the test against one instance of the context
            //context = new TestObjectContextCore(options);
        }


        public PeerRepositoryIntegrationTests(ITestOutputHelper output) : base(output)
        {
            //Setup();
        }

        private async Task Mempool_can_save_and_retrieve(FileInfo mempoolModuleFile)
        {
            System.Diagnostics.Debug.WriteLine(typeof(TestObjectContextCore).AssemblyQualifiedName);


            var alteredComponentsFile = await CreateAlteredConfigForMempool(mempoolModuleFile);

            var configFilesUsed = new[]
            {
                alteredComponentsFile,
                Path.Combine(Constants.ConfigSubFolder, Constants.SerilogJsonConfigFile),
                Path.Combine(Constants.ConfigSubFolder, Constants.NetworkConfigFile(Network.Dev))
            };

            _containerProvider = new ContainerProvider(configFilesUsed, FileSystem, Output);

            _containerProvider.ConfigureContainerBuilder();

            using (var scope = _containerProvider.Container.BeginLifetimeScope(mempoolModuleFile))
            {
                var mempool = scope.Resolve<IMempool>();

                var guid = CorrelationId.GenerateCorrelationId().ToString();
                var mempoolDocument = new MempoolDocument
                { Transaction = TransactionHelper.GetTransaction(signature: guid) };

                mempool.SaveMempoolDocument(mempoolDocument);

                var retrievedTransaction = mempool.GetMempoolDocument(mempoolDocument.Transaction.Signature);

                retrievedTransaction.Should().Be(mempoolDocument);
                retrievedTransaction.Transaction.Signature.SchnorrSignature.Should()
                   .BeEquivalentTo(guid.ToUtf8ByteString());
            }
        }

        private async Task<string> CreateAlteredConfigForMempool(FileInfo mempoolConfigFile)
        {
            var originalContent = await File.ReadAllTextAsync(mempoolConfigFile.FullName);
            var newContent =
                originalContent.Replace("\"Config/Modules/mempool.json\"",
                    JsonConvert.ToString(mempoolConfigFile.FullName));
            var newJsonPath = Path.Combine(FileSystem.GetCatalystDataDir().FullName,
                $"components.{mempoolConfigFile.Name}");
            File.WriteAllText(newJsonPath, newContent);
            return newJsonPath;
        }

        [Fact]
        [Trait(Traits.TestType, Traits.IntegrationTest)]
        public void Mempool_with_InEFRepo_can_save_and_retrieve()
        {
            try
            {
                var repositoryEf = new EfCoreRepository<Contact, string>(context);
                using (var trans = new TransactionScope())
                {
                    repositoryEf.Add(new Contact { Name = "Contact " + new Random().Next() });
                    trans.Complete();
                }
            }
            catch (Exception e) { }
        }

        [Fact]
        [Trait(Traits.TestType, Traits.IntegrationTest)]
        public async Task Mempool_with_InMemoryRepo_can_save_and_retrieve()
        {
            var fi = new FileInfo(Path.Combine(Constants.ConfigSubFolder, Constants.ModulesSubFolder,
                "peerrepo.entityframework.sql.json"));
            await Mempool_can_save_and_retrieve(fi);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing)
            {
                return;
            }

            _containerProvider?.Dispose();
        }
    }
}

//public class TempoEfCoreRepository<T, TKey> : EfCoreRepository<T, TKey> where T : class
    //{
    //    public TempoEfCoreRepository(TestObjectContextCore dbContext, ICachingStrategy<T, TKey> cachingStrategy = null) : base(dbContext, cachingStrategy)
    //    {

    //    }
    //}


