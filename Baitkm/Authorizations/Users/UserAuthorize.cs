using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Baitkm.Authorizations.Users
{
    public class UserAuthorize : TypeFilterAttribute
    {
        public UserAuthorize() : base(typeof(UserPermission))
        {
        }
    }
}