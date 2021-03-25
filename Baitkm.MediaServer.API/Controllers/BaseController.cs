using Baitkm.Enums.Attachments;
using Baitkm.Infrastructure.Helpers.ResponseModels;
using Baitkm.MediaServer.BLL.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Baitkm.MediaServer.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class BaseController : Controller
    {
        protected string GetModelStateErrors()
        {
            return string.Join("; ", ModelState.Values
                .SelectMany(x => x.Errors)
                .Select(x => x.ErrorMessage));
        }

        protected void CreateErrorResult(ServiceResult serviceResult, Exception ex)
        {
            serviceResult.Success = false;
            serviceResult.Messages.AddMessage(MessageType.Error, ex.Message);
            serviceResult.Messages.AddMessage(MessageType.Error, ex.InnerException?.Message);
        }

        protected void CreateSuccessResult(ServiceResult serviceResult, object data, string message)
        {
            serviceResult.Success = true;
            serviceResult.Data = data;
            serviceResult.Messages.AddMessage(MessageType.Info, message);
        }

        protected async Task<IActionResult> MakeActionCallAsync<TResult>(Func<Task<TResult>> action)
        {
            var serviceResult = new ServiceResult();
            try
            {
                var result = await action();
                CreateSuccessResult(serviceResult, result, "OK");
            }
            catch (Exception e)
            {
                CreateErrorResult(serviceResult, e);
            }
            return Json(serviceResult);
        }

        protected IActionResult MakeActionCall<TResult>(Func<TResult> action)
        {
            var serviceResult = new ServiceResult();
            try
            {
                var result = action();
                CreateSuccessResult(serviceResult, result, "OK");
            }
            catch (Exception e)
            {
                CreateErrorResult(serviceResult, e);
            }
            return Json(serviceResult);
        }

        protected async Task<IActionResult> MakeActionCallWithModelAsync<TResult, TModel>(Func<Task<TResult>> action, TModel model) where TModel : class, IViewModel
        {
            var serviceResult = new ServiceResult();
            try
            {
                if (model == null)
                    throw new Exception($"No field in {typeof(TModel)} model was initialized");
                if (!ModelState.IsValid)
                    throw new Exception(GetModelStateErrors());
                var result = await action();
                CreateSuccessResult(serviceResult, result, "OK");
            }
            catch (Exception e)
            {
                CreateErrorResult(serviceResult, e);
            }
            return Json(serviceResult);
        }

        protected IActionResult MakeActionCallWithModel<TResult, TModel>(Func<TResult> action, TModel model) where TModel : class, IViewModel
        {
            var serviceResult = new ServiceResult();
            try
            {
                if (model == null)
                    throw new Exception($"No field in {typeof(TModel)} model was initialized");
                if (!ModelState.IsValid)
                    throw new Exception(GetModelStateErrors());
                var result = action();
                CreateSuccessResult(serviceResult, result, "OK");
            }
            catch (Exception e)
            {
                CreateErrorResult(serviceResult, e);
            }
            return Json(serviceResult);
        }
    }
}