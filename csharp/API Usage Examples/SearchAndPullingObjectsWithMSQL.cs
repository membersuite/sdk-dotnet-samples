using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using MemberSuite.SDK.Concierge;
using MemberSuite.SDK.Searching;
using MemberSuite.SDK.Searching.Operations;
using MemberSuite.SDK.Types;

namespace API_Usage_Examples
{
    public class SearchAndPullingObjectsWithMSQL : ConciergeSampleBase
    {
        public override void Run()
        {
            /* This sample is designed to demonstrate running a search in MemberSuite
             * using MSQL. Instead of returning a table, this operation will return MemberSuiteObjects
             * we can operate on. We'll do a search for all people who have first OR last names that
             * begin with the letter A */

            // First, we need to prepare the proxy with the proper security settings.
            // This allows the proxy to generate the appropriate security header. For more information
            // on how to get these settings, see http://api.docs.membersuite.com in the Getting Started section
            ConciergeAPIProxyGenerator.SetAccessKeyId(ConfigurationManager.AppSettings["AccessKeyID"]);
            ConciergeAPIProxyGenerator.SetSecretAccessKey(ConfigurationManager.AppSettings["SecretAccessKey"]);
            ConciergeAPIProxyGenerator.AssociationId = ConfigurationManager.AppSettings["AssociationID"];

            // ok, let's generate our API proxy
            IConciergeAPIService api = ConciergeAPIProxyGenerator.GenerateProxy();

            // now, run a WhoAmI to establish a session
            var loginResponse = api.WhoAmI();

            if (!loginResponse.Success)
                throw new ApplicationException("unable to login: " + loginResponse.FirstErrorMessage);

            Console.WriteLine("Login successful.");

            // let's set the session ID, so we don't have to rebuild the session on each call
            // This isn't required, but it makes accessing the API faster since we can cache your
            // login credentials
            ConciergeAPIProxyGenerator.SessionID = loginResponse.ResultValue.SessionID;

            string msql = "select OBJECTS() from Individual where LastName like 'A*' or FirstName like 'A*' order by LastName";

            var result = api.ExecuteMSQL(msql, 0, null);

            if (!result.Success)
            {
                Console.WriteLine("Search failed: {0}", result.FirstErrorMessage);
                return;
            }

            Console.WriteLine("Search successful: {0} results returned.", result.ResultValue.ObjectSearchResult.TotalRowCount );


            foreach (MemberSuiteObject row in result.ResultValue.ObjectSearchResult.Objects)
            {
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("#{0} - {1}, {2} - Properties are as follows:",
                                  row["LocalID"], row["LastName"], row["FirstName"]);
                Console.WriteLine( "-------------------------------------------------" );
                // we'll write out all of the object fields
                foreach (var entry in row.Fields)
                    if (entry.Value != null)
                        Console.WriteLine("{0}: {1}", entry.Key, entry.Value);
            }



        }
    }
}
