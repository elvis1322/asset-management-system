using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using Domain.Entities.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace AssetManagementSystem.Infrastructure.Interceptors;

/// <summary>
/// Shton LastModifiedAt dhe LastModifiedBy në çdo ExecuteUpdate mbi një IAuditableEntity.
/// </summary>
public sealed class AuditExecuteUpdateInterceptor : IQueryExpressionInterceptor
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuditExecuteUpdateInterceptor> _logger;

    public AuditExecuteUpdateInterceptor(
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuditExecuteUpdateInterceptor> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public Expression QueryCompilationStarting(Expression query, QueryExpressionEventData eventData)
    {
        if (query is not MethodCallExpression call
            || (call.Method.Name != "ExecuteUpdate" && call.Method.Name != "ExecuteUpdateAsync"))
        {
            return query;
        }
        var genericArguments = call.Method.GetGenericArguments();
        if (genericArguments.Length == 0)
        {
            return query;
        }
        var entityType = genericArguments[0];
        if (!typeof(IAuditableEntity).IsAssignableFrom(entityType))
        {
            return query;
        }
        if (call.Arguments.Count < 2)
        {
            return query;
        }
        if (StripQuotes(call.Arguments[1]) is not NewArrayExpression setterArray)
        {
            _logger.LogError(
                "AUDIT: forma e setters-ave nuk njihet ({Type}). LastModifiedAt/LastModifiedBy NUK u shtuan "
                + "për {Entity} — ka gjasa që versioni i EF Core-it të ketë ndryshuar.",
                call.Arguments[1].GetType().Name,
                entityType.Name);
            return query;
        }
        var setters = setterArray.Expressions.ToList();
        var originalCount = setters.Count;
        var tupleConstructor = typeof(Tuple<Delegate, object>).GetConstructor([typeof(Delegate), typeof(object)]);
        if (tupleConstructor is null)
        {
            _logger.LogError("AUDIT: Nuk u gjet konstruktori i duhur për Tuple<Delegate, object>.");
            return query;
        }
        // 1. Shto LastModifiedAt VETËM nëse nuk është shtuar më parë
        AppendSetterIfMissing(setters, tupleConstructor, entityType,
            nameof(IAuditableEntity.LastModifiedAt),
            Expression.Constant((DateTime?)DateTime.UtcNow, typeof(DateTime?)));

        // 2. Shto LastModifiedBy VETËM nëse nuk është shtuar më parë
        var userIdStr = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrWhiteSpace(userIdStr))
        {
            AppendSetterIfMissing(setters, tupleConstructor, entityType,
                nameof(IAuditableEntity.LastModifiedBy),
                Expression.Constant(userIdStr, typeof(string)));
        }

        if (setters.Count == originalCount)
        {
            return query;
        }

        var arguments = call.Arguments.ToList();
        arguments[1] = Expression.NewArrayInit(typeof(ITuple), setters);

        _logger.LogInformation(
            "AUDIT (ExecuteUpdate): {Entity} — setters {Before} -> {After}.",
            entityType.Name, originalCount, setters.Count);

        return call.Update(call.Object, arguments);
    }

    private static void AppendSetterIfMissing(
        List<Expression> setters,
        ConstructorInfo tupleConstructor,
        Type entityType,
        string propertyName,
        Expression value)
    {
        var property = entityType.GetProperty(propertyName);
        if (property is null)
        {
            return;
        }

        // Kontrollojmë nëse kjo pronësi (property) ekziston tashmë në masivin e setters
        bool alreadyExists = setters.Any(s =>
        {
            if (s is NewExpression newExpr && newExpr.Arguments.Count > 0)
            {
                var lambda = StripQuotes(newExpr.Arguments[0]) as LambdaExpression;
                if (lambda?.Body is MemberExpression memberExpr)
                {
                    return memberExpr.Member.Name == propertyName;
                }
            }
            return false;
        });

        if (alreadyExists)
        {
            return; // Nëse ekziston tashmë, mos e shto dy herë!
        }

        var parameter = Expression.Parameter(entityType, "a");
        var selector = Expression.Lambda(Expression.Property(parameter, property), parameter);

        setters.Add(Expression.New(
            tupleConstructor,
            selector,
            Expression.Convert(value, typeof(object))));
    }

    private static Expression StripQuotes(Expression expression) =>
        expression is UnaryExpression { NodeType: ExpressionType.Quote } unary
            ? unary.Operand
            : expression;
}