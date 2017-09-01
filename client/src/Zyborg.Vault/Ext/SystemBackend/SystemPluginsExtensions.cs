using System;
using System.Threading.Tasks;
using Zyborg.Vault.Model;

namespace Zyborg.Vault.Ext.SystemBackend
{
    public static class SystemPluginsExtensions
    {
        /// <summary>
        /// Lists the plugins in the catalog.
        /// </summary>
        public static async Task<ReadResponse<KeysData>> ListPluginsAsync(
                this VaultClient client,
                SystemBackendOptions options = null)
        {
            return await client.ListAsync<ReadResponse<KeysData>>("sys/plugins/catalog",
                    on404: resp => null,
                    options: options);
        }

        /// <summary>
        /// Lists the plugins in the catalog.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="name">Specifies the name of the plugin to retrieve.</param>
        /// <param name="options"></param>
        /// <remarks>
        /// <para>
        /// <b><i>sudo required – This operation requires sudo capability in addition to any
        ///     path-specific capabilities.</i></b>
        /// </para>
        /// </remarks>
        public static async Task<ReadResponse<PluginInfo>> ReadPluginAsync(
                this VaultClient client,
                string name,
                SystemBackendOptions options = null)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            name = name.TrimStart('/');

            return await client.ReadAsync<ReadResponse<PluginInfo>>($"sys/plugins/catalog/{name}",
                    options: options);
        }

        /// <summary>
        /// Registers a new plugin, or updates an existing one with the supplied name.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="name">Specifies the name for this plugin. The name is what is used to
        ///     look up plugins in the catalog.</param>
        /// <param name="sha256">This is the SHA256 sum of the plugin's binary. Before a plugin
        ///     is run it's SHA will be checked against this value, if they do not match the
        ///     plugin can not be run.</param>
        /// <param name="command">Specifies the command used to execute the plugin. This is
        ///     relative to the plugin directory. Example:  <c>>myplugin --my_flag=1<c>></param>
        /// <param name="args"></param>
        /// <param name="options"></param>
        /// <remarks>
        /// <para>
        /// <b><i>sudo required – This operation requires sudo capability in addition to any
        ///     path-specific capabilities.</i></b>
        /// </para>
        /// </remarks>
        public static async Task RegisterPluginAsync(
                this VaultClient client,
                string name,
                string sha256,
                string command,
                string[] args = null,
                SystemBackendOptions options = null)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrEmpty(sha256))
                throw new ArgumentNullException(nameof(sha256));
            if (string.IsNullOrEmpty(command))
                throw new ArgumentNullException(nameof(command));
            
            name = name.TrimStart('/');

            await client.WriteAsync($"sys/plugins/catalog/{name}",
                    new RegisterPluginRequest
                    {
                        Sha256 = sha256,
                        Command = command,
                        Args = args,
                    },
                    options: options);

        }

        /// <summary>
        /// Removes the plugin with the given name.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="name">Specifies the name of the plugin to delete.</param>
        /// <param name="options"></param>
        /// <remarks>
        /// <para>
        /// <b><i>sudo required – This operation requires sudo capability in addition to any
        ///     path-specific capabilities.</i></b>
        /// </para>
        /// </remarks>
        public static async Task RemovePluginAsync(
                this VaultClient client,
                string name,
                SystemBackendOptions options = null)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            name = name.TrimStart('/');

            await client.DeleteAsync($"sys/plugins/catalog/{name}",
                    options: options);
       }    }
}