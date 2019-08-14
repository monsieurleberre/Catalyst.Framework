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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Catalyst.Common.Modules.Mempool.Models;
using Microsoft.EntityFrameworkCore;
using SharpRepository.EfCoreRepository;
using SharpRepository.Repository;
using SharpRepository.Repository.Caching;

namespace Catalyst.Common.Repository
{
    public class TempEfCoreRepository : EfCoreRepository<MempoolDocument, string> 
    {
        public TempEfCoreRepository(TestObjectContextCore dbContext, ICachingStrategy<MempoolDocument, string> cachingStrategy = null) : base(dbContext, cachingStrategy)
        {

        }
    }

    public class TestObjectContextCore : Microsoft.EntityFrameworkCore.DbContext
    {
        //public TestObjectContextCore()
        //{ }

        //public TestObjectContextCore(string connectionString)
        //    : base(new DbContextOptionsBuilder<TestObjectContextCore>()
        //       .UseSqlServer(connectionString).Options)
        //{
        //    //var options = new DbContextOptionsBuilder<TestObjectContextCore>()
        //    //   .UseSqlServer(connectionString).Options;
        //}

        public TestObjectContextCore()
            : base(new DbContextOptionsBuilder<TestObjectContextCore>()
               .UseSqlServer("Server=databasemachine.traderiser.com\\SQL2012, 49175; Database = AtlasCity; User Id = developer; Password = d3v3lop3rhous3;").Options)
        {
            //var options = new DbContextOptionsBuilder<TestObjectContextCore>()
            //   .UseSqlServer(connectionString).Options;
        }

        //public TestObjectContextCore(DbContextOptions<TestObjectContextCore> options)
        //    : base(options)
        //{ }

        public Microsoft.EntityFrameworkCore.DbSet<Contact> Contacts { get; set; }
        public Microsoft.EntityFrameworkCore.DbSet<PhoneNumber> PhoneNumbers { get; set; }
        public Microsoft.EntityFrameworkCore.DbSet<EmailAddress> EmailAddresses { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasKey(u => new {u.Username, u.Age});

            modelBuilder.Entity<Node>();
        }
    }


    public class Contact
    {
        [Key]
        public string ContactId { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public int ContactTypeId { get; set; } // for partitioning on 

        public decimal SumDecimal { get; set; }

        public virtual List<EmailAddress> EmailAddresses { get; set; }
        public virtual List<PhoneNumber> PhoneNumbers { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }


    public class ContactItem : Microsoft.EntityFrameworkCore.DbContext
    {
        public ContactItem() : base()
        {

        }

        [Key]
        public string ContactId { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public int ContactTypeId { get; set; } // for partitioning on 

        public decimal SumDecimal { get; set; }

        public virtual List<EmailAddress> EmailAddresses { get; set; }
        public virtual List<PhoneNumber> PhoneNumbers { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }

    public class EmailAddress
    {
        public int EmailAddressId { get; set; }

        public string ContactId { get; set; }

        public Contact Contact { get; set; }

        public string Label { get; set; }
        public string Email { get; set; }
    }

    public class PhoneNumber
    {
        public int PhoneNumberId { get; set; }
        public int ContactId { get; set; }
        public string Label { get; set; }
        public string Number { get; set; }
    }

    public class ContactInt : ICloneable
    {
        [Key]
        public int ContactIntId { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public int ContactTypeId { get; set; } // for partitioning on 

        public decimal SumDecimal { get; set; }

        public virtual List<EmailAddress> EmailAddresses { get; set; }
        public virtual List<PhoneNumber> PhoneNumbers { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }

    public class Node
    {
        [RepositoryPrimaryKey(Order = 1)]
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string Name { get; set; }
        public virtual Node Parent { get; private set; }
    }

    public class User
    {
        [RepositoryPrimaryKey(Order = 1)]
        public string Username { get; set; }

        [RepositoryPrimaryKey(Order = 1)]
        public int Age { get; set; }

        public string FullName { get; set; }

        public int ContactTypeId { get; set; }
    }
}

