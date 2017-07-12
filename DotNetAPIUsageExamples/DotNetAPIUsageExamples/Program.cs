using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MemberSuite.SDK.Concierge;
using MemberSuite.SDK.Searching;
using MemberSuite.SDK.Searching.Operations;
using MemberSuite.SDK.Types;

namespace DotNetAPIUsageExamples
{
    class Program
    {
        static void Main(string[] args)
        {
            // You will need to change the creditials in this method call to match the ones created for your organization. See below links for more information
            // https://help.production.membersuite.com/hc/en-us/articles/115002511483-How-To-Locate-Your-Association-ID
            // https://help.production.membersuite.com/hc/en-us/articles/115002511423-How-To-Create-a-New-Access-Key
            ProxySetup.SetupProxy();

            SearchMembershipRecords(); //Change this to any of the other method calls to test

            Console.ReadLine();
        }

        /// <summary>
        /// Demonstrates how to log into the portal and then pull information on the person who is logged in
        /// Also demonstrates how to user our Membersuite Query Language (MSQL) to search Membersuite records
        /// For more information, please see https://help.production.membersuite.com/hc/en-us/articles/115001903246-Search-in-MemberSuite
        /// </summary>
        private static void LoginPullIndividualRecords()
        {
            using (var api = ConciergeAPIProxyGenerator.GenerateProxy())
            {
                //you would need to enter your userName and userPassword to get this to work
                var loginResult = api.LoginToPortal("userName", "userPassword");
                if (loginResult.Success)
                {
                    var userID = loginResult.ResultValue.PortalUser.Fields["Owner"];

                    var msql = string.Format("SELECT TOP 1 Name FROM Organization WHERE Individual.ID = '{0}'", userID);
                    var individualResult = api.ExecuteMSQL(msql, 0, null);


                    if (individualResult.Success)
                    {
                        Console.WriteLine(string.Format("FirstName = {0}, LastName = {1}", individualResult.ResultValue.SingleObject.Fields["FirstName"], individualResult.ResultValue.SingleObject.Fields["LastName"]));
                    }
                }
            }
        }

        /// <summary>
        /// Demonstrates how to either get PortalUser information for a provided EmailAddress or create a PortalUser account if one doesn't exist
        /// The API GetOrCreatePortalUser will attempt to match the supplied credentials to a portal user, individual, or organization. 
        /// If a portal user is found it will be returned.  If not and an individual / organization uses the email address it will create and return a new Portal User
        /// </summary>
        /// <param name="emailAddress"></param>
        private static void FindOrCreatePortalUser(string emailAddress)
        {
            if (!string.IsNullOrWhiteSpace(emailAddress))
            {
                using (var api = ConciergeAPIProxyGenerator.GenerateProxy())
                {
                    var result = api.SearchAndGetOrCreatePortalUser(emailAddress);

                    //The portal user in the result will be null if the e-mail didn't match anywhere
                    if (result.ResultValue != null)
                    {
                        var portalUser = result.ResultValue.ConvertTo<msPortalUser>();
                        Console.WriteLine(string.Format("PortalUser FirstName {0}, PortalUser LastName {1}, PortalUser EmailAddress {2}, PortalUser CreatedDate",
                            portalUser.FirstName, portalUser.LastName, portalUser.EmailAddress, portalUser.CreatedDate));
                    }
                    else
                    {
                        Console.WriteLine("The email address does not match any Portal User, Individual or Organization");
                    }
                }
            }
            else
            {
                Console.WriteLine("Email Address is empty");
            }
        }

        /// <summary>
        /// Demonstrates how to retrieve Membership information based on a provided Individual ID
        /// Also demonstrates howto use the MemberSuite Search Object
        /// For more information, please see https://help.production.membersuite.com/hc/en-us/articles/115001903246-Search-in-MemberSuite
        /// </summary>
        private static void SearchMembershipRecords()
        {
            using (var api = ConciergeAPIProxyGenerator.GenerateProxy())
            {
                // you will need to provide a valid Individual ID for to get this to work
                string individualID = "cbc968d7-0006-c479-1a71-0b3b91980e7a";

                var s = new Search("Membership");
                s.AddOutputColumn("Status");
                s.AddOutputColumn("Name");
                s.AddOutputColumn("ReceivesMemberBenefits");
                s.AddCriteria(Expr.Equals("Individual.Id", individualID));

                var result = api.ExecuteSearch(s, 0, 0);

                if (result.Success && result.ResultValue.Table.Rows.Count > 0)
                {
                    Console.WriteLine(string.Format("Name = {0}, ReceivesMemberBenefits = {1}, Status = {2}",
                        result.ResultValue.Table.Rows[0]["Name"], result.ResultValue.Table.Rows[0]["ReceivesMemberBenefits"], result.ResultValue.Table.Rows[0]["Status"]));
                }
                else
                {
                    Console.WriteLine("Fail");
                }
            }
        }

