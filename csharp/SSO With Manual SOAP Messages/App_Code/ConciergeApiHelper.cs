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

    #region Concierge API SOAP Methods

    /// <summary>
    /// Helper method to create a SOAP envelope containing a request to CreatePortalSecurityToken and POST it to the Concierge API
    /// </summary>
    /// <param name="portalUserName"></param>
    /// <returns></returns>
    public static HttpWebResponse PostPortalSecurityTokenRequest(string portalUserName)
    {
        HttpWebRequest request = CreatePortalSecurityTokenWebRequest();
        XDocument soapMessage = CreatePortalSecurityTokenRequest(portalUserName);

        Stream stream = request.GetRequestStream();
        soapMessage.Save(stream);
        stream.Close();

        return (HttpWebResponse)request.GetResponse();
    }

    /// <summary>
    /// Helper method to create a web request to POST a request to the CreatePortalSecurityToken method of the Concierge API
    /// </summary>
    /// <returns></returns>
    private static HttpWebRequest CreatePortalSecurityTokenWebRequest()
    {
        HttpWebRequest result = (HttpWebRequest)WebRequest.Create((string) ConciergeUri);

        result.Headers.Add("SOAPAction",
                           "http://membersuite.com/contracts/IConciergeAPIService/CreatePortalSecurityToken");
        result.ContentType = "text/xml;charset=\"utf-8\"";
        result.Accept = "text/xml";
        result.Method = "POST";

        return result;
    }

    /// <summary>
    /// Helper method to generate a SOAP envelope from the CreatePortalSecurityTokenRequest.xml template
    /// and populate it with required Concierge API configuration values (Access Key ID, Association ID, Session ID Signature)
    /// </summary>
    /// <param name="portalUserName"></param>
    /// <returns></returns>
    private static XDocument CreatePortalSecurityTokenRequest(string portalUserName)
    {
        //Get the path to the Signing Certificate key in the bin directory
        string requestTemplateFilePath = HttpContext.Current.Server.MapPath("~/bin/CreatePortalSecurityTokenRequest.xml");
        if (String.IsNullOrWhiteSpace(requestTemplateFilePath) || !File.Exists(requestTemplateFilePath))
            throw new ApplicationException("Unable to locate CreatePortalSecurityTokenRequest template file in the bin directory");

        //Load the template into an XDocument to make it easier to navigate with XPath
        XDocument result = XDocument.Load(requestTemplateFilePath);
        
        //Get a table of namespaces and aliases to use in XPath queries
        XmlNamespaceManager namespaceManager = CreateNamespaceManager();

        //Get the action of the message.  We will need this value to create a signature using the Secret Access Key
        string action = result.XPathSelectElement("//Action", namespaceManager).Value;

        //Set the Concierge API Uri
        result.XPathSelectElement("//To", namespaceManager).Value = ConciergeUri;

        //Set the SOAP Header values
        result.XPathSelectElement("//ms:AccessKeyId", namespaceManager).Value = AccessKeyId;
        result.XPathSelectElement("//ms:AssociationId", namespaceManager).Value = AssociationId;

        //If the current web user has a Concierge API session add the ID
        if (!String.IsNullOrWhiteSpace(ConciergeSessionId))
            result.XPathSelectElement("//ms:SessionId", namespaceManager).Value = ConciergeSessionId;


        //Sign the message
        result.XPathSelectElement("//ms:Signature", namespaceManager).Value = GenerateMessageSignature(SecretAccessKey,
                                                                                                       action, ConciergeSessionId,
                                                                                                       AssociationId);

        //Set the Portal Security Token request values
        result.XPathSelectElement("//mc:portalUserName", namespaceManager).Value = portalUserName;
        result.XPathSelectElement("//mc:signingCertificateId", namespaceManager).Value = SigningCertificateId;

        //Sign the Portal Security Token request using our Signing Certificate ID
        result.XPathSelectElement("//mc:signature", namespaceManager).Value =
            Convert.ToBase64String(GenerateDigitalSignature(portalUserName));

        return result;
    }

    /// <summary>
    /// Helper method to create a table of namespaces and aliases
    /// </summary>
    /// <returns></returns>
    public static XmlNamespaceManager CreateNamespaceManager()
    {
        XmlNamespaceManager result = new XmlNamespaceManager(new NameTable());
        result.AddNamespace("s", "http://schemas.xmlsoap.org/soap/envelope/");
        result.AddNamespace("a", "http://schemas.microsoft.com/ws/2005/05/addressing/none");
        result.AddNamespace("ms", "http://membersuite.com/schemas");
        result.AddNamespace("mc", "http://membersuite.com/contracts");
        result.AddNamespace("r", "http://schemas.datacontract.org/2004/07/MemberSuite.SDK.Results");
        result.AddNamespace("c", "http://schemas.datacontract.org/2004/07/MemberSuite.SDK.Concierge");

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

    private static byte[] GenerateDigitalSignature(string dataToSign)
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