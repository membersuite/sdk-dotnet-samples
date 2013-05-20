using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MemberSuite.SDK.Concierge;
using MemberSuite.SDK.Results;
using MemberSuite.SDK.Types;

public partial class _Default : IntegrationLinkPage 
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            // now, let's assume the identity
            // first, we need a proxy
            // The "False" means we DON'T want the API to throw an exception if an error occurs
            IConciergeAPIService proxy = ConciergeAPIProxyGenerator.GenerateProxy();

            // now, let's contact the API and use the session ID provided
            var loginResult = proxy.WhoAmI();

            if (!loginResult.Success) // something went wrong
            {
                Response.Write("There was a problem: " + loginResult.Errors[0].Message);
                return;
            }

            // let's save the session ID - now, its part of the header
            // once we've done that, we can use the API across postbacks and pages
            // b/c the session will be sent in the header of each request
            ConciergeAPIProxyGenerator.SessionID = loginResult.ResultValue.SessionID;

            msUser user = new msUser(loginResult.ResultValue.User);

            // ok, let's set our user name
            lblUserName.Text = user.Name;
            tbUserDepartment.Text = user.Department;
            lblUserDepartment.Text = user.Department;

            
        }

    }

    protected void btnChangeDepartment_Click(object sender, EventArgs e)
    {
        if (!IsValid)
            return;

        IConciergeAPIService proxy = ConciergeAPIProxyGenerator.GenerateProxy();

        // let's get the user from the session
        var loginResult = proxy.WhoAmI();

        if (!loginResult.Success) // something went wrong
        {
            Response.Write("There was a problem: " + loginResult.Errors[0].Message);
            return;
        }

        
        msUser user = new msUser(loginResult.ResultValue.User);


        // now, change the department
        user.Department = tbUserDepartment.Text;

        // now, we have to save it
        // again, the false keeps an exception from being thrown
      
        ConciergeResult<MemberSuiteObject> result = proxy.Save(user);

        if (!result.Success)
        {
            Response.Write("Error occured: " + result.Errors[0].Message);
            return;
        }

        user = new msUser(result.ResultValue);
        // store the user in the session - it's serializable
       
        lblUserDepartment.Text = user.Department;
        tbUserDepartment.Text = "";

        Response.Write("Department Successfully updated.");


    }
}