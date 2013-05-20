using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using ConciergeAPI;

/// <summary>
/// Summary description for ConciergeAPIHelper
/// </summary>
public static class ConciergeApiHelper
{
    #region Properties

    /// <summary>
    /// Concierge API Access Key ID as defined in the web.config (see Portal Single Sign On.docx for more details)
    /// </summary>
    public static string AccessKeyId
    {
        get { return ConfigurationManager.AppSettings["AccessKeyId"]; }
    }

    /// <summary>
    /// Concierge API Secret Access Key as defined in the web.config (see Portal Single Sign On.docx for more details)
    /// </summary>
    public static string SecretAccessKey
    {
        get { return ConfigurationManager.AppSettings["SecretAccessKey"]; }
    }

    /// <summary>
    /// Association ID as defined in the web.config (see Portal Single Sign On.docx for more details)
    /// </summary>
    public static string AssociationId
    {
        get { return ConfigurationManager.AppSettings["AssociationId"]; }
    }

    /// <summary>
    /// Signing Certificate ID as defined in the web.config (see Portal Single Sign On.docx for more details)
    /// </summary>
    public static string SigningCertificateId
    {
        get { return ConfigurationManager.AppSettings["SigningCertificateId"]; }
    }

    /// <summary>
    /// Portal URL as defined in the web.config 
    /// </summary>
    public static string PortalUrl
    {
        get { return ConfigurationManager.AppSettings["PortalUrl"]; }
    }

    /// <summary>
    /// URI of the Concierge API as defined in the web.config 
    /// </summary>
    public static string ConciergeUri
    {
        get { return ConfigurationManager.AppSettings["Default_ConciergeUri"]; }
    }

    /// <summary>
    /// Web user's unique Session ID with the Concierge API
    /// </summary>
    public static string ConciergeSessionId
    {
        get { return HttpContext.Current.Session["ConciergeSessionId"] as string; }
        set { HttpContext.Current.Session["ConciergeSessionId"] = value; }
    }

    /// <summary>
    /// Property to set/retrieve a portal security token from the web user's session
    /// </summary>
    public static byte[] PortalLoginToken
    {
        get { return HttpContext.Current.Session["PortalLoginToken"] as byte[]; }
        set { HttpContext.Current.Session["PortalLoginToken"] = value; }
    }

    #endregion

    #region Concierge API Helper Methods

    /// <summary>
    /// Helper method to create the message header required for each Concierge API method call and set required values
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public static ConciergeRequestHeader CreateRequestHeader(string action)
    {
        return CreateRequestHeader(AccessKeyId, SecretAccessKey, action, ConciergeSessionId, AssociationId);
    }

    /// <summary>
    /// Helper method to create the message header required for each Concierge API method call and set required values
    /// </summary>
    /// <param name="accessKeyId"></param>
    /// <param name="secretAccessKey"></param>
    /// <param name="action"></param>
    /// <param name="sessionId"></param>
    /// <param name="associationId"></param>
    /// <returns></returns>
    public static ConciergeRequestHeader CreateRequestHeader(string accessKeyId, string secretAccessKey, string action, string sessionId, string associationId)
    {
        ConciergeRequestHeader result = new ConciergeRequestHeader
        {
            AccessKeyId = accessKeyId,
            Signature =
                GenerateMessageSignature(secretAccessKey, action, sessionId, associationId),
            SessionId = sessionId,
            AssociationId = associationId
        };
        return result;
    }

    #endregion

    #region Digital Signatures

    /// <summary>
    /// Helper method to create a unique message signature for a Concierge API message using your Secret Access Key
    /// </summary>
    /// <param name="secretAccessKey"></param>
    /// <param name="action"></param>
    /// <param name="sessionId"></param>
    /// <param name="associationId"></param>
    /// <returns></returns>
    private static string GenerateMessageSignature(string secretAccessKey, string action, string sessionId,
                                                   string associationId)
    {
        //Construct the string to sign for this message.  It is always the message action + association ID (if any) + session ID (if any)
        string dataToSign = action;

        if (!String.IsNullOrEmpty(associationId))
            dataToSign += associationId;

        if (!String.IsNullOrEmpty(sessionId))
            dataToSign += sessionId;

        //Create the object that can generate the message signature and set the key to your Secret Access Key
        HMACSHA1 signer = new HMACSHA1(Convert.FromBase64String(secretAccessKey));

        //Calculate the signature for this message and return it Base64 encoded
        byte[] signature = signer.ComputeHash(Encoding.ASCII.GetBytes(dataToSign));
        string result = Convert.ToBase64String(signature);

        return result;
    }

    public static byte[] GenerateDigitalSignature(string dataToSign)
    {
        //Get the path to the Signing Certificate key in the bin directory
        string keyFilePath = HttpContext.Current.Server.MapPath("~/bin/signingcertificate.xml");
        if (String.IsNullOrWhiteSpace(keyFilePath) || !File.Exists(keyFilePath))
            throw new ApplicationException("Unable to locate Signing Certificate file in the bin directory");

        //This is an XML file so use Unicode encoding
        string xmlKeyString = File.ReadAllText(keyFilePath, Encoding.Unicode);

        //Create a new signer using RSA and a SHA1 hash using the embedded key
        RSACryptoServiceProvider signer = new RSACryptoServiceProvider();
        signer.FromXmlString(xmlKeyString);
        return signer.SignData(Encoding.ASCII.GetBytes(dataToSign), new SHA1CryptoServiceProvider());
    }

    #endregion
}