using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zyborg.Vault.MockServer.Util;
using Zyborg.Vault.MockServer.Storage;

namespace Zyborg.Vault.MockServer.Auth
{
    public abstract class LocallyRoutedAuthBackend<T> : IAuthBackend
    {
        protected LocalRouter<T> _router;
        protected readonly T _this;
        protected LocallyRoutedAuthBackend(bool initOnConstruct = true)
        {
            // We have to widen, then narrow
            _this = (T)(object)this;

            if (initOnConstruct)
                Init();
        }

        protected void Init()
        {
            _router = new LocalRouter<T>();
            _router.Init();
        }

        public async Task<IEnumerable<string>> ListAsync(string path)
        {
            path = PathMap<object>.NormalizePath(path);

            var (match, result) = await _router.ListAsync(_this, path);
            if (match == null)
                throw new NotSupportedException("unsupported path");
            
            return result;
        }

        public async Task<object> ReadAsync(string path)
        {
            path = PathMap<object>.NormalizePath(path);

            var (match, result) = await _router.ReadAsync(_this, path);
            if (match == null)
                throw new NotSupportedException("unsupported path");

            return result;
        }

        public async Task<object> WriteAsync(string path, string payload)
        {
            path = PathMap<object>.NormalizePath(path);

            var (match, result) = await _router.WriteAsync(_this, path, payload);
            if (match == null)
                throw new NotSupportedException("unsupported path");
            
            return result;
        }

        public async Task DeleteAsync(string path)
        {
            path = PathMap<object>.NormalizePath(path);

            var (match, result) = await _router.DeleteAsync(_this, path);
            if (match == null)
                throw new NotSupportedException("unsupported path");
        }    }
}