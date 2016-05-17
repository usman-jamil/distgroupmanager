using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Configuration;

namespace ADManager
{
    class Program
    {
        static void Main(string[] args)
        {
            string auth = ConfigurationManager.AppSettings["ADManager"];
            IADManager manager = null;

            if (auth.Equals("Special"))
            {
                Console.WriteLine("Enter username:"); // Prompt
                string username = Console.ReadLine();
                Console.WriteLine("Enter password (will not be shown):"); // Prompt
                string password = GetConsolePassword();
                manager = new ADMethodsAccountManagement(username, password);
            }
            else
            {
                manager = new ADMethodsAccountManagementDefault();
            }

            bool isValid = manager.ValidateCredentials();
            if (!isValid)
            {
                Console.WriteLine("The provided username and password is incorrect!");
                Environment.Exit(0);
            }
            else
            {
                Console.WriteLine("Credentials Validated!");
            }

            string strAddUsers = ConfigurationManager.AppSettings["AddUsers"];
            string[] arrAddUsers = !string.IsNullOrEmpty(strAddUsers) ? strAddUsers.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries) : new string[] { };
            string strRemoveUsers = ConfigurationManager.AppSettings["RemoveUsers"];
            string[] arrRemoveUsers = !string.IsNullOrEmpty(strRemoveUsers) ? strRemoveUsers.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries) : new string[] { };
            string strGroupName = ConfigurationManager.AppSettings["Group"];

            //Start removing users
            foreach (string userToRemove in arrRemoveUsers)
            {
                bool success = manager.RemoveUserFromGroup(userToRemove, strGroupName);
                if (success)
                {
                    Console.WriteLine("User: {0} removed from Group: {1}", userToRemove, strGroupName);
                }
                else
                {
                    Console.WriteLine("Error removing User: {0} from Group: {1}", userToRemove, strGroupName);
                }
            }

            //Start adding users
            foreach (string userToAdd in arrAddUsers)
            {
                bool success = manager.AddUserToGroup(userToAdd, strGroupName);
                if (success)
                {
                    Console.WriteLine("User: {0} added to Group: {1}", userToAdd, strGroupName);
                }
                else
                {
                    Console.WriteLine("Error adding User: {0} to Group: {1}", userToAdd, strGroupName);
                }
            }

            //Show list of Users
            List<string> users = manager.GroupMembers(strGroupName);
            string groupInfo = manager.GetGroupInfo(strGroupName);
            Console.WriteLine(groupInfo);
            Console.WriteLine("Users in this group: ");
            Console.WriteLine(string.Join(", ", users));
        }

        private static string GetConsolePassword()
        {
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                ConsoleKeyInfo cki = Console.ReadKey(true);
                if (cki.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }

                if (cki.Key == ConsoleKey.Backspace)
                {
                    if (sb.Length > 0)
                    {
                        Console.Write("\b\0\b");
                        sb.Length--;
                    }

                    continue;
                }

                Console.Write('*');
                sb.Append(cki.KeyChar);
            }

            return sb.ToString();
        }
    }
}
