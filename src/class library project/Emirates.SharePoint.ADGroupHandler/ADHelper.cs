using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Web.Script.Serialization;
using System.Security.Principal;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System.Text;

namespace Emirates.SharePoint.ADGroupHandler
{
    public sealed class ADHelper
    {
        static readonly ADHelper instance = new ADHelper();

        /* ======================== */

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static ADHelper()
        {
        }

        ADHelper()
        {
        }

        public static ADHelper Instance
        {
            get
            {
                return instance;
            }
        }

        private string SerializeToJson(object data)
        {
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            return javaScriptSerializer.Serialize(data);
        }

        public string GetGroupsAsString()
        {
            string groupString = string.Empty;
            List<string> userGroups = GetGroupsFromLDAP();

            groupString = SerializeToJson(userGroups.ToArray());

            return groupString;
        }

        public string GetUsersAsString(string groupName)
        {
            string userString = string.Empty;

            if (!string.IsNullOrEmpty(groupName))
            {
                List<string> userGroups = GetMembers(groupName);
                
                userString = SerializeToJson(userGroups.ToArray());
            }

            return userString;
        }

        private List<string> GetGroupsFromLDAP()
        {
            List<string> result = new List<string>();

            try
            {
                using (System.Web.Hosting.HostingEnvironment.Impersonate())
                {
                    string userName = HttpContext.Current.User.Identity.Name;
                    string dlManagerUserName = AppCredentials.Instance.UserName;
                    string dlManagerPassword = AppCredentials.Instance.Password;

                    using (PrincipalContext oPrincipalContext = new PrincipalContext(ContextType.Domain,
                            Environment.UserDomainName,
                            dlManagerUserName,
                            dlManagerPassword))
                    {
                        // find your user
                        UserPrincipal user = UserPrincipal.FindByIdentity(oPrincipalContext, userName);

                        // if found - grab its groups
                        if (user != null)
                        {
                            DirectoryEntry domainConnection = new DirectoryEntry();
                            DirectorySearcher samSearcher = new DirectorySearcher();
                            samSearcher.SearchRoot = domainConnection;
                            samSearcher.Filter = "(&(objectClass=group)(managedBy=" + user.DistinguishedName + "))";//"(samAccountName=" + uP.SamAccountName + ")";

                            SearchResultCollection results = samSearcher.FindAll();

                            foreach (SearchResult samResult in results)
                            {
                                result.Add(Convert.ToString(samResult.Properties["name"][0]));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingHelper.Instance.LogError(LogOptions.GetGroups, ex);
            }

            return result;
        }

        private List<string> GetMembers(string groupName)
        {
            List<string> users = new List<string>();

            try
            {
                using (System.Web.Hosting.HostingEnvironment.Impersonate())
                {
                    string dlManagerUserName = AppCredentials.Instance.UserName;
                    string dlManagerPassword = AppCredentials.Instance.Password;

                    using (PrincipalContext oPrincipalContext = new PrincipalContext(ContextType.Domain,
                            Environment.UserDomainName,
                            dlManagerUserName,
                            dlManagerPassword))
                    {

                        // find the group in question
                        GroupPrincipal group = GroupPrincipal.FindByIdentity(oPrincipalContext, groupName);

                        // if found....
                        if (group != null)
                        {
                            // iterate over members
                            foreach (Principal p in group.GetMembers())
                            {
                                // do whatever you need to do to those members
                                if (p is UserPrincipal)
                                {
                                    // do whatever you need to do to those members
                                    UserPrincipal theUser = p as UserPrincipal;
                                    users.Add(theUser.SamAccountName);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingHelper.Instance.LogError(LogOptions.GetMembers, ex);
            }

            return users;
        }

        public bool IsUserGroupOwner(string group)
        {
            bool isOwner = false;

            try
            {
                using (System.Web.Hosting.HostingEnvironment.Impersonate())
                {
                    string userName = HttpContext.Current.User.Identity.Name;
                    string dlManagerUserName = AppCredentials.Instance.UserName;
                    string dlManagerPassword = AppCredentials.Instance.Password;

                    using (PrincipalContext oPrincipalContext = new PrincipalContext(ContextType.Domain,
                            Environment.UserDomainName,
                            dlManagerUserName,
                            dlManagerPassword))
                    {
                        UserPrincipal oUserPrincipal = UserPrincipal.FindByIdentity(oPrincipalContext, userName);
                        GroupPrincipal oGroupPrincipal = GroupPrincipal.FindByIdentity(oPrincipalContext, group);
                        if (oUserPrincipal != null && oGroupPrincipal != null)
                        {
                            DirectoryEntry dE = (DirectoryEntry)oGroupPrincipal.GetUnderlyingObject();
                            isOwner = dE.Properties["managedBy"].Value != null && ((string)dE.Properties["managedBy"].Value).Equals(oUserPrincipal.DistinguishedName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingHelper.Instance.LogError(LogOptions.CheckAdmin, ex);
            }

            return isOwner;
        }

        public bool RemoveUsers(string group, string []users)
        {
            bool finalSuccess = true;

            foreach(string user in users)
            {
                finalSuccess = finalSuccess && RemoveUserFromGroup(user, group);
            }

            return finalSuccess;
        }

        public bool AddUsers(string group, string[] users)
        {
            bool finalSuccess = true;

            foreach (string user in users)
            {
                finalSuccess = finalSuccess && AddUserToGroup(user, group);
            }

            return finalSuccess;
        }

        public bool AddUserToGroup(string sUserName, string sGroupName)
        {
            bool success = false;

            bool isOwner = IsUserGroupOwner(sGroupName);
            if (!isOwner)
                return false;

            try
            {
                using (System.Web.Hosting.HostingEnvironment.Impersonate())
                {
                    string dlManagerUserName = AppCredentials.Instance.UserName;
                    string dlManagerPassword = AppCredentials.Instance.Password;

                    using (PrincipalContext oPrincipalContext = new PrincipalContext(ContextType.Domain,
                            Environment.UserDomainName,
                            dlManagerUserName,
                            dlManagerPassword))
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
            }
            catch (Exception ex)
            {
                StringBuilder additionalInfo = new StringBuilder();
                additionalInfo.Append("Exception while trying to add user: '");
                additionalInfo.Append(sUserName);
                additionalInfo.Append("' to the group: '");
                additionalInfo.Append(sGroupName);
                additionalInfo.Append("'.");
                LoggingHelper.Instance.LogError(LogOptions.AddUsers, ex, additionalInfo.ToString());
            }

            return success;
        }

        public bool RemoveUserFromGroup(string sUserName, string sGroupName)
        {
            bool success = false;

            bool isOwner = IsUserGroupOwner(sGroupName);
            if (!isOwner)
                return false;

            try
            {
                using (System.Web.Hosting.HostingEnvironment.Impersonate())
                {
                    string dlManagerUserName = AppCredentials.Instance.UserName;
                    string dlManagerPassword = AppCredentials.Instance.Password;

                    using (PrincipalContext oPrincipalContext = new PrincipalContext(ContextType.Domain,
                            Environment.UserDomainName,
                            dlManagerUserName,
                            dlManagerPassword))
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
            }
            catch (Exception ex)
            {
                StringBuilder additionalInfo = new StringBuilder();
                additionalInfo.Append("Exception while trying to remove user: '");
                additionalInfo.Append(sUserName);
                additionalInfo.Append("' from the group: '");
                additionalInfo.Append(sGroupName);
                additionalInfo.Append("'.");
                LoggingHelper.Instance.LogError(LogOptions.RemoveUsers, ex, additionalInfo.ToString());
            }

            return success;
        }
    }
}