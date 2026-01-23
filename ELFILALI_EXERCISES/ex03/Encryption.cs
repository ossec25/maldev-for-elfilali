using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ex03
{
    class Encryption
    {
        public static string Encrypt(byte[] data, string key)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] encrypted = new byte[data.Length];

            // XOR
            for (int i = 0; i < data.Length; i++)
            {
                encrypted[i] = (byte)(data[i] ^ keyBytes[i % keyBytes.Length]);
            }

            // Encodage Base64
            return Convert.ToBase64String(encrypted);
        }
    }

    class Decryption
    {
        public static byte[] Decrypt(string dataBase64, string key)
        {
            // On transforme le texte Base64 en octets chiffrés
            byte[] encrypted = Convert.FromBase64String(dataBase64);

            byte[] keyBytes = System.Text.Encoding.UTF8.GetBytes(key);
            byte[] decrypted = new byte[encrypted.Length];

            // XOR
            for (int i = 0; i < encrypted.Length; i++)
            {
                decrypted[i] = (byte)(encrypted[i] ^ keyBytes[i % keyBytes.Length]);
            }

            return decrypted;
        }
    }
}
