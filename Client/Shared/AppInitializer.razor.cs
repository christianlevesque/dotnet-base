using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using Client.Services;
using Client.State;
using Core.Account;

namespace Client.Shared;

public partial class AppInitializer
{
	[CascadingParameter]
	private Task<AuthenticationState> AuthState { get; set; } = default!;

	[Parameter]
	public RenderFragment ChildContent { get; set; } = default!;

	[Inject]
	private IAccountService AccountService { get; set; } = default!;

	[Inject]
	private ILogger<AppInitializer> Logger { get; set; } = default!;

	[Inject]
	private LoadingStateProvider LoadingStateProvider { get; set; } = default!;

	protected override async Task OnInitializedAsync()
	{
		Logger.LogInformation("Checking if user is logged in");

		if ((await AuthState).User.Identity?.IsAuthenticated ?? false)
		{
			LoadingStateProvider.AddLoadingOperation();
			await AccountService.GetUserInfo(new ClaimsPrincipal());
			LoadingStateProvider.RemoveLoadingOperation();
		}
	}
}