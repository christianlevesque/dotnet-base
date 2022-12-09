using Microsoft.AspNetCore.Components;

namespace Client.Shared.Document;

public partial class DocTitle
{
	private const string DefaultSubtitle = "#sitename#"; // TODO: Set up site identity
	private string? _subtitle;

	[Parameter]
	public string? Title { get; set; }

	[Parameter]
	public string Subtitle
	{
		get => _subtitle ?? DefaultSubtitle;
		set => _subtitle = value;
	}

	private string ComputedTitle =>
		string.IsNullOrEmpty(Title)
			? Subtitle
			: $"{Title} | {Subtitle}";
}