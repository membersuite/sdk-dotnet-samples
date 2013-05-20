using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using MemberSuite.SDK.Concierge;

public class ConciergeSettingsProvider : IConciergeAPIAssociationIdProvider, IConciergeAPISessionIdProvider
{
    public bool TryGetAssociationId(out string associationId)
    {
        //Retrieve the Association ID from the web.config
        associationId = ConfigurationManager.AppSettings["AssociationId"];

        return true;
    }

    public bool SetAssociationId(string associationId)
    {
        //This website can only access one Association so this value is readonly and cannot be set
        throw new NotImplementedException();
    }

    public bool TryGetSessionId(out string sessionId)
    {
        //Confirm the web user's session is available
        if (HttpContext.Current == null || HttpContext.Current.Session == null)
        {
            sessionId = null;
            return false;
        }

        //Get the Concierge API Session ID in the web user's Session. 
        sessionId = HttpContext.Current.Session["ConciergeAPISessionID"] as string;

        return true;
    }

    public bool SetSessionId(string sessionId)
    {
        //Confirm the web user's session is available
        if (HttpContext.Current == null || HttpContext.Current.Session == null)
            return false;

        //Set the Concierge API Session ID in the web user's Session.  In this way each user of this website will have their own
        //session with the Concierge API
        HttpContext.Current.Session["ConciergeAPISessionID"] = sessionId;

        return true;
    }
}