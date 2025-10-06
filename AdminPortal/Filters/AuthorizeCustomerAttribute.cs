using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AdminPortal.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeCustomerAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (context.ActionDescriptor.EndpointMetadata.Any(x => x is AllowAnonymousAttribute))
            return;

        var isAdmin = context.HttpContext.Session.GetString("IsAdmin");
        if (string.IsNullOrEmpty(isAdmin)) // Changed from !string.IsNullOrEmpty
            context.Result = new RedirectToActionResult("Login", "Login", null);
    }
}