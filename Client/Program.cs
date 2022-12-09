using System;
using System.Net.Http;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MudBlazor.Services;
using Client;
using Client.Tools.Options;
using Microsoft.Extensions.Configuration;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));

ConfigureServices(builder.Services, builder.Configuration);

await builder.Build()
             .RunAsync();

static void ConfigureServices(IServiceCollection services, IConfiguration config)
{
	services.AddOptions()
		.Configure<ApiOptions>(o =>
		{
			o.BaseUrl = config.GetSection("Api:BaseUrl").Value;
		})
		.Configure<SiteOptions>(o =>
		{
			o.Name = config.GetSection("Site:Name").Value;
			o.Url = config.GetSection("Site:Url").Value;
		})
		.AddAuthorizationCore()
		.AddBlazoredLocalStorage()
		.AddMudServices()
		.AddScoped(sp =>
		{
			var apiOptions = sp.GetRequiredService<IOptions<ApiOptions>>().Value;
			var siteOptions = sp.GetRequiredService<IOptions<SiteOptions>>().Value;
			var uri = apiOptions.BaseUrl.Contains("http")
				? apiOptions.BaseUrl
				: $"{siteOptions.Url}{apiOptions.BaseUrl}";

			return new HttpClient { BaseAddress = new Uri(uri, UriKind.Absolute) };
		})
		.AutoinjectServicesFromAssembly(typeof(Program).Assembly);
}