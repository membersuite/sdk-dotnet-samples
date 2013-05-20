using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mime;
using System.Text;
using MemberSuite.SDK.Concierge;
using MemberSuite.SDK.Types;

namespace API_Usage_Examples
{
    public class UsingTheErrorHandler : ConciergeSampleBase 
    {
        public override void Run()
        {
            /* The point of this sample is to demonstrate the use of a handy SDK feature - the error handler.
             * This handler will automatically throw an exception, to be caught by a method of your choice, any time
             * a request is unsuccessful. This keeps you from having to write code like:
             
            if (!result.Success)
            {
                Console.WriteLine("Operation failed: {0}", result.FirstErrorMessage);
                return;
            }
              
             * ...after every request. We use this extensively in our API-consuming code to simply our code base; executing
             * code can always assume an API request was successful.*/

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
            ConciergeClientExtensions.OnResultError += new EventHandler<ConciergeResultErrorArgs>(ConciergeClientExtensions_OnResultError);
            ConciergeClientExtensions.SessionExpired += new EventHandler(ConciergeClientExtensions_SessionExpired);

            // now, let's intentionally cause an error by trying to save an empty individual
            msIndividual indiv = new msIndividual();
            api.Save(indiv);

            Console.WriteLine("This line will never be reached, as an exception has occurred in the last request.");

        }

        void ConciergeClientExtensions_SessionExpired(object sender, EventArgs e)
        {
            throw new ApplicationException("The session has expired an needs to be recreated");
            
        }

        void ConciergeClientExtensions_OnResultError(object sender, ConciergeResultErrorArgs e)
        {
            throw new ApplicationException("The following error has occurred: " + e.Message);
        }
    }
}
