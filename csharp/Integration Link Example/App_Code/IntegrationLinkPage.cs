using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using MemberSuite.SDK.Concierge;
using MemberSuite.SDK.Results;
using MemberSuite.SDK.Types;
using MemberSuite.SDK.Utilities;

/// <summary>
/// This is designed to be derived from by web pages that intend to function as integration links for the
/// MemberSuite console.
/// </summary>
public class IntegrationLinkPage : Page
{
    /// <summary>
    /// Gets the conciege API proxy.
    /// </summary>
    /// <returns></returns>
    /// <remarks>We</remarks>
    public IConciergeAPIService GetConciegeAPIProxy()
    {
        return ConciergeAPIProxyGenerator.GenerateProxy();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        loginToConsole();
    }

    /// <summary>
    /// Uses the session ID link to log back into the console
    /// </summary>
    private void loginToConsole()
    {
        // ok - are we already logged in?
        if (CurrentUser != null)
            return;

        if (!Request.Form.AllKeys.Contains("Token"))
            return;

        string tokenString = Request.Form["Token"];
        if (string.IsNullOrWhiteSpace(tokenString))
            return;

        byte[] token = Convert.FromBase64String(tokenString);

        //Sign the data using the Integration Link's private key.  Concierge API will use this to authenticate the originator of the request
        byte[] integrationLinkTokenSignature = Sign(token);

        ConciergeResult<LoginResult> result;

        // ok, login
        using (IConciergeAPIService proxy = GetConciegeAPIProxy())
        {
            result = proxy.LoginWithToken(token,
                                          ConfigurationManager.AppSettings["SigningCertificateId"],
                                          integrationLinkTokenSignature);
        }

        if (result == null || !result.Success)
            return;

        CurrentUser = result.ResultValue.User.ConvertTo<msUser>();
        CurrentAssociation = result.ResultValue.Association.ConvertTo<msAssociation>();
        ConciergeAPIProxyGenerator.SessionID = result.ResultValue.SessionID;
    }

    private byte[] Sign(byte[] data)
    {
        //If this portal does not have a portal specific RSA key attempt to use the default certificate (this only works for servers on the MemberSuite network)
        string keyFilePath = Server.MapPath("signingcertificate.xml");
        if (!File.Exists(keyFilePath))
        {
            if (string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["SigningCertificateSubject"]))
                Response.Redirect(
                    "/SystemUnavailable.html?AdditionalInfo=Invalid configuration. Either a signingcertificate.xml file must be present or a SigningCertificateSubject must be defined in the web.config");
            try
            {
                return CryptoManager.Sign(data, ConfigurationManager.AppSettings["SigningCertificateSubject"],
                                          ConfigurationManager.AppSettings["CertificatesStoreName"]);
            }
            catch (Exception ex)
            {
                Response.Redirect(
                    "/SystemUnavailable.html?AdditionalInfo=Unable to sign token with certificate defined in web.config");
                return null;
            }
        }

        try
        {
            return CryptoManager.Sign(data, keyFilePath);
        }
        catch
        {
            Response.Redirect(
                    "/SystemUnavailable.html?AdditionalInfo=Unable to sign token with the key in signingcertificate.xml file");
            return null;
        }
    }

    public static msUser CurrentUser
    {
        get { return (msUser)HttpContext.Current.Session["IL:ConciergeAPICurrentUser"]; }
        set { HttpContext.Current.Session["IL:ConciergeAPICurrentUser"] = value; }
    }


    public static msAssociation CurrentAssociation
    {
        get { return (msAssociation)HttpContext.Current.Session["IL:ConciergeAPICurrentAssociation"]; }
        set { HttpContext.Current.Session["IL:ConciergeAPICurrentAssociation"] = value; }
    }

}