using Baitkm.BLL.Services.Users;
using Baitkm.BLL.Services.Users.Guests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Baitkm.Authorizations.UserGuests
{
    public class UserGuestPermission : IAuthorizationFilter
    {
        private readonly IUserService userService;
        private readonly IGuestService guestService;

        public UserGuestPermission(IUserService userService, IGuestService guestService)
        {
            this.userService = userService;
            this.guestService = guestService;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (context.HttpContext.User.Claims.Count() != 0)
            {
                var user = userService.CheckExistUser(int.Parse(context.HttpContext.User.Claims.Single(u => u.Type == "userId").Value));
                if (user == null)
                {
                    context.Result = new JsonResult("User not found") { StatusCode = 401 };
                    return;
                }
            }
            else
            {
                context.HttpContext.Request.Headers.TryGetValue("DeviceId", out var deviceId);
                var guest = guestService.CheckExistGuest(deviceId.ToString());
                if (!guest)
                {
                    context.Result = new JsonResult("Guest not found") { StatusCode = 401 };
                    return;
                }
            }
        }
    }
}