using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Core;
using Core.Account;
using Client.Shared;
using Client.Shared.Modals.Users;
using Client.Tools;

namespace Client.Pages.Dashboard.Users;

[Layout(typeof(DashboardNarrowLayout))]
[Authorize(Roles = Roles.Admin)]
[Route($"{Urls.Dashboard.Users.Index}/{{userId}}")]
public partial class ViewUserData
{
	[Parameter]
	public string UserId { get; set; } = default!;

	[Inject]
	private IDialogService DialogService { get; set; } = default!;

	private ApplicationUserDto? _user;

	private void ShowUserPasswordModal()
	{
		if (_user == null)
		{
			return;
		}

		var parameters = new DialogParameters
		{
			{ "UserId", _user.Id },
			{ "Username", _user.UserName }
		};

		DialogService.Show<UserChangePassword>(string.Empty, parameters);
	}

	private void ShowUserEmailModal()
	{
		if (_user == null)
		{
			return;
		}

		var parameters = new DialogParameters
		{
			{ "UserId", _user.Id },
			{ "Username", _user.UserName },
			{ "Email", _user.Email }
		};

		DialogService.Show<UserChangeEmail>(string.Empty, parameters);
	}

	protected override async Task OnInitializedAsync()
	{
		_user = await SubmitRequest(() => Service.GetUserData(UserId));
	}
}