using Baitkm.Infrastructure.Helpers.Binders;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Baitkm.Application.Middlewares.Language
{
    public class LanguageMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IOptionsBinder _optionsBinder;

        public LanguageMiddleware(RequestDelegate next,
            IOptionsBinder optionsBinder)
        {
            _next = next;
            _optionsBinder = optionsBinder;
        }

        public Task Invoke(HttpContext httpContext)
        {
            httpContext.Request.Headers.TryGetValue("Language", out var lang);
            Enum.TryParse(lang.FirstOrDefault(), out Baitkm.Enums.Language language);

            var error = _optionsBinder.GetErrorMessages(language);
            _optionsBinder.Get(ref error);

            return _next(httpContext);
        }
    }
}