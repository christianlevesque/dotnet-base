using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Core.Account;
using Autoinjector;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Client.Services.Infrastructure;
using Client.State;
using Core;

namespace Client.Services;

[Service(ServiceLifetime.Scoped, typeof(IAccountService))]
public class AccountService : BaseHttpService<AccountService>, IAccountService
{
	private readonly AuthStateProvider _authState;
	private readonly AccountStateProvider _accountState;

	public AccountService(HttpClient client, ILogger<AccountService> logger, ISnackbarService snackbar, AuthenticationStateProvider authState, AccountStateProvider accountState) : base("account", client, logger, snackbar)
	{
		_authState = (authState as AuthStateProvider)!;
		_accountState = accountState;
	}

	public async Task<ServiceResult<TokenDto>> Login(LoginDto login)
	{
		Logger.LogInformation("Initializing login process");
		try
		{
			var result = await SendWithRawResponse($"{Endpoint}/login", input: login, method: HttpMethod.Post);
			if (!result.IsSuccessStatusCode)
			{
				var errorMessage = await HandleFailureResponse(result);
				return ServiceResult<TokenDto>.Failure(errorMessage);
			}

			var response = await GetResponse<TokenDto>(result);

			if (!string.IsNullOrWhiteSpace(response?.Token))
			{
				await _authState.NotifyUserAuthentication(response.Token);

				// This ensures the HttpClient has had the bearer token applied
				await _authState.GetAuthenticationStateAsync();

				// Now we can load the user info
				var loginResult = await GetUserInfo( new ClaimsPrincipal());
				if (loginResult.WasSuccessful)
				{
					Logger.LogInformation("User logged in successfully");
                    return ServiceResult<TokenDto>.Ok(response);
				}
			}
		}
		catch (Exception e)
		{
			Logger.LogError(e, "Failed to log in user");
			Snackbar.Error(e.Message);
			return ServiceResult<TokenDto>.Failure(e.Message);
		}

		Snackbar.Error(GenericErrorMessage);
		return ServiceResult<TokenDto>.Failure(GenericErrorMessage);
	}

	public Task<ServiceResult<bool>> Register(RegisterDto account)
	{
		
		return SendRequest(Endpoint, "Registered successfully!", input: account, method: HttpMethod.Post);
	}

	public Task<ServiceResult<bool>> ConfirmAccount(ConfirmAccountDto account) => SendRequest($"{Endpoint}/confirm", "Account confirmed successfully!", input: account, method: HttpMethod.Post);

	public Task<ServiceResult<bool>> ChangePassword(ChangePasswordDto account, ClaimsPrincipal p) => SendRequest($"{Endpoint}/password", "Password changed successfully!", input: account, method: HttpMethod.Post);

	public Task<ServiceResult<bool>> ForgotPassword(ForgotPasswordDto account) => SendRequest($"{Endpoint}/password", "Password reset requested successfully!", input: account, method: HttpMethod.Delete);

	public Task<ServiceResult<bool>> ResetPassword(ResetPasswordDto account) => SendRequest($"{Endpoint}/password", "Password reset successfully!", input: account, method: HttpMethod.Patch);

	public Task<ServiceResult<bool>> InitiateEmailChange(InitiateEmailChangeDto account, ClaimsPrincipal p) => SendRequest($"{Endpoint}/email", "Email change requested successfully!", input: account, method: HttpMethod.Post);

	public Task<ServiceResult<bool>> PerformEmailChange(PerformEmailChangeDto account, ClaimsPrincipal p) => SendRequest($"{Endpoint}/email", "Email changed successfully!", input: account, method: HttpMethod.Patch);

	public async Task<ServiceResult<bool>> DeleteAccount(ClaimsPrincipal p)
	{
		var deleted = await SendRequest(Endpoint, "Account deleted successfully!", method: HttpMethod.Delete);
		if (deleted.WasSuccessful)
		{
			await _authState.NotifyUserLogout();
		}

		return deleted;
	}

	public async Task<ServiceResult<byte[]>> GetPersonalData(ClaimsPrincipal p)
	{
		var result = await SendWithRawResponse($"{Endpoint}/data");
		if (!result.IsSuccessStatusCode)
		{
			return ServiceResult<byte[]>.Failure("Failed to fetch personal data.");
		}

		var data = await result.Content.ReadAsStreamAsync();

		// Seems risky, but the data returned should always be relatively small
		using var ms = new MemoryStream((int)data.Length);
		await data.CopyToAsync(ms);
		var bytes = ms.ToArray();

		return ServiceResult<byte[]>.Ok(bytes);
	}

	public async Task<ServiceResult<ApplicationUserDto>> GetUserInfo(ClaimsPrincipal p)
	{
		Logger.LogInformation("Getting info for the currently logged in user");
		try
		{
			var result = await SendWithRawResponse($"{Endpoint}/login");

			// If the result is unauthorized
			// we might have a JWT in local storage
			// so clear that out since it's invalid
			if (result.StatusCode == HttpStatusCode.Unauthorized)
			{
				await _authState.NotifyUserLogout();
				return ServiceResult<ApplicationUserDto>.Failure("User is not logged in");
			}

			// If the result wasn't successful
			// we don't have a user to store state for
			if (!result.IsSuccessStatusCode)
			{
				return ServiceResult<ApplicationUserDto>.Failure("Status code was unsuccessful");
			}

			var user = await GetResponse<ApplicationUserDto>(result);
			if (user is null)
			{
				return ServiceResult<ApplicationUserDto>.Failure("User object was null");
			}

			_accountState.User = user;
		}
		catch (Exception e)
		{
			Logger.LogError(e, "Failed to fetch user info");
			return ServiceResult<ApplicationUserDto>.Failure("Failed to fetch user info");
		}

		return ServiceResult<ApplicationUserDto>.Ok(_accountState.User);
	}

	public async Task<ServiceResult<bool>> Logout()
	{
		try
		{
			await _authState.NotifyUserLogout();
		}
		catch (Exception e)
		{
			Logger.LogError("Failed to log out user: {}", e.Message);
			return ServiceResult<bool>.Failure("Failed to log out user");
		}

		return ServiceResult<bool>.Ok(true);
	}
}