        /// <summary>
        /// Demonstrates how to retrieve Membership information based on a provided First and Last Name
        /// For more information, please see https://help.production.membersuite.com/hc/en-us/articles/115001903246-Search-in-MemberSuite
        /// </summary>
        private static void SearchMembershipRecords2()
        {
            string _userChapter = string.Empty;
            string _userLocalId = string.Empty;
            string _userName = string.Empty;
            string _status = string.Empty;

            using (var api = ConciergeAPIProxyGenerator.GenerateProxy())
            {
                Search chapSearch = new Search(msMembership.CLASS_NAME);
                chapSearch.AddOutputColumn("Membership.PrimaryChapter");
                chapSearch.AddOutputColumn("PrimaryChapter.Name");
                chapSearch.AddOutputColumn("LocalID");
                chapSearch.AddOutputColumn("Name");
                chapSearch.AddOutputColumn("Status.Name");
                chapSearch.AddCriteria(Expr.Equals("Name", "TestFirstName TestLastName"));

                var chapResult = api.ExecuteSearch(chapSearch, 0, null);

                if (chapResult.Success)
                {
                    if (chapResult.Errors.Count < 0 || chapResult.ResultValue.Table.Rows.Count > 0)
                    {
                        _userChapter = chapResult.ResultValue.Table.Rows[0]["PrimaryChapter.Name"].ToString();
                        _userLocalId = chapResult.ResultValue.Table.Rows[0]["LocalID"].ToString();
                        _userName = chapResult.ResultValue.Table.Rows[0]["Name"].ToString();
                        _status = chapResult.ResultValue.Table.Rows[0]["Status.Name"].ToString();
                    }
                }
            }

            Console.WriteLine(string.Format("UserChapter= {0}, UserID= {1}, UserName= {2}, Status= {3}", _userChapter, _userLocalId, _userName, _status));            
        }

