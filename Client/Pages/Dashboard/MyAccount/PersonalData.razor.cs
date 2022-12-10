using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using MudBlazor;
using Client.Shared;
using Client.Shared.Modals.MyAccount;
using Client.Tools;
using Core.Account;

namespace Client.Pages.Dashboard.MyAccount;

[Route(Urls.Dashboard.MyAccount.PersonalData)]
[Layout(typeof(DashboardNarrowLayout))]
public partial class PersonalData
{
	[Inject]
	private IAccountService Service { get; set; } = default!;

	[Inject]
	private ILogger<PersonalData> Logger { get; set; } = default!;

	[Inject]
	private IJSRuntime Interop { get; set; } = default!;

	[Inject]
	private IDialogService Dialog { get; set; } = default!;

	private bool _isLoading;

	private async Task DownloadPersonalData()
	{
		if (_isLoading)
		{
			return;
		}

		Logger.LogInformation("Downloading personal data");

		_isLoading = true;
		var data = await Service.GetPersonalData(new ClaimsPrincipal());
		using var stream = new MemoryStream(data.Result ?? Array.Empty<byte>());
		using var dataWrapper = new DotNetStreamReference(stream);
		await Interop.InvokeVoidAsync("downloadFileFromStream", "PersonalData.json", dataWrapper);
		_isLoading = false;
	}

	private void LaunchDeleteAccountDialog()
	{
		Dialog.Show<DeleteAccountConfirmation>();
	}
}