using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Zyborg.Vault.Server.Controllers
{
    public abstract class MountControllerBase : Controller
    {

        protected virtual Task<IActionResult> DecodeException(Exception ex)
        {
            var msgs = new string[0];
            if (!string.IsNullOrEmpty(ex.Message))
                msgs = new[] { ex.Message };

            switch (ex)
            {
                case NotSupportedException nse:
                    throw new VaultServerException(HttpStatusCode.NotFound, msgs);
                case ArgumentException ae:
                    throw new VaultServerException(HttpStatusCode.BadRequest, msgs);
                default:
                    throw new VaultServerException(HttpStatusCode.InternalServerError, msgs);
            }
        }
    }
}