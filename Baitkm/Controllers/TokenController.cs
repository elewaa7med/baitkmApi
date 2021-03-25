using Baitkm.BLL.Services.Token;
using Baitkm.DTO.ViewModels.Persons.Users.Verification;
using Baitkm.DTO.ViewModels.Social;
using Baitkm.DTO.ViewModels.Token;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace Baitkm.Controllers
{
    [AllowAnonymous]
    public class TokenController : BaseController
    {
        private readonly ITokenService _tokenService;
        public TokenController(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        [HttpPost]
        public async Task<IActionResult> SendKey([FromBody] SendKeyModel model)
        {
            return await MakeActionCallAsync(async () => await _tokenService.SendKeyAsync(model, GetLanguage()));
        }

        [HttpPost]
        public async Task<IActionResult> Verify([FromBody] TokenVerifyViewModel model)
        {
            return await MakeActionCallAsync(async () => await _tokenService.VerifyAsync(model,
                GetDeviceToken(), GetOsType(), GetLanguage(), GetDeviceId()));
        }

        [HttpPost]
        public async Task<IActionResult> Token([FromBody] TokenViewModel model)
        {
            return await MakeActionCallAsync(async () => await _tokenService.TokenAsync(model,
                GetDeviceToken(), GetOsType(), GetLanguage(), GetDeviceId()));
        }

        [HttpGet]
        public IActionResult TokenTest()
        {
            return MakeActionCall(() => _tokenService.TokenTest());
        }

        [HttpGet]
        public IActionResult ReturnString()
        {
            return MakeActionCall(() => _tokenService.ReturnString());
        }

        [HttpPost]
        public async Task<IActionResult> SocialLogin([FromBody] SocialLoginModel model)
        {
            return await MakeActionCallAsync(async () => await _tokenService.SocialLogin(model,
                GetDeviceToken(), GetLanguage(), GetOsType(), GetDeviceId(), UserCurrency));
        }

        [HttpPost]
        public async Task<IActionResult> AppleLogin([FromBody] AppleSignInRequestModel model)
        {
            var handler = new JwtSecurityTokenHandler();

            var jsonToken = handler.ReadToken(model.Token) as JwtSecurityToken;

            string email = jsonToken.Claims.Single(x => x.Type == "email").Value;
            string appleId = jsonToken.Claims.Single(x => x.Type == "sub").Value;
            return await MakeActionCallAsync(async () => await _tokenService.AppleLogin(model,
                email, appleId, GetDeviceToken(), GetLanguage(), GetDeviceId(), UserCurrency));
        }
    }
}