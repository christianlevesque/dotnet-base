using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Core;
using Client.Services.Infrastructure;

namespace Client.Services;

public class BaseHttpService<TService>
{
	public readonly string Endpoint;
	protected readonly HttpClient Client;
	protected readonly ILogger<TService> Logger;
	protected readonly ISnackbarService Snackbar;
	protected readonly JsonSerializerOptions JsonOptions;

	protected const string GenericErrorMessage = "An unknown error has occurred. If this persists, please contact IT.";
	protected const string NetworkErrorMessage = "A network error occurred. Are you connected to the internet?";

	public BaseHttpService(string endpoint, HttpClient client, ILogger<TService> logger, ISnackbarService snackbar)
	{
		Endpoint = endpoint;
		Client = client;
		Logger = logger;
		Snackbar = snackbar;
		JsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
	}

	protected void HandleException(Exception e)
	{
		switch (e)
		{
			case HttpRequestException:
				Logger.LogError(e, "Network error");
				Snackbar.Error(NetworkErrorMessage);
				break;
			case TaskCanceledException:
				Logger.LogError(e, "Network request timed out");
				Snackbar.Error("Network request timed out");
				break;
			default:
				Logger.LogError(e, GenericErrorMessage);
				Snackbar.Error(GenericErrorMessage);
				break;
		}
	}

	protected async Task<string> HandleFailureResponse(HttpResponseMessage message)
	{
		// Try to interpret the response as an ErrorDto
		// which optimistically assumes that the response
		// was technically successful
		try
		{
			var error = await GetResponse<ErrorDto>(message);
			if (!string.IsNullOrEmpty(error?.Message))
			{
				Snackbar.Error(error.Message);
				return error.Message;
			}
		}
		catch (Exception e)
		{
			Logger.LogError(e, "Unable to deserialize to ErrorDto");
		}

		string errorMessage;
		// Otherwise, use the status code
		// to generate an error message
		switch (message.StatusCode)
		{
			case HttpStatusCode.BadRequest:
				Logger.LogError("The request payload was not understood: {}", await message.Content.ReadAsStringAsync());
				errorMessage = "The request was malformed. Please try again. If you continue to have problems, contact IT.";
				break;
			case HttpStatusCode.UnprocessableEntity:
				Logger.LogError("There was a problem with the request data: {}", await message.Content.ReadAsStringAsync());
				errorMessage = "There was a problem with the data you entered. Please check for errors and try again";
				break;
			default:
				Logger.LogError("An unknown problem occurred: {}", await message.Content.ReadAsStringAsync());
				errorMessage = "An unknown problem occurred. If you continue to have problems, contact IT.";
				break;
		}

		Snackbar.Error(errorMessage);

		return errorMessage;
	}

	protected async Task<ServiceResult<bool>> SendRequest(string endpoint, string successMessage, object? input = null, HttpMethod? method = null)
	{
		method ??= HttpMethod.Get;
		var message = CreateRequestMessage(method, endpoint, input);
		string errorMessage;

		try
		{
			var result = (await Send(message))!;
			if (result.IsSuccessStatusCode)
			{
				Snackbar.Success(successMessage);
				return ServiceResult<bool>.Ok(true);
			}

			errorMessage = await HandleFailureResponse(result);
		}
		catch (Exception e)
		{
			HandleException(e);
			errorMessage = "HTTP request failed";
		}

		return ServiceResult<bool>.Failure(errorMessage);
	}

	protected async Task<ServiceResult<TReturn>> SendRequest<TReturn>(string endpoint, string? successMessage = null, object? input = null, HttpMethod? method = null)
	{
		method ??= HttpMethod.Get;
		var message = CreateRequestMessage(method, endpoint, input);
		string errorMessage;

		try
		{
			var result = (await Send(message))!;
			if (result.IsSuccessStatusCode)
			{
				var parsedResponse = await GetResponse<TReturn>(result);
				if (parsedResponse is not null)
				{
					if (!string.IsNullOrEmpty(successMessage))
					{
						Snackbar.Success(successMessage);
					}
					return ServiceResult<TReturn>.Ok(parsedResponse);
				}

				errorMessage = "The request was successful, but the server's response was not understood.";
			}
			else
			{
				errorMessage = await HandleFailureResponse(result);
			}
		}
		catch (Exception e)
		{
			HandleException(e);
			errorMessage = "An unknown error occurred.";
		}

		Snackbar.Error(errorMessage);
		return ServiceResult<TReturn>.Failure(errorMessage);
	}

	protected async Task<TReturn?> SendRequest<TReturn, TInput>(string endpoint, TInput input, HttpMethod? method = null)
	{
		method ??= HttpMethod.Get;
		var message = CreateRequestMessage(method, endpoint, input);

		try
		{
			var result = (await Send(message))!;
			if (result.IsSuccessStatusCode)
			{
				var parsedResponse = await GetResponse<TReturn>(result);
				if (parsedResponse is not null)
				{
					return parsedResponse;
				}

				Snackbar.Error("The request was successful, but the server's response was not understood.");
				return default;
			}

			await HandleFailureResponse(result);
		}
		catch (Exception e)
		{
			HandleException(e);
		}

		return default;
	}

	protected Task<HttpResponseMessage> SendWithRawResponse(string endpoint, object? input = null, HttpMethod? method = null)
	{
		method ??= HttpMethod.Get;
		var message = CreateRequestMessage(method, endpoint, input);
		return Send(message);
	}

	protected Task<TReturn?> GetResponse<TReturn>(HttpResponseMessage response) => response.Content.ReadFromJsonAsync<TReturn>(JsonOptions);

	protected Task<HttpResponseMessage> Send(HttpRequestMessage payload)
	{
		Logger.LogInformation("Sending {method} request to {base}/{endpoint}", payload.Method, Client.BaseAddress, payload.RequestUri);
		return Client.SendAsync(payload);
	}

	protected HttpRequestMessage CreateRequestMessage(HttpMethod method, string endpoint, object? input = null)
	{
		var message = new HttpRequestMessage(method, endpoint);
		if (input is null)
		{
			return message;
		}

		if (method == HttpMethod.Get)
		{
			CreateQueryPayload(message, input);
		}
		else
		{
			CreateContentPayload(message, input);
		}

		return message;
	}

	protected void CreateContentPayload(HttpRequestMessage m, object input)
	{
		m.Content = new StringContent(JsonSerializer.Serialize(input, JsonOptions),
		                              Encoding.UTF8,
		                              "application/json");
	}

	protected void CreateQueryPayload(HttpRequestMessage m, object input)
	{
		var sb = new StringBuilder(m.RequestUri!.OriginalString);
		sb.Append('?');

		var inputType = input.GetType();
		var defaultInstance = Activator.CreateInstance(inputType);

		foreach (var prop in inputType.GetProperties())
		{
			var instanceValue = prop.GetValue(input)?.ToString();
			var defaultValue = prop.GetValue(defaultInstance)?.ToString();
			if (instanceValue != defaultValue)
			{
				sb.Append($"{prop.Name}={instanceValue}&");
			}
		}

		m.RequestUri = new Uri(sb.ToString(), UriKind.Relative);
	}
}