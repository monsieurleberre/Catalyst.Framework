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
using Catalyst.Core.Lib.DAO;
using Microsoft.EntityFrameworkCore;

namespace Catalyst.Core.Lib.Repository
{
    public interface IDbContext : IDisposable
    {
        DbSet<TEntity> Set<TEntity>() where TEntity : class;
    }

    public sealed class EfCoreContext : DbContext, IDbContext
    {
        public EfCoreContext(string connectionString)
            : base(new DbContextOptionsBuilder<EfCoreContext>()
               .UseSqlServer(connectionString).Options) { }

        public DbSet<PeerIdDao> PeerIdDaoStore { get; set; }
        public DbSet<PeerDao> PeerDaoStore { get; set; }
        public DbSet<TransactionBroadcastDao> TransactionBroadcastStore { get; set; }
        public DbSet<PublicEntryDao> PublicEntryDaoStore { get; set; }
        public DbSet<ConfidentialEntryDao> ConfidentialEntryDaoStore { get; set; }
        public DbSet<ContractEntryDao> ContractEntryDaoStore { get; set; }
        public DbSet<BaseEntryDao> BaseEntryDaoStore { get; set; }
        public DbSet<SignatureDao> SignatureDaoStore { get; set; }
        public DbSet<SigningContextDao> SigningContextDaoStore { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Required code stub
        }
    }
}
