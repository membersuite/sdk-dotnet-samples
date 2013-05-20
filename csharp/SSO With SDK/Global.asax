<%@ Application Language="C#" %>
<%@ Import Namespace="MemberSuite.SDK.Concierge" %>

<script runat="server">

    void Application_Start(object sender, EventArgs e) 
    {
        //Set the MemberSuite SDK configuration settings from the web.config (see Portal Single Sign On.docx for more details)
        ConciergeAPIProxyGenerator.SetAccessKeyId(ConfigurationManager.AppSettings["AccessKeyId"]);
        ConciergeAPIProxyGenerator.SetSecretAccessKey(ConfigurationManager.AppSettings["SecretAccessKey"]);

        //Register the optional custom implementations to set/retrieve Association ID and Session ID
        ConciergeAPIProxyGenerator.RegisterAssociationIdProvider(new ConciergeSettingsProvider());
        ConciergeAPIProxyGenerator.RegisterSessionIDProvider(new ConciergeSettingsProvider());
    }
    
    void Application_End(object sender, EventArgs e) 
    {

    }
        
    void Application_Error(object sender, EventArgs e) 
    { 

    }

    void Session_Start(object sender, EventArgs e) 
    {

    }

    void Session_End(object sender, EventArgs e) 
    {
        
    }
       
</script>
