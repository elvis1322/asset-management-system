using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Security.Filters;

public sealed class FluentValidationActionFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        var errors = new List<ValidationFailure>();
        var serviceProvider = context.HttpContext.RequestServices;

        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
                continue;
            var validatorType = typeof(IValidator<>)
                .MakeGenericType(argument.GetType());
            var validators = serviceProvider.GetServices(validatorType)
                .Cast<IValidator>();
            foreach (var validator in validators)
            {
                var validationContext =
                    new ValidationContext<object>(argument);
                var result = await validator.ValidateAsync(
                    validationContext,
                    context.HttpContext.RequestAborted);
                errors.AddRange(result.Errors);
            }
        }
        if (errors.Count > 0)
        {
            var groupedErrors = errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .Select(e => e.ErrorMessage)
                        .Distinct()
                        .ToArray());
            context.Result = new BadRequestObjectResult(new
            {
                isSuccess = false,
                message = "Validimi i të dhënave dështoi.",
                errors = groupedErrors,
                statusCode = StatusCodes.Status400BadRequest
            });
            return;
        }
        await next();
    }
}
