using System.Net.Http;
using Autoinjector;
using Microsoft.Extensions.Logging;
using Core.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Core;

namespace Client.Services.Infrastructure;

[Service(ServiceLifetime.Scoped, typeof(ICrudService<StateDto>))]
public class StateService : CrudService<StateDto, StateService>, ICrudService<StateDto>
{
	public StateService(HttpClient client,
	                    ILogger<StateService> logger,
	                    ISnackbarService snackbar)
		: base("states", client, logger, snackbar)
	{
		AddSuccessMessage = "State created successfully!";
		EditSuccessMessage = "State updated successfully!";
		DeleteSuccessMessage = "State deleted successfully!";
	}
}