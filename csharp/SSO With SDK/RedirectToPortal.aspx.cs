using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class RedirectToPortal : System.Web.UI.Page
{
    /// <summary>
    /// URL of the portal as defined in the web.config
    /// </summary>
    protected string PortalUrl
    {
        get { return ConfigurationManager.AppSettings["PortalUrl"]; }
    }

    /// <summary>
    /// Property to retrieve a portal security token from the web user's session
    /// </summary>
    protected byte[] PortalLoginToken
    {
        get { return Session["PortalLoginToken"] as byte[]; }
        set { Session["PortalLoginToken"] = value; }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        //If there's no portal security token in the web user's session send them back to the main page
        if(Request.UrlReferrer == null || PortalLoginToken == null)
        {
            Response.Redirect("~/default.aspx");
            return;
        }

        //Define the form variables to POST to the portal Login.aspx
        litReturnUrl.Text = Request.UrlReferrer.ToString(); //This will populate the return link in the portal
        litToken.Text = Convert.ToBase64String(PortalLoginToken); //This token allows the Single Sign On for the portal
        //litNextUrl.Text = "profile/EditIndividualInfo.aspx"; //This OPTIONAL value tells the portal where to redirect the user after login.  This MUST be a relative URI
        litAction.Text = PortalUrl + "/Login.aspx";

        //Clear out the portal security token from the web user's session
        PortalLoginToken = null;

        //This page should never be cached
        Response.Cache.SetCacheability(HttpCacheability.NoCache);
    }
}