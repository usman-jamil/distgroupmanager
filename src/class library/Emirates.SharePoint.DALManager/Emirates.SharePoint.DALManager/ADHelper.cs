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

namespace Emirates.SharePoint.DALManager
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

            //groupString = SerializeToJson(userGroups.ToArray());
            groupString = SerializeToJson(userGroups.ToArray());

            return groupString;
        }

        public string GetUsersAsString_New(string groupName)
        {
            string userString = string.Empty;

            if (!string.IsNullOrEmpty(groupName))
            {
                List<ADUser> userGroups = GetMembersLDAP(groupName);
                
                
                userString = SerializeToJson(userGroups);
            }

            return userString;
        }

        public string GetUsersAsString(string groupName)
        {
            string userString = string.Empty;

            if (!string.IsNullOrEmpty(groupName))
            {
                List<string> userGroups = GetMembers_Old(groupName);

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
                        //"S736354";
                        //HttpContext.Current.User.Identity.Name;
                        //"S391308";
                        //HttpContext.Current.User.Identity.Name;
                        //"S736354";
                        //HttpContext.Current.User.Identity.Name; 
                    //"S130406"; //"S161206";//"S7363654";
                    //
                    string dlManagerUserName =  AppCredentials.Instance.UserName;
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
                            samSearcher.Filter = "(&(objectClass=group)(|(msexchcomanagedbylink=" + user.DistinguishedName + ")(managedBy=" + user.DistinguishedName + ")))";

                            SearchResultCollection results = samSearcher.FindAll();

                            foreach (SearchResult samResult in results)
                            {
                                

                                result.Add(Convert.ToString(samResult.Properties["name"][0]));

                                //ADUser obj = new ADUser();
                                //obj.Name = samResult.Properties["displayname"][0] + "";
                                //obj.Email = samResult.Properties["emailaddress"][0] + "";
                                //obj.StaffID = samResult.Properties["name"][0] + "";

                                // result.Add(obj);
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

        private List<ADUser> GetMembersLDAP(string groupName)
        {
            List<ADUser> users = new List<ADUser>();

            try
            {
                using (System.Web.Hosting.HostingEnvironment.Impersonate())
                {
                    string dlManagerUserName = AppCredentials.Instance.UserName;
                    string dlManagerPassword = AppCredentials.Instance.Password;
                    int dlThreshold = Convert.ToInt32(AppCredentials.Instance.Threshold);


                    using (PrincipalContext oPrincipalContext = new PrincipalContext(ContextType.Domain,
                            Environment.UserDomainName,
                            dlManagerUserName,
                            dlManagerPassword))
                    {
                        GroupPrincipal group = GroupPrincipal.FindByIdentity(oPrincipalContext, groupName);

                        DirectoryEntry entry = new DirectoryEntry();
                        PrincipalSearcher srch = new PrincipalSearcher();
                        DirectorySearcher search = new DirectorySearcher(entry);
                        string query = "(&(objectCategory=person)(objectClass=user)(memberOf=" + group.DistinguishedName + "))";
                        search.Filter = query;
                        search.PropertiesToLoad.Add("memberOf");
                        search.PropertiesToLoad.Add("displayname");
                        search.PropertiesToLoad.Add("mail");
                        search.PropertiesToLoad.Add("samaccountname");
                        search.PageSize = 20000;
                        ADUser obj = null;
                        System.DirectoryServices.SearchResultCollection mySearchResultColl = search.FindAll();
                        //Console.WriteLine("Members of the {0} Group in the {1} Domain", groupName, domainName);
                        int i = 0;
                        foreach (SearchResult result in mySearchResultColl)
                        {
                            foreach (string prop in result.Properties["memberOf"])
                            {
                                if (prop.Contains(groupName))
                                {
                                    try
                                    {
                                        
                                        obj = new ADUser();
                                        obj.Name = result.Properties["displayname"][0].ToString();// .Properties["displayname"][0] + "";
                                        obj.Email = result.Properties["mail"][0].ToString();
                                        obj.StaffID = result.Properties["samaccountname"][0].ToString();

                                        users.Add(obj);
                                        if (i == dlThreshold + 1)
                                        {
                                            break;
                                        }

                                        i++;
                                    }
                                    catch {
                                        //LoggingHelper.Instance.LogAudit("Error User-" + groupName, result.Properties["samaccountname"][0].ToString());
                                    }
                                    //Console.WriteLine("    " + result.Properties["name"][0].ToString());
                                }
                            }

                            if (i == dlThreshold + 1)
                            {
                                break;
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
        private List<ADUser> GetMembers(string groupName)
        {
            List<ADUser> users = new List<ADUser>();

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

                       // Principal group = GroupPrincipal.FindByIdentity(oPrincipalContext, groupName);

                        // if found....
                        if (group != null)
                        {
                            // iterate over members                           

                            var searchPrincipal = new UserPrincipal(oPrincipalContext);
                            
                            PrincipalSearcher insPrincipalSearcher = new PrincipalSearcher();
                            insPrincipalSearcher.QueryFilter = searchPrincipal;
                            PrincipalSearchResult<Principal> results = insPrincipalSearcher.FindAll();
                            ADUser obj = null;
                            UserPrincipal theUser = null;
                           
                            foreach (Principal p in results)
                            {
                                if (p is UserPrincipal)
                                {
                                    // do whatever you need to do to those members
                                    theUser = p as UserPrincipal;

                                    obj = new ADUser();
                                    obj.Name = theUser.Name;// .Properties["displayname"][0] + "";
                                    obj.Email = theUser.EmailAddress;
                                    obj.StaffID = theUser.SamAccountName;

                                    users.Add(obj);

                                  
                                    //users.Add(theUser.SamAccountName);
                                }
                            }


                            //PrincipalSearchResult<Principal> lstMembers = group.GetMembers();
                            //ADUser obj = null;
                            //UserPrincipal theUser = null;
                            //foreach (Principal p in lstMembers)
                            //{

                            //    // do whatever you need to do to those members
                            //    if (p is UserPrincipal)
                            //    {
                            //        // do whatever you need to do to those members
                            //        theUser = p as UserPrincipal;

                            //        obj = new ADUser();
                            //        obj.Name = theUser.Name;// .Properties["displayname"][0] + "";
                            //        obj.Email = theUser.EmailAddress;
                            //        obj.StaffID = theUser.SamAccountName;

                            //        users.Add(obj);

                            //        //users.Add(theUser.SamAccountName);
                            //    }
                            //}


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
        private List<string> GetMembers_Old(string groupName)
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

                                    //ADUser obj = new ADUser();
                                    //obj.Name = theUser.Name;// .Properties["displayname"][0] + "";
                                    //obj.Email = theUser.EmailAddress;
                                    //obj.StaffID = theUser.SamAccountName;

                                    //users.Add(obj);

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
            bool isOwner = true;

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
                            if (dE.Properties["msexchcomanagedbylink"].Value != null)
                            {
                                object[] msExchangeValues = dE.Properties["msexchcomanagedbylink"].Value as object[];
                                foreach (object msExchangeValue in msExchangeValues)
                                {
                                    string value = (string)msExchangeValue;
                                    Console.WriteLine("MSExchange Managed By: " + value);
                                    isOwner = isOwner || value.Equals(oUserPrincipal.DistinguishedName);
                                }
                            }
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

            //bool isOwner = IsUserGroupOwner(sGroupName);
            //if (!isOwner)
                //return false;

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

            //bool isOwner = IsUserGroupOwner(sGroupName);
            //if (!isOwner)
                //return false;

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

    public class ADUser
    {
        public string Name {get;set;}
        public string Email { get; set; }
        public string StaffID { get; set; }
    }
}