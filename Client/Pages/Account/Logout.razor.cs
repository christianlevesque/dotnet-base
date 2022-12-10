using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Client.Tools;
using Core.Account;

namespace Client.Pages.Account;

[Route(Urls.Account.Logout)]
[AllowAnonymous]
public partial class Logout
{
	[Inject]
	private IAccountService AccountService { get; set; } = default!;

	[Inject]
	private NavigationManager NavManager { get; set; } = default!;

	private string _message = "Logging you out. Please wait...";

	protected override async Task OnParametersSetAsync()
	{
		var result = await AccountService.Logout(); 
		if (result.WasSuccessful)
		{
			NavManager.NavigateTo(Urls.Home);
		}
		else
		{
			_message = "We were unable to log you out.";
		}
	}
}