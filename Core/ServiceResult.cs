namespace Core;

public class ServiceResult<T>
{
	/// <summary>
	/// Creates a successful <see cref="ServiceResult{T}"/>
	/// </summary>
	/// <returns>a <see cref="ServiceResult<T>"/> that indicates a service call was successful</returns>
	public static ServiceResult<T> Ok() => new () { ServiceError = ServiceError.None };

	/// <summary>
	/// Creates a successful <see cref="ServiceResult{T}"/> that includes a result payload
	/// </summary>
	/// <param name="result">The result payload to return from the service call</param>
	/// <returns>a <see cref="ServiceResult{T}"/> that indicates a service callw as successful, along with the result payload</returns>
	public static ServiceResult<T> Ok(T result) => new()
	{
		ServiceError = ServiceError.None,
		Result = result
	};

	/// <summary>
	/// Creates a failed <see cref="ServiceResult{T}"/> with generic error information
	/// </summary>
	/// <returns>a <see cref="ServiceResult{T}"/> that indicates a service call was unsuccessful</returns>
	public static ServiceResult<T> Failure() => new()
	{
		ServiceError = ServiceError.Unknown,
		ErrorMessage = "An unknown error has occurred."
	};

	/// <summary>
	/// Creates a failed <see cref="ServiceResult{T}"/> with the specified error message
	/// </summary>
	/// <param name="errorMessage">A message with more specific information about why a service call failed</param>
	/// <returns></returns>
	public static ServiceResult<T> Failure(string errorMessage) => new()
	{
		ServiceError = ServiceError.Unknown,
		ErrorMessage = errorMessage
	};

	/// <summary>
	/// Creates a failed <see cref="ServiceResult{T}"/> with the specified error information
	/// </summary>
	/// <param name="errorType">The <see cref="ServiceError"/> representing the result of the service</param>
	/// <param name="errorMessage">A message with more specific information about why a service call failed</param>
	/// <returns></returns>
	public static ServiceResult<T> Failure(ServiceError errorType, string? errorMessage) => new()
	{
		ServiceError = errorType,
		ErrorMessage = errorMessage
	};

	/// <summary>
	/// Creates a failed <see cref="ServiceResult{T}"/> indicating the specified resource couldn't be found
	/// </summary>
	/// <param name="errorMessage">A message with more specific information about why a service call failed</param>
	/// <returns></returns>
	public static ServiceResult<T> NotFound(string? errorMessage = null) => Failure(ServiceError.NotFound, errorMessage);

	/// <summary>
	/// Creates a failed <see cref="ServiceResult{T}"/> indicating the current user is not authenticated
	/// </summary>
	/// <param name="errorMessage">A message with more specific information about why a service call failed</param>
	/// <returns></returns>
	public static ServiceResult<T> Unauthorized(string? errorMessage = null) => Failure(ServiceError.Unauthorized, errorMessage);

	/// <summary>
	/// Creates a failed <see cref="ServiceResult{T}"/> indicating the current user is not authorized to perform an action
	/// </summary>
	/// <param name="errorMessage">A message with more specific information about why a service call failed</param>
	/// <returns></returns>
	public static ServiceResult<T> Forbidden(string? errorMessage = null) => Failure(ServiceError.Forbidden, errorMessage);

	/// <summary>
	/// Creates a failed <see cref="ServiceResult{T}"/> indicating the request could not be processed with the given data
	/// </summary>
	/// <param name="errorMessage">A message with more specific information about why a service call failed</param>
	/// <returns></returns>
	public static ServiceResult<T> Unprocessable(string? errorMessage = null) => Failure(ServiceError.Unprocessable, errorMessage);

	/// <summary>
	/// Creates a failed <see cref="ServiceResult{T}"/> indicating there was conflicting data that prevented the request from processing
	/// </summary>
	/// <param name="errorMessage">A message with more specific information about why a service call failed</param>
	/// <returns></returns>
	public static ServiceResult<T> Conflict(string? errorMessage = null) => Failure(ServiceError.DataConflict, errorMessage);

	/// <summary>
	/// Creates a failed <see cref="ServiceResult{T}"/> indicating the resource being updated was modified by another user
	/// </summary>
	/// <param name="errorMessage">A message with more specific information about why a service call failed</param>
	/// <returns></returns>
	public static ServiceResult<T> ConcurrencyConflict(string? errorMessage = null) => Failure(ServiceError.DatabaseConcurrency, errorMessage);

	public bool WasSuccessful => ServiceError == ServiceError.None;

	public ServiceError ServiceError { get; private init; }

	public T? Result { get; set; }

	public string? ErrorMessage { get; set; }
}