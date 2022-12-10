using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Core.Account;
using Client.Shared.Pages;
using Client.Tools;

namespace Client.Pages.Account.Register;

[Route(Urls.Account.Register.Index)]
[AllowAnonymous]
public partial class RegisterIndex : FormPage<RegisterIndex, IAccountService, RegisterDto>
{
	protected override async Task OnSubmit()
	{
		// MudBlazor doesn't show errors on checkboxes
		// so unfortunately, a [RequireTrue] wouldn't do any good
		if (!Model.AcceptTos)
		{
			ErrorMessage = "You must accept the Terms of Service and Privacy Policy to register.";
			return;
		}

		SetFormCompletionTime(Model);

		await SubmitRequest(() => Service.Register(Model));

		if (WasSuccessful)
		{
			NavManager.NavigateTo($"{Urls.Account.Register.Successful}?username={Model.UserName}&email={Model.Email}");
		}
	}
}