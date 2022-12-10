using System;
using System.Net;
using System.Threading.Tasks;
using Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Server.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class CrudControllerBase<TController, TService, TDto>
	: AppControllerBase<TController, TService>
	where TController : ControllerBase
	where TDto : EntityBase
	where TService : ICrudService<TDto>
{
	/// <inheritdoc />
	public CrudControllerBase(ILogger<TController> logger, TService service) : base(logger, service)
	{
	}

	[HttpGet]
	[AllowAnonymous]
	public virtual Task<IActionResult> GetAll([FromQuery] Filter filter)
		=> ProcessServiceCall(() => Service.Get(filter));

	[HttpGet("{id:guid}")]
	[AllowAnonymous]
	public virtual Task<IActionResult> GetById(Guid id)
		=> ProcessServiceCall(() => Service.Get(id));

	[HttpPost]
	public virtual Task<IActionResult> Create(TDto entity)
		=> ProcessServiceCall(() => Service.Add(entity));

	[HttpPut("{id:guid}")]
	public virtual Task<IActionResult> Update(Guid id, TDto entity)
	{
		entity.Id = id;
		return ProcessServiceCall(
			() => Service.Edit(entity),
			HttpStatusCode.NoContent);
	}

	[HttpDelete("{id:guid}")]
	public virtual Task<IActionResult> Delete(Guid id) 
		=> ProcessServiceCall(
			() => Service.Delete(id),
			HttpStatusCode.NoContent);
}