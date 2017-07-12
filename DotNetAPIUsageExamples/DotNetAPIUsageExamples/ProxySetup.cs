using MemberSuite.SDK.Concierge;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetAPIUsageExamples
{
    public class ProxySetup
    {
        /// <summary>
        /// You will need to change the creditials in this method call to match the ones created for your organization. See below links for more information
        /// https://help.production.membersuite.com/hc/en-us/articles/115002511483-How-To-Locate-Your-Association-ID
        /// https://help.production.membersuite.com/hc/en-us/articles/115002511423-How-To-Create-a-New-Access-Key
        /// </summary>
        public static void SetupProxy()
        {
            if (!ConciergeAPIProxyGenerator.IsSecretAccessKeySet)
            {
                ConciergeAPIProxyGenerator.SetAccessKeyId("");
                ConciergeAPIProxyGenerator.SetSecretAccessKey("");
            }

            if (string.IsNullOrWhiteSpace(ConciergeAPIProxyGenerator.AssociationId))
            {
                ConciergeAPIProxyGenerator.AssociationId = "";
            }
        }

        /// <summary>
        /// Alternate method
        /// </summary>
        //public static void SetUpProxy()
        //{
        //    if (!ConciergeAPIProxyGenerator.IsSecretAccessKeySet)
        //    {
        //        ConciergeAPIProxyGenerator.SetAccessKeyId(ConfigurationManager.AppSettings["AccessKeyID"]);
        //        ConciergeAPIProxyGenerator.SetSecretAccessKey(ConfigurationManager.AppSettings["SecretAccessKey"]);
        //    }

        //    if (string.IsNullOrWhiteSpace(ConciergeAPIProxyGenerator.AssociationId) && !string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["AssociationId"]))
        //    {
        //        ConciergeAPIProxyGenerator.AssociationId = ConfigurationManager.AppSettings["AssociationId"];
        //    }
        //}
    }
}
