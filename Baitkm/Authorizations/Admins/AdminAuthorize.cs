using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Baitkm.Authorizations.Admins
{
    public class AdminAuthorize : TypeFilterAttribute
    {
        public AdminAuthorize() : base(typeof(AdminPermission))
        {
        }
    }
}