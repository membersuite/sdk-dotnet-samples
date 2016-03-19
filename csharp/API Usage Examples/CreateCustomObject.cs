using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MemberSuite.SDK.Concierge;
using MemberSuite.SDK.Searching;
using MemberSuite.SDK.Searching.Operations;
using MemberSuite.SDK.Types;

namespace API_Usage_Examples
{
    class CreateCustomObject : ConciergeSampleBase
    {
        private const string CustomActivityName = "CustomActivity__c";

        private const string CategoryFieldName = "Category__c";

        public override void Run()
        {
            /* This sample is going to login to the association and create a record with a name, 
             * email, and birthday that you specify. Once saved, the system will display the ID 
             * of the individual created, the age (which MemberSuite calculates), and will automatically
             * launch the 360 view of that record in the web browser */

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
                // now, we want to create a new individual
                // First, we need to get a description of the individual
                // The description will tell the client SDK how to "build" the object; in other words, what the fields are
                // and what the default values are. So if you set a default value for LastName to 'Smith', the Describe
                // operation will have that in the metadata. When you construct the object, it will then have the last name
                // defaulted to 'Smith'
                var meta = api.DescribeObject(CustomActivityName).ResultValue;

                if (meta == null)
                {
                    var classDef = new MemberSuiteObject {ClassType = msCustomObject.CLASS_NAME};
                    classDef.Fields[msCustomObject.FIELDS.ClassDefinition] = new ClassMetadata
                    {
                        Name = CustomActivityName,
                        Label = "Custom Activity",
                        LabelPlural = "Custom Activities",
                        Module = "CRM",
                    };

                    // ok, so we can save this now
                    var defResult = api.Save(classDef);

                    if (!defResult.Success)
                        throw new ApplicationException("Unable to create class definition: " + defResult.FirstErrorMessage);

                    meta = api.DescribeObject(CustomActivityName).ResultValue;

                    if (meta == null)
                        throw new ApplicationException("Class definition not created.");
                }

                if (meta.Fields.All(f => f.Name != CategoryFieldName))
                {
                    var fieldDef = new MemberSuiteObject { ClassType = msCustomField.CLASS_NAME };
                    fieldDef.Fields[msCustomField.FIELDS.CustomObject] = meta.CustomObjectID;
                    fieldDef.Fields[msCustomField.FIELDS.ApplicableType] = CustomActivityName;
                    fieldDef.Fields[msCustomField.FIELDS.FieldDefinition] = new FieldMetadata
                    {
                        Name = CategoryFieldName,
                        Label = "Category",
                        DataType = FieldDataType.Text,
                        DisplayType = FieldDisplayType.TextBox,
                    };

                    // ok, so we can save this now
                    var defResult = api.Save(fieldDef);

                    if (!defResult.Success)
                        throw new ApplicationException("Unable to create field definition: " + defResult.FirstErrorMessage);

                    meta = api.DescribeObject(CustomActivityName).ResultValue;

                    if (meta == null || meta.Fields.All(f => f.Name != CategoryFieldName))
                        throw new ApplicationException("Field definition not created.");
                }

                Console.WriteLine("Please enter a Custom Activity Name and press ENTER.");
                var activityName = Console.ReadLine();

                // Search for an activity by Name
                var nameSearch = new Search {Type = CustomActivityName};
                nameSearch.AddCriteria(Expr.Equals("Name", activityName));
                var searchResult = api.GetObjectBySearch(nameSearch, null);
                if (!searchResult.Success)
                    throw new ApplicationException("Unable to search: " + searchResult.FirstErrorMessage);

                MemberSuiteObject mso;
                if (searchResult.ResultValue != null)
                {
                    mso = searchResult.ResultValue;
                    Console.WriteLine("Match found, let's update it. Cateogry currently: {0}",
                        mso.SafeGetValue<string>(CategoryFieldName));
                }
                else
                {
                    Console.WriteLine("No match found, let's create it.");

                    mso = new MemberSuiteObject {ClassType = CustomActivityName};

                    // You can also generate an Object from the full Metadata:  
                    ////mso = MemberSuiteObject.FromClassMetadata(meta);

                    mso.Fields["Name"] = activityName;
                }

                Console.WriteLine("Please enter a Category for this Activity and press ENTER.");
                mso.Fields[CategoryFieldName] = Console.ReadLine();

                // ok, so we can save this now
                var saveResult = api.Save(mso);

                if (!saveResult.Success)
                    throw new ApplicationException("Unable to save: " + saveResult.FirstErrorMessage);

                var savedObject = saveResult.ResultValue;

                Console.WriteLine("Successfully saved Custom Activity #{0} - {1}.",
                    savedObject.SafeGetValue<string>("ID"),
                    savedObject.SafeGetValue<string>("Name"));

            }
        }
    }
}
