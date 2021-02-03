﻿// ***
// *** Copyright(C) 2019-2021, Daniel M. Porrey. All rights reserved.
// *** 
// *** This program is free software: you can redistribute it and/or modify
// *** it under the terms of the GNU Lesser General Public License as published
// *** by the Free Software Foundation, either version 3 of the License, or
// *** (at your option) any later version.
// *** 
// *** This program is distributed in the hope that it will be useful,
// *** but WITHOUT ANY WARRANTY; without even the implied warranty of
// *** MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// *** GNU Lesser General Public License for more details.
// *** 
// *** You should have received a copy of the GNU Lesser General Public License
// *** along with this program. If not, see http://www.gnu.org/licenses/.
// *** 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Diamond.Patterns.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Diamond.Patterns.Repository.EntityFrameworkCore
{
	/// <summary>
	/// This repository implements a base repository for an Entity (TEntity) that
	/// implements interface TItem.
	/// </summary>
	/// <typeparam name="TInterface">The interface type that the entity implements.</typeparam>
	/// <typeparam name="TEntity">The entity object type.</typeparam>
	/// <typeparam name="TContext">The Entity Framework database context.</typeparam>
	public abstract class EntityFrameworkRepository<TInterface, TEntity, TContext> : IWritableRepository<TInterface>
		where TEntity : class, TInterface, new()
		where TInterface : IEntity
		where TContext : DbContext
	{
		public EntityFrameworkRepository(IContextFactory<TContext> contextFactory, IEntityFactory<TInterface> modelFactory)
		{
			this.ContextFactory = contextFactory;
			this.ModelFactory = modelFactory;
		}

		protected IContextFactory<TContext> ContextFactory { get; set; }
		protected abstract DbSet<TEntity> MyDbSet(TContext model);
		protected abstract TContext GetNewDbContext { get; }
		public virtual IEntityFactory<TInterface> ModelFactory { get; set; }
		public ILoggerSubscriber LoggerSubscriber { get; set; } = new NullLoggerSubscriber();

		public virtual Task<IEnumerable<TInterface>> GetAllAsync()
		{
			IEnumerable<TInterface> returnValue = null;

			this.LoggerSubscriber.Verbose($"{nameof(GetAllAsync)} called for type '{typeof(TInterface).Name}'.");

			using (TContext db = this.GetNewDbContext)
			{
				returnValue = this.MyDbSet(db).AsNoTracking().ToArray();
			}

			return Task.FromResult(returnValue);
		}

		public virtual Task<IEnumerable<TInterface>> GetAsync(Expression<Func<TInterface, bool>> predicate)
		{
			IEnumerable<TInterface> returnValue = null;

			this.LoggerSubscriber.Verbose($"{nameof(GetAsync)} called for type '{typeof(TInterface).Name}'.");

			using (TContext db = this.GetNewDbContext)
			{
				returnValue = this.MyDbSet(db).AsNoTracking().Where(predicate).ToArray();
			}

			return Task.FromResult(returnValue);
		}

		public virtual Task<IRepositoryContext> GetContextAsync()
		{
			this.LoggerSubscriber.Verbose($"{nameof(GetContextAsync)} called.");
			return Task.FromResult((IRepositoryContext)this.GetNewDbContext);
		}

		public virtual Task<IQueryable<TInterface>> GetQueryableAsync(IRepositoryContext context)
		{
			IQueryable<TInterface> returnValue = null;

			this.LoggerSubscriber.Verbose($"{nameof(GetQueryableAsync)} called for type '{typeof(TInterface).Name}'.");

			if (context is TContext db)
			{
				returnValue = this.MyDbSet(db).AsQueryable<TInterface>();
			}
			else
			{
				throw new InvalidContextException();
			}

			return Task.FromResult(returnValue);
		}

		public virtual async Task<bool> UpdateAsync(TInterface item)
		{
			bool returnValue = false;

			this.LoggerSubscriber.Verbose($"{nameof(UpdateAsync)} called for type '{typeof(TInterface).Name}'.");

			using (TContext db = this.GetNewDbContext)
			{
				db.Entry((TEntity)item).State = EntityState.Modified;
				int result = await db.SaveChangesAsync();
				this.LoggerSubscriber.Verbose($"{nameof(UpdateAsync)}: Records updated = {result}.");
				returnValue = (result == 1);
			}

			return returnValue;
		}

		public virtual async Task<(bool, TInterface)> AddAsync(TInterface item)
		{
			(bool result, TInterface entity) = (false, default);

			this.LoggerSubscriber.Verbose($"{nameof(AddAsync)} called for type '{typeof(TInterface).Name}'.");

			using (TContext db = this.GetNewDbContext)
			{
				entity = this.MyDbSet(db).Add((TEntity)item).Entity;
				result = (await db.SaveChangesAsync() == 1);
				this.LoggerSubscriber.Verbose($"{nameof(AddAsync)}: Records updated = {result}.");
			}

			return (result, entity);
		}

		public virtual async Task<bool> DeleteAsync(TInterface item)
		{
			bool returnValue = false;

			this.LoggerSubscriber.Verbose($"{nameof(DeleteAsync)} called for type '{typeof(TInterface).Name}'.");

			using (TContext db = this.GetNewDbContext)
			{
				db.Entry((TEntity)item).State = EntityState.Deleted;
				int result = await db.SaveChangesAsync();
				this.LoggerSubscriber.Verbose($"{nameof(DeleteAsync)}: Records updated = {result}.");
				returnValue = (result == 1);
			}

			return returnValue;
		}

		public virtual Task<bool> UpdateAsync(IRepositoryContext repositoryContext, TInterface item)
		{
			this.LoggerSubscriber.Verbose($"{nameof(UpdateAsync)} called for type '{typeof(TInterface).Name}' with context.");
			((TContext)repositoryContext).Entry((TEntity)item).State = EntityState.Modified;
			return Task.FromResult(true);
		}

		public virtual Task<TInterface> AddAsync(IRepositoryContext repositoryContext, TInterface item)
		{
			this.LoggerSubscriber.Verbose($"{nameof(AddAsync)} called for type '{typeof(TInterface).Name}' with context.");
			return Task.FromResult<TInterface>(this.MyDbSet((TContext)repositoryContext).Add((TEntity)item).Entity);
		}

		public virtual Task<bool> DeleteAsync(IRepositoryContext repositoryContext, TInterface item)
		{
			this.LoggerSubscriber.Verbose($"{nameof(DeleteAsync)} called for type '{typeof(TInterface).Name}' with context.");
			((TContext)repositoryContext).Entry((TEntity)item).State = EntityState.Deleted;
			return Task.FromResult(true);
		}
	}
}