using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MemberSuite.SDK.Concierge;
using MemberSuite.SDK.Results;
using MemberSuite.SDK.Utilities;

public partial class _Default : System.Web.UI.Page
{
    /// <summary>
    /// The ID of the Signing Certificate as defined in the web.config
    /// </summary>
    protected string SigningCertificateId
    {
        get { return ConfigurationManager.AppSettings["SigningCertificateId"]; }
    }

    /// <summary>
    /// Write only property to store the Portal Security Token in the web user's session
    /// </summary>
    protected byte[] PortalLoginToken
    {
        set { Session["PortalLoginToken"] = value; }
    }

    protected bool VerifyCredentialsAgainstMembersuite
    {
        get
        {
            //bool result;

            //if (!bool.TryParse(ConfigurationManager.AppSettings["VerifyCredentialsAgainstMembersuite"], out result))
            //    return false;
            
            return !cbVerifyCrentials.Checked ;
        }
    }

    protected void btnLogin_Click(object sender, EventArgs e)
    {
        lblError.Visible = false;

        //Verify the user's rights to login.  In this sample, this is done using the MemberSuite API
        //If MemberSuite is not the authority storing your credentials, you can verify this using any method you choose
        //For instance, your own database, ASP.NET Membership, digital certificates, or any other method.
        //The actual single sign on only requires the username and expects you have already verified the user's rights to log in.
        if(!VerifyPortalUsersRights())
        {
            lblError.Visible = true;
            return;
        }

        //Get the supplied portal username
        string portalUserName = tbUserName.Text;

        //Generate a Digital Signature of the portal username using the Signing Certificate
        byte[] signature = GenerateDigitalSignature(portalUserName);

        using(IConciergeAPIService proxy = ConciergeAPIProxyGenerator.GenerateProxy())
        {
            //Request a portal security token specifying the portal username, ID of the signing certificate, and digital signature
            ConciergeResult<byte[]> portalLoginTokenResult = proxy.CreatePortalSecurityToken(portalUserName,
                                                                                             SigningCertificateId,
                                                                                             signature);

            //If the Concierge API indicated a fault, write a friendly error message
            if(!portalLoginTokenResult.Success)
            {
                lblError.Text = string.IsNullOrWhiteSpace(portalLoginTokenResult.FirstErrorMessage) ? "API operation failed" : portalLoginTokenResult.FirstErrorMessage;
                lblError.Visible = true;
                return;
            }

            //Set the portal security token in the web user's session so it can be used by the RedirectToPortal page
            PortalLoginToken = portalLoginTokenResult.ResultValue;
        }

        //Send the web user to the page that will construct a form and POST the portal security token to the portal's Login.aspx page
        Response.Redirect("~/RedirectToPortal.aspx");
    }

    byte[] GenerateDigitalSignature(string dataToSign)
    {
        //Get the path to the Signing Certificate key in the bin directory
        string keyFilePath = Server.MapPath("~/bin/signingcertificate.xml");
        if (string.IsNullOrWhiteSpace(keyFilePath) || !File.Exists(keyFilePath))
            throw new ApplicationException("Unable to locate Signing Certificate file in the bin directory");

        //Use the CryptoManager helper class to sign the data.  This will create a new RSACryptoServiceProvider and will load the key from the xml string
        //it will then use the RSACryptoServiceProvider to generate a digital signature
        byte[] result = CryptoManager.Sign(Encoding.ASCII.GetBytes(dataToSign), keyFilePath);
        return result;
    }

    bool VerifyPortalUsersRights()
    {
        //If this sample is not configured to verify credentials (username AND password) against Membersuite
        //then the user is automatically authorized and the SSO can occur with just the username
        if (!VerifyCredentialsAgainstMembersuite)
            return true;

        ConciergeResult<LoginResult> result;

        using(IConciergeAPIService proxy = ConciergeAPIProxyGenerator.GenerateProxy())
        {
            result = proxy.LoginToPortal(tbUserName.Text, tbPassword.Text);
        }

        if (result.Success)
            return true;

        lblError.Text = result.FirstErrorMessage;
        return false;
    }
}
