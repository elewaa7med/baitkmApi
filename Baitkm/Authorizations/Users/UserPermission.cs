using Baitkm.BLL.Services.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Baitkm.Authorizations.Users
{
    public class UserPermission : IAuthorizationFilter
    {
        private readonly IUserService userService;

        public UserPermission(IUserService userService)
        {
            this.userService = userService;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (context.HttpContext.User.Claims.Count() == 0)
            {
                context.Result = new JsonResult("Invalid token") { StatusCode = 401 };
                return;
            }
            var user = userService.CheckExistUser(int.Parse(context.HttpContext.User.Claims.Single(u => u.Type == "userId").Value));
            if (user == null)
            {
                context.Result = new JsonResult("User not found") { StatusCode = 401 };
                return;
            }
        }
    }
}