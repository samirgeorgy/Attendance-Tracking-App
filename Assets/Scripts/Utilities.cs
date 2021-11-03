using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

/// <summary>
/// Contains tools to be used across the program
/// </summary>
public class Utilities
{
    #region Private Variables

    private static string key = "b14ca5898a4e4133bbce2ea2315a1916";

    #endregion

    #region Supporting Functions

    /// <summary>
    /// Encrypts a text
    /// </summary>
    /// <param name="data">The data to be encrypted</param>
    /// <returns>The encrypted data</returns>
    public static string EncryptString(string data)
    {
        byte[] iv = new byte[16];
        byte[] array;

        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.IV = iv;

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                    {
                        streamWriter.Write(data);
                    }

                    array = memoryStream.ToArray();
                }
            }
        }

        return Convert.ToBase64String(array);
    }

    /// <summary>
    /// Decrypts data
    /// </summary>
    /// <param name="data">The data to be decrypted</param>
    /// <returns>The decrypted data</returns>
    public static string DecryptString(string data)
    {
        byte[] iv = new byte[16];
        byte[] buffer = Convert.FromBase64String(data);

        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.IV = iv;
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using (MemoryStream memoryStream = new MemoryStream(buffer))
            {
                using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                    {
                        return streamReader.ReadToEnd();
                    }
                }
            }
        }
    }

    #endregion
}

