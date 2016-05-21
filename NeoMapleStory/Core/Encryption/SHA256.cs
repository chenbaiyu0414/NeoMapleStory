using System;
using System.Security.Cryptography;
using System.Text;

namespace NeoMapleStory.Core.Encryption
{
    public static class Sha256
    {
        public static string Get(string password, Guid guid) => Get(password, guid.ToString());

        public static string Get(string password, string salt)
        {
            using (var sha256 = new SHA256Managed())
            {
                var tmpByte = sha256.ComputeHash(Encoding.Default.GetBytes(password + salt));
                return BitConverter.ToString(tmpByte).Replace("-", "").ToUpper();
            }
        }
    }
}