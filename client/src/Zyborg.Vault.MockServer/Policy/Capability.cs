using System.Collections.Generic;

namespace Zyborg.Vault.MockServer.Policy
{
    public static class Capability
    {
        public const string List = "list";
        public const string Read = "read";
        public const string Create = "create";
        public const string Update = "update";
        public const string Delete = "delete";
        public const string Sudo = "sudo";
        public const string Deny = "deny";

        public static readonly IEnumerable<string> All = new[]
        {
            List, Read, Create, Update, Delete, Sudo, Deny
        };

        public static readonly IEnumerable<string> AllAllow = new[]
        {
            List, Read, Create, Update, Delete, Sudo
        };

        public static readonly IEnumerable<string> AllDeny = new[]
        {
            Deny
        };
    }}