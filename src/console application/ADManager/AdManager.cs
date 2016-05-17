using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Configuration;

namespace ADManager
{
    public class ADMethodsAccountManagement : IADManager
    {

        #region Variables

        private string sServiceUser = null;
        private string sServicePassword = null;
        private ContextOptions contextOptionsValidate = ContextOptions.SimpleBind;
        private ContextOptions contextOptionsPrincipalContext = ContextOptions.SimpleBind;

        #endregion
        #region Validate Methods

        public ADMethodsAccountManagement(string username, string password)
        {
            sServiceUser = username;
            sServicePassword = password;

            string appContextOptionsValidate = ConfigurationManager.AppSettings["ContextOptionsValidate"];
            string appContextOptionsPrincipalContext = ConfigurationManager.AppSettings["ContextOptionsContext"];
            contextOptionsValidate = (ContextOptions)Enum.Parse(typeof(ContextOptions), appContextOptionsValidate, true);
            contextOptionsPrincipalContext = (ContextOptions)Enum.Parse(typeof(ContextOptions), appContextOptionsPrincipalContext, true);
        }

        /// <summary>
        /// Validates the username and password of a given user
        /// </summary>
        /// <param name="sUserName">The username to validate</param>
        /// <param name="sPassword">The password of the username to validate</param>
        /// <returns>Returns True of user is valid</returns>
        public bool ValidateCredentials()
        {
            bool result = false;

            try
            {
                using (PrincipalContext oPrincipalContext = new PrincipalContext(ContextType.Domain,
                    Environment.UserDomainName,
                    sServiceUser,
                    sServicePassword))
                {
                    if (oPrincipalContext != null)
                    {
                        result = oPrincipalContext.ValidateCredentials(sServiceUser, sServicePassword, contextOptionsValidate);
                    }
                }
            }
            catch (Exception ex)
            {
                List<string> logLines = new List<string>();
                DateTime time = DateTime.Now;
                string header = string.Format("###########################################################");
                logLines.Add(string.Format("{0,-20}{1}", time.ToLongTimeString(), header));
                string message = string.Format("Exception while validating credentials");
                logLines.Add(string.Format("{0,-20}{1}", time.ToLongTimeString(), message));
                logLines.Add(string.Format("{0,-20}{1}", time.ToLongTimeString(), ex.Message));
                logLines.Add(string.Format("{0,-20}{1}", time.ToLongTimeString(), ex.StackTrace));
                if (ex.InnerException != null)
                {
                    string innerMessage = string.Format("Inner Exception...");
                    logLines.Add(string.Format("{0,-20}{1}", time.ToLongTimeString(), innerMessage));
                    logLines.Add(string.Format("{0,-20}{1}", time.ToLongTimeString(), ex.InnerException.Message));
                    logLines.Add(string.Format("{0,-20}{1}", time.ToLongTimeString(), ex.InnerException.StackTrace));
                }

                System.IO.File.AppendAllLines("LogFile.txt", logLines);
            }

            return result;
        }

        #endregion

        #region Group Methods

