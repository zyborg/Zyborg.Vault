using System;
using System.Net;
using System.Runtime.Serialization;
using Zyborg.Vault.Model;

namespace Zyborg.Vault
{
    public class VaultClientException : Exception
    {
        public VaultClientException()
        { }

        public VaultClientException(string message) : base(message)
        { }

        public VaultClientException(string message, Exception innerException) : base(message, innerException)
        { }

        protected VaultClientException(SerializationInfo info, StreamingContext context) : base(info, context)
        { }

        public HttpStatusCode StatusCode
        { get; set; }

        public string ReasonPhrase
        { get; set; }

        public ErrorResponse Errors
        { get; set; }
    }
}