using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Client.Tools;

namespace Client.Shared.Modals.MyAccount;

public partial class DeleteAccountConfirmation
{
	[CascadingParameter]
	private MudDialogInstance Instance { get; set; } = default!;

	private async Task HandleSubmit()
	{
		await SubmitRequest(() => Service.DeleteAccount(new ClaimsPrincipal()));
		if (WasSuccessful)
		{
			NavManager.NavigateTo(Urls.Account.Deleted);
			Close();
		}
	}

	private void Close()
	{
		Instance.Close();
	}
}