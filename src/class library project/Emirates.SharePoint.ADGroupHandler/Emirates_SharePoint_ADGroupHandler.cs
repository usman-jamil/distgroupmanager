using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Hosting;
using System.Security.Principal;
using Microsoft.IdentityModel;
using System.Threading;
using Microsoft.IdentityModel.WindowsTokenService;
using Microsoft.IdentityModel.Claims;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;

namespace Emirates.SharePoint.ADGroupHandler
{
    public class Emirates_SharePoint_ADGroupHandler : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            string op = context.Request.QueryString["op"];

            switch (op)
            {
                case "groups":
                    string groups = ADHelper.Instance.GetGroupsAsString();
                    context.Response.Write(groups);
                    break;
                case "users":
                    string groupName = string.IsNullOrEmpty(context.Request["group"]) ? "" : context.Request["group"];
                    string users = ADHelper.Instance.GetUsersAsString(groupName);
                    context.Response.Write(users);
                    break;
                case "addusers":
                    string usersToAdd = string.IsNullOrEmpty(context.Request["users"]) ? "" : context.Request["users"];
                    string additionToGroup = string.IsNullOrEmpty(context.Request["group"]) ? "" : context.Request["group"];
                    string[] addUsersArray = javaScriptSerializer.Deserialize<string[]>(usersToAdd);

                    bool addSuccess = ADHelper.Instance.AddUsers(additionToGroup, addUsersArray);
                    if(addSuccess)
                    {
                        LoggingHelper.Instance.LogAudit("User(s) Added", string.Format("User(s) Added: {0} to Group: {1}", string.Join(", ", addUsersArray), additionToGroup));
                    }
                    context.Response.Write(javaScriptSerializer.Serialize(addSuccess));
                    break;
                case "delusers":
                    string usersToDel = string.IsNullOrEmpty(context.Request["users"]) ? "" : context.Request["users"];
                    string deletionFromGroup = string.IsNullOrEmpty(context.Request["group"]) ? "" : context.Request["group"];
                    string[] usersArray = javaScriptSerializer.Deserialize<string[]>(usersToDel);

                    bool delSuccess = ADHelper.Instance.RemoveUsers(deletionFromGroup, usersArray);
                    if (delSuccess)
                    {
                        LoggingHelper.Instance.LogAudit("User(s) Removed", string.Format("User(s) Removed: {0} from Group: {1}", string.Join(", ", usersArray), deletionFromGroup));
                    }
                    context.Response.Write(javaScriptSerializer.Serialize(delSuccess));
                    break;
                default:
                    context.Response.Write(string.Format("Hello from Emirates_SharePoint_ADGroupHandler.ashx"));
                    break;
            }
            

            context.Response.Flush();
            context.Response.End();
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}
