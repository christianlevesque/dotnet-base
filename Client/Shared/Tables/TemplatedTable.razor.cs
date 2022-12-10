using System;
using System.Threading.Tasks;
using Core;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Client.Shared.Tables;

[CascadingTypeParameter(nameof(TItem))]
public partial class TemplatedTable<TItem>
{
	[Parameter]
	public string TableTitle { get; set; } = default!;

	[Parameter]
	public bool HideSearch { get; set; }

	[Parameter]
	public bool FlexToolbarContent { get; set; }

	[Parameter]
	public int RowsPerPage { get; set; } = 5;

	[Parameter]
	public EventCallback<int> RowsPerPageChanged { get; set; }

	[Parameter]
	public int[] PageSizeOptions { get; set; } =
	{
		5,
		10,
		25
	};

	[Parameter]
	public Type RowTemplate { get; set; } = default!;

	[Parameter]
	public RenderFragment? ToolBarContent { get; set; }

	[Parameter]
	public RenderFragment? HeaderContent { get; set; }

	[Parameter]
	public Func<Filter?, Task<ServiceResult<PagedDto<TItem>>>> LoadData { get; set; } = default!;

	private MudTable<TItem> Table { get; set; } = default!;
	private bool _loading;
	private string _searchTerm = string.Empty;

	private string SearchTerm
	{
		get => _searchTerm;
		set
		{
			if (_searchTerm == value)
			{
				return;
			}

			_searchTerm = value;
			Table.ReloadServerData();
		}
	}


	private async Task<TableData<TItem>> LoadDataFunc(TableState state)
	{
		_loading = true;
		StateHasChanged();

		var filter = new Filter
		{
			SortDescending = state.SortDirection == SortDirection.Descending,
			Page = state.Page + 1, // MudBlazor is 0-indexed
			PageSize = RowsPerPage = state.PageSize
		};

		if (state.SortDirection != SortDirection.None && !string.IsNullOrWhiteSpace(state.SortLabel))
		{
			filter.SortName = state.SortLabel;
		}

		if (!string.IsNullOrWhiteSpace(SearchTerm))
		{
			filter.SearchTerm = SearchTerm;
		}

		var result = ((await LoadData(filter)).Result)!;
		_loading = false;

		return new TableData<TItem>
		{
			Items = result.Items,
			TotalItems = result.TotalCount
		};
	}
}