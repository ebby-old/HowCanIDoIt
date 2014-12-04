using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace SharePointLogging.Common
{
    /// <summary>
    ///     Helps to log data to Event Viewer, ULS Logs & Trace Logs
    /// </summary>
    public class Logger : SPDiagnosticsServiceBase
    {
        private const string TraceLogFormat = "{0} : {1} : {2} : {3} : {4}"; //Update the log format as you want.
        private const string CacheName = "LogSettings";
        private static Logger _current;

        private Logger()
            : base("Workspace2 Logging Service", SPFarm.Local)
        {
        }

        public static Logger Current
        {
            get { return _current ?? (_current = new Logger()); }
        }

        /// <summary>
        ///     Defines all the required logging Categories.
        /// </summary>
        /// <returns></returns>
        protected override IEnumerable<SPDiagnosticsArea> ProvideAreas()
        {
            var areas = new List<SPDiagnosticsArea>
            {
                new SPDiagnosticsArea(Constants.ApplicationName,
                    new List<SPDiagnosticsCategory>
                    {
                        new SPDiagnosticsCategory(LogCategory.General, TraceSeverity.Medium, EventSeverity.Information),
                        new SPDiagnosticsCategory(LogCategory.Logging, TraceSeverity.Medium, EventSeverity.Information),
                        new SPDiagnosticsCategory(LogCategory.Demo, TraceSeverity.Verbose, EventSeverity.Information),
                        new SPDiagnosticsCategory(LogCategory.Unknown, TraceSeverity.High, EventSeverity.ErrorCritical)
                    })
            };
            return areas;
        }

        /// <summary>
        ///     Gets the category.
        /// </summary>
        /// <param name="categoryName">Name of the category.</param>
        /// <returns></returns>
        private static SPDiagnosticsCategory GetCategory(string categoryName)
        {
            SPDiagnosticsCategory category = Current.Areas[Constants.ApplicationName].Categories[categoryName] ??
                                             Current.Areas[Constants.ApplicationName].Categories[LogCategory.Unknown];

            return category;
        }

        /// <summary>
        ///     Checks if the log message needs to be displayed in Event Viewer.
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        /// <param name="site">The site. [Optional, if an SPContext is available]</param>
        /// <returns>True if the message needs to be displayed in Event Viewer</returns>
        private static bool DisplayInEventViewer(string messageType, SPSite site = null)
        {
            SPSite rootSite = site ?? SPContext.Current.Site; //If site is null, take Site from SPContext

            if (rootSite == null)
            {
                Trace.Write(
                    string.Format(TraceLogFormat, Constants.ApplicationName, "Error",
                        (int) Constants.EventId.UnableToCheckLogConfiguration, "Logging Error",
                        "Unable to check log configuration. Possible Cause : SPContext is null and No SPSite parameter is passed."));

                //Returning true to write to event viewer.
                return true;
            }

            var eventViewerLevelConfig = rootSite.Cache[CacheName] as Dictionary<string, bool>;

            //If the config values are not already cached, then get the values and cache it. 
            if (eventViewerLevelConfig == null || eventViewerLevelConfig.Count == 0)
            {
                eventViewerLevelConfig = new Dictionary<string, bool>
                {
                    //TODO: Make this configurable. In web config or a SharePoint list.
                    {Constants.ErrorConfigPropertyName, true},
                    {Constants.WarningConfigPropertyName, true},
                    {Constants.InfoConfigPropertyName, true}
                };

                rootSite.Cache[CacheName] = eventViewerLevelConfig;
            }

            switch (messageType.ToUpperInvariant())
            {
                case "ERROR":
                    return eventViewerLevelConfig[Constants.ErrorConfigPropertyName];
                case "WARNING":
                    return eventViewerLevelConfig[Constants.WarningConfigPropertyName];
                case "INFO":
                    return eventViewerLevelConfig[Constants.InfoConfigPropertyName];
                default:
                    return false;
            }
        }

        /// <summary>
        ///     Logs an error message.
        /// </summary>
        /// <param name="eventId">The event identifier.</param>
        /// <param name="categoryName">Name of the category.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="site">The site. [Optional, if an SPContext is available]</param>
        public static void LogError(Constants.EventId eventId, string categoryName, string errorMessage,
            SPSite site = null)
        {
            SPDiagnosticsCategory category = GetCategory(categoryName);

            if (DisplayInEventViewer("Error", site))
            {
                try
                {
                    Current.WriteEvent((ushort) eventId, category, EventSeverity.Error, errorMessage);
                }
                catch (Exception ex)
                {
                    Trace.Write(
                        string.Format(TraceLogFormat, Constants.ApplicationName, "Error",
                            (int) Constants.EventId.UnableToWriteToEventViewer, "Logging Error",
                            "Unable to Write to Event Viewer. " + ex.StackTrace),
                        "Logging Error");
                }
            }
            else
            {
                try
                {
                    Current.WriteTrace((uint) eventId, category, TraceSeverity.Unexpected, errorMessage);
                }
                catch (Exception ex)
                {
                    Trace.Write(
                        string.Format(TraceLogFormat, Constants.ApplicationName, "Error",
                            (int) Constants.EventId.UnableToWriteToUlsLog, "Logging Error",
                            "Unable to Write to Uls Log. " + ex.StackTrace)
                        , "Logging Error");
                }
            }

            Trace.Write(string.Format(TraceLogFormat, Constants.ApplicationName, "Error", (int) eventId, categoryName,
                errorMessage));
        }

        /// <summary>
        ///     Logs a warning message.
        /// </summary>
        /// <param name="eventId">The event identifier.</param>
        /// <param name="categoryName">Name of the category.</param>
        /// <param name="warningMessage">The error message.</param>
        /// <param name="site">The site. [Optional, if an SPContext is available]</param>
        public static void LogWarning(Constants.EventId eventId, string categoryName, string warningMessage,
            SPSite site = null)
        {
            SPDiagnosticsCategory category = GetCategory(categoryName);

            if (DisplayInEventViewer("Warning", site))
            {
                try
                {
                    Current.WriteEvent((ushort) eventId, category, EventSeverity.Warning, warningMessage);
                }
                catch (Exception ex)
                {
                    Trace.Write(
                        string.Format(TraceLogFormat, Constants.ApplicationName, "Error",
                            (int) Constants.EventId.UnableToWriteToEventViewer, "Logging Error",
                            "Unable to Write to Event Viewer. " + ex.StackTrace)
                        , "Logging Error");
                }
            }
            else
            {
                try
                {
                    Current.WriteTrace((uint) eventId, category, TraceSeverity.Monitorable, warningMessage);
                }
                catch (Exception ex)
                {
                    Trace.Write(
                        string.Format(TraceLogFormat, Constants.ApplicationName, "Error",
                            (int) Constants.EventId.UnableToWriteToUlsLog, "Logging Error",
                            "Unable to Write to Uls Log. " + ex.StackTrace)
                        , "Logging Error");
                }
            }

            Trace.Write(string.Format(TraceLogFormat, Constants.ApplicationName, "Warning", (int) eventId, categoryName,
                warningMessage));
        }

        /// <summary>
        ///     Logs an information message.
        /// </summary>
        /// <param name="eventId">The event identifier.</param>
        /// <param name="categoryName">Name of the category.</param>
        /// <param name="informationMessage">The error message.</param>
        /// <param name="site">The site. [Optional, if an SPContext is available]</param>
        public static void LogInfo(Constants.EventId eventId, string categoryName, string informationMessage,
            SPSite site = null)
        {
            SPDiagnosticsCategory category = GetCategory(categoryName);

            if (DisplayInEventViewer("Info", site))
            {
                try
                {
                    Current.WriteEvent((ushort) eventId, category, EventSeverity.Information, informationMessage);
                }
                catch (Exception ex)
                {
                    Trace.Write(
                        string.Format(TraceLogFormat, Constants.ApplicationName, "Error",
                            (int) Constants.EventId.UnableToWriteToEventViewer, "Logging Error",
                            "Unable to Write to Event Viewer. " + ex.StackTrace)
                        , "Logging Error");
                }
            }
            else
            {
                try
                {
                    Current.WriteTrace((uint) eventId, category, TraceSeverity.Medium, informationMessage);
                }
                catch (Exception ex)
                {
                    Trace.Write(
                        string.Format(TraceLogFormat, Constants.ApplicationName, "Error",
                            (int) Constants.EventId.UnableToWriteToUlsLog, "Logging Error",
                            "Unable to Write to Uls Log. " + ex.StackTrace)
                        , "Logging Error");
                }
            }

            Trace.Write(string.Format(TraceLogFormat, Constants.ApplicationName, "Info", (int) eventId, categoryName,
                informationMessage));
        }

        /// <summary>
        ///     Logs a verbose message to Trace Log.
        /// </summary>
        /// <param name="categoryName">Name of the category.</param>
        /// <param name="message">The message.</param>
        public static void LogVerbose(string categoryName, string message)
        {
            Trace.Write(string.Format(TraceLogFormat, Constants.ApplicationName, "Verbose", "Verbose", categoryName,
                message));
        }
    }

    /// <summary>
    ///     The Log Categories
    /// </summary>
    public static class LogCategory
    {
        public static string General = "General";
        public static string Logging = "Logging";
        public static string Demo = "Demo";
        public static string Unknown = "Unknown";
    }
}