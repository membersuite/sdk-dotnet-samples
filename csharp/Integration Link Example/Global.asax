<%@ Application Language="C#" %>
<%@ Import Namespace="MemberSuite.SDK.Concierge" %>
<script runat="server">

    void Application_Start(object sender, EventArgs e) 
    {
        // let's register the session ID provider, which keeps the session ID
        // for a Concierge API connection in Session State
        ConciergeAPIProxyGenerator.RegisterSessionIDProvider(new ConciergeAPIProvider());
        ConciergeAPIProxyGenerator.RegisterAssociationIdProvider(new ConciergeAPIProvider());

        ConciergeAPIProxyGenerator.SetAccessKeyId(ConfigurationManager.AppSettings["AccessKeyId"]);
        ConciergeAPIProxyGenerator.SetSecretAccessKey(ConfigurationManager.AppSettings["SecretAccessKey"]);
       
       
        /* Finally, we want to listen for an event anytime an API result fails for any reason. We want to throw an
         * exception when that happens, which is caught by PortalPage.RaisePostbackEvent and shown to the user gracefully.
         * This way, we don't have to interrogate every API result for success every time we make an API call - our code can assume
         * that the API call was successful */
        ConciergeClientExtensions.OnResultError += new EventHandler<ConciergeResultErrorArgs>(ConciergeClientExtensions_OnResultError);
        ConciergeClientExtensions.SessionExpired += new EventHandler(ConciergeClientExtensions_SessionExpired);

    }

    void ConciergeClientExtensions_SessionExpired(object sender, EventArgs e)
    {
        throw new ApplicationException("Session expired - please reload link.");
    }

    
    void ConciergeClientExtensions_OnResultError(object sender, ConciergeResultErrorArgs e)
    {
        var cnt = HttpContext.Current;
 
        if (cnt == null)
            // we're going to throw a client exception on errors
            throw new ConciergeClientException(e.ErrorID, e.Code, e.Message);

      
       // just throw the exception
            throw new ConciergeClientException(e.ErrorID, e.Code, e.Message);

      
    }

    void Application_End(object sender, EventArgs e) 
    {
        //  Code that runs on application shutdown

    }
        
    void Application_Error(object sender, EventArgs e) 
    { 
        // Code that runs when an unhandled error occurs

    }

    void Session_Start(object sender, EventArgs e) 
    {
        // Code that runs when a new session is started

    }

    void Session_End(object sender, EventArgs e) 
    {
        // Code that runs when a session ends. 
        // Note: The Session_End event is raised only when the sessionstate mode
        // is set to InProc in the Web.config file. If session mode is set to StateServer 
        // or SQLServer, the event is not raised.

    }
       
</script>