        /// <summary>
        /// Demonstrates how to retrieve Event, Session, and Event Location Room information from our system
        /// For more information, please see...
        /// https://help.production.membersuite.com/hc/en-us/articles/115001949626-User-Guide-Events
        /// https://help.production.membersuite.com/hc/en-us/articles/115001913423-User-Guide-Reference-Sessions
        /// https://help.production.membersuite.com/hc/en-us/articles/215741443-How-to-Create-an-Event-Rooms
        /// </summary>
        private static void SearchEventInformation()
        {
            using (var api = ConciergeAPIProxyGenerator.GenerateProxy())
            {
                //Event Info

                // you will need to provide a valid Event ID for to get this to work
                var eventID = "00000000-0078-cbb3-af9e-0b3c5496833f";

                var s = new Search(msEvent.CLASS_NAME);
                s.AddOutputColumn("ID");
                s.AddOutputColumn("Name");
                s.AddOutputColumn("Code");
                s.AddOutputColumn("StartDate");
                s.AddOutputColumn("EndDate");

                s.AddCriteria(Expr.Equals("ID", eventID));

                var result = api.ExecuteSearch(s, 0, 0);

                Console.WriteLine(string.Format("Event information for - {0}", eventID));

                if (result.Success && result.ResultValue.Table.Rows.Count > 0)
                {
                    foreach (DataRow r in result.ResultValue.Table.Rows)
                    {
                        Console.WriteLine(string.Format("ID = {0}, Name = {1}, Code = {2}, StartDate = {3}, EndDate = {4}", r["ID"], r["Name"], r["Code"], r["StartDate"], r["EndDate"]));
                    }
                }

                // Session Info
                
                var s1 = new Search(msSession.CLASS_NAME);
                s1.AddOutputColumn("ID");
                s1.AddOutputColumn("Name");
                s1.AddOutputColumn("Room");
                s1.AddOutputColumn("StartDate");
                s1.AddOutputColumn("EndDate");
                s1.AddOutputColumn("TimeSlot");

                s1.AddCriteria(Expr.Equals("ParentEvent.ID", eventID));

                var result1 = api.ExecuteSearch(s1, 0, 0);

                Console.WriteLine();
                Console.WriteLine(string.Format("Session information for Event - {0}", eventID));

                if (result1.Success && result1.ResultValue.Table.Rows.Count > 0)
                {
                    foreach (DataRow row in result1.ResultValue.Table.Rows)
                    {
                        Console.WriteLine(string.Format("ID = {0}, Name = {1}, StartDate = {2}, EndDate = {3}, TimeSlot = {4}, Room(s) = {5}", row["ID"], row["Name"], row["StartDate"], row["EndDate"], row["TimeSlot"], row["Room"]));
                        Console.WriteLine();

                        var roomID = row["Room"].ToString();

                        if (!string.IsNullOrWhiteSpace(roomID))
                        {
                            // Event LocationRoom Info
                            var s2 = new Search(msEventLocationRoom.CLASS_NAME);
                            s2.AddOutputColumn("ID");
                            s2.AddOutputColumn("Name");
                            s2.AddOutputColumn("Event");

                            s2.AddCriteria(Expr.Equals("ID", roomID));

                            var result2 = api.ExecuteSearch(s2, 0, 0);

                            Console.WriteLine();
                            Console.WriteLine(string.Format("EventLocationRoom information for - {0}", roomID));
                            if (result2.Success && result2.ResultValue.Table.Rows.Count > 0)
                            {
                                foreach (DataRow row2 in result2.ResultValue.Table.Rows)
                                {
                                    Console.WriteLine(string.Format("ID = {0}, Name = {1}, Event = {2}", row2["ID"], row2["Name"], row2["Event"]));
                                }
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Fail");
                }
            }
        }

        /// <summary>
        /// Demonstrates how to access our Membersuite ConfigurationSettings
        /// For more information see https://help.production.membersuite.com/hc/en-us/articles/115001977806-Configuration-Settings
        /// </summary>
        private static void GetConfigurationSetting()
        {
            using (var api = ConciergeAPIProxyGenerator.GenerateProxy())
            {
                var result = api.GetConfigurationSetting("Portal", "EmailFromAddress");

                if (result.ResultValue != null)
                {
                    Console.WriteLine(result.ResultValue);
                }
            }
        }

        /// <summary>
        /// Demonstrates how to generate an Email using a preConfigured Email Template
        /// For more information, please see...
        /// https://help.production.membersuite.com/hc/en-us/articles/115000141106-How-to-Setup-a-Custom-Email-Template
        /// </summary>
        private static void SendEmailUsingConfiguredEmailTemplate()
        {
            using (var api = ConciergeAPIProxyGenerator.GenerateProxy())
            {
                var result = api.GetEmailTemplate("Welcome Email");

                if (result.Success)
                {
                    Console.WriteLine("Subject = {0}, HTML Message Body = {1}, Text-Only Message Body = {2}", result.ResultValue.Subject, result.ResultValue.HtmlBody, result.ResultValue.TextBody);

                    //Generate email

                    var emailList = new List<string>();
                    emailList.Add("00000000-0006-c936-0070-0b3cdf80adf1"); //ID represents the individual in this case. You'll need to input a real ID for this to work 

                    var emailResult = api.SendCustomizedEmail(result.ResultValue, emailList, null);
                }
            }
        }

        /// <summary>
        /// Demonstrates how to generate an email off of an in memory EmailTemplate
        /// </summary>
        private static void SendEmailUsingTemplate()
        {
            EmailTemplate eTemplate = new EmailTemplate()
            {
                FromName = "test@test.com",
                ReplyTo = "test@test.com",
                Subject = "Test Subject",
                TextBody = "This is a test email",
            };

            var emailList = new List<string>();
            emailList.Add("00000000-0006-c936-0070-0b3cdf80adf1"); //ID represents the individual in this case. You'll need to input a real ID for this to work 

            using (var api = ConciergeAPIProxyGenerator.GenerateProxy())
            {
                var eResult = api.SendCustomizedEmail(eTemplate, emailList, null);
            }
        }

        /// <summary>
        /// Demonstrates reading custom field(s) that are tied to a Built-In Membersuite object
        /// For more information, please see...
        /// https://help.production.membersuite.com/hc/en-us/articles/115002960103-System-Overview-Configuration-How-To-Create-a-Custom-Object
        /// https://help.production.membersuite.com/hc/en-us/articles/115002985646-System-Overview-Configuration-Reference-Create-a-Custom-Object-Field-Descriptions
        /// https://help.production.membersuite.com/hc/en-us/articles/115002985666-System-Overview-Configuration-Custom-Objects
        /// https://help.production.membersuite.com/hc/en-us/articles/115002985586-System-Overview-Configuration-How-To-Edit-a-Custom-Object
        /// https://help.production.membersuite.com/hc/en-us/articles/115002986266-System-Overview-Configuration-How-To-Delete-a-Custom-Object
        /// </summary>
        private static void GetCustomObjects()
        {
            using (var api = ConciergeAPIProxyGenerator.GenerateProxy())
            {
                var search = new Search { Type = "EventRegistration" };
                search.AddOutputColumn("Event.Name");
                search.AddOutputColumn("Name");
                search.AddOutputColumn("CustomObject1__c"); //this object derrives from the EventRegistration Object
                search.AddOutputColumn("CustomObject2__c"); //this object derrives from the EventRegistration Object
                search.AddOutputColumn("Individual.FirstName");
                search.AddOutputColumn("Individual.LastName");
                search.AddOutputColumn("Individual.EmailAddress");
                search.AddOutputColumn("Individual.CustomObject1__c"); //this object derrives from the Individual Object
                search.AddOutputColumn("Order.BalanceDue");

                var localID = 10009; //this represents the localID for the eventRegistration. You'll need to input a real ID for this to work
                search.AddCriteria(Expr.Equals("LocalID", localID));

                var result = api.ExecuteSearch(search, 0, 0);

                if (result.Success && result.ResultValue.Table.Rows.Count > 0)
                {
                    foreach (DataRow row in result.ResultValue.Table.Rows)
                    {
                        Console.WriteLine(string.Format("EventName = {0}, Name = {1}, EventReg CustomObject1__c = {2}, EventReg CustomObject2 = {3}, FirstName = {4}, LastName = {5}, EmailAddress = {6}, Individual CustomObject1 = {7}, BalanceDue = {8}",
                            row["Event.Name"], row["Name"], row["CustomObject1__c"], row["CustomObject2__c"], row["Individual.FirstName"], row["Individual.LastName"], row["Individual.EmailAddress"], row["Individual.CustomObject1__c"], row["Order.BalanceDue"]));
                    }
                }
            }
        }

        /// <summary>
        /// Demonstrates updating a Custom Object record and Inserting a new one
        /// For more information, please see...
        /// https://help.production.membersuite.com/hc/en-us/articles/115002960103-System-Overview-Configuration-How-To-Create-a-Custom-Object
        /// https://help.production.membersuite.com/hc/en-us/articles/115002985646-System-Overview-Configuration-Reference-Create-a-Custom-Object-Field-Descriptions
        /// https://help.production.membersuite.com/hc/en-us/articles/115002985666-System-Overview-Configuration-Custom-Objects
        /// https://help.production.membersuite.com/hc/en-us/articles/115002985586-System-Overview-Configuration-How-To-Edit-a-Custom-Object
        /// https://help.production.membersuite.com/hc/en-us/articles/115002986266-System-Overview-Configuration-How-To-Delete-a-Custom-Object
        /// </summary>
        private static void UpdateCustomObjects()
        {
            using(var api = ConciergeAPIProxyGenerator.GenerateProxy())
            {
                var cusObjSearch = new Search("FakeCustomObject__c"); //You'll need to create your own objects for this to work
                cusObjSearch.AddCriteria(Expr.Equals("Owner.LocalID", 101292)); //In this example, the FakeCustomObject derrives from the Individual Object. So we are using the Owner.Local to query the Individual's LocalID and get back a specific FakeCustomObject record
                var cusObjResult = api.GetObjectBySearch(cusObjSearch, null);

                if (cusObjResult.ResultValue != null) //Update existing record
                {
                    var mso = new MemberSuiteObject();
                    mso.Fields["CompanyName__c"] = "TestCompany4";
                    mso.Fields["Description__c"] = "This is a test";
                    mso.Fields["Website__c"] = "www.test.com";
                    mso.Fields["Name"] = "test Trans";
                    mso.Fields["Owner"] = cusObjResult.ResultValue;

                    var saveResult = api.Save(mso);
                }
                else //Insert new record
                {
                    var meta = api.DescribeObject("FakeCustomObject__c").ResultValue;
                    var instance = MemberSuiteObject.FromClassMetadata(meta).ConvertTo<msCustomObjectInstance>();

                    instance.Owner = cusObjResult.ResultValue.Fields["ID"].ToString();
                    instance.Fields["CompanyName__c"] = "TestCompany4";
                    instance.Fields["Description__c"] = "This is a test";
                    instance.Fields["Website__c"] = "www.test.com";
                    instance.Fields["Name"] = "test Trans";

                    var insertResult = api.Save(instance);
                }
            }
        }
    }
}
