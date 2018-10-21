#region License
// Copyright (c) 2013 Pieter Alec Myburgh
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using System.Text;

//Security Provider
using System.Security.Cryptography;
using System.IO;

namespace PMCore.Security
{
    public static class EncryptionUtils
    {
        #region Public Methods

        /// <summary>
        /// Encrypt String
        /// </summary>
        /// <param name="clearText">Clear Text to be Encrypted</param>
        /// <param name="Password">Password to use during encryption</param>
        /// <param name="Salt">Salt to use during Encryption</param>
        /// <returns></returns>
        public static string Encrypt(string clearText, string Password, string Salt)
        {
            byte[] salt = Encoding.Unicode.GetBytes(Salt);
            byte[] clearBytes = System.Text.Encoding.Unicode.GetBytes(clearText);
            PasswordDeriveBytes pdb = new PasswordDeriveBytes(Password, salt); //e.g. salt =  new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 }
            byte[] encryptedData = Encrypt(clearBytes, pdb.GetBytes(32), pdb.GetBytes(16));
            return Convert.ToBase64String(encryptedData);
        }

        /// <summary>
        /// Decrypt String
        /// </summary>
        /// <param name="cipherText">Encrypted Text to be decrypted</param>
        /// <param name="Password">Password to use during decryption</param>
        /// <param name="Salt">Salt to use during Encryption</param>
        /// <returns></returns>
        public static string Decrypt(string cipherText, string Password, string Salt)
        {
            byte[] salt = Encoding.Unicode.GetBytes(Salt);
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            PasswordDeriveBytes pdb = new PasswordDeriveBytes(Password, salt);
            byte[] decryptedData = Decrypt(cipherBytes, pdb.GetBytes(32), pdb.GetBytes(16));
            return System.Text.Encoding.Unicode.GetString(decryptedData);
        }

        /// <summary>
        /// Create a Random Salt
        /// </summary>
        /// <param name="Password">Intended Password</param>
        /// <returns>Random Salt</returns>
        public static string CreateRandomSalt(string Password)
        {
            int length = Password.Length * 2;

            // Create a buffer 
            byte[] randBytes;

            if (length >= 1)
            {
                randBytes = new byte[length];
            }
            else
            {
                randBytes = new byte[1];
            }

            // Create a new RNGCryptoServiceProvider.
            RNGCryptoServiceProvider rand = new RNGCryptoServiceProvider();

            // Fill the buffer with random bytes.
            rand.GetBytes(randBytes);

            // return the bytes. 
            return Encoding.UTF8.GetString(randBytes);
        }

        /// <summary>
        /// Create a Random Salt
        /// </summary>
        /// <param name="size">Size of intended Salt</param>
        /// <returns>Random Salt</returns>
        public static string CreateRandomSalt(int size)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;

            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(38 * random.NextDouble() + 65)));
                builder.Append(ch);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Create a Random Salt
        /// </summary>
        /// <param name="size">Size of intended Salt</param>
        /// <param name="strong">Make use of Special Characters</param>
        /// <returns>Random Salt</returns>
        public static string CreateRandomSalt(int size, bool strong)
        {
            Random random = new Random();
            int seed = random.Next(1, int.MaxValue);
            const string allowedChars = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ0123456789";
            const string specialCharacters = @"!#$%&'()*+,-./:;<=>?@[\]_";

            var chars = new char[size];
            var rd = new Random(seed);

            for (var i = 0; i < size; i++)
            {
                // If we are to use special characters
                if (strong && i % random.Next(3, size) == 0)
                {
                    chars[i] = specialCharacters[rd.Next(0, specialCharacters.Length)];
                }
                else
                {
                    chars[i] = allowedChars[rd.Next(0, allowedChars.Length)];
                }
            }

            return new string(chars);
        }

        #endregion

        #region Private Methods

        private static byte[] Encrypt(byte[] clearText, byte[] Key, byte[] IV)
        {
            MemoryStream ms = new MemoryStream();
            Rijndael alg = Rijndael.Create();
            alg.Key = Key;
            alg.IV = IV;
            CryptoStream cs = new CryptoStream(ms, alg.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(clearText, 0, clearText.Length);
            cs.Close();
            byte[] encryptedData = ms.ToArray();
            return encryptedData;
        }

        private static byte[] Decrypt(byte[] cipherData, byte[] Key, byte[] IV)
        {
            MemoryStream ms = new MemoryStream();
            Rijndael alg = Rijndael.Create();
            alg.Key = Key;
            alg.IV = IV;
            CryptoStream cs = new CryptoStream(ms, alg.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(cipherData, 0, cipherData.Length);
            cs.Close();
            byte[] decryptedData = ms.ToArray();
            return decryptedData;
        }

        private static void ClearBytes(byte[] buffer)
        {
            // Check arguments. 
            if (buffer == null)
            {
                return;
            }

            // Set each byte in the buffer to 0. 
            for (int x = 0; x < buffer.Length; x++)
            {
                buffer[x] = 0;
            }
        }

        #endregion
    }
}