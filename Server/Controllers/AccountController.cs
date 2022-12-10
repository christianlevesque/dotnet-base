using System.Net;
using System.Threading.Tasks;
using Core.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Server.Controllers;

[Route("/api/[controller]")]
[ApiController]
[Authorize]
public class AccountController : AppControllerBase<AccountController, IAccountService>
{
	/// <inheritdoc />
	public AccountController(ILogger<AccountController> logger, IAccountService service) : base(logger, service)
	{
	}

	[AllowAnonymous]
	[HttpPost]
	public Task<IActionResult> Register(RegisterDto userData)
	{
		return ProcessServiceCall(
			() => Service.Register(userData),
			HttpStatusCode.NoContent);
	}

	[HttpGet("login")]
	public Task<IActionResult> GetUserData()
		=> ProcessServiceCall(() => Service.GetUserInfo(User));

	[AllowAnonymous]
	[HttpPost("login")]
	public Task<IActionResult> Login(LoginDto login) 
		=> ProcessServiceCall(() => Service.Login(login));

	[AllowAnonymous]
	[HttpPost("confirm")]
	public Task<IActionResult> ConfirmAccount(ConfirmAccountDto accountDto)
	{
		return ProcessServiceCall(
			() => Service.ConfirmAccount(accountDto), 
			HttpStatusCode.NoContent);
	}

	[HttpPost("email")]
	public  Task<IActionResult> InitiateEmailChange(InitiateEmailChangeDto emailDto)
	{
		return ProcessServiceCall(
			() => Service.InitiateEmailChange(emailDto, User), 
			HttpStatusCode.NoContent);
	}

	[HttpPatch("email")]
	public Task<IActionResult> PerformEmailChange(PerformEmailChangeDto changeDto)
	{
		return ProcessServiceCall(
			() => Service.PerformEmailChange(changeDto, User), 
			HttpStatusCode.NoContent);
	}

	[HttpPost("password")]
	public Task<IActionResult> ChangePassword(ChangePasswordDto passwordDto)
	{
		return ProcessServiceCall(
			() => Service.ChangePassword(passwordDto, User),
			HttpStatusCode.NoContent);
	}

	[AllowAnonymous]
	[HttpDelete("password")]
	public Task<IActionResult> ForgotPassword(ForgotPasswordDto passwordDto)
	{
		return ProcessServiceCall(
			() => Service.ForgotPassword(passwordDto),
			HttpStatusCode.Accepted);
	}

	[AllowAnonymous]
	[HttpPatch("password")]
	public Task<IActionResult> ResetPassword(ResetPasswordDto passwordDto)
	{
		return ProcessServiceCall(
			() => Service.ResetPassword(passwordDto), 
			HttpStatusCode.NoContent);
	}

	[HttpGet("data")]
	public Task<IActionResult> GetPersonalData()
	{
		return ProcessServiceCallReturningFile(
			() => Service.GetPersonalData(User),
			"PersonalData.json",
			"application/json");
	}

	[HttpDelete]
	public Task<IActionResult> DeleteAccount()
	{
		return ProcessServiceCall(
			() => Service.DeleteAccount(User),
			HttpStatusCode.NoContent);
	}
}