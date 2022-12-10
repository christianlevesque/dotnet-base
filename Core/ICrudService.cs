using System;
using System.Threading.Tasks;

namespace Core;

public interface ICrudService<TDto, TKey>
	where TDto : EntityBase<TKey>
	where TKey : struct, IEquatable<TKey>
{
	/// <summary>
	/// Creates a new entry in the backend
	/// </summary>
	/// <param name="model">The DTO representing the entity to create</param>
	/// <returns><c>Task</c></returns>
	Task<ServiceResult<TKey>> Add(TDto model);

	/// <summary>
	/// Updates an existing entity in the backend
	/// </summary>
	/// <param name="model">The DTO representing the entity to update</param>
	/// <returns><c>Task</c></returns>
	Task<ServiceResult<bool>> Edit(TDto model);

	/// <summary>
	/// Gets an entity by primary key
	/// </summary>
	/// <param name="id">The primary key of the entity to retrieve</param>
	/// <returns>the requested <c>TModel</c></returns>
	Task<ServiceResult<TDto>> Get(TKey id);

	/// <summary>
	/// Gets a list of all entities in the backend
	/// </summary>
	/// <returns>a list of all entities in the database</returns>
	Task<ServiceResult<PagedDto<TDto>>> Get(Filter? filter = null);

	/// <summary>
	/// Deletes an entity by primary key
	/// </summary>
	/// <param name="id">The primary key of the entity to delete</param>
	/// <returns><c>Task</c></returns>
	Task<ServiceResult<bool>> Delete(TKey id);
}

public interface ICrudService<TDto> : ICrudService<TDto, Guid>
	where TDto : EntityBase
{}