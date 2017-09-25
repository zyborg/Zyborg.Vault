using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Zyborg.Vault.MockServer.Controllers
{
    public abstract class MountControllerBase : Controller
    {

        protected virtual Task<IActionResult> DecodeException(Exception ex)
        {
            throw SysController.DecodeServerException(ex);
        }
    }
}