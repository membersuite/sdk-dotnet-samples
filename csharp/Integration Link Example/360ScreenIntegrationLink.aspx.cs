using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MemberSuite.SDK.Concierge;

public partial class _360ScreenIntegrationLink : IntegrationLinkPage 
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            string contextID = Request["contextID"];
            if (string.IsNullOrEmpty(contextID)) contextID = "2537d8c3-0006-ce10-841f-0b371e6de173";

            lRecordID.Text = contextID;

            if (contextID == null) return;

            var proxy = ConciergeAPIProxyGenerator.GenerateProxy();

            var mso = proxy.Get(contextID).ResultValue;

            if (mso == null)
            {
                lRecordName.Text = "Error loading record";
                return;
            }

            lRecordName.Text = mso.SafeGetValue<string>("Name");    // every object has a name
            lRecordType.Text = mso.ClassType;

            rptFields.DataSource = mso.Fields;
            rptFields.DataBind();

        }
    }
}