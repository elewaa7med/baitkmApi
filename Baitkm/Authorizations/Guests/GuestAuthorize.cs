using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Baitkm.Authorizations.Guests
{
    public class GuestAuthorize : TypeFilterAttribute
    {
        public GuestAuthorize() : base(typeof(GuestPermission))
        {
        }
    }
}