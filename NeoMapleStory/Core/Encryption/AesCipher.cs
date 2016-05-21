using System;
using System.Security.Cryptography;

namespace NeoMapleStory.Core.Encryption
{
    internal sealed class AesCipher
    {
        private readonly ICryptoTransform m_mCrypto;

        public AesCipher(byte[] aesKey)
        {
            if (aesKey == null)
                throw new ArgumentNullException("key");

            if (aesKey.Length != 32)
                throw new ArgumentOutOfRangeException("Key length needs to be 32", "key");

            var aes = new RijndaelManaged
            {
                Key = aesKey,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };

            using (aes)
            {
                m_mCrypto = aes.CreateEncryptor();
            }
        }

        internal void Transform(byte[] data, byte[] iv)
        {
            var morphKey = new byte[16];
            var remaining = data.Length;
            var start = 0;
            var length = 0x5B0;

            while (remaining > 0)
            {
                for (var i = 0; i < 16; i++)
                    morphKey[i] = iv[i%4];

                if (remaining < length)
                    length = remaining;

                for (var index = start; index < start + length; index++)
                {
                    if ((index - start)%16 == 0)
                        m_mCrypto.TransformBlock(morphKey, 0, 16, morphKey, 0);

                    data[index] ^= morphKey[(index - start)%16];
                }

                start += length;
                remaining -= length;
                length = 0x5B4;
            }
        }
    }
}