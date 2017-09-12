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
            switch (ex)
            {
                case NotSupportedException nse:
                    throw new VaultServerException(HttpStatusCode.NotFound, nse.Message);
                case ArgumentException ae:
                    throw new VaultServerException(HttpStatusCode.BadRequest, ae.Message);
                default:
                    throw new VaultServerException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}