using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Core;
using Core.Infrastructure;

namespace Client.Shared.Tables.Templates.Dashboard;

public partial class StateTemplate
{
	[Inject]
	private ICrudService<StateDto> Service { get; set; } = default!;

	private async Task DeleteState(Guid id)
	{
		var result = await Service.Delete(id); 
		if (result.WasSuccessful)
		{
			await Table.ReloadServerData();
		}
	}
}