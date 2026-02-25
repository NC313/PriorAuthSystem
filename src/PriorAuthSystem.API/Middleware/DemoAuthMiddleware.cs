using PriorAuthSystem.API.Services;

namespace PriorAuthSystem.API.Middleware;

public class DemoAuthMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, DemoUserService demoUserService)
    {
        var role = context.Request.Headers["X-Demo-Role"].FirstOrDefault() ?? "Provider";

        if (role is not ("Admin" or "Reviewer" or "Provider"))
            role = "Provider";

        var demoUser = demoUserService.GetUserByRole(role);

        context.Items["Role"] = demoUser.Role;
        context.Items["DemoUser"] = demoUser;

        await next(context);
    }
}
