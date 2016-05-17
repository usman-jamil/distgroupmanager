using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADManager
{
    interface IADManager
    {
        bool ValidateCredentials();
        bool AddUserToGroup(string sUserName, string sGroupName);
        bool RemoveUserFromGroup(string sUserName, string sGroupName);
        List<string> GroupMembers(string sGroupName);
        string GetGroupInfo(string sGroupName);
    }
}
