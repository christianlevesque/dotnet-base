using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Core;
using Core.Infrastructure;

namespace Server.Controllers;

[Authorize(Roles = Roles.Admin)]
public class StatesController : CrudControllerBase<StatesController, ICrudService<StateDto>, StateDto>
{
	/// <inheritdoc />
	public StatesController(ILogger<StatesController> logger, ICrudService<StateDto> service) : base(logger, service)
	{
	}
}