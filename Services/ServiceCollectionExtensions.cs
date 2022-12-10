using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Repositories;
using Repositories.Identity;
using SendGrid;
using Services.Email;
using Services.Identity;

namespace Services;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration config, IWebHostEnvironment env)
	{
		return services
			.AddCustomDatabase(config)
			.AddCustomIdentity()
			.ConfigureCustomIdentity()
			.AddOptions(config)
			.AddApplicationServices(config);
	}

	public static IServiceCollection AddCustomDatabase(this IServiceCollection services, IConfiguration config)
	{
		return services.AddDbContext<ApplicationDbContext>(options =>
		{
			options.UseSqlServer(config.GetConnectionString("Default"));
		});
	}

	public static IServiceCollection AddCustomIdentity(this IServiceCollection services)
	{
		// Set up Identity Core and Identity's EF Core
		services.AddIdentityCore<ApplicationUser>(o => { o.Stores.MaxLengthForKeys = 128; })
				.AddSignInManager<SignInService>()
				.AddUserManager<UserService>()
				.AddRoles<IdentityRole>()
				.AddRoleStore<RoleStore<IdentityRole, ApplicationDbContext, string>>()
				.AddEntityFrameworkStores<ApplicationDbContext>()
				.AddDefaultTokenProviders();

		services.TryAddScoped<ISecurityStampValidator, SecurityStampValidator<ApplicationUser>>();

		return services;
	}

	public static IServiceCollection ConfigureCustomIdentity(this IServiceCollection services)
	{
		// Configure Identity Password, Username, Email, Account settings
		services.Configure<IdentityOptions>(options =>
		{
			// Password settings.
			options.Password.RequireDigit = true;
			options.Password.RequireLowercase = true;
			options.Password.RequireNonAlphanumeric = true;
			options.Password.RequireUppercase = true;
			options.Password.RequiredLength = 8;
			options.Password.RequiredUniqueChars = 1;

			// Lockout settings.
			options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
			options.Lockout.MaxFailedAccessAttempts = 5;
			options.Lockout.AllowedForNewUsers = true;

			// User settings.
			options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_.";
			options.User.RequireUniqueEmail = true;

			options.SignIn.RequireConfirmedEmail = true;
			options.SignIn.RequireConfirmedAccount = true;
		});

		return services;
	}

	private static IServiceCollection AddOptions(this IServiceCollection services, IConfiguration config)
	{
		return services
			.Configure<EmailOptions>(config.GetSection("Email"))
			.Configure<JwtOptions>(config.GetSection("Jwt")); // TODO: remove or disable if using SSR
	}

	private static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
	{
		return services
			.AddScoped<ISendGridClient>(p => new SendGridClient(config["Email:ApiKey"]))
			.AutoinjectServicesFromAssembly(typeof(IRepository<>).Assembly)
			.AutoinjectServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly);
	}
}