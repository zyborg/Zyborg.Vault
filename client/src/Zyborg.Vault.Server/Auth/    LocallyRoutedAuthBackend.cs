using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zyborg.Vault.Server.Protocol;
using Zyborg.Vault.Server.Storage;

namespace Zyborg.Vault.Server.Auth
{
    public abstract class LocallyRoutedAuthBackend<T> : IAuthBackend
    {
        protected LocalRouter<LocallyRoutedAuthBackend<T>> _router;

        protected LocallyRoutedAuthBackend(bool initOnConstruct = true)
        {
            if (initOnConstruct)
                Init();
        }

        protected void Init()
        {
            _router = new LocalRouter<LocallyRoutedAuthBackend<T>>();
            _router.Init();
        }

        public async Task<IEnumerable<string>> ListAsync(string path)
        {
            path = PathMap<object>.NormalizePath(path);

            var (match, result) = await _router.ListAsync(this, path);
            if (match == null)
                throw new NotSupportedException("unsupported path");
            
            return result;
        }

        public async Task<string> ReadAsync(string path)
        {
            path = PathMap<object>.NormalizePath(path);

            var (match, result) = await _router.ReadAsync(this, path);
            if (match == null)
                throw new NotSupportedException("unsupported path");

            return result;
        }

        public async Task WriteAsync(string path, string payload)
        {
            path = PathMap<object>.NormalizePath(path);

            var (match, result) = await _router.WriteAsync(this, path, payload);
            if (match == null)
                throw new NotSupportedException("unsupported path");
        }

        public async Task DeleteAsync(string path)
        {
            path = PathMap<object>.NormalizePath(path);

            var (match, result) = await _router.DeleteAsync(this, path);
            if (match == null)
                throw new NotSupportedException("unsupported path");
        }    }
}