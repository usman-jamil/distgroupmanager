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
using Microsoft.SharePoint.Administration;
using System.Runtime.InteropServices;
using System.Security;
using Microsoft.BusinessData.Infrastructure.SecureStore;
using Microsoft.Office.SecureStoreService.Server;

namespace Emirates.SharePoint.ADGroupHandler
{
    public enum CredentialType
    {
        Domain,
        Generic
    }

    public sealed class SecureStoreHelper
    {
        static readonly SecureStoreHelper instance = new SecureStoreHelper();

        /* ======================== */

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static SecureStoreHelper()
        {
        }

        SecureStoreHelper()
        {
        }

        public static SecureStoreHelper Instance
        {
            get
            {
                return instance;
            }
        }

        private SPSite GetCentralAdministrationSite()
        {
            var webApplication = SPAdministrationWebApplication.Local;
            if (webApplication == null)
            {
                throw new NullReferenceException("Unable to get the Central Administration Site.");
            }
            var caWebUrl = webApplication.GetResponseUri(SPUrlZone.Default);
            if (caWebUrl == null)
            {
                throw new NullReferenceException("Unable to get the Central Administration Site. Could get the URL of the Default Zone.");
            }
            return webApplication.Sites[caWebUrl.AbsoluteUri];
        }

        public UserCredentials GetCredentialsFromSecureStoreService(string applicationId, CredentialType credentialType)
        {
            ISecureStoreProvider provider = SecureStoreProviderFactory.Create();
            if (provider == null)
            {
                throw new InvalidOperationException("Unable to get an ISecureStoreProvider");
            }
            var providerContext = provider as ISecureStoreServiceContext;
            if (providerContext == null)
            {
                throw new InvalidOperationException("Failed to get the provider context as ISecureStoreServiceContext");
            }
            providerContext.Context = SPServiceContext.GetContext(GetCentralAdministrationSite());
            using (SecureStoreCredentialCollection credentials = provider.GetCredentials(applicationId))
            {
                var un = from c in credentials
                         where c.CredentialType == (credentialType == CredentialType.Domain ? SecureStoreCredentialType.WindowsUserName : SecureStoreCredentialType.UserName)
                         select c.Credential;

                var pd = from c in credentials
                         where c.CredentialType == (credentialType == CredentialType.Domain ? SecureStoreCredentialType.WindowsPassword : SecureStoreCredentialType.Password)
                         select c.Credential;

                var dm = from c in credentials
                         where c.CredentialType == SecureStoreCredentialType.Key
                         select c.Credential;

                SecureString userName = un.First(d => d.Length > 0);
                SecureString password = pd.First(d => d.Length > 0);
                SecureString domain = dm.First(d => d.Length > 0);
                var userCredientals = new UserCredentials(userName, password, domain);
                return userCredientals;
            }
        }
    }
}