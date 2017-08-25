using System;
using System.Collections.Generic;
using System.Net;

namespace Zyborg.Vault.Server
{
    public class VaultServerException : Exception
    {
        public VaultServerException(HttpStatusCode statusCode, params string[] errors)
        {
            StatusCode = statusCode;
            Errors = errors;
        }

        public HttpStatusCode StatusCode
        { get; set; }

        public IEnumerable<string> Errors
        { get; set; }

    }
}