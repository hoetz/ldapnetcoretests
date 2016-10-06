using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Novell.Directory.Ldap;

namespace ConsoleApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var config = new ConfigurationBuilder()
                                .SetBasePath(Directory.GetCurrentDirectory())
                                .AddJsonFile("secret.json")
                                .Build();

            LdapConnection con = new LdapConnection();
            con.Connect("172.16.250.139", 389);
            con.Bind("CN=Administrator,CN=Users,DC=flo,DC=loc", config["password"]);

            var searchBase = "CN=Users,DC=flo,DC=loc";
            var searchFilter = "(objectclass=*)";

            LdapSearchQueue queue = con.Search(searchBase,
                LdapConnection.SCOPE_SUB,
                 searchFilter,
                 new string[] { "sAMAccountName" },
                 false,
                (LdapSearchQueue)null,
                (LdapSearchConstraints)null);

            LdapMessage message;
            while ((message = queue.getResponse()) != null)
            {
                if (message is LdapSearchResult)
                {
                    LdapEntry entry = ((LdapSearchResult)message).Entry;
                    System.Console.Out.WriteLine("\n" + entry.DN);
                    System.Console.Out.WriteLine("\tAttributes: ");
                    LdapAttributeSet attributeSet = entry.getAttributeSet();
                    System.Collections.IEnumerator ienum = attributeSet.GetEnumerator();
                    while (ienum.MoveNext())
                    {
                        LdapAttribute attribute = (LdapAttribute)ienum.Current;
                        string attributeName = attribute.Name;
                        string attributeVal = attribute.StringValue;
                        Console.WriteLine(attributeName + "value:" +
                                                                     attributeVal);
                    }
                }
            }
            con.Disconnect();


        }
    }
}
