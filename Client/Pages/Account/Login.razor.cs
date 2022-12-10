using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Core.Account;
using Client.Shared.Pages;
using Client.Tools;

namespace Client.Pages.Account;

[Route(Urls.Account.Login)]
[AllowAnonymous]
public partial class Login : FormPage<Login, IAccountService, LoginDto>
{
	[Parameter]
	[SupplyParameterFromQuery]
	public string? ReturnUri { get; set; }

	protected override async Task OnSubmit()
	{
		SetFormCompletionTime(Model);

		await SubmitRequest(() => Service.Login(Model));
		if (WasSuccessful)
		{
			NavManager.NavigateTo(ReturnUri ?? Urls.Dashboard.Index);
		}
	}
}