using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace StudentManagementSystem.Filters
{
    public class AuthorizeRoleAttribute : Attribute, IAuthorizationFilter
 {
 private readonly string[] _roles;

      public AuthorizeRoleAttribute(params string[] roles)
 {
   _roles = roles;
   }

  public void OnAuthorization(AuthorizationFilterContext context)
        {
    var userRole = context.HttpContext.Session.GetString("UserRole");
  var userId = context.HttpContext.Session.GetString("UserId");

    Console.WriteLine($"[AuthorizeRole] Path: {context.HttpContext.Request.Path}, UserRole: {userRole}, UserId: {userId}, RequiredRoles: {string.Join(",", _roles)}");

    if (string.IsNullOrEmpty(userRole) || string.IsNullOrEmpty(userId))
 {
       Console.WriteLine($"[AuthorizeRole] FORBIDDEN - No session found");
       // For API endpoints, return 401/403 instead of redirect
       if (context.HttpContext.Request.Path.StartsWithSegments("/api"))
       {
           context.Result = new ForbidResult();
           return;
       }
       context.Result = new RedirectToActionResult("Login", "Account", null);
      return;
     }

     if (_roles.Length > 0 && !_roles.Contains(userRole))
            {
        Console.WriteLine($"[AuthorizeRole] FORBIDDEN - Role '{userRole}' not in required roles");
        // For API endpoints, return 403 instead of redirect
        if (context.HttpContext.Request.Path.StartsWithSegments("/api"))
        {
            context.Result = new ForbidResult();
            return;
        }
        context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
    }
        }
    }
}
