using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Security;

namespace Emirates.SharePoint.DALManager
{
    public class UserCredentials : IDisposable
    {
        private readonly SecureString _userName;
        public String UserName
        {
            get { return ConvertToUnsecuredString(_userName); }
        }

        public String DomainName;

        private readonly SecureString _password;
        public String Password
        {
            get { return ConvertToUnsecuredString(_password); }
        }
        public UserCredentials(SecureString username, SecureString password)
        {
            _userName = username.Copy();
            _password = password.Copy();
        }

        public UserCredentials(SecureString username, SecureString password, SecureString domain)
        {
            _userName = username.Copy();
            _password = password.Copy();
            DomainName = ConvertToUnsecuredString(domain);
        }

        private static string ConvertToUnsecuredString(SecureString securedString)
        {
            if (securedString == null) return String.Empty;
            IntPtr uString = IntPtr.Zero;
            try
            {
                uString = Marshal.SecureStringToGlobalAllocUnicode(securedString);
                return Marshal.PtrToStringUni(uString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(uString);
            }
        }

        private Boolean _isDisposed;
        public void Dispose()
        {
            if (_isDisposed) return;
            _userName.Dispose();
            _password.Dispose();
            _isDisposed = true;
        }
    }
}
