using MCBA.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MCBA.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeCustomerAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if(context.ActionDescriptor.EndpointMetadata.Any(x => x is AllowAnonymousAttribute))
            return;

        var customerId = context.HttpContext.Session.GetInt32(nameof(Customer.CustomerId));
        if(!customerId.HasValue)
            context.Result = new RedirectToActionResult("Index", "/", null);
    }
}
