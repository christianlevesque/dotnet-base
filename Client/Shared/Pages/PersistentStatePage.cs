using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace Client.Shared.Pages;

public abstract class PersistentStatePage<TPage> : ComponentBase, IDisposable
	where TPage : ComponentBase
{
	private readonly string _storageKey;
	private Dictionary<string, object> _storage;
	private readonly JsonSerializerOptions _serializerOptions;

	protected PersistentStatePage()
	{
		_storageKey = typeof(TPage).FullName!;
		_storage = new Dictionary<string, object>();
		_serializerOptions = new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true
		};
	}

	private PersistingComponentStateSubscription _stateSubscription;

	[Inject]
	protected PersistentComponentState State { get; set; } = default!;

	// State deserialization takes place in OnInitialized() for two reasons:
	// 1. State deserialization is inherently synchronous, so no need for a Task
	// 2. OnInitialized() is always called before OnInitializedAsync(),
	// so regardless of which method you use, state will be deserialized
	// by the time you need it.
	// Don't forget that if you override OnInitialized(), you need to call
	// base.OnInitialized() in your component...but that's probably not going
	// to happen anyway.
	protected override void OnInitialized()
	{
		_stateSubscription = State.RegisterOnPersisting(Persist);
		if (State.TryTakeFromJson<Dictionary<string, object>>(_storageKey, out var storage)
		    && storage is not null)
		{
			_storage = storage;
		}
	}

	private Task Persist()
	{
		SaveState();
		State.PersistAsJson(_storageKey, _storage);
		return Task.CompletedTask;
	}

	/// <summary>
	/// Registers state to be persisted between Server and Client during SSR
	/// </summary>
	protected abstract void SaveState();

	/// <summary>
	/// Stores a piece of data in the local state for the current component
	/// </summary>
	/// <param name="key">The unique key of the state to store. Consider using the name of the field or property in the component to ensure uniqueness</param>
	/// <param name="data">The data to store</param>
	protected void Store(string key, object data)
	{
		_storage[key] = data;
	}

	/// <summary>
	/// Retrieves a piece of state from 
	/// </summary>
	/// <param name="key">The unique key of the state to store. Consider using the name of the field or property in the component to ensure uniqueness</param>
	/// <param name="hydrate">A function call to hydrate the data if it is not present in the state</param>
	/// <typeparam name="T">The type of the data to retrieve</typeparam>
	/// <returns>The specified piece of data deserialized to an instance of <c>T</c></returns>
	protected async Task<T> Retrieve<T>(string key, Func<Task<T>> hydrate)
	{
		if (!_storage.ContainsKey(key))
		{
			Store(key, (await hydrate())!);
		}

		if (_storage[key] is JsonElement e)
		{
			var text = e.GetRawText();
			_storage[key] = JsonSerializer.Deserialize<T>(text, _serializerOptions)!;
		}

		return (T)_storage[key];
	}

	public void Dispose()
	{
		_stateSubscription.Dispose();
		GC.SuppressFinalize(this);
	}
}