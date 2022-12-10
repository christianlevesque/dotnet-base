using System;
using Client.State;
using Client.Tools.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MudBlazor.Services;

namespace Server.Tools;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddServerServices(this IServiceCollection services, IHostEnvironment env, IConfiguration config)
	{
		return services
			.AddMudServices()
			.AddRoutingServices(env)
			.AddServerOptions(config);
	}

	private static IServiceCollection AddRoutingServices(this IServiceCollection services, IHostEnvironment env)
	{
		services
			.AddRouting()
			.AddControllersWithViews();
		services.AddRazorPages();
		services.AddServerSideBlazor();

		services.AddScoped<LoadingStateProvider>()
			.AddScoped<AccountStateProvider>()
			.AutoinjectServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly);

		if (env.IsDevelopment())
		{
			services.AddCors(options =>
			{
				options.AddDefaultPolicy(policy =>
				{
					policy
						.AllowAnyOrigin()
						.AllowAnyHeader()
						.AllowAnyMethod();
				});
			});
		}

		return services;
	}

	private static IServiceCollection AddServerOptions(this IServiceCollection services, IConfiguration config)
	{
		// Changes the invalid model state response
		services.Configure<ApiBehaviorOptions>(
			options =>
			{
				options.InvalidModelStateResponseFactory = context => new UnprocessableEntityObjectResult(context.ModelState);
			}
		);

		services.Configure<DataProtectionTokenProviderOptions>(o =>
		{
			o.TokenLifespan = TimeSpan.FromDays(30);
		});

		services.Configure<SiteOptions>(config.GetSection("Site"));

		return services;
	}
}