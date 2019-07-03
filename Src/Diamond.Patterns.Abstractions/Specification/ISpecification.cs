﻿using System.Threading.Tasks;

namespace Diamond.Patterns.Abstractions
{
	public interface ISpecification
	{
	}

	public interface ISpecification<TResult> : ISpecification
	{
		Task<TResult> ExecuteQueryAsync();
	}

	public interface ISpecification<TParameter, TResult> : ISpecification
	{
		Task<TResult> ExecuteQueryAsync(TParameter filter);
	}
}