using System;
using System.Configuration;
using System.IO;
using MemberSuite.SDK.Concierge;
using MemberSuite.SDK.Results;
using MemberSuite.SDK.Types;
using MemberSuite.SDK.Utilities;

public partial class ReceiveSSOToken : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        ResultsMessage.Text = string.Format("Url to jump to: {0}", Request.Form["NextUrl"]);
        ResultsMessage.Text += string.Format("<br/><br/>Referrer Url: {0}", Request.Form["ReturnUrl"]);

        loginWithToken();
    }

    private void loginWithToken()
    {
        string tokenString = Request.Form["Token"];
        if (string.IsNullOrWhiteSpace(tokenString))
            return;

        byte[] token = Convert.FromBase64String(tokenString);

        // Sign the data using the portal's private key.  Concierge API will use this to authenticate the 
        // originator of the request
        byte[] portalTokenSignature = signToken(token);

        ConciergeResult<LoginResult> result;

        using (IConciergeAPIService proxy = ConciergeAPIProxyGenerator.GenerateProxy())
        {
            result = proxy.LoginWithToken(token, ConfigurationManager.AppSettings["SigningCertificateId"], portalTokenSignature);
        }

        if (result == null || !result.Success)
        {
            ResultsMessage.Text += string.Format("<br/><br/>Error: {0}", result == null ? "No Results" : result.FirstErrorMessage);
            return;
        }

        setSession(result.ResultValue);
    }

    private byte[] signToken(byte[] data)
    {
        string keyFilePath = Server.MapPath("~/bin/signingcertificate.xml");
        if (!File.Exists(keyFilePath))
        {
            throw new Exception("Invalid portal configuration. The signingcertificate.xml file must be present.");
        }

        try
        {
            return CryptoManager.Sign(data, keyFilePath);
        }
        catch (Exception ex)
        {
            throw new Exception("Unable to sign token with portal certificate.");
        }
    }

    private void setSession(LoginResult result)
    {
        // Cache the just opened SessionID so we can continue to use for future API calls.
        // Note: This assumes that the site has implemented the sample IConceirgeAPISessionIdProvider from:
        //   https://github.com/membersuite/sdk-dotnet-samples/tree/master/csharp/SSO%20With%20SDK
        ConciergeAPIProxyGenerator.SessionID = result.SessionID;

        // The following are the initial data constructs retrieved with the successful login. If you need more
        //  than just these, you can retrieve from the API.
        var currentUser = result.PortalUser.ConvertTo<msPortalUser>();
        var currentEntity = result.PortalEntity.ConvertTo<msEntity>();
        var currentAssociation = result.Association.ConvertTo<msAssociation>();

        /*
            Additional session creation operations for the target site(s) should be done here using the data above.
        */
        ResultsMessage.Text += string.Format("<br/><br/>Successfully validated login request from {0}.", currentUser.Name);
    }

}