using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Core.Account;
using Client.Shared.Pages;
using Client.Tools;

namespace Client.Pages.Account.ForgotPassword;

[Route(Urls.Account.ForgotPassword.Index)]
[AllowAnonymous]
public partial class ForgotPasswordIndex : FormPage<ForgotPasswordIndex, IAccountService, ForgotPasswordDto>
{
	protected override async Task OnSubmit()
	{
		SetFormCompletionTime(Model);

		await SubmitRequest(() => Service.ForgotPassword(Model));
		if (WasSuccessful)
		{
			NavManager.NavigateTo(Urls.Account.ForgotPassword.Successful);
		}
	}
}