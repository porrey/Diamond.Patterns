﻿using System;
using System.Data.Entity;
using System.Threading.Tasks;
using Diamond.Patterns.Abstractions;

namespace Diamond.Patterns.Repository.EntityFramework
{
	public class RepositoryTransactionContext : DisposableObject, IRepositoryTransactionContext
	{
		internal RepositoryTransactionContext(DbContextTransaction context)
		{
			this.Context = context;
		}

		internal DbContextTransaction Context { get; set; }

		public Task CommitTransactionAsync()
		{
			this.Context.Commit();
			return Task.FromResult(0);
		}

		public Task RollbackTransactionAsync()
		{
			this.Context.Rollback();
			return Task.FromResult(0);
		}

		protected override void OnDisposeManagedObjects()
		{
			if (this.Context != null)
			{
				this.Context = null;
			}
		}
	}
}
