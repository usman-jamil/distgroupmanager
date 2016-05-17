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

namespace Emirates.SharePoint.ADGroupHandler
{
    public enum LogOptions
    {
        GetGroups = 0,
        GetMembers = 1,
        AddUsers = 2,
        RemoveUsers = 3,
        CheckAdmin = 4,
        UserAdded = 5,
        UserRemoved = 6
    }

    public sealed class LoggingHelper
    {
        private readonly string[] categories = { "Get-Groups",
            "Get-Members",
            "Add-Users",
            "Remove-Users",
            "Check-Admin",
            "User-Added",
            "User-Removed" };

        //private readonly string webUri = "http://infospace-stg.emirates.com/sites/admanager";

        static readonly LoggingHelper instance = new LoggingHelper();

        /* ======================== */

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static LoggingHelper()
        {
        }

        LoggingHelper()
        {
        }

        public static LoggingHelper Instance
        {
            get
            {
                return instance;
            }
        }

        public void LogError(LogOptions option, Exception ex, string additionalInfo)
        {
            try
            {
                SPContext context = SPContext.GetContext(HttpContext.Current);
                SPSecurity.RunWithElevatedPrivileges(delegate ()
                {
                    using (SPWeb web = new SPSite(context.Web.Url).OpenWeb())
                    {
                        web.AllowUnsafeUpdates = true;
                        // implementation details omitted
                        SPList list = web.Lists["Error-Log"];
                        SPField authorField = list.Fields.GetFieldByInternalName("Author0");
                        SPListItem newItem = list.AddItem();
                        int maxLength = 255;
                        string trimmedTitle = ex.Message.Length > maxLength - 1 ? ex.Message.Substring(0, maxLength) : ex.Message;

                        int userId = context.Web.CurrentUser.ID;
                        string userName = context.Web.CurrentUser.LoginName;
                        SPFieldUserValue authorFieldValue = new SPFieldUserValue(web, userId, userName);

                        newItem["Title"] = trimmedTitle;
                        newItem["Comment"] = ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine + (ex.InnerException != null ? ex.InnerException.Message : "No Inner Exception") + Environment.NewLine + (ex.InnerException != null ? ex.InnerException.StackTrace : "No Inner Exception Detail" + Environment.NewLine + additionalInfo);
                        newItem["Category"] = categories[(int)option];
                        newItem[authorField.Id] = authorFieldValue;
                        newItem["IP"] = HttpContext.Current.Request.UserHostAddress;
                        newItem.Update();

                        web.AllowUnsafeUpdates = false;
                    }
                });
            }
            catch { }
        }

        public void LogError(LogOptions option, Exception ex)
        {
            LogError(option, ex, string.Empty);
        }

        public void LogAudit(string title, string message)
        {
            try
            {
                SPContext context = SPContext.GetContext(HttpContext.Current);
                SPSecurity.RunWithElevatedPrivileges(delegate ()
                {
                    using (SPWeb web = new SPSite(context.Web.Url).OpenWeb())
                    {
                        int userId = context.Web.CurrentUser.ID;
                        string userName = context.Web.CurrentUser.LoginName;
                        SPFieldUserValue authorFieldValue = new SPFieldUserValue(web, userId, userName);

                        web.AllowUnsafeUpdates = true;
                        // implementation details omitted
                        SPList list = web.Lists["Audit-Log"];
                        SPField authorField = list.Fields.GetFieldByInternalName("Author0");
                        SPListItem newItem = list.AddItem();
                        newItem["Title"] = title;
                        newItem["Log"] = message;
                        newItem[authorField.Id] = authorFieldValue;
                        newItem["IP"] = HttpContext.Current.Request.UserHostAddress;
                        newItem.Update();
                        web.AllowUnsafeUpdates = false;
                    }
                });
            }
            catch { }
        }
    }
}