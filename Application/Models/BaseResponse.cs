using System.Text.Json.Serialization;
using ErrorOr;
namespace Application.Models;
public class BaseResponse
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
    public int StatusCode { get; set; }
}
public class BaseResponse<T> : BaseResponse
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public T? Data { get; set; }
  public static BaseResponse<T> SuccessByMethod(string httpMethod, T? data, string? customMessage = null)
    {
        return httpMethod.ToUpper() switch
        {
            "POST" => Created(data, customMessage ?? "Resursi u krijua me sukses."),
            "DELETE" => NoContent(customMessage ?? "Veprimi u krye me sukses."),
            _ => Ok(data, customMessage ?? "Operacioni u krye me sukses.")
        };
    }
    public static BaseResponse<T> Success(T? data, string message = "Operacioni u krye me sukses.", int statusCode = 200) => new()
    {
        IsSuccess = true,
        Message = message,
        Data = data,
        StatusCode = statusCode,
        Errors = new()
    };
    public static BaseResponse<T> Ok(T? data, string message = "Operacioni u krye me sukses.") 
        => Success(data, message, 200);
    public static BaseResponse<T> Created(T? data, string message = "Resursi u krijua me sukses.") 
        => Success(data, message, 201);
    public static BaseResponse<T> Accepted(T? data, string message = "Kërkesa u pranua për përpunim.") 
        => Success(data, message, 202);
    public static BaseResponse<T> NoContent(string message = "Veprimi u krye me sukses.") => new()
    {
        IsSuccess = true,
        Message = message,
        StatusCode = 204,
        Data = default,
        Errors = new()
    };
    public static BaseResponse<T> Failure(int statusCode, string message, List<string>? errors = null) => new()
    {
        IsSuccess = false,
        Message = message,
        StatusCode = statusCode,
        Data = default,
        Errors = errors ?? new() { message }
    };
    public static BaseResponse<T> BadRequest(string message = "Kërkesë e pavlefshme.", List<string>? errors = null) 
        => Failure(400, message, errors);
    public static BaseResponse<T> Unauthorized(string message = "Nuk keni autorizim.") 
        => Failure(401, message);
    public static BaseResponse<T> Forbidden(string message = "Qasja është e ndaluar.") 
        => Failure(403, message);
    public static BaseResponse<T> NotFound(string message = "Resursi nuk u gjet.") 
        => Failure(404, message);
    public static BaseResponse<T> Conflict(string message = "Ekziston një konflikt në të dhëna.") 
        => Failure(409, message);
    public static BaseResponse<T> UnprocessableEntity(string message = "Të dhënat nuk mund të përpunohen.", List<string>? errors = null) 
        => Failure(422, message, errors);
    public static BaseResponse<T> TooManyRequests(string message = "Keni tejkaluar numrin e lejuar të tentativave.", List<string>? errors = null) 
        => Failure(429, message, errors);
    public static BaseResponse<T> InternalServerError(string message = "Gabim i papritur në server.", List<string>? errors = null) 
        => Failure(500, message, errors);
    public static BaseResponse<T> FromErrors(List<Error> errors)
    {
        if (errors == null || errors.Count == 0)
        {
            return InternalServerError("Ndodhi një gabim i papritur.");
        }
        var firstError = errors.First();
        var errorMessages = errors.Select(e => e.Description).ToList();
        var primaryMessage = firstError.Description;
        if (firstError.NumericType == 429 || firstError.Code == "TooManyRequests")
        {
            return TooManyRequests(primaryMessage, errorMessages);
        }
        return firstError.Type switch
        {
            ErrorType.Validation => BadRequest(primaryMessage, errorMessages),
            ErrorType.Unauthorized => Unauthorized(primaryMessage),
            ErrorType.Forbidden => Forbidden(primaryMessage),
            ErrorType.NotFound => NotFound(primaryMessage),
            ErrorType.Conflict => Conflict(primaryMessage),
            ErrorType.Failure => UnprocessableEntity(primaryMessage, errorMessages),
            _ => InternalServerError(primaryMessage, errorMessages)
        };
    }
}