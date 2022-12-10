using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Core;
using Microsoft.Extensions.Logging;
using Core.Account;
using Autoinjector;
using Core.Users;
using Microsoft.Extensions.DependencyInjection;
using Client.Services.Infrastructure;

namespace Client.Services;

[Service(ServiceLifetime.Scoped, typeof(IUserAdminService))]
public class UserAdminService : BaseHttpService<UserAdminService>, IUserAdminService
{
	public UserAdminService(HttpClient client, ILogger<UserAdminService> logger, ISnackbarService snackbar) : base("users", client, logger, snackbar) {}

	public Task<ServiceResult<PagedDto<ApplicationUserDto>>> GetAllUsers(Filter? filter = null) => SendRequest<PagedDto<ApplicationUserDto>>(Endpoint, input: filter);

	public Task<ServiceResult<ApplicationUserDto>> GetUserData(string id) => SendRequest<ApplicationUserDto>($"{Endpoint}/{id}");

	public Task<ServiceResult<bool>> UpdateUserEmail(UserChangeEmailDto user) => SendRequest($"{Endpoint}/email", "Updated user email successfully!", input: user, method: HttpMethod.Patch);

	public Task<ServiceResult<bool>> UpdateUserPassword(UserChangePasswordDto user) => SendRequest($"{Endpoint}/password", "Updated user password successfully!", input: user, method: HttpMethod.Patch);
	public Task<ServiceResult<IEnumerable<string>>> GetAvailableRoles() => throw new InvalidOperationException("Roles are stored statically in the client");

	public Task<ServiceResult<bool>> AddUserToRole(UserUpdateRolesDto user) => SendRequest($"{Endpoint}/roles", "Updated user roles successfully!", input: user, method: HttpMethod.Put);
}