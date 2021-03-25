using Baitkm.Authorizations.Admins;
using Baitkm.Authorizations.Users;
using Baitkm.BLL.Services.Users;
using Baitkm.DTO.ViewModels.ForgotPasswords;
using Baitkm.DTO.ViewModels.Helpers;
using Baitkm.DTO.ViewModels.Helpers.Paging;
using Baitkm.DTO.ViewModels.Persons.Users;
using Baitkm.DTO.ViewModels.Persons.Users.Verification;
using Baitkm.DTO.ViewModels.Subscription;
using Baitkm.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Baitkm.Controllers
{
    public class UserController : BaseController
    {
        private readonly IUserService _service;
        public UserController(IUserService service)
        {
            _service = service;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            return await MakeActionCallAsync(async () => await _service.RegisterAsync(model, GetDeviceToken(), GetOsType(),
                GetLanguage(), GetDeviceId(), UserCurrency));
        }

        [HttpPut]
        [Authorize(Roles = Roles.User)]
        [UserAuthorize]
        public async Task<IActionResult> Edit([FromBody] UserEditModel model)
        {
            return await MakeActionCallAsync(async () => await _service.EditAsync(model, GetPersonId(), GetLanguage()));
        }

        [HttpGet]
        [Authorize(Roles = Roles.User)]
        [UserAuthorize]
        public async Task<IActionResult> GetDetails()
        {
            return await MakeActionCallAsync(async () => await _service.UserDetailsAsync(GetPersonId(), GetLanguage()));
        }

        [HttpGet]
        [Authorize(Roles = Roles.User)]
        [UserAuthorize]
        public async Task<IActionResult> GetUserProfile()
        {
            return await MakeActionCallAsync(async () => await _service.UserProfileAsync(GetPersonId()));
        }

        [HttpGet]
        [Route("{userId}")]
        [AdminAuthorize]
        public async Task<IActionResult> GetUserById(int userId)
        {
            return await MakeActionCallAsync(async () => await _service.GetByIdAsync(userId));
        }

        [HttpPost]
        [AdminAuthorize]
        public async Task<IActionResult> GetAdminUserList([FromBody] PagingRequestModel model)
        {
            return await MakeActionCallAsync(async () => await _service.GetUserListAsync(model));
        }

        [HttpPut]
        [Authorize(Roles = Roles.User)]
        [UserAuthorize]
        public async Task<IActionResult> Photo([FromForm] UploadFileModel model)
        {
            return await MakeActionCallAsync(async () => await _service.Photo(model, GetPersonId()));
        }

        [HttpPut]
        [Authorize(Roles = Roles.User)]
        [UserAuthorize]
        public async Task<IActionResult> EditSubscription([FromBody] UpdateSubscriptionModel model)
        {
            return await MakeActionCallAsync(async () => await _service.EditSubscription(model, GetPersonId()));
        }

        [HttpGet]
        [Authorize(Roles = Roles.User)]
        [UserAuthorize]
        public async Task<IActionResult> GetSubscriptionList()
        {
            return await MakeActionCallAsync(async () => await _service.GetSubscription(GetPersonId()));
        }

        [HttpPost]
        [Authorize(Roles = Roles.User)]
        [UserAuthorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
        {
            return await MakeActionCallAsync(async () => await _service.ChangePasswordAsync(model, GetPersonId(), GetLanguage()));
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> SendKeyForgotPassword([FromBody] SendKeyForgotPasswordModel model)
        {
            return await MakeActionCallAsync(async () => await _service.ForgotPasswordCheckVerified(model, GetLanguage()));
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> CheckForgotKey([FromBody] CheckForgotKeyModel model)
        {
            return await MakeActionCallAsync(async () => await _service.ForgotPasswordChangeVerified(model, GetLanguage()));
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordChangePasswordModel model)
        {
            return await MakeActionCallAsync(async () => await _service.ForgotPasswordChangePassword(model, GetLanguage()));
        }

        [HttpDelete]
        public async Task<IActionResult> Logout()
        {
            return await MakeActionCallAsync(async () => await _service.Logout(GetDeviceId(), GetOsType()));
        }

        [HttpPost]
        [Route("{userId}")]
        [AdminAuthorize]
        public async Task<IActionResult> Block(int userId, int day)
        {
            return await MakeActionCallAsync(async () => await _service.Block(userId, day));
        }

        [HttpPost]
        [Route("{userId}")]
        [AdminAuthorize]
        public async Task<IActionResult> UnBlock(int userId)
        {
            return await MakeActionCallAsync(async () => await _service.UnBlock(userId));
        }

        //[HttpDelete]
        //[Route("{userId}")]
        //[Authorize(Roles = Roles.Admin)]
        //public async Task<IActionResult> Delete(int userId)
        //{
        //    return await MakeActionCallAsync(async () => await _service.Delete(userId, GetPerson(), GetLanguage()));
        //}

        [HttpPut]
        [Route("{userId}")]
        [AdminAuthorize]
        public async Task<IActionResult> ResetPassword([FromBody] UserResetPasswordModel model, int userId)
        {
            return await MakeActionCallAsync(async () => await _service.ResetPassword(model, userId, GetLanguage(), GetPersonId()));
        }

        [HttpPost]
        [AdminAuthorize]
        public async Task<IActionResult> UserFilter([FromBody] UserFilterModel model)
        {
            return await MakeActionCallAsync(async () => await _service.UserFilter(model, GetLanguage(), GetPersonId()));
        }

        [HttpPost]
        [Authorize(Roles = Roles.User)]
        [UserAuthorize]
        public async Task<IActionResult> NotificationList([FromBody] PagingRequestModel model)
        {
            return await MakeActionCallAsync(async () => await _service.NotificationList(model, GetPersonId(), GetLanguage()));
        }

        [HttpGet]
        public async Task<IActionResult> Currency()
        {
            return await MakeActionCallAsync(async () => await _service.GetUserCurrency(GetPersonId()));
        }
    }
}