using System.Net;
using System.Threading.Tasks;
using Core;
using Core.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Server.Controllers;

[ApiController]
[Route("/api/[controller]")]
[Authorize(Roles = Roles.Admin)]
public class UsersController : AppControllerBase<UsersController, IUserAdminService>
{
	/// <inheritdoc />
	public UsersController(ILogger<UsersController> logger, IUserAdminService service) : base(logger, service)
	{
	}

	[HttpGet]
	public Task<IActionResult> GetAllUsers([FromQuery] Filter filter)
		=> ProcessServiceCall(() => Service.GetAllUsers(filter));

	[HttpGet("{userId}")]
	public Task<IActionResult> GetUserData(string userId)
		=> ProcessServiceCall(() => Service.GetUserData(userId));

	[HttpPatch("email")]
	public Task<IActionResult> UpdateUserEmail(UserChangeEmailDto dto)
	{
		return ProcessServiceCall(
			() => Service.UpdateUserEmail(dto),
			HttpStatusCode.NoContent);
	}

	[HttpPatch("password")]
	public Task<IActionResult> UpdateUserPassword(UserChangePasswordDto dto)
	{
		return ProcessServiceCall(
			() => Service.UpdateUserPassword(dto),
			HttpStatusCode.NoContent);
	}

	[HttpGet("roles")]
	public Task<IActionResult> GetAvailableRoles() => ProcessServiceCall(() => Service.GetAvailableRoles());

	[HttpPut("roles")]
	public Task<IActionResult> AddUserToRole(UserUpdateRolesDto dto)
	{
		return ProcessServiceCall(
			() => Service.AddUserToRole(dto),
			HttpStatusCode.NoContent);
	}
}