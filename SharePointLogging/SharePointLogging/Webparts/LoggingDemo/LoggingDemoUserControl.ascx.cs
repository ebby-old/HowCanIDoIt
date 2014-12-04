using System;
using System.Web.UI;
using SharePointLogging.Common;

namespace SharePointLogging.Webparts.LoggingDemo
{
    public partial class LoggingDemoUserControl : UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void LogError_OnClick(object sender, EventArgs e)
        {
            Logger.LogError(Constants.EventId.GeneralSharePointMessage, 
                LogCategory.Demo, 
                "This is a test error message");
        }

        protected void LogWarning_OnClick(object sender, EventArgs e)
        {
            Logger.LogWarning(Constants.EventId.GeneralSharePointMessage, 
                LogCategory.Demo,
                "This is a test warning message");
        }

        protected void LogInfo_OnClick(object sender, EventArgs e)
        {
            Logger.LogInfo(Constants.EventId.GeneralSharePointMessage, 
                LogCategory.Demo, 
                "This is a test info message");
        }

        protected void LogVerbose_OnClick(object sender, EventArgs e)
        {
            Logger.LogVerbose(LogCategory.Demo, 
                "This is a test error message");
        }
    }
}