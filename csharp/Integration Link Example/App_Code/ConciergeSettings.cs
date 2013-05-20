using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using MemberSuite.SDK.Concierge;
using MemberSuite.SDK.Results;
using MemberSuite.SDK.Types;

/// <summary>
/// Summary description for ConciergeSettings
/// </summary>
public static class ConciergeSettings
{
    #region Properties

    private static object threadLock = new object();

    private static Dictionary<string, List<NameValuePair>> CachedConfigurationSettings
    {
        get
        {
            return HttpContext.Current.Session["ConciergeSettings:ConfigurationSettings"] as Dictionary<string, List<NameValuePair>>;
        }
        set { HttpContext.Current.Session["ConciergeSettings:ConfigurationSettings"] = value; }
    }

    private static string DefaultNamespace
    {
        get { return ConfigurationManager.AppSettings["ConciergeConfigurationNamespace"]; }
    }

    #endregion

    #region Constructors

    #endregion

    #region Methods

    public static string GetConfigurationSetting(string settingName)
    {
        return GetConfigurationSetting(DefaultNamespace, settingName, false);
    }

    public static string GetConfigurationSetting(string settingName, bool refreshCache)
    {
        return GetConfigurationSetting(DefaultNamespace, settingName, refreshCache);
    }

    public static string GetConfigurationSetting(string ns, string settingName)
    {
        return GetConfigurationSetting(ns, settingName, false);
    }

    public static string GetConfigurationSetting(string ns, string settingName, bool refreshCache)
    {
        if (string.IsNullOrWhiteSpace(ns))
            throw new ApplicationException("You must specify a namespace or configure a default namespace to load Concierge configuration settings");

        lock (threadLock)
        {
            if (CachedConfigurationSettings == null)
                CachedConfigurationSettings = new Dictionary<string, List<NameValuePair>>();

            if (refreshCache || !CachedConfigurationSettings.ContainsKey(ns.ToLower()))
                cacheAllConfigurationSettings(ns);

            List<NameValuePair> settings;

            if (!CachedConfigurationSettings.TryGetValue(ns.ToLower(), out settings) || settings == null)
                return null;

            var setting =
                settings.FirstOrDefault(
                    x => string.Equals(x.Name, settingName, StringComparison.InvariantCultureIgnoreCase));
            return setting.Value as string;
        }
    }

    private static void cacheAllConfigurationSettings(string ns)
    {
        if (string.IsNullOrWhiteSpace(ns))
            return;

        lock (threadLock)
        {
            using (IConciergeAPIService proxy = ConciergeAPIProxyGenerator.GenerateProxy())
            {
                ConciergeResult<List<NameValuePair>> result = proxy.GetAllConfigurationSettings(ns);

                if (!result.Success) return;

                CachedConfigurationSettings[ns.ToLower()] = result.ResultValue;
            }
        }
    }

    #endregion
}