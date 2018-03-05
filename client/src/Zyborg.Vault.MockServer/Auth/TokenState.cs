using System;
using System.Collections.Generic;
using Zyborg.Vault.Model;

namespace Zyborg.Vault.MockServer.Auth
{
    /// <summary>
    /// Defines the durable representation of a Token.
    /// </summary>
    /// <remarks>
    /// All the elements and state that must be persisted for a token
    /// are represented by this class which is an internal implemenation
    /// detail managed by the <see cref="TokenManager"/>.
    /// </remarks>
    public class TokenState
    {
        public AuthInfo AuthInfo
        { get; set; }

        public DateTime CreateTime
        { get; set; }

        public DateTime? ExpiresTime
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
        /// Policies to associate with this token.
        /// </summary>
        public string[] Policies
        { get; set; }

        /// <summary>
        /// The token value that clients will use to authenticate with Vault.
        /// If not provided this defaults to a 36 character UUID.
        /// A root token is required to specify the ID of a token.
        /// </summary>
        public string Id
        { get; set; }

        /// <summary>
        /// Metadata to associate with the token. This shows up in the audit log.
        /// </summary>
        public Dictionary<string, string> Metadata
        { get; set; }

        /// <summary>
        /// When tokens are created, a token accessor is also created and returned.
        /// </summary>
        /// <remarks>
        /// This accessor is a value that acts as a reference to a token and can
        /// only be used to perform limited actions:
        /// <list>
        /// <item>Look up a token's properties (not including the actual token ID)</item>
        /// <item>Look up a token's capabilities on a path</item>
        /// <item>Revoke the token</item>
        /// </list>
        /// <see cref="https://www.vaultproject.io/docs/concepts/tokens.html#token-accessors"
        /// >More details.</see>
        /// </remarks>
        public string Accessor
        { get; set; }

        /// <summary>
        /// The number of times this token has been used.
        /// </summary>
        public int UseCount
        { get; set; }
    }
}