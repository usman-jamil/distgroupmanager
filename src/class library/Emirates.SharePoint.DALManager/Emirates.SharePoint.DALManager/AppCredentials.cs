using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Web.Script.Serialization;
using System.Security.Principal;
using Microsoft.SharePoint;
using System.Configuration;
using System.Security.Cryptography;
using System.IO;

namespace Emirates.SharePoint.DALManager
{
    public sealed class AppCredentials
    {
        static readonly AppCredentials instance = new AppCredentials();
        private readonly string key = "EK";
        private readonly string usernameKey = "admanagerusername";
        private readonly string passwordKey = "admanagerpassword";
        private readonly string thresholdkey = "admanagerthreshold";

        private string userName = "";
        private string password = "";
        private string threshold = "";

        /* ======================== */

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static AppCredentials()
        {
            Console.WriteLine("Static Constructor");
        }

        AppCredentials()
        {
            ReadCredentials();
        }

        public static AppCredentials Instance
        {
            get
            {
                return instance;
            }
        }

        public string UserName
        {
            get
            {
                return userName;
            }
        }

        public string Password
        {
            get
            {
                return password;
            }
        }

        public string Threshold
        {
            get
            {
                return threshold;
            }
        }

        private void ReadCredentials()
        {
            string username = ConfigurationManager.AppSettings[usernameKey];
            string securePassword = ConfigurationManager.AppSettings[passwordKey];
            string thresholdlimit = ConfigurationManager.AppSettings[thresholdkey];

            string unsecurePassword = Decrypt(securePassword, key);
            userName = username;
            password = unsecurePassword;
            threshold = thresholdlimit;
        }

        public string Decrypt(string cipherText, string Password)
        {
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            PasswordDeriveBytes pdb = new PasswordDeriveBytes(Password,
            new byte[] {0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65,
                        0x64, 0x76, 0x65, 0x64, 0x65, 0x76});
            byte[] decryptedData = Decrypts(cipherBytes, pdb.GetBytes(32), pdb.GetBytes(16));
            return System.Text.Encoding.Unicode.GetString(decryptedData);
        }

        public static byte[] Decrypts(byte[] cipherData, byte[] Key, byte[] IV)
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
    }
}