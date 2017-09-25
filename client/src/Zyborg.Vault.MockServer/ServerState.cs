using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Zyborg.Vault.MockServer
{
    public class ServerState
    {
        public string UnsealNonce
        { get; set; }

        public List<string> UnsealProgress
        { get; set; }

        public IEnumerable<byte[]> UnsealKeys
        { get; set; }

        public byte[] RootKey
        { get; set; }

        public string RootTokenId
        { get; set; }

        public DurableServerState Durable
        { get; set; }
    }

    public class DurableServerState
    {
        public string ClusterId
        { get; set; }

        public string ClusterName
        { get; set; }

        public int SecretShares
        { get; set; }

        public int SecretThreshold
        { get; set; }

        public int? RootKeyTerm
        { get; set; }

        public DateTime? RootKeyInstallTime
        { get; set; }

        public byte[] RootKeyEncrypted
        { get; set; }

        public byte[] RootKeyHash
        { get; set; }

        public byte[] RootTokenHash
        { get; set; }

        // TODO: Remove this???
        public string RootTokenId
        { get; set; }
    }
}