using System;
using System.Linq;
using System.Threading.Tasks;
using Autoinjector;
using Core;
using Repositories.Infrastructure;
using Services.Errors;
using Core.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Repositories;

namespace Services.Infrastructure;

[Service(ServiceLifetime.Scoped, typeof(ICrudService<StateDto>))]
public class StateService
	: CrudService<State, StateDto, IRepository<State>>,
		ICrudService<StateDto>
{
	public StateService(IRepository<State> repo, IDtoAdapter<State, StateDto> adapter)
		: base(repo, adapter)
	{
	}

	/// <inheritdoc />
	public override async Task<ServiceResult<Guid>> Add(StateDto model)
	{
		try
		{
			await VerifyStateUnique(model);
		}
		catch (Exception e)
		{
			return CreateErrorResult<Guid>(e);
		}

		return await base.Add(model);
	}

	/// <inheritdoc />
	public override async Task<ServiceResult<bool>> Edit(StateDto model)
	{
		try
		{
			await VerifyStateUnique(model);
		}
		catch (Exception e)
		{
			return CreateErrorResult<bool>(e);
		}

		return await base.Edit(model);
	}

	private async Task VerifyStateUnique(StateDto model)
	{
		if ((await Repo.Find(s => s.Id != model.Id && s.Name == model.Name, take: 1)).Any())
		{
			throw new AppConflictException(ErrorMessages.States.DuplicateName);
		}

		if ((await Repo.Find(s => s.Id != model.Id && s.Abbreviation == model.Abbreviation, take: 1)).Any())
		{
			throw new AppConflictException(ErrorMessages.States.DuplicateAbbreviation);
		}
	}
}