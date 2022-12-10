using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Core;
using Client.Services.Infrastructure;

namespace Client.Services;

public abstract class CrudService<TDto, TKey, TService> : BaseHttpService<TService>, ICrudService<TDto, TKey>
	where TDto : EntityBase<TKey>
	where TKey : struct, IEquatable<TKey>
{
	protected string AddSuccessMessage = string.Empty;
	protected string EditSuccessMessage = string.Empty;
	protected string DeleteSuccessMessage = string.Empty;

	protected CrudService(string endpoint,
	                   HttpClient client,
	                   ILogger<TService> logger,
	                   ISnackbarService snackbar)
		: base(endpoint, client, logger, snackbar)
	{
	}

	/// <inheritdoc/>
	public Task<ServiceResult<TKey>> Add(TDto model) => SendRequest<TKey>(Endpoint, AddSuccessMessage, input: model, method: HttpMethod.Post);

	/// <inheritdoc/>
	public Task<ServiceResult<bool>> Edit(TDto model) => SendRequest($"{Endpoint}/{model.Id}", EditSuccessMessage, input: model, method: HttpMethod.Put);

	/// <inheritdoc/>
	public Task<ServiceResult<TDto>> Get(TKey id) => SendRequest<TDto>($"{Endpoint}/{id}");

	/// <inheritdoc/>
	/// <remarks>
	/// The <see cref="PagedDto{TDto}"/> returned from this method is nullable. However, to make table paging simpler, we want this to never be null. So this method ensures that the <see cref="PagedDto{TDto}"/> is always initialized, even if it's null in the <see cref="ServiceResult{T}"/>.
	/// </remarks>
	public async Task<ServiceResult<PagedDto<TDto>>> Get(Filter? filter = null)
	{
		if (filter?.SearchTerm != null)
		{
			filter.SearchTerm = filter.SearchTerm.ToLower();
		}

		var response = await SendRequest<PagedDto<TDto>>(Endpoint, input: filter);
		response.Result ??= new PagedDto<TDto>();
		return response;
	}

	/// <inheritdoc/>
	public Task<ServiceResult<bool>> Delete(TKey id) => SendRequest($"{Endpoint}/{id}", DeleteSuccessMessage, method: HttpMethod.Delete);
}

public abstract class CrudService<TDto, TService> : CrudService<TDto, Guid, TService>
	where TDto : EntityBase
{
	/// <inheritdoc />
	protected CrudService(string endpoint,
	                      HttpClient client,
	                      ILogger<TService> logger,
	                      ISnackbarService snackbar)
		: base(endpoint, client, logger, snackbar)
	{
	}
}