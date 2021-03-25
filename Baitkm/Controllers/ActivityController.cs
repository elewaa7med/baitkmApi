using Baitkm.BLL.Services;
using Baitkm.BLL.Services.Activities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Baitkm.Controllers
{
    public class ActivityController : BaseController
    {
        private readonly IActivityService activityService;

        public ActivityController(IActivityService activityService)
        {
            this.activityService = activityService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return await MakeActionCallAsync(async () => await activityService.GetByUserId(GetUserId(), GetDeviceId()));
        }
    }
}
