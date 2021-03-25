using Baitkm.BLL.Services.Users;
using Baitkm.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Baitkm.Authorizations.Admins
{
    public class AdminPermission : IAuthorizationFilter
    {
        private readonly IUserService userService;

        public AdminPermission(IUserService userService)
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
                context.Result = new JsonResult("Admin not found") { StatusCode = 401 };
                return;
            }
            if (user.RoleEnum != Role.Admin)
            {
                context.Result = new JsonResult("Permission denied") { StatusCode = 403 };
                return;
            }
        }
    }
}
