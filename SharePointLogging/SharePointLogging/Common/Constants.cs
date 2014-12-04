namespace SharePointLogging.Common
{
    public static class Constants
    {
        public enum EventId
        {
            //General Events (1000 - 1004)
            CatastrophicSharePointError = 1000,
            GeneralSharePointMessage = 1001,

            //Logging Events (1005 - 1009)
            UnableToWriteToEventViewer = 1005,
            UnableToWriteToUlsLog = 1006,
            UndefinedLogCategory = 1007,
            ConfigListNotFound = 1008,
            UnableToCheckLogConfiguration = 1009,

            //Caching Events (1010 - 1014)
            CachingProcessStart = 1010,
            CachingProcessEnd = 1011,
            NullCacheObject = 1012,
            CacheMiss = 1013,
            CacheAdded = 1014,
        }

        //Logging
        public const string ApplicationName = "HowCanIDoIt";
        public const string ErrorConfigPropertyName = "Error to Event Viewer";
        public const string WarningConfigPropertyName = "Warning to Event Viewer";
        public const string InfoConfigPropertyName = "Info to Event Viewer";
    }
}