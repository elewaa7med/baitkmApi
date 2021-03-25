using Baitkm.BLL.Services.Users.Guests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Baitkm.Authorizations.Guests
{
    public class GuestPermission : IAuthorizationFilter
    {
        private readonly IGuestService guestService;

        public GuestPermission(IGuestService guestService)
        {
            this.guestService = guestService;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
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