        /// <summary>
        /// Adds the user for a given group
        /// </summary>
        /// <param name="sUserName">The user you want to add to a group</param>
        /// <param name="sGroupName">The group you want the user to be added in</param>
        /// <returns>Returns true if successful</returns>
        public bool AddUserToGroup(string sUserName, string sGroupName)
        {
            bool success = false;

            try
            {
                using (PrincipalContext oPrincipalContext = new PrincipalContext(ContextType.Domain,
                    Environment.UserDomainName,
                    sServiceUser,
                    sServicePassword))
                {
                    UserPrincipal oUserPrincipal = UserPrincipal.FindByIdentity(oPrincipalContext, sUserName);
                    GroupPrincipal oGroupPrincipal = GroupPrincipal.FindByIdentity(oPrincipalContext, sGroupName);
                    if (oUserPrincipal != null && oGroupPrincipal != null)
                    {
                        bool isMember = oGroupPrincipal.Members.Contains(oUserPrincipal);
                        if (!isMember)
                        {
                            oGroupPrincipal.Members.Add(oUserPrincipal);
                            oGroupPrincipal.Save();
                            DirectoryEntry entry = (DirectoryEntry)oGroupPrincipal.GetUnderlyingObject();
                            entry.RefreshCache(new string[] { "member" });
                            success = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                List<string> logLines = new List<string>();
                DateTime time = DateTime.Now;
                string header = string.Format("###########################################################");
                logLines.Add(string.Format("{0,-20}{1}", time.ToLongTimeString(), header));
                string message = string.Format("Exception while trying to add User: {0} to Group: {1}",
                    sUserName, sGroupName);
                logLines.Add(string.Format("{0,-20}{1}", time.ToLongTimeString(), message));
                logLines.Add(string.Format("{0,-20}{1}", time.ToLongTimeString(), ex.Message));
                logLines.Add(string.Format("{0,-20}{1}", time.ToLongTimeString(), ex.StackTrace));
                if(ex.InnerException != null)
                {
                    string innerMessage = string.Format("Inner Exception...");
                    logLines.Add(string.Format("{0,-20}{1}", time.ToLongTimeString(), innerMessage));
                    logLines.Add(string.Format("{0,-20}{1}", time.ToLongTimeString(), ex.InnerException.Message));
                    logLines.Add(string.Format("{0,-20}{1}", time.ToLongTimeString(), ex.InnerException.StackTrace));
                }

                System.IO.File.AppendAllLines("LogFile.txt", logLines);
            }

            return success;
        }

        /// <summary>
        /// Removes user from a given group
        /// </summary>
        /// <param name="sUserName">The user you want to remove from a group</param>
        /// <param name="sGroupName">The group you want the user to be removed from</param>
        /// <returns>Returns true if successful</returns>
        public bool RemoveUserFromGroup(string sUserName, string sGroupName)
        {
            bool success = false;

            try
            {
                using (PrincipalContext oPrincipalContext = new PrincipalContext(ContextType.Domain,
                    Environment.UserDomainName,
                    sServiceUser,
                    sServicePassword))
                {
                    UserPrincipal oUserPrincipal = UserPrincipal.FindByIdentity(oPrincipalContext, sUserName);
                    GroupPrincipal oGroupPrincipal = GroupPrincipal.FindByIdentity(oPrincipalContext, sGroupName);
                    if (oUserPrincipal != null && oGroupPrincipal != null)
                    {
                        bool isMember = oGroupPrincipal.Members.Contains(oUserPrincipal);
                        if (isMember)
                        {
                            oGroupPrincipal.Members.Remove(oUserPrincipal);
                            oGroupPrincipal.Save();
                            DirectoryEntry entry = (DirectoryEntry)oGroupPrincipal.GetUnderlyingObject();
                            entry.RefreshCache(new string[] { "member" });
                            success = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                List<string> logLines = new List<string>();
                DateTime time = DateTime.Now;
                string header = string.Format("###########################################################");
                logLines.Add(string.Format("{0,-20}{1}", time.ToLongTimeString(), header));
                string message = string.Format("Exception while trying to remove User: {0} from Group: {1}",
                    sUserName, sGroupName);
                logLines.Add(string.Format("{0,-20}{1}", time.ToLongTimeString(), message));
                logLines.Add(string.Format("{0,-20}{1}", time.ToLongTimeString(), ex.Message));
                logLines.Add(string.Format("{0,-20}{1}", time.ToLongTimeString(), ex.StackTrace));
                if (ex.InnerException != null)
                {
                    string innerMessage = string.Format("Inner Exception...");
                    logLines.Add(string.Format("{0,-20}{1}", time.ToLongTimeString(), innerMessage));
                    logLines.Add(string.Format("{0,-20}{1}", time.ToLongTimeString(), ex.InnerException.Message));
                    logLines.Add(string.Format("{0,-20}{1}", time.ToLongTimeString(), ex.InnerException.StackTrace));
                }

                System.IO.File.AppendAllLines("LogFile.txt", logLines);
            }

            return success;
        }

        public List<string> GroupMembers(string sGroupName)
        {
            List<string> users = new List<string>();

            using (PrincipalContext oPrincipalContext = new PrincipalContext(ContextType.Domain,
                Environment.UserDomainName,
                sServiceUser,
                sServicePassword))
            {
                GroupPrincipal oGroupPrincipal = GroupPrincipal.FindByIdentity(oPrincipalContext, sGroupName);

                if (oGroupPrincipal != null)
                {
                    foreach (Principal principal in oGroupPrincipal.Members)
                    {
                        users.Add(principal.Name);
                    }
                }
            }

            return users;
        }

        public string GetGroupInfo(string sGroupName)
        {
            string info = string.Empty;

            DirectoryEntry RootDirEntry = new DirectoryEntry("LDAP://RootDSE");
            Object distinguishedName = RootDirEntry.Properties["defaultNamingContext"].Value;
            string strDN = distinguishedName.ToString();

            using (PrincipalContext oPrincipalContext = new PrincipalContext(ContextType.Domain,
                Environment.UserDomainName, strDN))
            {
                GroupPrincipal oGroupPrincipal = GroupPrincipal.FindByIdentity(oPrincipalContext, sGroupName);
                if (oGroupPrincipal != null)
                {
                    info = oGroupPrincipal.DistinguishedName;
                }
            }

            return info;
        }

        #endregion

    }
}
