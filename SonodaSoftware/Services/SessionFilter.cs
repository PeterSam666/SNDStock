using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SonodaSoftware.Data;

namespace SonodaSoftware.Services
{
    public class SessionFilter : ActionFilterAttribute
    {
        public string[] AllowAction;
        public string UserRole = "";

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var actionDes = context.ActionDescriptor as ControllerActionDescriptor;

            if (!AllowAction?.Contains(actionDes?.ActionName) ?? true)
            {
                var user = context.HttpContext.Session.Get<User_UserBase>("UserLogin");
                if (user is null)
                {
                    context.Result = new RedirectToActionResult("Login", "Home", new { err = "Session_timeout" });
                }
                else
                {
                    if (!string.IsNullOrEmpty(UserRole))
                    {
                        //if (UserRole != user.Role)
                        //{
                        //    context.Result = new UnauthorizedResult();
                        //}
                    }
                }
            }

            base.OnActionExecuting(context);
        }
    }
}