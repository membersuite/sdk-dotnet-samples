using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class IntegrationLink : System.Web.UI.MasterPage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
            initializeBreadcrumbBar();

    }

    private void initializeBreadcrumbBar()
    {

        if (IntegrationLinkPage.CurrentUser != null)
            lblCurrentUser.Text = string.Format("{0} {1}", IntegrationLinkPage.CurrentUser.FirstName, IntegrationLinkPage.CurrentUser.LastName);
    }
}
