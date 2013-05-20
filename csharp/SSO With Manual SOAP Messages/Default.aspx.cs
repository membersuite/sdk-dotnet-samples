using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;


public partial class _Default : System.Web.UI.Page
{
    protected void btnLogin_Click(object sender, EventArgs e)
    {
        lblError.Visible = false;

        //Get the supplied portal username
        string portalUserName = tbUserName.Text;

        //Create a SOAP envelope to request a portal security token, POST it to the Concierge API and get the response
        using (HttpWebResponse response = ConciergeApiHelper.PostPortalSecurityTokenRequest(portalUserName))
        {
            //Get a stream containing the response SOAP envelope
            using (Stream receiveStream = response.GetResponseStream())
            {
                //If there was no response show a friendly error message - should not happen
                if (receiveStream == null)
                {
                    lblError.Text = "Unable to read response from Concierge API";
                    lblError.Visible = true;
                    return;
                }

                //Read the SOAP response in to an XDocument to make it easier to navigate
                Encoding encode = Encoding.GetEncoding("utf-8");
                StreamReader responseStream = new StreamReader(receiveStream, encode);
                XDocument soapResponse = XDocument.Load(responseStream);
                
                //Get a table containing the namespaces and aliases we will use in our XPath to retreive values from the response
                XmlNamespaceManager namespaceManager = ConciergeApiHelper.CreateNamespaceManager();

                //Use XPath to check if the Concierge API indicates the request was processed successfully.  If it was not, display a friendly error massage
                if (soapResponse.XPathSelectElement("//mc:CreatePortalSecurityTokenResult/r:Success", namespaceManager).Value !=
                    "true")
                {
                    lblError.Text = "Portal token generation failed.";
                    lblError.Visible = true;

                    XElement firstError =
                        soapResponse.XPathSelectElement("//mc:CreatePortalSecurityTokenResult/r:Errors/c:ConciergeError[1]/c:Message",
                                                        namespaceManager);

                    if (firstError != null)
                    {
                        lblError.Text += " Concierge API returned error: " + firstError.Value;
                    }

                    return;
                }

                //Retrieve the portal security token from the SOAP response and store it in the web user's session so it can be used by the RedirectToPortal page
                ConciergeApiHelper.PortalLoginToken = Convert.FromBase64String(soapResponse.XPathSelectElement("//mc:CreatePortalSecurityTokenResult/r:ResultValue", namespaceManager).Value);
            }
        }

        //Send the web user to the page that will construct a form and POST the portal security token to the portal's Login.aspx page
        Response.Redirect("~/RedirectToPortal.aspx");
    }

}
