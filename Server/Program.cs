using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Server.Policies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using Server.Tools;
using Services;

var path = $"{Directory.GetCurrentDirectory()}/log";
if (!Directory.Exists(path))
{
	Directory.CreateDirectory(path);
}

Log.Logger = new LoggerConfiguration()
	.WriteTo.Console()
	.CreateBootstrapLogger();

Log.Information("Starting #sitename#"); // TODO: set up site identity

try
{
	var builder = WebApplication.CreateBuilder(args);

	builder.Host.UseSerilog((ctx, config) =>
	{
		config.MinimumLevel.Debug()
			.WriteTo.File(
				$"{path}/log-.log",
				rollingInterval: RollingInterval.Hour,
				restrictedToMinimumLevel: LogEventLevel.Error,
				retainedFileCountLimit: 168)
			.WriteTo.Console()
			.WriteTo.Debug();
	});

	AddApiAuthentication(builder.Services, builder.Configuration);
	AddCustomAuthorization(builder.Services);

	builder.Services
		.AddServerServices(builder.Environment)
		.AddServices(builder.Configuration, builder.Environment);

	var app = builder.Build();

	if (app.Environment.IsDevelopment())
	{
		app.UseDeveloperExceptionPage();
		app.UseWebAssemblyDebugging();
	}

	app.UseSerilogRequestLogging();
	app.UseRouting();

	app.UseBlazorFrameworkFiles();
	app.UseStaticFiles();

	if (app.Environment.IsDevelopment())
	{
		app.UseCors();
	}

	app.UseAuthentication();
	app.UseAuthorization();
	app.UseEndpoints(e =>
	{
		e.MapControllers();
		e.MapRazorPages();
		e.MapFallbackToPage("/_Host");
	});

	app.Run();
}
catch (Exception e)
{
	Log.Fatal(e, "#sitename# terminated unexpectedly"); // TODO: set up site identity
}
finally
{
	Log.Information("Shutting down #sitename#"); // TODO: set up site identity
	Log.CloseAndFlush();
}

void AddApiAuthentication(IServiceCollection services, IConfiguration config)
{
	var jwt = config.GetSection("Jwt");

	// Set up Authentication
	services.AddAuthentication(o =>
	        {
		        o.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
		        o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
	        })
	        .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme,o =>
	        {
		        var keyBytes = Encoding.UTF8.GetBytes(jwt["SecurityKey"]);

		        o.TokenValidationParameters = new TokenValidationParameters
		        {
			        ValidateIssuer = true,
			        ValidateAudience = true,
			        ValidateLifetime = true,
			        ValidateIssuerSigningKey = true,
			        ValidIssuer = jwt["Issuer"],
			        ValidAudience = jwt["Audience"],
			        IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
		        };
	        });
}

void AddCustomAuthorization(IServiceCollection services)
{
	var schemes = new List<string>
	{
		JwtBearerDefaults.AuthenticationScheme
	};

	// Configure Authorization pipelines
	services.AddAuthorization(options =>
	{
		var defaultPolicy = new AuthorizationPolicyBuilder
		{
			AuthenticationSchemes = schemes
		};

		options.DefaultPolicy = defaultPolicy
			.RequireAuthenticatedUser()
		    .Build();

		options.AddPolicy(
			NotLoggedInRequirement.Name,
			policy =>
			{
				policy.AuthenticationSchemes = schemes;
				policy.Requirements.Add(new NotLoggedInRequirement());
			}
		);
	});

	services.AddTransient<IAuthorizationHandler, NotLoggedInHandler>();
}
