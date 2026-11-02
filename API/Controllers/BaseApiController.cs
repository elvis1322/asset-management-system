using Application.Models;
using ErrorOr;
using Microsoft.AspNetCore.Mvc;
namespace API.Controllers;
[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
 protected ActionResult HandleResult<T>(ErrorOr<T> result, string? customMessage = null, int? customStatusCode = null)
    {
        return result.Match<ActionResult>(
            value => 
            {
                if (customStatusCode.HasValue)
                {
                    if (customStatusCode.Value == StatusCodes.Status204NoContent)
                    {
                        return NoContent();
                    }
                    var resp = BaseResponse<T>.Success(value, customMessage ?? "Operacioni u krye me sukses.", customStatusCode.Value);
                    return StatusCode(resp.StatusCode, resp);
                }
                var response = BaseResponse<T>.SuccessByMethod(HttpContext.Request.Method, value, customMessage);
                if (response.StatusCode == StatusCodes.Status204NoContent)
                {
                    return NoContent();
                }
                return StatusCode(response.StatusCode, response);
            },
            errors => 
            {
                var errorResponse = BaseResponse<T>.FromErrors(errors);
                return StatusCode(errorResponse.StatusCode, errorResponse);
            }
        );
    }
}