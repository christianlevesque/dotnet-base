using System;
using System.Net;
using System.Threading.Tasks;
using Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Services.Errors;

namespace Server.Controllers;

public class AppControllerBase<TController, TService> : ControllerBase
{
	protected readonly ILogger<TController> Logger;
	protected readonly TService Service;

	/// <inheritdoc />
	public AppControllerBase(ILogger<TController> logger, TService service)
	{
		Logger = logger;
		Service = service;
	}

	protected async Task<IActionResult> ProcessServiceCall<T>(
		Func<Task<ServiceResult<T>>> action,
	    HttpStatusCode successStatusCode = HttpStatusCode.OK)
	{
		try
		{
			var result = await action();
			if (!result.WasSuccessful)
			{
				return GenerateErrorResponse(result);
			}

			return result.Result == null
				       ? StatusCode((int)successStatusCode)
				       : StatusCode((int)successStatusCode, result.Result);
		}
		catch (Exception e)
		{
			Logger.LogError(e, ErrorMessages.Generic.UnhandledByServiceLayer);
			return GenerateErrorResponse(ErrorMessages.Generic.Unknown);
		}
	}

	protected async Task<IActionResult> ProcessServiceCallReturningFile(
		Func<Task<ServiceResult<byte[]>>> action,
	    string filename,
	    string mimeType)
	{
		try
		{
			var result = await action();
			if (!result.WasSuccessful)
			{
				return GenerateErrorResponse(result);
			}

			var file = result.Result!;
			Response.Headers.Add("Content-Disposition", $"attachment; filename=${filename}");
			return new FileContentResult(file, mimeType);
		}
		catch (Exception e)
		{
			Logger.LogError(e, ErrorMessages.Generic.UnhandledByServiceLayer);
			return GenerateErrorResponse(ErrorMessages.Generic.Unknown);
		}
	}

	protected IActionResult GenerateErrorResponse<T>(ServiceResult<T> result)
	{
		HttpStatusCode status;
		var errorMessage = result.ErrorMessage;

		switch (result.ServiceError)
		{
			case ServiceError.None:
				throw new InvalidOperationException($"Unable to generate an error response without a valid service error. Error provided: {nameof(ServiceError.None)}");
			case ServiceError.NotFound:
				status = HttpStatusCode.NotFound;
				errorMessage ??= ErrorMessages.Generic.NotFound;
				break;
			case ServiceError.Unauthorized:
				status = HttpStatusCode.Unauthorized;
				errorMessage ??= ErrorMessages.Generic.NotLoggedIn;
				break;
			case ServiceError.Forbidden:
				status = HttpStatusCode.Forbidden;
				errorMessage ??= ErrorMessages.Generic.NoPermission;
				break;
			case ServiceError.Unprocessable:
				status = HttpStatusCode.UnprocessableEntity;
				errorMessage ??= ErrorMessages.Generic.Unprocessable;
				break;
			case ServiceError.DataConflict:
				status = HttpStatusCode.Conflict;
				errorMessage ??= ErrorMessages.Generic.DataConflict;
				break;
			case ServiceError.DatabaseConcurrency:
				status = HttpStatusCode.Conflict;
				errorMessage ??= ErrorMessages.Generic.DataConcurrencyConflict;
				break;
			case ServiceError.Unknown:
			default:
				status = HttpStatusCode.InternalServerError;
				errorMessage ??= ErrorMessages.Generic.Unknown;
				break;
		}

		return GenerateErrorResponse(errorMessage, status);
	}

	protected IActionResult GenerateErrorResponse(string errorMessage, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
	{
		var dto = new ErrorDto($"{errorMessage} If you continue to have problems, please contact the IT team.");

		return StatusCode((int)statusCode, dto);
	}
}