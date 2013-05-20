using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ConciergeAPI;


public partial class _Default : System.Web.UI.Page
{
    protected void btnLogin_Click(object sender, EventArgs e)
    {
        lblError.Visible = false;

        //Get the supplied portal username
        string portalUserName = tbUserName.Text;

        //Generate a Digital Signature of the portal username using the Signing Certificate
        byte[] signature = ConciergeApiHelper.GenerateDigitalSignature(portalUserName);

        using (ConciergeAPIService proxy = new ConciergeAPIService())
        {
            //Generate a header for this request using the Concierge API settings and the action to be called
            proxy.ConciergeRequestHeaderValue = ConciergeApiHelper.CreateRequestHeader("http://membersuite.com/contracts/IConciergeAPIService/CreatePortalSecurityToken");

            //Request a portal security token from the Concierge API
            var portalLoginTokenResult = proxy.CreatePortalSecurityToken(portalUserName, ConciergeApiHelper.SigningCertificateId, signature);

            //If the Concierge API indicated a fault, write a friendly error message
            if (!portalLoginTokenResult.Success)
            {
                lblError.Text = portalLoginTokenResult.Errors != null && portalLoginTokenResult.Errors.Any() ? portalLoginTokenResult.Errors[0].Message : "API operation failed";
                lblError.Visible = true;
                return;
            }

            //Set the web user's session id in their session
            ConciergeApiHelper.ConciergeSessionId = proxy.ConciergeResponseHeaderValue.SessionId;
            
            //Set the portal security token in the web user's session so it can be used by the RedirectToPortal page
            ConciergeApiHelper.PortalLoginToken = portalLoginTokenResult.ResultValue;
        }

        //Send the web user to the page that will construct a form and POST the portal security token to the portal's Login.aspx page
        Response.Redirect("~/RedirectToPortal.aspx");
    }
}
