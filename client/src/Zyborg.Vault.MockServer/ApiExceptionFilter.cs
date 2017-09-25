using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Zyborg.Vault.MockServer
{
    /// <summary>
    /// Based on:
    ///     https://weblog.west-wind.com/posts/2016/oct/16/error-handling-and-exceptionfilter-dependency-injection-for-aspnet-core-apis
    /// </summary>
    public class ApiExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            if (context.Exception is VaultServerException ex)
            {
                context.Exception = null;
                context.HttpContext.Response.StatusCode = (int)ex.StatusCode;
                if (ex.Errors != null)
                {
                    if (!(ex.Errors is string[] errors))
                        errors = ex.Errors.ToArray();

                    context.Result = new JsonResult(new Model.ErrorResponse { Errors = errors });
                }
            }

            base.OnException(context);
        }
    }
}