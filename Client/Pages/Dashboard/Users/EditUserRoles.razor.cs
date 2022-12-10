using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Core;
using Core.Account;
using Client.Shared;
using Client.Shared.UI.Forms;
using Client.Tools;

namespace Client.Pages.Dashboard.Users;

[Layout(typeof(DashboardNarrowLayout))]
[Authorize(Roles = Roles.Admin)]
[Route($"{Urls.Dashboard.Users.Index}/{{userId}}/roles")]
public partial class EditUserRoles
{
	[Parameter]
	public string UserId { get; set; } = default!;

	private CheckboxAggregator _aggregator = default!;

	private ApplicationUserDto? _user;

	protected override async Task OnInitializedAsync()
	{
		_user = await SubmitRequest(() => Service.GetUserData(UserId));
		Model.UserId = UserId;
	}

	protected override async Task OnSubmit()
	{
		Model.Roles = _aggregator.GetSelected();
		await SubmitRequest(() => Service.AddUserToRole(Model));
	}
}