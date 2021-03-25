using Baitkm.BLL.Services.Users;
using Baitkm.Enums.UserRelated;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Baitkm.Application.Middlewares.AuthorizationHandler
{
    public class ApiAuthorize : AuthorizationHandler<AuthorizeRequirement>
    {
        private readonly IUserService _userService;
        public ApiAuthorize(IUserService userService)
        {
            _userService = userService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AuthorizeRequirement requirement)
        {
            if (!context.User.Claims.Any())
            {
                context.Fail();
            }
            else
            {
                if (context.User.FindFirst(ClaimTypes.NameIdentifier).Value == null)
                    context.Fail();
                else
                {
                    var identifier = context.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                    var verified = context.User.Claims.Where(c => c.Type == "verifiedBy").Select(c => c.Value).FirstOrDefault();
                    Enum.TryParse(verified, out VerifiedBy verifiedBy);
                    var user = await _userService.GetUser(identifier, verifiedBy);
                    if (user == null)
                        context.Fail();
                    else
                    {
                        if (user.IsBlocked)
                            context.Fail();
                        else
                            context.Succeed(requirement);
                    }
                }
            }
        }
    }
}
