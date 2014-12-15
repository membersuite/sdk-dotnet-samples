using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using MemberSuite.SDK.Concierge;
using MemberSuite.SDK.Types;

namespace API_Usage_Examples
{
    public class DeterminingMembershipStatus : ConciergeSampleBase 
    {
        public override void Run()
        {
            /* This demo is designed to show you whether or not an individual is an member is active or not.
             * Remember the MemberSuite object model as follows:
             * 
             *  Individual  -this is a person
             *     |          
             *     ---->  Membership -  a membership record is tied to an individual. A member is an individual (or organization) with a membership record tied to it.
             *     
             * Fortunately, there's a field on the Individual search called Membership. We can use that to figure out a person's status. Generally, an individual
             * is a member if they have a membership record with ReceivesMemberBenefits set to true
             * 
             * Remember you can always see objects/fields by going to the api documentation at http://api.docs.membersuite.com.
             * 
             * For another (more detailed) example, check out LoginToPortalAndPullMemberInformation sample.
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
                string msql = "select LocalID,FirstName, LastName, Membership.ReceivesMemberBenefits from Individual order by LastName";

                // If I needed to check the membership status of someone whose ID I knew, I would say:
                // string msql = "select TOP 1 LocalID,FirstName, LastName, Membership.ReceivesMemberBenefits from Individual where ID = '%%Id%%' order by LastName";

                var result = api.ExecuteMSQL(msql, 0, null);

                if (!result.Success)
                {
                    Console.WriteLine("Search failed: {0}", result.FirstErrorMessage);
                    return;
                }

                Console.WriteLine("Search successful: {0} results returned.",
                    result.ResultValue.SearchResult.TotalRowCount);


                foreach (DataRow row in result.ResultValue.SearchResult.Table.Rows)
                    Console.WriteLine("#{0} - {1}, {2} - Is this person a member? {3}",
                        row["LocalID"], row["LastName"], row["FirstName"], row["Membership.ReceivesMemberBenefits"]);
            }
        }
    }
}
