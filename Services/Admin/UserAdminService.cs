using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autoinjector;
using Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Repositories.Identity;
using Core.Account;
using Core.Users;
using Microsoft.Extensions.DependencyInjection;
using Services.Email;
using Services.Identity;

namespace Services.Admin;

[Service(ServiceLifetime.Scoped, typeof(IUserAdminService))]
public class UserAdminService : IUserAdminService
{
	private readonly IUserRepository _userRepo;
	private readonly UserService _userService;
	private readonly RoleManager<IdentityRole> _roleManager;
	private readonly IDtoAdapter<ApplicationUser, ApplicationUserDto> _userDtoAdapter;
	private readonly IEmailSenderService _emailService;

	public UserAdminService(IUserRepository userRepo,
	                        UserService userService,
	                        RoleManager<IdentityRole> roleManager,
	                        IDtoAdapter<ApplicationUser, ApplicationUserDto> userDtoAdapter,
	                        IEmailSenderService emailService)
	{
		_userRepo = userRepo;
		_userService = userService;
		_roleManager = roleManager;
		_userDtoAdapter = userDtoAdapter;
		_emailService = emailService;
	}

	public async Task<ServiceResult> GetAllUsers(Filter filter)
	{
		var users = await _userRepo.GetPagified(filter);

		var items = new List<ApplicationUserDto>();
		foreach (var user in users)
		{
			items.Add(await _userDtoAdapter.MapToDto(user));
		}

		var result = new PagedDto<ApplicationUserDto>
		{
			Items = items,
			TotalCount = await _userRepo.GetCount(filter)
		};

		return ServiceResult.Ok(result);
	}

	public async Task<ServiceResult> GetUserData(string userId)
	{
		var user = await _userRepo.FindById(userId);
		return ServiceResult.Ok(await _userDtoAdapter.MapToDto(user));
	}

	public async Task<ServiceResult> UpdateUserEmail(UserChangeEmailDto data)
	{
		// Find user
		var user = await _userRepo.FindById(data.UserId);

		// Add pending email
		user.PendingEmail = data.Email;
		await _userRepo.Update(user);

		// Generate verification code
		var code = await _userService.GenerateChangeEmailTokenAsync(user, user.PendingEmail);
		var codeBytes = Encoding.UTF8.GetBytes(code);
		code = WebEncoders.Base64UrlEncode(codeBytes);

		// Send verification code
		await _emailService.SendEmailChangeConfirmationEmail(user.UserName, user.PendingEmail, user.Id, code);
		return ServiceResult.Ok();
	}

	public async Task<ServiceResult> UpdateUserPassword(UserChangePasswordDto data)
	{
		var user = await _userRepo.FindById(data.UserId);

		var result = await _userService.ChangePasswordAsync(user, data.NewPassword);

		return result.Succeeded
			       ? ServiceResult.Ok()
			       : ServiceResult.Unprocessable(result.Errors.First().Description);
	}

	public Task<ServiceResult> GetAvailableRoles()
	{
		var roles = _roleManager.Roles.Select(r => r.Name);
		var result = ServiceResult.Ok(roles);
		return Task.FromResult(result);
	}

	public async Task<ServiceResult> AddUserToRole(UserUpdateRolesDto data)
	{
		var user = await _userRepo.FindById(data.UserId);
		var roles = await _userService.GetRolesAsync(user);
		if (roles.Count > 0)
		{
			var removeFromRolesResult = await _userService.RemoveFromRolesAsync(user, roles);
            if (!removeFromRolesResult.Succeeded)
            {
                return ServiceResult.Unprocessable("Failed to remove user from roles");
            }
		}

		var result = await _userService.AddToRolesAsync(user, data.Roles);

		return result.Succeeded
			       ? ServiceResult.Ok()
			       : ServiceResult.Unprocessable("Failed to add user to roles");
	}
}

public interface IUserAdminService
{
	Task<ServiceResult> GetAllUsers(Filter filter);
	Task<ServiceResult> GetUserData(string userId);
	Task<ServiceResult> UpdateUserEmail(UserChangeEmailDto data);
	Task<ServiceResult> UpdateUserPassword(UserChangePasswordDto data);
	Task<ServiceResult> GetAvailableRoles();
	Task<ServiceResult> AddUserToRole(UserUpdateRolesDto data);
}