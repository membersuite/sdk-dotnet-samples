using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using MemberSuite.SDK.Concierge;
using MemberSuite.SDK.Results;
using MemberSuite.SDK.Types;

namespace API_Usage_Examples
{
    public class LoginToPortalAndPullMemberInformation : ConciergeSampleBase 
    {
        public override void Run()
        {
            /* The purpose of this sample is to show you how to use the API to execute a portal login
             * on behalf of an individual, and once that portal login is completed, how to query on a member's status.
             * We reuse some of what's in DeterminingMembershipStatus here.
             */

            // First, we need to prepare the proxy with the proper security settings.
            // This allows the proxy to generate the appropriate security header. For more information
            // on how to get these settings, see http://api.docs.membersuite.com in the Getting Started section
            if (!ConciergeAPIProxyGenerator.IsSecretAccessKeySet)
            {
                ConciergeAPIProxyGenerator.SetAccessKeyId(ConfigurationManager.AppSettings["AccessKeyID"]);
                ConciergeAPIProxyGenerator.SetSecretAccessKey(ConfigurationManager.AppSettings["SecretAccessKey"]);
                ConciergeAPIProxyGenerator.AssociationId = ConfigurationManager.AppSettings["AssociationID"];
            }

            // ok, let's generate our API proxy
            using (var api = ConciergeAPIProxyGenerator.GenerateProxy())
            {
                Console.WriteLine(
                    "Please enter login credentials. If you don't have specific credentials, try 'test' for both the user and password to get Terry Smith.");
                Console.WriteLine();
                Console.WriteLine("Please enter the username and hit ENTER.");
                string username = Console.ReadLine();

                Console.WriteLine("Please enter the password and hit ENTER.");
                string password = Console.ReadLine();

                ConciergeResult<LoginResult> portalLoginResult = api.LoginToPortal(username, password);

                if (!portalLoginResult.Success)
                {
                    Console.WriteLine("Portal login failed with this error: " + portalLoginResult.FirstErrorMessage);
                    return;
                }

                Console.WriteLine("Portal login successful - accessing member information...");

                // this is the "entity" that you've logged in as.
                // remember an entity is either an organization OR an individual - it's a base class.
                MemberSuiteObject entity = portalLoginResult.ResultValue.PortalEntity;

                // now, let's get that entity's ID
                string entityID = entity.SafeGetValue<string>("ID");

                // there's a lot of confusion between the different types of users, so let's list them
                Console.WriteLine();
                Console.WriteLine("Users");
                Console.WriteLine("-----------------------------");
                Console.WriteLine();
                Console.WriteLine("API User: {0} - this is the security context that your application is using.",
                    portalLoginResult.ResultValue.User["Name"]);
                Console.WriteLine(
                    "Portal User: {0} - this is the security context that you are logging your member in as. It's linked to the actual member record via the Owner property.",
                    portalLoginResult.ResultValue.PortalUser["Name"]);
                Console.WriteLine(
                    "Individual/Organization: {0} - this is the entity record that the portal user is tied to. ",
                    portalLoginResult.ResultValue.PortalEntity["Name"]);
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("The owner of the login has an ID of '{0}' - accessing...",
                    entityID);

                // ok, let's get the membership status
                string msql = string.Format(
                    "select TOP 1 FirstName, LocalID, LastName, Membership.Type.Name, Membership.PrimaryChapter.Name, Membership.ExpirationDate, Membership.ReceivesMemberBenefits from Individual where ID = '{0}' order by LastName",
                    entityID);

                var result = api.ExecuteMSQL(msql, 0, null);

                if (!result.Success)
                {
                    Console.WriteLine("Search failed: {0}", result.FirstErrorMessage);
                    return;
                }

                DataRow resultRow = result.ResultValue.SearchResult.Table.Rows[0];
                // we know there's only one row, since the ID is unqiue
                Console.WriteLine("ID: " + resultRow["LocalID"]);
                Console.WriteLine("First Name: " + resultRow["FirstName"]);
                Console.WriteLine("Last Name: " + resultRow[msIndividual.FIELDS.LastName]);
                    // <--- this is how you would use constants, which is the better way to go
                Console.WriteLine("Member? " + resultRow["Membership.ReceivesMemberBenefits"]);
                Console.WriteLine("Member Type: " + resultRow["Membership.Type.Name"]);
                Console.WriteLine("Chapter: " + resultRow["Membership.PrimaryChapter.Name"]);
                Console.WriteLine("Expiration Date: " + resultRow["Membership.ExpirationDate"]);
                Console.WriteLine("Search successful: {0} results returned.",
                    result.ResultValue.SearchResult.TotalRowCount);

            }
        }
    }
}
