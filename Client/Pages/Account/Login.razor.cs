using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Core.Account;
using Client.Shared.Pages;
using Client.Tools;
using Client.Tools.Extensions;
using Core.Infrastructure;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Options;

namespace Client.Pages.Account;

[Route(Urls.Account.Login)]
[AllowAnonymous]
public partial class Login : FormPage<Login, IAccountService, LoginDto>
{
	private bool _validatingLogin = true;

	[Parameter]
	[SupplyParameterFromQuery]
	public string? ReturnUri { get; set; }

	[CascadingParameter]
	private Task<AuthenticationState> AuthState { get; set; } = default!;

	[Inject]
	private IOptions<SharedDataOptions> SharedData { get; set; } = default!;

	protected override async Task OnInitializedAsync()
	{
		// We want the server to always render the login validation overlay
		// and we want the client to never render the login validation overlay
		// Reason being, if the user navigates to login on the client side,
		// they are already known not to be logged
		if (SharedData.Value.IsServer)
		{
			return;
		}

		if ((await AuthState).IsAuthenticated())
		{
			DoNav();
		}
		else
		{
			_validatingLogin = false;
		}
	}

	protected override async Task OnSubmit()
	{
		SetFormCompletionTime(Model);

		await SubmitRequest(() => Service.Login(Model));
		if (WasSuccessful)
		{
			DoNav();
		}
	}

	private void DoNav()
	{
		NavManager.NavigateTo(ReturnUri ?? Urls.Dashboard.Index);
	}
}