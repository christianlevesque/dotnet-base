using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Core.Account;
using Client.Shared;
using Client.Shared.Pages;
using Client.Tools;

namespace Client.Pages.Account.ResetPassword;

[Route(Urls.Account.ResetPassword.Index)]
[AllowAnonymous]
[Layout(typeof(StandaloneNarrowLayout))]
public partial class ResetPasswordIndex : FormPage<ResetPasswordIndex, IAccountService, ResetPasswordDto>
{
	[Parameter]
	[SupplyParameterFromQuery]
	public string? UserId { get; set; }

	[Parameter]
	[SupplyParameterFromQuery]
	public string? Code { get; set; }

	protected override void OnInitialized()
	{
		Model.UserId = UserId ?? string.Empty;
		Model.VerificationCode = Code ?? string.Empty;
	}

	protected override async Task OnSubmit()
	{
		SetFormCompletionTime(Model);

		await SubmitRequest(() => Service.ResetPassword(Model));
		if (WasSuccessful)
		{
			NavManager.NavigateTo(Urls.Account.ResetPassword.Successful);
		}
	}
}