using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Com.WaitWha.ThreadFix.Utils
{
    public class StringSec
    {
        /// <summary>
        /// Converts the given string input to a SecureString
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static SecureString GetSecureString(string input)
        {
            SecureString s = new SecureString();
            foreach (char c in input)
                s.AppendChar(c);

            s.MakeReadOnly();
            return s;
        }

        /// <summary>
        /// Converts the given SecureString to a less secure string.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string GetUnsecureString(SecureString s)
        {
            string r = string.Empty;
            IntPtr ptr = Marshal.SecureStringToBSTR(s);
            try
            {
                r = Marshal.PtrToStringBSTR(ptr);
            }
            finally
            {
                Marshal.ZeroFreeBSTR(ptr);
            }

            return r;
        }

        /// <summary>
        /// Encrypts the given SecureString for more permenant storage.
        /// </summary>
        /// <param name="input">SecureString to protect</param>
        /// <param name="salt">Salt</param>
        /// <returns>string</returns>
        public static string EncryptString(SecureString input, byte[] salt)
        {
            byte[] d =
                ProtectedData.Protect(
                    Encoding.Unicode.GetBytes(GetUnsecureString(input)),
                    salt,
                    DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(d);
        }

        /// <summary>
        /// Decrypts a given string using the given salt.
        /// </summary>
        /// <param name="input">string to decrypt</param>
        /// <returns>SecureString</returns>
        public static SecureString DecryptString(string input, byte[] salt)
        {
            byte[] d = ProtectedData.Unprotect(Convert.FromBase64String(input), salt, DataProtectionScope.CurrentUser);
            return GetSecureString(Encoding.Unicode.GetString(d));
        }
    }
}
