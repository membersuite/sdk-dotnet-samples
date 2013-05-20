using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceModel.Channels;
using System.Web;
using MemberSuite.SDK.Concierge;
using MemberSuite.SDK.Results;
using MemberSuite.SDK.Types;

/// <summary>
/// Summary description for ConciergeAPIProvider
/// </summary>
public class ConciergeAPIProvider : IConciergeAPISessionIdProvider, IConciergeAPIAssociationIdProvider
{
    private const string SessionCacheKey = "ConciergeAPISessionID";

    public static bool TryGetSessionId(out string sessionId)
    {
        if (HttpContext.Current == null)
        {
            sessionId = null;
            return false;
        }

        //By using HttpContext.Current.Items AND HttpContext.Current.SessionId the console should use this everywhere and never fall back 
        //on the ThreadStatic variable in the ConciergeAPIProxyGenerator.SessionId
        //Try the per-request cache first
        sessionId = HttpContext.Current.Items[SessionCacheKey] as string;

        if (string.IsNullOrWhiteSpace(sessionId) && HttpContext.Current.Session != null)
            sessionId = HttpContext.Current.Session[SessionCacheKey] as string;

        return true;
    }

    /// <summary>
    /// Sets the session.
    /// </summary>
    /// <param name="sessionId">The session id.</param>
    /// <returns></returns>
    public static bool SetSessionId(string sessionId)
    {
        if (HttpContext.Current == null)
            return false;

        HttpContext.Current.Items[SessionCacheKey] = sessionId;

        if (HttpContext.Current.Session != null)
            HttpContext.Current.Session[SessionCacheKey] = sessionId;

        return true;
    }

    #region IConciergeAPISessionIdProvider Implemetation

    bool IConciergeAPISessionIdProvider.TryGetSessionId(out string sessionId)
    {
        return TryGetSessionId(out sessionId);
    }

    bool IConciergeAPISessionIdProvider.SetSessionId(string sessionId)
    {
        return SetSessionId(sessionId);
    }


    #endregion

    public bool TryGetAssociationId(out string associationId)
    {
        associationId = ConfigurationManager.AppSettings["AssociationId"];

        if (!string.IsNullOrWhiteSpace(associationId))
            return true;

        msAssociation association = IntegrationLinkPage.CurrentAssociation;
        if (association == null)
            return false;

        associationId = association.ID;
        return true;
    }

    public bool SetAssociationId(string associationId)
    {
        //Not implemented because the assocition ID will be determined by the web.config or the current association property
        throw new NotImplementedException();
    }
}