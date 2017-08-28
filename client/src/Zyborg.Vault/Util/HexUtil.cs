using System;

namespace Zyborg.Vault.Util
{
    public static class HexUtil
    {
        public static byte[] HexToByteArray(this string hex)
        {
            int len = hex.Length;
            byte[] bytes = new byte[len / 2];

            for (int i = 0; i < len; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);

            return bytes;
        }
    }
}