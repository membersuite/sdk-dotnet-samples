using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using MemberSuite.SDK;
using MemberSuite.SDK.Concierge;
using MemberSuite.SDK.Types;

namespace API_Usage_Examples
{
    public class CreatingARecord : ConciergeSampleBase 
    {
        public override void Run()
        {
            /* This sample is going to login to the association and create a record with a name, 
             * email, and birthday that you specify. It will also add two phone numbers and one
             * address using the flattened fields created for each AddressType/PhoneNumberType.
             * Once saved, the system will display the ID of the individual created, 
             * the age (which MemberSuite calculates), and will automatically
             * launch the 360 view of that record in the web browser */

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

            // now, we want to create a new individual
            // First, we need to get a description of the individual
            // The description will tell the client SDK how to "build" the object; in other words, what the fields are
            // and what the default values are. So if you set a default value for LastName to 'Smith', the Describe
            // operation will have that in the metadata. When you construct the object, it will then have the last name
            // defaulted to 'Smith'
            ClassMetadata meta = api.DescribeObject("Individual").ResultValue;

            // now, create our MemberSuiteObject
            MemberSuiteObject mso = MemberSuiteObject.FromClassMetadata(meta);

            /* it's always easier to use the typed MemberSuiteObject
               
             * You could just instantiate this directly by saying:
             * 
             *      msIndividual p = new msIndividual();
             *      
             * This would work - but without the class metadata, the object would not respect any of the defaults
             * that are set up. It would be a totally blank object.*/
            msIndividual person = mso.ConvertTo<msIndividual>();

            // now, you don't have to use dictionaries and string keys, you can access the object directly

            Console.WriteLine("Please enter a first name and press ENTER.");
            person.FirstName = Console.ReadLine();

            Console.WriteLine("Please enter a last name and press ENTER.");
            person.LastName = Console.ReadLine();
            
            person.EmailAddress = "apitest@membersuite.com";

            Console.WriteLine("Please enter a birthdate and press ENTER.");
            string date = Console.ReadLine();
            DateTime dob;
            if (!DateTime.TryParse(date, out dob))
            {
                Console.WriteLine("Invalid birthdate... exiting...");
                return;
            }
            person.DateOfBirth = dob;

            //Now add a phone number of the type "Home" and "Work" using the flattened phone number fields
            //that are dynamically created for each defined PhoneNumberType
            person["Work_PhoneNumber"] = "555-555-5555";
            person["Home_PhoneNumber"] = "555-555-1234";

            //And set the preferred phone number type to be the one with the type "Home"
            person.PreferredPhoneNumberType = getIdFromCode(api, "PhoneNumberType", "Home");

            //Now add an address of the type "Home" using the flattened address field
            //that is dynamically created for each defined AddressType
            Address a = new Address();
            a.Line1 = "1060 W Addison St";
            a.City = "Chicago";
            a.State = "IL";
            a.PostalCode = "60613";
            a.Country = "USA";

            person["Home_Address"] = a;

            //Set the preferred address type 
            //Since there's only one it would already be preferred but this is how to set it if there are multiple
            person.PreferredAddressType = getIdFromCode(api, "AddressType", "Home");

            // ok, so we can save this now
            var result = api.Save(person);

            if (!result.Success)
                throw new ApplicationException("Unable to save: " + result.FirstErrorMessage);

            msIndividual newPerson = result.ResultValue.ConvertTo<msIndividual>();

            Console.WriteLine("Successfully saved individual #{0} - {1}. Age is {2}. ", newPerson.LocalID, newPerson.Name, newPerson.Age );
            Console.WriteLine("URL is:");
            Console.WriteLine("https://console.production.membersuite.com/app/console/individual/view?c=" + newPerson.ID); 

             

        }

        private string getIdFromCode (IConciergeAPIService api, string type, string code)
        {
            //Query for the ID of a record of the given type matching the given code
            string msql = string.Format("SELECT ID FROM {0} WHERE Code = '{1}'", type, code);
            var searchResult = api.ExecuteMSQL(msql, 0, 1);
            string result = searchResult.ResultValue.SearchResult.Table.Rows[0]["ID"].ToString();
            
            return result;
        }
    }
}
