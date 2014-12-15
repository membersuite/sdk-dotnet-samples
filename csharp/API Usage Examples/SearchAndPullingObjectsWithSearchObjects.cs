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
    public class SearchAndPullingObjectsWithSearchObjects : ConciergeSampleBase
    {
        public override void Run()
        {
            /* This sample is designed to demonstrate running a search in MemberSuite
             * using search objects. Instead of returning a table, this operation will return MemberSuiteObjects
             * we can operate on. We'll do a search for all people who have first OR last names that
             * begin with the letter A */

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
                // now, let's create our search object
                Search searchToUse = new Search();
                searchToUse.Type = msIndividual.CLASS_NAME; // we're looking for people. Always good to use the constants provided instead of string literals
                searchToUse.Context = null; // this line isn't necessary (it's null by default), but it's here for effect

                // now, we can add criteria using the AddCriteria shortcut methods
                searchToUse.AddCriteria(Expr.Contains(msIndividual.FIELDS.FirstName, "A*"));
                searchToUse.AddCriteria(Expr.Contains(msIndividual.FIELDS.LastName, "A*"));
                searchToUse.GroupType = SearchOperationGroupType.Or;

                searchToUse.AddOutputColumn("LocalID");
                searchToUse.AddOutputColumn("FirstName");
                searchToUse.AddOutputColumn("LastName");

                // and now, sort
                searchToUse.AddSortColumn("LastName");


                /* 
                Above is the short way - the long way to do the same thing as above is listed below:
             
                SearchOperationGroup group = new SearchOperationGroup();
                group.GroupType = SearchOperationGroupType.Or;

                Contains c1 = new Contains();
                c1.FieldName = msIndividual.FIELDS.FirstName;
                c1.ValuesToOperateOn = new List<object> { "A*" };

                Contains c2 = new Contains();
                c1.FieldName = msIndividual.FIELDS.LastName ;
                c1.ValuesToOperateOn = new List<object> { "A*" };

                group.Criteria.Add(c1);
                group.Criteria.Add(c2);
                searchToUse.Criteria.Add(group);
             
                searchToUse.OutputColumns.Add(new SearchOutputColumn { Name = "LocalID" });
                searchToUse.OutputColumns.Add(new SearchOutputColumn { Name = "FirstName" });
                searchToUse.OutputColumns.Add(new SearchOutputColumn { Name = "LastName" });

                searchToUse.SortColumns.Add(new SearchSortColumn{ Name = "LastName" });
                */

                var result = api.GetObjectsBySearch(searchToUse, null, 0, null);

                if (!result.Success)
                {
                    Console.WriteLine("Search failed: {0}", result.FirstErrorMessage);
                    return;
                }

                Console.WriteLine("Search successful: {0} results returned.", result.ResultValue.TotalRowCount);


                foreach (MemberSuiteObject row in result.ResultValue.Objects)
                {
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine("#{0} - {1}, {2} - Properties are as follows:",
                        row["LocalID"], row["LastName"], row["FirstName"]);
                    Console.WriteLine("-------------------------------------------------");
                    // we'll write out all of the object fields
                    foreach (var entry in row.Fields)
                        if (entry.Value != null)
                            Console.WriteLine("{0}: {1}", entry.Key, entry.Value);
                }
            }
        }
    }
}
