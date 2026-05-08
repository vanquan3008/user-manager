using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace UserService.Api.Filters;

public sealed class AuthorizeRolesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var authorizeAttr = context.MethodInfo.DeclaringType?
            .GetCustomAttributes(true)
            .OfType<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>()
            .FirstOrDefault() ?? context.MethodInfo.GetCustomAttributes(true)
            .OfType<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>()
            .FirstOrDefault();

        if (authorizeAttr == null) return;

        var roles = authorizeAttr.Roles?.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(r => r.Trim()).ToArray();
        var policy = authorizeAttr.Policy;
        var desc = policy != null ? $"Policy: {policy} (role cần permission tương ứng trong DB)" : (roles != null && roles.Length > 0 ? string.Join(" hoặc ", roles) : null);
        if (desc == null) return;

        operation.Responses.TryAdd("401", new OpenApiResponse
        {
            Description = "Unauthorized - Chưa đăng nhập hoặc token hết hạn"
        });
        operation.Responses.TryAdd("403", new OpenApiResponse
        {
            Description = $"Forbidden - {desc}"
        });

        if (operation.Security == null)
            operation.Security = new List<OpenApiSecurityRequirement>();

        operation.Security.Add(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                roles ?? Array.Empty<string>()
            }
        });
    }
}
