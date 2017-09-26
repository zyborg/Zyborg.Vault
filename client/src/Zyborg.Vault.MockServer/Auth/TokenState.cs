using System;
using Zyborg.Vault.Model;

namespace Zyborg.Vault.MockServer.Auth
{
    public class TokenState
    {
        public AuthInfo AuthInfo
        { get; set; }

        public DateTime CreateTime
        { get; set; }

        public string CreateSource
        { get; set; }

        /// <summary>
        /// This token will be created as a child of your token. The created token
        /// will inherit your policies, or can be assigned a subset of your policies.
        /// </summary>
        public string ParentId
        { get; set; }

        /// <summary>
        /// A display name to associate with this token. This
        /// is a non-security sensitive value used to help
        /// identify created secrets, i.e. prefixes.
        /// </summary>
        public string DisplayName
        { get; set; }

        /// <summary>
        /// If set, the token will be created against the named
        /// role. The role may override other parameters. This
        /// requires the client to have permissions on the
        /// appropriate endpoint (auth/token/create/<name>).
        /// </summary>
        public string Role
        { get; set; }

        /// <summary>
        /// A lease can also be associated with the token. If a lease is not associated
        /// with the token, then it cannot be renewed. If a lease is associated with
        /// the token, it will expire after that amount of time unless it is renewed.
        /// </summary>
        public Duration? Ttl
        { get; set; }

        /// <summary>
        /// An explicit maximum lifetime for the token. Unlike
        /// normal token TTLs, which can be renewed up until the
        /// maximum TTL set on the auth/token mount or the system
        /// configuration file, this lifetime is a hard limit set
        /// on the token itself and cannot be exceeded.

        /// </summary>
        public Duration? MaxTtl
        { get; set; }

        /// <summary>
        /// Whether or not the token is renewable to extend its
        /// TTL up to Vault's configured maximum TTL for tokens.
        /// This defaults to true; set to false to disable
        /// renewal of this token.
        /// </summary>
        public bool Renewable
        { get; set; } = true;

        /// <summary>
        /// If specified, the token will be periodic; it will
        /// have no maximum TTL (unless an "explicit-max-ttl" is
        /// also set) but every renewal will use the given
        /// period. Requires a root/sudo token to use.
        /// </summary>
        public Duration? RenewalPeriod
        { get; set; }

        /// <summary>
        /// The number of times this token can be used until it is automatically revoked.
        /// </summary>
        public int? UseLimit
        { get; set; }

        /// <summary>
        /// The number of times this token has been used.
        /// </summary>
        public int UseCount
        { get; set; }
    }
}