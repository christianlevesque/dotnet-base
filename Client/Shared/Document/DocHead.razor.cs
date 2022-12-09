using Client.Tools.Options;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using MudBlazor;

namespace Client.Shared.Document;

public partial class DocHead
{
	private string? _imageAlt;

	[Parameter]
	public string? Description { get; set; }

	[Parameter]
	public string? Title { get; set; }

	[Parameter]
	public string? Subtitle { get; set; }

	[Parameter]
	public string? HeaderImage { get; set; }

	[Parameter]
	public string? ImageAlt
	{
		get => _imageAlt ?? $"{SiteOptions.Value.Name} header image";
		set => _imageAlt = value;
	}

	[Inject]
	private NavigationManager NavManager { get; set; } = default!;

	[Inject]
	private IOptions<SiteOptions> SiteOptions { get; set; } = default!;

	// TODO: Add support for multiple types from OpenGraph. These should be determined automatically based on data passed into <DocHead>
	private string GenerateOgType() => "website";
}